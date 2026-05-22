"use client";

import Link from "next/link";
import { useParams, useRouter, useSearchParams } from "next/navigation";
import { useEffect, useMemo, useState } from "react";
import ProtectedRoute from "@/components/ProtectedRoute";
import Toast, { type ToastState } from "@/components/Toast";
import {
  applyMeetup,
  approveParticipant,
  deleteMeetup,
  getMeetup,
  quitMeetup,
  rejectParticipant,
  updateMeetupStatus,
  type MeetupEventDto,
} from "../../lib/meetup-api";
import { getProfile, type UserProfile } from "../../lib/profile-api";

const statusOptions = [
  "Open",
  "Confirming",
  "Confirmed",
  "Completed",
  "Cancelled",
] as const;

function formatDateInput(value?: string | null) {
  if (!value) return "";
  return value.split("T")[0];
}

function formatTimeInput(value?: string | null) {
  if (!value) return "";
  return value.slice(0, 5);
}

export default function MeetupDetailPage() {
  const params = useParams<{ meetupId: string }>();
  const router = useRouter();
  const searchParams = useSearchParams();
  const [toast, setToast] = useState<ToastState | null>(null);
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [meetup, setMeetup] = useState<MeetupEventDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const meetupId = useMemo(() => Number(params.meetupId), [params.meetupId]);
  const backHref = useMemo(() => {
    const tab = searchParams.get("tab");
    if (tab === "create" || tab === "manage" || tab === "match") {
      return `/meetups?tab=${tab}`;
    }
    return "/meetups";
  }, [searchParams]);

  function showToast(nextToast: ToastState) {
    setToast(nextToast);
    globalThis.setTimeout(() => setToast(null), 3600);
  }

  async function loadMeetup() {
    if (!Number.isFinite(meetupId)) {
      showToast({ tone: "error", message: "Invalid meetup id." });
      setIsLoading(false);
      return;
    }

    setIsLoading(true);
    try {
      const [profileData, meetupData] = await Promise.all([
        getProfile(),
        getMeetup(meetupId),
      ]);
      setProfile(profileData);
      setMeetup(meetupData);
    } catch (error) {
      const message = error instanceof Error ? error.message : "Unable to load meetup.";
      showToast({ tone: "error", message });
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    loadMeetup();
  }, [meetupId]);

  const userParticipant = useMemo(() => {
    if (!meetup || !profile) return null;
    return meetup.participants.find((participant) => participant.userId === profile.id) ?? null;
  }, [meetup, profile]);

  const isCreator = profile && meetup ? profile.id === meetup.creatorId : false;
  const canLeave =
    userParticipant && !["Left", "Rejected"].includes(userParticipant.status);
  const approvedParticipants = useMemo(() => {
    if (!meetup) return [];
    return meetup.participants.filter(
      (participant) => participant.status === "Approved",
    );
  }, [meetup]);
  const pendingParticipants = useMemo(() => {
    if (!meetup) return [];
    const base = meetup.participants.filter(
      (participant) => participant.status === "Pending",
    );
    if (isCreator) return base;
    return profile?.id ? base.filter((participant) => participant.userId === profile.id) : [];
  }, [isCreator, meetup, profile?.id]);
  const removedParticipants = useMemo(() => {
    if (!meetup) return [];
    const base = meetup.participants.filter((participant) =>
      ["Left", "Rejected"].includes(participant.status),
    );
    if (isCreator) return base;
    return profile?.id ? base.filter((participant) => participant.userId === profile.id) : [];
  }, [isCreator, meetup, profile?.id]);

  async function handleJoin() {
    if (!meetup) return;
    if (!profile?.isVerified) {
      showToast({ tone: "error", message: "Verify your avatar before joining." });
      return;
    }

    setIsSubmitting(true);
    try {
      await applyMeetup(meetup.id);
      showToast({ tone: "success", message: "Join request sent." });
      await loadMeetup();
    } catch (error) {
      const message = error instanceof Error ? error.message : "Unable to join meetup.";
      showToast({ tone: "error", message });
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleLeave() {
    if (!meetup) return;

    setIsSubmitting(true);
    try {
      await quitMeetup(meetup.id);
      showToast({ tone: "success", message: "Meetup left." });
      await loadMeetup();
    } catch (error) {
      const message = error instanceof Error ? error.message : "Unable to leave meetup.";
      showToast({ tone: "error", message });
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleApproveParticipant(participantId: number) {
    if (!meetup) return;

    setIsSubmitting(true);
    try {
      await approveParticipant(meetup.id, participantId);
      showToast({ tone: "success", message: "Participant approved." });
      await loadMeetup();
    } catch (error) {
      const message = error instanceof Error ? error.message : "Unable to approve participant.";
      showToast({ tone: "error", message });
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleRejectParticipant(participantId: number) {
    if (!meetup) return;

    setIsSubmitting(true);
    try {
      await rejectParticipant(meetup.id, participantId);
      showToast({ tone: "success", message: "Participant rejected." });
      await loadMeetup();
    } catch (error) {
      const message = error instanceof Error ? error.message : "Unable to reject participant.";
      showToast({ tone: "error", message });
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleStatusChange(status: string) {
    if (!meetup) return;

    setIsSubmitting(true);
    try {
      await updateMeetupStatus(meetup.id, status);
      showToast({ tone: "success", message: "Status updated." });
      await loadMeetup();
    } catch (error) {
      const message = error instanceof Error ? error.message : "Unable to update status.";
      showToast({ tone: "error", message });
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleDelete() {
    if (!meetup) return;

    setIsSubmitting(true);
    try {
      await deleteMeetup(meetup.id);
      showToast({ tone: "success", message: "Meetup deleted." });
      router.push("/meetups");
    } catch (error) {
      const message = error instanceof Error ? error.message : "Unable to delete meetup.";
      showToast({ tone: "error", message });
    } finally {
      setIsSubmitting(false);
    }
  }

  let primaryAction: React.ReactNode = null;
  if (!isCreator) {
    if (canLeave) {
      primaryAction = (
        <button
          type="button"
          className="rounded-full border border-rose-200 bg-rose-50 px-4 py-2 text-xs font-semibold text-rose-700 transition hover:border-rose-300"
          onClick={handleLeave}
          disabled={isSubmitting}
        >
          Leave meetup
        </button>
      );
    } else {
      primaryAction = (
        <button
          type="button"
          className="rounded-full bg-emerald-600 px-4 py-2 text-xs font-semibold text-white transition hover:bg-emerald-700"
          onClick={handleJoin}
          disabled={isSubmitting}
        >
          Join meetup
        </button>
      );
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
            <header className="flex flex-wrap items-center justify-between gap-4">
              <div>
                <Link className="text-lg font-semibold tracking-wide" href={backHref}>
                  RealLifeConnections
                </Link>
                <p className="mt-2 text-xs font-semibold uppercase tracking-[0.2em] text-emerald-700">
                  Meetup Details
                </p>
                <h1 className="mt-3 text-3xl font-bold leading-tight sm:text-4xl">
                  {meetup?.title ?? "Meetup"}
                </h1>
              </div>
              <div className="flex items-center gap-3">
                <Link
                  className="rounded-md border border-zinc-300 bg-white px-4 py-2 text-sm font-semibold text-zinc-900 shadow-sm transition hover:border-zinc-500"
                  href={backHref}
                >
                  Back to meetups
                </Link>
              </div>
            </header>

            <Toast toast={toast} />

            {isLoading ? (
              <div className="mt-10 rounded-2xl border border-zinc-200 bg-white/80 p-6 shadow-sm">
                <p className="text-sm text-zinc-600">Loading meetup details...</p>
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
                          {meetup.title}
                        </h2>
                      </div>
                      <span className="rounded-full bg-emerald-50 px-3 py-1 text-xs font-semibold text-emerald-700">
                        {approvedParticipants.length}/{meetup.maxParticipants} joined
                      </span>
                    </div>

                    <div className="mt-4 space-y-2 text-sm text-zinc-600">
                      <p>
                        {meetup.region} | {meetup.suburb}
                        {meetup.locationName ? ` | ${meetup.locationName}` : ""}
                      </p>
                      <p>
                        {formatDateInput(meetup.eventDate)} | {formatTimeInput(
                          meetup.startTime,
                        )}
                        {meetup.endTime ? ` - ${formatTimeInput(meetup.endTime)}` : ""}
                      </p>
                      <p>Max distance: {meetup.maxDistanceKm} km</p>
                      <p>Host: {meetup.creatorName}</p>
                    </div>

                    {meetup.description ? (
                      <p className="mt-4 text-sm text-zinc-600">
                        {meetup.description}
                      </p>
                    ) : null}

                    <div className="mt-5 flex flex-wrap gap-2">
                      {meetup.activities.map((activity) => (
                        <span
                          key={`${meetup.id}-${activity.name}`}
                          className="rounded-full border border-emerald-200 bg-emerald-50 px-3 py-1 text-xs font-semibold text-emerald-700"
                        >
                          {activity.name} | {activity.type}
                        </span>
                      ))}
                    </div>

                    <div className="mt-6 flex flex-wrap gap-3">
                      {primaryAction}
                      <button
                        type="button"
                        className="rounded-full border border-zinc-300 bg-white px-4 py-2 text-xs font-semibold text-zinc-700 transition hover:border-zinc-500"
                        onClick={loadMeetup}
                        disabled={isSubmitting}
                      >
                        Refresh
                      </button>
                    </div>
                  </div>

                  <div className="rounded-2xl border border-zinc-200 bg-white/90 p-6 shadow-sm">
                    <div className="flex flex-wrap items-center justify-between gap-3">
                      <p className="text-xs font-semibold uppercase tracking-[0.2em] text-emerald-700">
                        Participants
                      </p>
                      <span className="rounded-full bg-zinc-100 px-3 py-1 text-xs font-semibold text-zinc-700">
                        {approvedParticipants.length} total
                      </span>
                    </div>

                    {approvedParticipants.length === 0 ? (
                      <p className="mt-4 text-sm text-zinc-600">
                        No participants yet.
                      </p>
                    ) : (
                      <div className="mt-4 space-y-3">
                        {approvedParticipants.map((participant) => (
                          <div
                            key={participant.id}
                            className="flex flex-wrap items-center justify-between gap-3 rounded-lg border border-zinc-200 bg-white px-4 py-3"
                          >
                            <div>
                              <p className="text-sm font-semibold text-zinc-800">
                                {participant.userName}
                              </p>
                              <p className="text-xs text-zinc-500">
                                Approved | Joined {formatDateInput(participant.joinedAt)}
                              </p>
                            </div>
                            {isCreator ? (
                              <button
                                type="button"
                                className="rounded-full border border-rose-200 bg-rose-50 px-3 py-1 text-xs font-semibold text-rose-700 transition hover:border-rose-300"
                                onClick={() => handleRejectParticipant(participant.userId)}
                                disabled={isSubmitting}
                              >
                                Kick
                              </button>
                            ) : null}
                          </div>
                        ))}
                      </div>
                    )}
                  </div>
                  {pendingParticipants.length > 0 ? (
                    <div className="rounded-2xl border border-amber-200 bg-amber-50/60 p-6 shadow-sm">
                      <div className="flex flex-wrap items-center justify-between gap-3">
                        <p className="text-xs font-semibold uppercase tracking-[0.2em] text-amber-700">
                          Pending
                        </p>
                        <span className="rounded-full bg-white px-3 py-1 text-xs font-semibold text-amber-700">
                          {pendingParticipants.length} total
                        </span>
                      </div>
                      <div className="mt-4 space-y-3">
                        {pendingParticipants.map((participant) => (
                          <div
                            key={participant.id}
                            className="flex flex-wrap items-center justify-between gap-3 rounded-lg border border-amber-200 bg-white px-4 py-3"
                          >
                            <div>
                              <p className="text-sm font-semibold text-zinc-800">
                                {participant.userName}
                              </p>
                              <p className="text-xs text-zinc-500">
                                Pending | Joined {formatDateInput(participant.joinedAt)}
                              </p>
                            </div>
                            {isCreator ? (
                              <div className="flex flex-wrap gap-2">
                                <button
                                  type="button"
                                  className="rounded-full bg-emerald-600 px-3 py-1 text-xs font-semibold text-white transition hover:bg-emerald-700"
                                  onClick={() => handleApproveParticipant(participant.userId)}
                                  disabled={isSubmitting}
                                >
                                  Approve
                                </button>
                                <button
                                  type="button"
                                  className="rounded-full border border-rose-200 bg-rose-50 px-3 py-1 text-xs font-semibold text-rose-700 transition hover:border-rose-300"
                                  onClick={() => handleRejectParticipant(participant.userId)}
                                  disabled={isSubmitting}
                                >
                                  Reject
                                </button>
                              </div>
                            ) : null}
                          </div>
                        ))}
                      </div>
                    </div>
                  ) : null}
                  {removedParticipants.length > 0 ? (
                    <div className="rounded-2xl border border-rose-200 bg-rose-50/60 p-6 shadow-sm">
                      <div className="flex flex-wrap items-center justify-between gap-3">
                        <p className="text-xs font-semibold uppercase tracking-[0.2em] text-rose-700">
                          Removed
                        </p>
                        <span className="rounded-full bg-white px-3 py-1 text-xs font-semibold text-rose-700">
                          {removedParticipants.length} total
                        </span>
                      </div>
                      <div className="mt-4 space-y-3">
                        {removedParticipants.map((participant) => (
                          <div
                            key={participant.id}
                            className="flex flex-wrap items-center justify-between gap-3 rounded-lg border border-rose-200 bg-white px-4 py-3"
                          >
                            <div>
                              <p className="text-sm font-semibold text-zinc-800">
                                {participant.userName}
                              </p>
                              <p className="text-xs text-zinc-500">
                                {participant.status} | Joined {formatDateInput(participant.joinedAt)}
                              </p>
                            </div>
                          </div>
                        ))}
                      </div>
                    </div>
                  ) : null}

                  <div className="rounded-2xl border border-zinc-200 bg-white/90 p-6 shadow-sm">
                    <p className="text-xs font-semibold uppercase tracking-[0.2em] text-emerald-700">
                      Location suggestions
                    </p>
                    {meetup.locationSuggestions.length === 0 ? (
                      <p className="mt-3 text-sm text-zinc-600">
                        No location suggestions yet.
                      </p>
                    ) : (
                      <div className="mt-4 space-y-3">
                        {meetup.locationSuggestions.map((suggestion) => (
                          <div
                            key={suggestion.id}
                            className="rounded-lg border border-zinc-200 bg-white px-4 py-3"
                          >
                            <div className="flex flex-wrap items-center justify-between gap-2">
                              <p className="text-sm font-semibold text-zinc-800">
                                {suggestion.name}
                              </p>
                              {suggestion.isChosen ? (
                                <span className="rounded-full bg-emerald-50 px-2 py-1 text-xs font-semibold text-emerald-700">
                                  Chosen
                                </span>
                              ) : null}
                            </div>
                            <p className="mt-1 text-xs text-zinc-500">
                              {suggestion.type} | Suggested by {suggestion.suggestedByUserName}
                            </p>
                            {suggestion.address ? (
                              <p className="mt-1 text-xs text-zinc-500">
                                {suggestion.address}
                              </p>
                            ) : null}
                          </div>
                        ))}
                      </div>
                    )}
                  </div>
                </div>

                <aside className="space-y-6">
                  {isCreator ? (
                    <div className="rounded-2xl border border-emerald-200 bg-emerald-50/70 p-6 shadow-sm">
                      <p className="text-xs font-semibold uppercase tracking-[0.2em] text-emerald-700">
                        Host actions
                      </p>
                      <div className="mt-4 space-y-4">
                        <label className="block">
                          <span className="text-sm font-semibold text-emerald-900">Update status</span>
                          <select
                            className="mt-2 h-11 w-full rounded-md border border-emerald-200 bg-white px-3 text-sm outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
                            value={meetup.status}
                            onChange={(event) => handleStatusChange(event.target.value)}
                          >
                            {statusOptions.map((status) => (
                              <option key={status} value={status}>
                                {status}
                              </option>
                            ))}
                          </select>
                        </label>
                        <button
                          type="button"
                          className="w-full rounded-full border border-rose-200 bg-rose-50 px-4 py-2 text-xs font-semibold text-rose-700 transition hover:border-rose-300"
                          onClick={handleDelete}
                          disabled={isSubmitting}
                        >
                          Delete meetup
                        </button>
                      </div>
                    </div>
                  ) : null}

                  <div className="rounded-2xl border border-zinc-200 bg-white/90 p-6 shadow-sm">
                    <p className="text-xs font-semibold uppercase tracking-[0.2em] text-amber-600">
                      Your status
                    </p>
                    <div className="mt-3 text-sm text-zinc-600">
                      {userParticipant
                        ? `You are ${userParticipant.status.toLowerCase()} for this meetup.`
                        : "You have not joined this meetup yet."}
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
