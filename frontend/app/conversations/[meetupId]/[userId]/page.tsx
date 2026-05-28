"use client";

import Link from "next/link";
import { useParams } from "next/navigation";
import { useEffect, useMemo, useState } from "react";
import ProtectedRoute from "@/components/ProtectedRoute";
import TopBar from "@/components/TopBar";
import Toast, { type ToastState } from "@/components/Toast";
import {
  getMeetupMessages,
  sendMeetupMessage,
  type MessageResponseDto,
} from "../../../lib/chat-api";
import { getMeetup, type MeetupEventDto } from "../../../lib/meetup-api";
import { getProfile, type UserProfile } from "../../../lib/profile-api";

function getMeetupEndsAt(meetup: MeetupEventDto) {
  if (meetup.completedAt) return new Date(meetup.completedAt);
  if (!meetup.endTime) return null;
  const baseDate = new Date(meetup.eventDate);
  if (Number.isNaN(baseDate.getTime())) return null;
  const [hours, minutes, seconds] = meetup.endTime.split(":").map(Number);
  if (Number.isNaN(hours) || Number.isNaN(minutes) || Number.isNaN(seconds)) {
    return null;
  }
  const endsAt = new Date(baseDate);
  endsAt.setHours(hours, minutes, seconds, 0);
  const [startHours] = meetup.startTime.split(":").map(Number);
  if (!Number.isNaN(startHours) && hours < startHours) {
    endsAt.setDate(endsAt.getDate() + 1);
  }
  return endsAt;
}

function formatDateTime(value?: string | null) {
  if (!value) return "";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "";
  return date.toLocaleString();
}

function getLastSeenKey(meetupId: number, otherUserId: number) {
  return `chat:lastSeen:${meetupId}:${otherUserId}`;
}

function markConversationSeen(meetupId: number, otherUserId: number) {
  if (typeof window === "undefined") return;
  localStorage.setItem(getLastSeenKey(meetupId, otherUserId), new Date().toISOString());
}

