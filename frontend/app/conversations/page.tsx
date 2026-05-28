"use client";

import Link from "next/link";
import { useEffect, useMemo, useState } from "react";
import ProtectedRoute from "@/components/ProtectedRoute";
import TopBar from "@/components/TopBar";
import Toast, { type ToastState } from "@/components/Toast";
import {
  getConversations,
  type ConversationDto,
} from "../lib/chat-api";
import { getProfile, type UserProfile } from "../lib/profile-api";

function formatDateTime(value?: string | null) {
  if (!value) return "";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "";
  return date.toLocaleString();
}

function getLastSeenKey(meetupId: number, otherUserId: number) {
  return `chat:lastSeen:${meetupId}:${otherUserId}`;
}

function getLastSeenAt(meetupId: number, otherUserId: number) {
  if (typeof window === "undefined") return null;
  const value = localStorage.getItem(getLastSeenKey(meetupId, otherUserId));
  if (!value) return null;
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? null : date;
}

export default function ConversationsPage() {
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [conversations, setConversations] = useState<ConversationDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [toast, setToast] = useState<ToastState | null>(null);

  function showToast(nextToast: ToastState) {
    setToast(nextToast);
    globalThis.setTimeout(() => setToast(null), 3600);
  }

  async function loadConversations() {
    setIsLoading(true);
    try {
      const [profileData, conversationData] = await Promise.all([
        getProfile(),
        getConversations(),
      ]);
      setProfile(profileData);
      setConversations(conversationData);
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Unable to load conversations.";
      showToast({ tone: "error", message });
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    loadConversations();
  }, []);

  useEffect(() => {
    const intervalId = globalThis.setInterval(() => {
      loadConversations();
    }, 12000);
    return () => globalThis.clearInterval(intervalId);
  }, []);

  const meetupConversations = useMemo(
    () => conversations.filter((conversation) => conversation.meetupEventId),
    [conversations],
  );

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
                <Link className="text-lg font-semibold tracking-wide" href="/meetups">
                  RealLifeConnections
                </Link>
                <p className="mt-2 text-xs font-semibold uppercase tracking-[0.2em] text-emerald-700">
                  Conversations
                </p>
                <h1 className="mt-3 text-3xl font-bold leading-tight sm:text-4xl">
                  Meetup chats
                </h1>
              </div>
              <div className="flex items-center gap-3">
                <button
                  type="button"
                  className="rounded-md border border-zinc-300 bg-white px-4 py-2 text-sm font-semibold text-zinc-900 shadow-sm transition hover:border-zinc-500"
                  onClick={loadConversations}
                >
                  Refresh
                </button>
              </div>
            </header>

            <Toast toast={toast} />

            {isLoading ? (
              <div className="mt-10 rounded-2xl border border-zinc-200 bg-white/80 p-6 shadow-sm">
                <p className="text-sm text-zinc-600">Loading conversations...</p>
              </div>
            ) : null}

            {!isLoading ? (
              <section className="mt-10 rounded-2xl border border-zinc-200 bg-white/90 p-6 shadow-sm">
                <div className="flex flex-wrap items-center justify-between gap-3">
                  <p className="text-xs font-semibold uppercase tracking-[0.2em] text-emerald-700">
                    Active chats
                  </p>
                  <span className="rounded-full bg-zinc-100 px-3 py-1 text-xs font-semibold text-zinc-700">
                    {meetupConversations.length} total
                  </span>
                </div>

                {meetupConversations.length === 0 ? (
                  <p className="mt-4 text-sm text-zinc-600">
                    You have no meetup chats yet.
                  </p>
                ) : (
                  <div className="mt-5 grid gap-4 md:grid-cols-2">
                    {meetupConversations.map((conversation) => {
                      const meetupId = conversation.meetupEventId ?? 0;
                      const lastSeenAt = getLastSeenAt(meetupId, conversation.otherUserId);
                      const lastMessageAt = new Date(conversation.lastMessageAt);
                      const hasUnread =
                        !Number.isNaN(lastMessageAt.getTime()) &&
                        (!lastSeenAt || lastMessageAt > lastSeenAt);

                      return (
                        <Link
                          key={conversation.conversationId}
                          href={`/conversations/${conversation.meetupEventId}/${conversation.otherUserId}`}
                          className="rounded-2xl border border-zinc-200 bg-white p-5 shadow-sm transition hover:border-emerald-200"
                        >
                          <div className="flex items-center justify-between gap-3">
                            <div>
                              <p className="text-xs font-semibold uppercase tracking-[0.2em] text-amber-600">
                                {conversation.meetupStatus ?? "Meetup"}
                              </p>
                              <h3 className="mt-2 text-lg font-bold text-zinc-900">
                                {conversation.meetupTitle ?? "Meetup"}
                              </h3>
                            </div>
                            <div className="flex flex-wrap items-center gap-2">
                              {hasUnread ? (
                                <span className="rounded-full bg-rose-50 px-2 py-1 text-xs font-semibold text-rose-700">
                                  New
                                </span>
                              ) : null}
                              <span className="rounded-full bg-emerald-50 px-3 py-1 text-xs font-semibold text-emerald-700">
                                {conversation.otherUserName ?? "Participant"}
                              </span>
                            </div>
                          </div>
                          <div className="mt-3 text-xs text-zinc-500">
                            Last message {formatDateTime(conversation.lastMessageAt)}
                          </div>
                          <div className="mt-3 flex flex-wrap gap-2 text-xs text-zinc-500">
                            {conversation.isClosed || conversation.isExpired ? (
                              <span className="rounded-full bg-rose-50 px-2 py-1 font-semibold text-rose-700">
                                Closed
                              </span>
                            ) : (
                              <span className="rounded-full bg-emerald-50 px-2 py-1 font-semibold text-emerald-700">
                                Open
                              </span>
                            )}
                            {conversation.endsAt ? (
                              <span className="rounded-full bg-zinc-100 px-2 py-1 font-semibold text-zinc-600">
                                Ends {formatDateTime(conversation.endsAt)}
                              </span>
                            ) : null}
                          </div>
                        </Link>
                      );
                    })}
                  </div>
                )}
              </section>
            ) : null}
          </div>
        </div>
      </main>
    </ProtectedRoute>
  );
}