export default function ConversationDetailPage() {
  const params = useParams<{ meetupId: string; userId: string }>();
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [meetup, setMeetup] = useState<MeetupEventDto | null>(null);
  const [messages, setMessages] = useState<MessageResponseDto[]>([]);
  const [chatDraft, setChatDraft] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [isSending, setIsSending] = useState(false);
  const [toast, setToast] = useState<ToastState | null>(null);

  const meetupId = useMemo(() => Number(params.meetupId), [params.meetupId]);
  const otherUserId = useMemo(() => Number(params.userId), [params.userId]);

  function showToast(nextToast: ToastState) {
    setToast(nextToast);
    globalThis.setTimeout(() => setToast(null), 3600);
  }

  async function loadConversation() {
    if (!Number.isFinite(meetupId) || !Number.isFinite(otherUserId)) {
      showToast({ tone: "error", message: "Invalid conversation route." });
      setIsLoading(false);
      return;
    }

    setIsLoading(true);
    try {
      const [profileData, meetupData, messageData] = await Promise.all([
        getProfile(),
        getMeetup(meetupId),
        getMeetupMessages(meetupId, otherUserId),
      ]);
      setProfile(profileData);
      setMeetup(meetupData);
      setMessages(messageData);
      markConversationSeen(meetupId, otherUserId);
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Unable to load conversation.";
      showToast({ tone: "error", message });
    } finally {
      setIsLoading(false);
    }
  }

  async function refreshMessages() {
    if (!Number.isFinite(meetupId) || !Number.isFinite(otherUserId)) return;
    try {
      const messageData = await getMeetupMessages(meetupId, otherUserId);
      setMessages(messageData);
      markConversationSeen(meetupId, otherUserId);
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Unable to refresh messages.";
      showToast({ tone: "error", message });
    }
  }

  useEffect(() => {
    loadConversation();
  }, [meetupId, otherUserId]);

  useEffect(() => {
    const intervalId = globalThis.setInterval(() => {
      refreshMessages();
    }, 12000);
    return () => globalThis.clearInterval(intervalId);
  }, [meetupId, otherUserId]);

  const meetupEndsAt = useMemo(() => (meetup ? getMeetupEndsAt(meetup) : null), [meetup]);
  const otherUserName = useMemo(() => {
    if (!meetup) return "Participant";
    if (meetup.creatorId === otherUserId) return meetup.creatorName;
    const participant = meetup.participants.find(
      (item) => item.userId === otherUserId,
    );
    return participant?.userName ?? "Participant";
  }, [meetup, otherUserId]);
  const isChatClosed = useMemo(() => {
    if (!meetup) return true;
    if (meetup.status === "Cancelled" || meetup.status === "Completed") return true;
    if (!meetupEndsAt) return false;
    return meetupEndsAt.getTime() < Date.now();
  }, [meetup, meetupEndsAt]);

  async function handleSend() {
    const trimmed = chatDraft.trim();
    if (!trimmed || !meetup || !Number.isFinite(otherUserId)) return;
    if (isChatClosed) {
      showToast({ tone: "error", message: "Chat is closed for this meetup." });
      return;
    }

    setIsSending(true);
    try {
      await sendMeetupMessage({
        receiverId: otherUserId,
        content: trimmed,
        meetupEventId: meetup.id,
      });
      setChatDraft("");
      await refreshMessages();
      markConversationSeen(meetupId, otherUserId);
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Unable to send message.";
      showToast({ tone: "error", message });
    } finally {
      setIsSending(false);
    }
  }

  return (
    <ProtectedRoute>
      <main className="min-h-screen bg-[#f7faf4] text-zinc-950 font-sans">
        <div className="relative overflow-hidden">
          <div className="pointer-events-none absolute inset-0">
            <div className="absolute -left-24 top-10 h-60 w-60 rounded-full bg-emerald-200/40 blur-3xl" />
            <div className="absolute right-0 top-0 h-72 w-72 rounded-full bg-amber-200/40 blur-3xl" />
          </div>

          <div className="relative mx-auto flex min-h-screen w-full max-w-6xl flex-col px-5 py-6 sm:px-8 lg:px-10">
            <TopBar profile={profile} />

            <header className="flex flex-wrap items-center justify-between gap-4">
              <div>
                <Link className="text-lg font-semibold tracking-wide" href="/conversations">
                  Conversations
                </Link>
                <p className="mt-2 text-xs font-semibold uppercase tracking-[0.2em] text-emerald-700">
                  Meetup chat
                </p>
                <h1 className="mt-3 text-3xl font-bold leading-tight sm:text-4xl">
                  {meetup?.title ?? "Meetup"}
                </h1>
              </div>
              <div className="flex items-center gap-3">
                <button
                  type="button"
                  className="rounded-md border border-zinc-300 bg-white px-4 py-2 text-sm font-semibold text-zinc-900 shadow-sm transition hover:border-zinc-500"
                  onClick={refreshMessages}
                >
                  Refresh
                </button>
              </div>
            </header>

            <Toast toast={toast} />

            {isLoading ? (
              <div className="mt-10 rounded-2xl border border-zinc-200 bg-white/80 p-6 shadow-sm">
                <p className="text-sm text-zinc-600">Loading conversation...</p>
              </div>
            ) : null}

            {!isLoading && meetup ? (
              <section className="mt-10 grid gap-8 lg:grid-cols-[1.1fr_0.9fr]">
                <div className="space-y-6">
                  <div className="rounded-2xl border border-zinc-200 bg-white/90 p-6 shadow-sm">
                    <div className="flex flex-wrap items-center justify-between gap-3">
                      <div>
                        <p className="text-xs font-semibold uppercase tracking-[0.2em] text-amber-600">
                          {meetup.status}
                        </p>
                        <h2 className="mt-2 text-2xl font-bold text-zinc-950">
                          Chat with {otherUserName}
                        </h2>
                      </div>
                      {meetupEndsAt ? (
                        <span className="rounded-full bg-emerald-50 px-3 py-1 text-xs font-semibold text-emerald-700">
                          Ends {meetupEndsAt.toLocaleString()}
                        </span>
                      ) : null}
                    </div>

                    <div className="mt-4 text-sm text-zinc-600">
                      {meetup.region} | {meetup.suburb}
                      {meetup.locationName ? ` | ${meetup.locationName}` : ""}
                    </div>

                    <div className="mt-4 max-h-[420px] space-y-3 overflow-y-auto rounded-xl border border-zinc-200 bg-white/70 p-4">
                      {messages.length === 0 ? (
                        <p className="text-sm text-zinc-600">No messages yet.</p>
                      ) : (
                        messages.map((message) => (
                          <div
                            key={message.id}
                            className={`flex ${
                              message.senderId === profile?.id
                                ? "justify-end"
                                : "justify-start"
                            }`}
                          >
                            <div
                              className={`max-w-[75%] rounded-2xl px-4 py-2 text-sm shadow-sm ${
                                message.senderId === profile?.id
                                  ? "bg-emerald-600 text-white"
                                  : "bg-white text-zinc-700"
                              }`}
                            >
                              <p>{message.content}</p>
                              <p
                                className={`mt-1 text-[10px] ${
                                  message.senderId === profile?.id
                                    ? "text-emerald-100"
                                    : "text-zinc-400"
                                }`}
                              >
                                {formatDateTime(message.createdAt)}
                              </p>
                            </div>
                          </div>
                        ))
                      )}
                    </div>

                    <div className="mt-4 space-y-3">
                      <textarea
                        className="min-h-[110px] w-full rounded-xl border border-zinc-200 bg-white px-3 py-2 text-sm text-zinc-700 outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
                        placeholder={
                          isChatClosed
                            ? "Chat closed after meetup ended."
                            : "Write a message..."
                        }
                        value={chatDraft}
                        onChange={(event) => setChatDraft(event.target.value)}
                        disabled={isChatClosed || isSending}
                      />
                      <div className="flex flex-wrap items-center justify-between gap-3">
                        <p className="text-xs text-zinc-500">
                          {isChatClosed
                            ? "Chat is closed for this meetup."
                            : "Messages stay open until the meetup finishes."}
                        </p>
                        <button
                          type="button"
                          className="rounded-full bg-emerald-600 px-4 py-2 text-xs font-semibold text-white transition hover:bg-emerald-700"
                          onClick={handleSend}
                          disabled={isSending || isChatClosed}
                        >
                          Send
                        </button>
                      </div>
                    </div>
                  </div>
                </div>

                <aside className="space-y-6">
                  <div className="rounded-2xl border border-zinc-200 bg-white/90 p-6 shadow-sm">
                    <p className="text-xs font-semibold uppercase tracking-[0.2em] text-emerald-700">
                      Meetup details
                    </p>
                    <div className="mt-3 space-y-2 text-sm text-zinc-600">
                      <p>Host: {meetup.creatorName}</p>
                      <p>
                        Date: {formatDateTime(meetup.eventDate)}
                      </p>
                      <p>
                        Time: {meetup.startTime}
                        {meetup.endTime ? ` - ${meetup.endTime}` : ""}
                      </p>
                      <p>Status: {meetup.status}</p>
                    </div>
                  </div>
                </aside>
              </section>
            ) : null}
          </div>
        </div>
      </main>
    </ProtectedRoute>
  );
}
