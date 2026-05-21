"use client";

import Link from "next/link";
import { useEffect, useMemo, useState } from "react";
import ProtectedRoute from "@/components/ProtectedRoute";
import Toast, { type ToastState } from "@/components/Toast";
import { nzLocations } from "../lib/nz-locations";
import {
  applyMeetup,
  approveParticipant,
  createMeetup,
  deleteMeetup,
  getCreatedMeetups,
  getJoinedMeetups,
  getMatchedMeetups,
  updateMeetup,
  updateMeetupStatus,
  quitMeetup,
  type ActivityInputPayload,
  type MeetupEventDto,
  type MeetupMatchDto,
} from "../lib/meetup-api";
import { getProfile, type UserProfile } from "../lib/profile-api";

const activityTypeOptions = [
  "Cafe",
  "Park",
  "Restaurant",
  "Gym",
  "Bar",
  "Custom",
] as const;

const statusOptions = [
  "Open",
  "Confirming",
  "Confirmed",
  "Completed",
  "Cancelled",
] as const;

type MeetupFormActivity = {
  id: string;
  name: string;
  description: string;
  type: string;
};

type MeetupFormState = {
  title: string;
  description: string;
  region: string;
  suburb: string;
  locationName: string;
  eventDate: string;
  startTime: string;
  endTime: string;
  maxParticipants: string;
  activities: MeetupFormActivity[];
};

type MatchFiltersState = {
  activityType: string;
  region: string;
  suburb: string;
  limit: string;
};

type SimpleFormEvent = { preventDefault: () => void };

type MeetupFieldChange = <K extends keyof MeetupFormState>(
  key: K,
  value: MeetupFormState[K],
) => void;

function formatDateInput(value?: string | null) {
  if (!value) return "";
  return value.split("T")[0];
}

function formatTimeInput(value?: string | null) {
  if (!value) return "";
  return value.slice(0, 5);
}

function normalizeTimeValue(value: string) {
  if (!value) return value;
  return value.length === 5 ? `${value}:00` : value;
}

function createActivityId() {
  if (globalThis.crypto !== undefined && "randomUUID" in globalThis.crypto) {
    return globalThis.crypto.randomUUID();
  }

  return `activity-${Date.now()}-${Math.random().toString(16).slice(2)}`;
}

function createActivity(): MeetupFormActivity {
  return {
    id: createActivityId(),
    name: "",
    description: "",
    type: "Custom",
  };
}

function createEmptyMeetupForm(region: string, suburb: string): MeetupFormState {
  return {
    title: "",
    description: "",
    region,
    suburb,
    locationName: "",
    eventDate: "",
    startTime: "",
    endTime: "",
    maxParticipants: "10",
    activities: [createActivity()],
  };
}

function buildActivities(activities: MeetupFormActivity[]): ActivityInputPayload[] {
  return activities
    .map((activity, index) => ({
      name: activity.name.trim(),
      description: activity.description.trim() || null,
      type: activity.type,
      order: index + 1,
    }))
    .filter((activity) => activity.name.length > 0);
}

function getDefaultRegion() {
  return nzLocations[0]?.region ?? "";
}

function getDefaultSuburb(region: string) {
  return (
    nzLocations.find((location) => location.region === region)?.cities[0] ?? ""
  );
}

type MeetupCreatePanelProps = {
  profile: UserProfile | null;
  createForm: MeetupFormState;
  suburbsForRegion: string[];
  isSubmitting: boolean;
  onSubmit: (event: SimpleFormEvent) => void;
  onChangeField: MeetupFieldChange;
  onChangeActivity: (index: number, patch: Partial<MeetupFormActivity>) => void;
  onAddActivity: () => void;
  onRemoveActivity: (index: number) => void;
};

function MeetupCreatePanel({
  profile,
  createForm,
  suburbsForRegion,
  isSubmitting,
  onSubmit,
  onChangeField,
  onChangeActivity,
  onAddActivity,
  onRemoveActivity,
}: Readonly<MeetupCreatePanelProps>) {
  return (
    <section className="mt-10 grid gap-8 lg:grid-cols-[1.1fr_0.9fr]">
      <form
        className="rounded-2xl border border-zinc-200 bg-white/90 p-6 shadow-sm"
        onSubmit={onSubmit}
      >
        <div className="flex flex-wrap items-center justify-between gap-3">
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.2em] text-emerald-700">
              Create Meetup
            </p>
            <h2 className="mt-2 text-xl font-bold text-zinc-950">
              Plan a new experience.
            </h2>
          </div>
          <span className="rounded-full bg-emerald-50 px-3 py-1 text-xs font-semibold text-emerald-700">
            {createForm.activities.length} / 3 activities
          </span>
        </div>

        <div className="mt-6 grid gap-5 sm:grid-cols-2">
          <label className="block sm:col-span-2">
            <span className="text-sm font-semibold text-zinc-800">Title</span>
            <input
              className="mt-2 h-12 w-full rounded-md border border-zinc-300 px-3 text-base outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
              type="text"
              value={createForm.title}
              onChange={(event) => onChangeField("title", event.target.value)}
              maxLength={100}
              required
            />
          </label>

          <label className="block sm:col-span-2">
            <span className="text-sm font-semibold text-zinc-800">Description</span>
            <textarea
              className="mt-2 min-h-[110px] w-full rounded-md border border-zinc-300 px-3 py-2 text-base outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
              value={createForm.description}
              onChange={(event) =>
                onChangeField("description", event.target.value)
              }
              maxLength={500}
            />
          </label>

          <label className="block">
            <span className="text-sm font-semibold text-zinc-800">Region</span>
            <select
              className="mt-2 h-12 w-full rounded-md border border-zinc-300 bg-white px-3 text-base outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
              value={createForm.region}
              onChange={(event) => {
                const nextRegion = event.target.value;
                onChangeField("region", nextRegion);
                onChangeField("suburb", getDefaultSuburb(nextRegion));
              }}
              required
            >
              <option value="">Select region</option>
              {nzLocations.map((location) => (
                <option key={location.region} value={location.region}>
                  {location.region}
                </option>
              ))}
            </select>
          </label>

          <label className="block">
            <span className="text-sm font-semibold text-zinc-800">Suburb</span>
            <select
              className="mt-2 h-12 w-full rounded-md border border-zinc-300 bg-white px-3 text-base outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
              value={createForm.suburb}
              onChange={(event) => onChangeField("suburb", event.target.value)}
              required
            >
              <option value="">Select suburb</option>
              {suburbsForRegion.map((suburb) => (
                <option key={suburb} value={suburb}>
                  {suburb}
                </option>
              ))}
            </select>
          </label>

          <label className="block">
            <span className="text-sm font-semibold text-zinc-800">Location name</span>
            <input
              className="mt-2 h-12 w-full rounded-md border border-zinc-300 px-3 text-base outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
              type="text"
              value={createForm.locationName}
              onChange={(event) =>
                onChangeField("locationName", event.target.value)
              }
              maxLength={100}
            />
          </label>

          <label className="block">
            <span className="text-sm font-semibold text-zinc-800">Event date</span>
            <input
              className="mt-2 h-12 w-full rounded-md border border-zinc-300 px-3 text-base outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
              type="date"
              value={createForm.eventDate}
              onChange={(event) => onChangeField("eventDate", event.target.value)}
              required
            />
          </label>

          <label className="block">
            <span className="text-sm font-semibold text-zinc-800">Start time</span>
            <input
              className="mt-2 h-12 w-full rounded-md border border-zinc-300 px-3 text-base outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
              type="time"
              value={createForm.startTime}
              onChange={(event) => onChangeField("startTime", event.target.value)}
              required
            />
          </label>

          <label className="block">
            <span className="text-sm font-semibold text-zinc-800">End time</span>
            <input
              className="mt-2 h-12 w-full rounded-md border border-zinc-300 px-3 text-base outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
              type="time"
              value={createForm.endTime}
              onChange={(event) => onChangeField("endTime", event.target.value)}
            />
          </label>

          <label className="block">
            <span className="text-sm font-semibold text-zinc-800">Max participants</span>
            <input
              className="mt-2 h-12 w-full rounded-md border border-zinc-300 px-3 text-base outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
              type="number"
              min={1}
              max={1000}
              value={createForm.maxParticipants}
              onChange={(event) =>
                onChangeField("maxParticipants", event.target.value)
              }
            />
          </label>
        </div>

        <div className="mt-8 space-y-4">
          <div className="flex items-center justify-between">
            <p className="text-sm font-semibold text-zinc-800">Activities</p>
            <button
              type="button"
              className="text-xs font-semibold text-emerald-700 transition hover:text-emerald-900"
              onClick={onAddActivity}
              disabled={createForm.activities.length >= 3}
            >
              Add activity
            </button>
          </div>

          {createForm.activities.map((activity, index) => (
            <div
              key={activity.id}
              className="rounded-xl border border-zinc-200 bg-zinc-50/60 p-4"
            >
              <div className="flex items-center justify-between gap-3">
                <p className="text-sm font-semibold text-zinc-800">Activity {index + 1}</p>
                <button
                  type="button"
                  className="text-xs font-semibold text-rose-600 transition hover:text-rose-700"
                  onClick={() => onRemoveActivity(index)}
                  disabled={createForm.activities.length <= 1}
                >
                  Remove
                </button>
              </div>
              <div className="mt-3 grid gap-4 sm:grid-cols-3">
                <label className="block sm:col-span-2">
                  <span className="text-xs font-semibold text-zinc-600">Name</span>
                  <input
                    className="mt-2 h-11 w-full rounded-md border border-zinc-300 bg-white px-3 text-sm outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
                    type="text"
                    value={activity.name}
                    onChange={(event) =>
                      onChangeActivity(index, { name: event.target.value })
                    }
                    maxLength={50}
                    required
                  />
                </label>

                <label className="block">
                  <span className="text-xs font-semibold text-zinc-600">Type</span>
                  <select
                    className="mt-2 h-11 w-full rounded-md border border-zinc-300 bg-white px-3 text-sm outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
                    value={activity.type}
                    onChange={(event) =>
                      onChangeActivity(index, { type: event.target.value })
                    }
                  >
                    {activityTypeOptions.map((option) => (
                      <option key={option} value={option}>
                        {option}
                      </option>
                    ))}
                  </select>
                </label>

                <label className="block sm:col-span-3">
                  <span className="text-xs font-semibold text-zinc-600">Description</span>
                  <input
                    className="mt-2 h-11 w-full rounded-md border border-zinc-300 bg-white px-3 text-sm outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
                    type="text"
                    value={activity.description}
                    onChange={(event) =>
                      onChangeActivity(index, {
                        description: event.target.value,
                      })
                    }
                    maxLength={200}
                  />
                </label>
              </div>
            </div>
          ))}
        </div>

        <button
          type="submit"
          className="mt-8 inline-flex items-center justify-center rounded-full bg-emerald-600 px-6 py-3 text-sm font-semibold text-white shadow-lg shadow-emerald-200 transition hover:bg-emerald-700 disabled:cursor-not-allowed disabled:opacity-70"
          disabled={isSubmitting}
        >
          {isSubmitting ? "Saving..." : "Create meetup"}
        </button>
      </form>

      <aside className="rounded-2xl border border-zinc-200 bg-white/90 p-6 shadow-sm">
        <p className="text-xs font-semibold uppercase tracking-[0.2em] text-amber-600">
          Quick checklist
        </p>
        <h3 className="mt-2 text-lg font-bold text-zinc-900">
          Keep your meetup ready to match.
        </h3>
        <ul className="mt-4 space-y-3 text-sm text-zinc-600">
          <li>Choose up to three activities to score higher matches.</li>
          <li>Pick a suburb that matches your profile location.</li>
          <li>Confirm status updates when you lock the time.</li>
        </ul>
        <div className="mt-6 rounded-xl border border-emerald-200 bg-emerald-50 p-4 text-sm text-emerald-900">
          {profile?.isVerified
            ? "You are verified and ready to host meetups."
            : "Verify your avatar in profile to unlock hosting."}
        </div>
      </aside>
    </section>
  );
}

type MeetupManagePanelProps = {
  createdMeetups: MeetupEventDto[];
  editingMeetupId: number | null;
  editForm: MeetupFormState | null;
  editSuburbsForRegion: string[];
  isSubmitting: boolean;
  onStartEditing: (meetup: MeetupEventDto) => void;
  onDeleteMeetup: (meetupId: number) => void;
  onStatusUpdate: (meetupId: number, status: string) => void;
  onApproveParticipant: (meetupId: number, participantId: number) => void;
  onUpdateMeetup: (meetupId: number) => void;
  onCancelEdit: () => void;
  onEditFieldChange: MeetupFieldChange;
  onEditActivityChange: (index: number, patch: Partial<MeetupFormActivity>) => void;
  onAddEditActivity: () => void;
  onRemoveEditActivity: (index: number) => void;
};

function MeetupManagePanel({
  createdMeetups,
  editingMeetupId,
  editForm,
  editSuburbsForRegion,
  isSubmitting,
  onStartEditing,
  onDeleteMeetup,
  onStatusUpdate,
  onApproveParticipant,
  onUpdateMeetup,
  onCancelEdit,
  onEditFieldChange,
  onEditActivityChange,
  onAddEditActivity,
  onRemoveEditActivity,
}: Readonly<MeetupManagePanelProps>) {
  return (
    <section className="mt-10 space-y-6">
      <div className="rounded-2xl border border-zinc-200 bg-white/90 p-6 shadow-sm">
        <p className="text-xs font-semibold uppercase tracking-[0.2em] text-emerald-700">
          Manage Meetups
        </p>
        <h2 className="mt-2 text-xl font-bold text-zinc-950">
          Control your hosted events.
        </h2>
        <p className="mt-3 text-sm text-zinc-600">
          Update details, approve participants, and close out events.
        </p>
      </div>

      {createdMeetups.length === 0 ? (
        <div className="rounded-2xl border border-dashed border-zinc-300 bg-white/70 p-8 text-center text-sm text-zinc-600">
          You have not created any meetups yet.
        </div>
      ) : (
        <div className="space-y-6">
          {createdMeetups.map((meetup) => {
            const pendingParticipants = meetup.participants.filter(
              (participant) => participant.status === "Pending",
            );

            return (
              <div
                key={meetup.id}
                className="rounded-2xl border border-zinc-200 bg-white/90 p-6 shadow-sm"
              >
                <div className="flex flex-wrap items-start justify-between gap-4">
                  <div>
                    <p className="text-xs font-semibold uppercase tracking-[0.2em] text-amber-600">
                      {meetup.status}
                    </p>
                    <h3 className="mt-2 text-xl font-bold text-zinc-950">
                      {meetup.title}
                    </h3>
                    <p className="mt-1 text-sm text-zinc-600">
                      {meetup.region} | {meetup.suburb}
                    </p>
                    {meetup.locationName ? (
                      <p className="mt-1 text-sm text-zinc-600">
                        Location: {meetup.locationName}
                      </p>
                    ) : null}
                    <p className="mt-1 text-sm text-zinc-600">
                      {formatDateInput(meetup.eventDate)} | {formatTimeInput(
                        meetup.startTime,
                      )}
                      {meetup.endTime
                        ? ` - ${formatTimeInput(meetup.endTime)}`
                        : ""}
                    </p>
                    {meetup.description ? (
                      <p className="mt-2 text-sm text-zinc-600">
                        {meetup.description}
                      </p>
                    ) : null}
                  </div>

                  <div className="flex flex-wrap items-center gap-3">
                    <span className="rounded-full bg-zinc-100 px-3 py-1 text-xs font-semibold text-zinc-700">
                      {meetup.currentParticipants}/{meetup.maxParticipants} joined
                    </span>
                    <Link
                      className="rounded-full border border-emerald-200 bg-emerald-50 px-4 py-2 text-xs font-semibold text-emerald-700 transition hover:border-emerald-300"
                      href={`/meetups/${meetup.id}`}
                    >
                      View details
                    </Link>
                    <button
                      type="button"
                      className="rounded-full border border-zinc-300 bg-white px-4 py-2 text-xs font-semibold text-zinc-700 transition hover:border-zinc-500"
                      onClick={() => onStartEditing(meetup)}
                    >
                      Edit
                    </button>
                    <button
                      type="button"
                      className="rounded-full border border-rose-200 bg-rose-50 px-4 py-2 text-xs font-semibold text-rose-700 transition hover:border-rose-300"
                      onClick={() => onDeleteMeetup(meetup.id)}
                      disabled={isSubmitting}
                    >
                      Delete
                    </button>
                  </div>
                </div>

                <div className="mt-4 flex flex-wrap gap-2">
                  {meetup.activities.map((activity) => (
                    <span
                      key={`${meetup.id}-${activity.name}`}
                      className="rounded-full border border-emerald-200 bg-emerald-50 px-3 py-1 text-xs font-semibold text-emerald-700"
                    >
                      {activity.name} | {activity.type}
                    </span>
                  ))}
                </div>

                <div className="mt-5 flex flex-wrap items-center gap-3">
                  <select
                    className="h-10 rounded-md border border-zinc-300 bg-white px-3 text-sm outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
                    value={meetup.status}
                    onChange={(event) =>
                      onStatusUpdate(meetup.id, event.target.value)
                    }
                  >
                    {statusOptions.map((status) => (
                      <option key={status} value={status}>
                        {status}
                      </option>
                    ))}
                  </select>
                  <span className="text-xs text-zinc-500">
                    Updating status will notify matched users.
                  </span>
                </div>

                {pendingParticipants.length > 0 ? (
                  <div className="mt-6 rounded-xl border border-zinc-200 bg-zinc-50/70 p-4">
                    <p className="text-sm font-semibold text-zinc-800">
                      Pending participants ({pendingParticipants.length})
                    </p>
                    <div className="mt-3 space-y-2">
                      {pendingParticipants.map((participant) => (
                        <div
                          key={participant.id}
                          className="flex flex-wrap items-center justify-between gap-3 rounded-lg border border-zinc-200 bg-white px-3 py-2"
                        >
                          <div>
                            <p className="text-sm font-semibold text-zinc-800">
                              {participant.userName}
                            </p>
                            <p className="text-xs text-zinc-500">
                              Joined {formatDateInput(participant.joinedAt)}
                            </p>
                          </div>
                          <button
                            type="button"
                            className="rounded-full bg-emerald-600 px-3 py-1 text-xs font-semibold text-white transition hover:bg-emerald-700"
                            onClick={() =>
                              onApproveParticipant(meetup.id, participant.userId)
                            }
                            disabled={isSubmitting}
                          >
                            Approve
                          </button>
                        </div>
                      ))}
                    </div>
                  </div>
                ) : (
                  <p className="mt-6 text-sm text-zinc-500">
                    No pending participants right now.
                  </p>
                )}

                {editingMeetupId === meetup.id && editForm ? (
                  <div className="mt-6 rounded-2xl border border-emerald-200 bg-emerald-50/60 p-5">
                    <h4 className="text-sm font-semibold text-emerald-900">
                      Edit meetup details
                    </h4>
                    <div className="mt-4 grid gap-4 sm:grid-cols-2">
                      <label className="block sm:col-span-2">
                        <span className="text-xs font-semibold text-emerald-900">Title</span>
                        <input
                          className="mt-2 h-11 w-full rounded-md border border-emerald-200 bg-white px-3 text-sm outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
                          value={editForm.title}
                          onChange={(event) =>
                            onEditFieldChange("title", event.target.value)
                          }
                          maxLength={100}
                        />
                      </label>
                      <label className="block sm:col-span-2">
                        <span className="text-xs font-semibold text-emerald-900">
                          Description
                        </span>
                        <textarea
                          className="mt-2 min-h-[90px] w-full rounded-md border border-emerald-200 bg-white px-3 py-2 text-sm outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
                          value={editForm.description}
                          onChange={(event) =>
                            onEditFieldChange("description", event.target.value)
                          }
                          maxLength={500}
                        />
                      </label>
                      <label className="block">
                        <span className="text-xs font-semibold text-emerald-900">Region</span>
                        <select
                          className="mt-2 h-11 w-full rounded-md border border-emerald-200 bg-white px-3 text-sm outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
                          value={editForm.region}
                          onChange={(event) => {
                            const nextRegion = event.target.value;
                            onEditFieldChange("region", nextRegion);
                            onEditFieldChange("suburb", getDefaultSuburb(nextRegion));
                          }}
                        >
                          {nzLocations.map((location) => (
                            <option key={location.region} value={location.region}>
                              {location.region}
                            </option>
                          ))}
                        </select>
                      </label>
                      <label className="block">
                        <span className="text-xs font-semibold text-emerald-900">Suburb</span>
                        <select
                          className="mt-2 h-11 w-full rounded-md border border-emerald-200 bg-white px-3 text-sm outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
                          value={editForm.suburb}
                          onChange={(event) =>
                            onEditFieldChange("suburb", event.target.value)
                          }
                        >
                          {editSuburbsForRegion.map((suburb) => (
                            <option key={suburb} value={suburb}>
                              {suburb}
                            </option>
                          ))}
                        </select>
                      </label>
                      <label className="block">
                        <span className="text-xs font-semibold text-emerald-900">
                          Location name
                        </span>
                        <input
                          className="mt-2 h-11 w-full rounded-md border border-emerald-200 bg-white px-3 text-sm outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
                          value={editForm.locationName}
                          onChange={(event) =>
                            onEditFieldChange("locationName", event.target.value)
                          }
                        />
                      </label>
                      <label className="block">
                        <span className="text-xs font-semibold text-emerald-900">Event date</span>
                        <input
                          className="mt-2 h-11 w-full rounded-md border border-emerald-200 bg-white px-3 text-sm outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
                          type="date"
                          value={editForm.eventDate}
                          onChange={(event) =>
                            onEditFieldChange("eventDate", event.target.value)
                          }
                        />
                      </label>
                      <label className="block">
                        <span className="text-xs font-semibold text-emerald-900">Start time</span>
                        <input
                          className="mt-2 h-11 w-full rounded-md border border-emerald-200 bg-white px-3 text-sm outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
                          type="time"
                          value={editForm.startTime}
                          onChange={(event) =>
                            onEditFieldChange("startTime", event.target.value)
                          }
                        />
                      </label>
                      <label className="block">
                        <span className="text-xs font-semibold text-emerald-900">End time</span>
                        <input
                          className="mt-2 h-11 w-full rounded-md border border-emerald-200 bg-white px-3 text-sm outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
                          type="time"
                          value={editForm.endTime}
                          onChange={(event) =>
                            onEditFieldChange("endTime", event.target.value)
                          }
                        />
                      </label>
                      <label className="block">
                        <span className="text-xs font-semibold text-emerald-900">Max participants</span>
                        <input
                          className="mt-2 h-11 w-full rounded-md border border-emerald-200 bg-white px-3 text-sm outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
                          type="number"
                          min={1}
                          max={1000}
                          value={editForm.maxParticipants}
                          onChange={(event) =>
                            onEditFieldChange("maxParticipants", event.target.value)
                          }
                        />
                      </label>
                    </div>

                    <div className="mt-5 space-y-3">
                      <div className="flex items-center justify-between">
                        <p className="text-sm font-semibold text-emerald-900">Activities</p>
                        <button
                          type="button"
                          className="text-xs font-semibold text-emerald-700 transition hover:text-emerald-900"
                          onClick={onAddEditActivity}
                          disabled={editForm.activities.length >= 3}
                        >
                          Add activity
                        </button>
                      </div>

                      {editForm.activities.map((activity, index) => (
                        <div
                          key={activity.id}
                          className="rounded-lg border border-emerald-200 bg-white p-4"
                        >
                          <div className="flex items-center justify-between gap-3">
                            <p className="text-xs font-semibold text-emerald-900">
                              Activity {index + 1}
                            </p>
                            <button
                              type="button"
                              className="text-xs font-semibold text-rose-600 transition hover:text-rose-700"
                              onClick={() => onRemoveEditActivity(index)}
                              disabled={editForm.activities.length <= 1}
                            >
                              Remove
                            </button>
                          </div>
                          <div className="mt-3 grid gap-3 sm:grid-cols-3">
                            <label className="block sm:col-span-2">
                              <span className="text-xs font-semibold text-emerald-900">Name</span>
                              <input
                                className="mt-2 h-10 w-full rounded-md border border-emerald-200 px-3 text-sm outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
                                value={activity.name}
                                onChange={(event) =>
                                  onEditActivityChange(index, {
                                    name: event.target.value,
                                  })
                                }
                              />
                            </label>
                            <label className="block">
                              <span className="text-xs font-semibold text-emerald-900">Type</span>
                              <select
                                className="mt-2 h-10 w-full rounded-md border border-emerald-200 px-3 text-sm outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
                                value={activity.type}
                                onChange={(event) =>
                                  onEditActivityChange(index, {
                                    type: event.target.value,
                                  })
                                }
                              >
                                {activityTypeOptions.map((option) => (
                                  <option key={option} value={option}>
                                    {option}
                                  </option>
                                ))}
                              </select>
                            </label>
                            <label className="block sm:col-span-3">
                              <span className="text-xs font-semibold text-emerald-900">
                                Description
                              </span>
                              <input
                                className="mt-2 h-10 w-full rounded-md border border-emerald-200 px-3 text-sm outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
                                value={activity.description}
                                onChange={(event) =>
                                  onEditActivityChange(index, {
                                    description: event.target.value,
                                  })
                                }
                              />
                            </label>
                          </div>
                        </div>
                      ))}
                    </div>

                    <div className="mt-5 flex flex-wrap gap-3">
                      <button
                        type="button"
                        className="rounded-full bg-emerald-600 px-4 py-2 text-xs font-semibold text-white transition hover:bg-emerald-700"
                        onClick={() => onUpdateMeetup(meetup.id)}
                        disabled={isSubmitting}
                      >
                        Save changes
                      </button>
                      <button
                        type="button"
                        className="rounded-full border border-emerald-200 bg-white px-4 py-2 text-xs font-semibold text-emerald-700"
                        onClick={onCancelEdit}
                      >
                        Cancel
                      </button>
                    </div>
                  </div>
                ) : null}
              </div>
            );
          })}
        </div>
      )}
    </section>
  );
}

type MeetupMatchPanelProps = {
  matchFilters: MatchFiltersState;
  matchSuburbs: string[];
  matches: MeetupMatchDto[];
  joinedMeetups: MeetupEventDto[];
  joinedMeetupIds: number[];
  isSubmitting: boolean;
  onSubmit: (event: SimpleFormEvent) => void;
  onChangeFilters: (patch: Partial<MatchFiltersState>) => void;
  onJoin: (meetupId: number) => void;
  onLeave: (meetupId: number) => void;
};

function MeetupMatchPanel({
  matchFilters,
  matchSuburbs,
  matches,
  joinedMeetups,
  joinedMeetupIds,
  isSubmitting,
  onSubmit,
  onChangeFilters,
  onJoin,
  onLeave,
}: Readonly<MeetupMatchPanelProps>) {
  return (
    <section className="mt-10 space-y-6">
      <form
        className="rounded-2xl border border-zinc-200 bg-white/90 p-6 shadow-sm"
        onSubmit={onSubmit}
      >
        <p className="text-xs font-semibold uppercase tracking-[0.2em] text-emerald-700">
          Match Meetups
        </p>
        <h2 className="mt-2 text-xl font-bold text-zinc-950">
          Find meetups tailored to your vibe.
        </h2>

        <div className="mt-5 grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          <label className="block">
            <span className="text-sm font-semibold text-zinc-800">Activity type</span>
            <select
              className="mt-2 h-11 w-full rounded-md border border-zinc-300 bg-white px-3 text-sm outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
              value={matchFilters.activityType}
              onChange={(event) =>
                onChangeFilters({ activityType: event.target.value })
              }
            >
              {activityTypeOptions.map((option) => (
                <option key={option} value={option}>
                  {option}
                </option>
              ))}
            </select>
          </label>

          <label className="block">
            <span className="text-sm font-semibold text-zinc-800">Region</span>
            <select
              className="mt-2 h-11 w-full rounded-md border border-zinc-300 bg-white px-3 text-sm outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
              value={matchFilters.region}
              onChange={(event) => {
                const nextRegion = event.target.value;
                onChangeFilters({
                  region: nextRegion,
                  suburb: getDefaultSuburb(nextRegion),
                });
              }}
            >
              <option value="">Select region</option>
              {nzLocations.map((location) => (
                <option key={location.region} value={location.region}>
                  {location.region}
                </option>
              ))}
            </select>
          </label>

          <label className="block">
            <span className="text-sm font-semibold text-zinc-800">Suburb</span>
            <select
              className="mt-2 h-11 w-full rounded-md border border-zinc-300 bg-white px-3 text-sm outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
              value={matchFilters.suburb}
              onChange={(event) => onChangeFilters({ suburb: event.target.value })}
            >
              <option value="">Select suburb</option>
              {matchSuburbs.map((suburb) => (
                <option key={suburb} value={suburb}>
                  {suburb}
                </option>
              ))}
            </select>
          </label>

          <label className="block">
            <span className="text-sm font-semibold text-zinc-800">Limit</span>
            <input
              className="mt-2 h-11 w-full rounded-md border border-zinc-300 bg-white px-3 text-sm outline-none transition focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100"
              type="number"
              min={1}
              max={100}
              value={matchFilters.limit}
              onChange={(event) => onChangeFilters({ limit: event.target.value })}
            />
          </label>
        </div>

        <button
          type="submit"
          className="mt-5 inline-flex items-center justify-center rounded-full bg-emerald-600 px-6 py-3 text-sm font-semibold text-white shadow-lg shadow-emerald-200 transition hover:bg-emerald-700 disabled:cursor-not-allowed disabled:opacity-70"
          disabled={isSubmitting}
        >
          {isSubmitting ? "Searching..." : "Find matches"}
        </button>
      </form>

      <div className="rounded-2xl border border-zinc-200 bg-white/90 p-6 shadow-sm">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.2em] text-emerald-700">
              Joined Meetups
            </p>
            <h3 className="mt-2 text-lg font-bold text-zinc-950">
              Your active join requests and confirmations.
            </h3>
          </div>
          <span className="rounded-full bg-zinc-100 px-3 py-1 text-xs font-semibold text-zinc-700">
            {joinedMeetups.length} total
          </span>
        </div>

        {joinedMeetups.length === 0 ? (
          <p className="mt-4 text-sm text-zinc-600">
            You have not joined any meetups yet.
          </p>
        ) : (
          <div className="mt-5 grid gap-4 md:grid-cols-2">
            {joinedMeetups.map((meetup) => (
              <div
                key={meetup.id}
                className="rounded-xl border border-zinc-200 bg-white p-4"
              >
                <div className="flex items-start justify-between gap-3">
                  <div>
                    <p className="text-xs font-semibold uppercase tracking-[0.2em] text-amber-600">
                      {meetup.status}
                    </p>
                    <h4 className="mt-2 text-base font-semibold text-zinc-950">
                      {meetup.title}
                    </h4>
                    <p className="mt-1 text-xs text-zinc-600">
                      {meetup.region} | {meetup.suburb}
                    </p>
                    {meetup.locationName ? (
                      <p className="mt-1 text-xs text-zinc-600">
                        Location: {meetup.locationName}
                      </p>
                    ) : null}
                    <p className="mt-1 text-xs text-zinc-600">
                      Host: {meetup.creatorName}
                    </p>
                  </div>
                  <span className="rounded-full bg-emerald-50 px-2 py-1 text-xs font-semibold text-emerald-700">
                    {meetup.currentParticipants}/{meetup.maxParticipants}
                  </span>
                </div>
                <p className="mt-2 text-xs text-zinc-600">
                  {formatDateInput(meetup.eventDate)} | {formatTimeInput(
                    meetup.startTime,
                  )}
                  {meetup.endTime ? ` - ${formatTimeInput(meetup.endTime)}` : ""}
                </p>
                {meetup.description ? (
                  <p className="mt-2 text-xs text-zinc-600">
                    {meetup.description}
                  </p>
                ) : null}
                <div className="mt-3 flex flex-wrap items-center gap-2">
                  {meetup.activities.map((activity) => (
                    <span
                      key={`${meetup.id}-${activity.name}`}
                      className="rounded-full border border-emerald-200 bg-emerald-50 px-2 py-1 text-[11px] font-semibold text-emerald-700"
                    >
                      {activity.name}
                    </span>
                  ))}
                </div>
                <div className="mt-4 flex flex-wrap gap-3">
                  <Link
                    className="rounded-full border border-emerald-200 bg-emerald-50 px-4 py-2 text-xs font-semibold text-emerald-700 transition hover:border-emerald-300"
                    href={`/meetups/${meetup.id}`}
                  >
                    View details
                  </Link>
                  <button
                    type="button"
                    className="rounded-full border border-rose-200 bg-rose-50 px-4 py-2 text-xs font-semibold text-rose-700 transition hover:border-rose-300"
                    onClick={() => onLeave(meetup.id)}
                    disabled={isSubmitting}
                  >
                    Leave meetup
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {matches.length === 0 ? (
        <div className="rounded-2xl border border-dashed border-zinc-300 bg-white/70 p-8 text-center text-sm text-zinc-600">
          No matches yet. Try different filters.
        </div>
      ) : (
        <div className="grid gap-5 md:grid-cols-2">
          {matches.map((match) => {
            const isJoined = joinedMeetupIds.includes(match.meetupId);
            return (
              <div
                key={match.meetupId}
                className="rounded-2xl border border-zinc-200 bg-white/90 p-6 shadow-sm"
              >
                <div className="flex items-start justify-between gap-4">
                  <div>
                    <p className="text-xs font-semibold uppercase tracking-[0.2em] text-amber-600">
                      {match.status}
                    </p>
                    <h3 className="mt-2 text-lg font-bold text-zinc-950">
                      {match.title}
                    </h3>
                    <p className="mt-1 text-sm text-zinc-600">
                      {match.region} | {match.suburb}
                    </p>
                  </div>
                  <span className="rounded-full bg-emerald-50 px-3 py-1 text-xs font-semibold text-emerald-700">
                    Score {match.matchScore}
                  </span>
                </div>
                <p className="mt-3 text-sm text-zinc-600">
                  {formatDateInput(match.eventDate)} | {formatTimeInput(
                    match.startTime,
                  )}
                </p>
                <p className="mt-1 text-sm text-zinc-600">Activity: {match.activityName}</p>
                <p className="mt-1 text-sm text-zinc-600">Host: {match.creatorName}</p>
                <p className="mt-1 text-sm text-zinc-600">
                  {match.currentParticipants}/{match.maxParticipants} joined
                </p>
                <div className="mt-4 flex flex-wrap gap-3">
                  {isJoined ? (
                    <button
                      type="button"
                      className="rounded-full border border-rose-200 bg-rose-50 px-4 py-2 text-xs font-semibold text-rose-700 transition hover:border-rose-300"
                      onClick={() => onLeave(match.meetupId)}
                      disabled={isSubmitting}
                    >
                      Leave
                    </button>
                  ) : (
                    <button
                      type="button"
                      className="rounded-full bg-emerald-600 px-4 py-2 text-xs font-semibold text-white transition hover:bg-emerald-700"
                      onClick={() => onJoin(match.meetupId)}
                      disabled={isSubmitting}
                    >
                      Join
                    </button>
                  )}
                  <Link
                    className="rounded-full border border-emerald-200 bg-emerald-50 px-4 py-2 text-xs font-semibold text-emerald-700 transition hover:border-emerald-300"
                    href={`/meetups/${match.meetupId}`}
                  >
                    View details
                  </Link>
                  {typeof match.distanceKm === "number" ? (
                    <span className="rounded-full bg-zinc-100 px-3 py-1 text-xs font-semibold text-zinc-700">
                      {match.distanceKm.toFixed(1)} km
                    </span>
                  ) : null}
                </div>
              </div>
            );
          })}
        </div>
      )}
    </section>
  );
}

export default function MeetupHubPage() {
  const [toast, setToast] = useState<ToastState | null>(null);
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [activePanel, setActivePanel] = useState<"create" | "manage" | "match">(
    "create",
  );
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [createdMeetups, setCreatedMeetups] = useState<MeetupEventDto[]>([]);
  const [joinedMeetups, setJoinedMeetups] = useState<MeetupEventDto[]>([]);
  const [editingMeetupId, setEditingMeetupId] = useState<number | null>(null);
  const [matches, setMatches] = useState<MeetupMatchDto[]>([]);
  const [joinedMeetupIds, setJoinedMeetupIds] = useState<number[]>([]);

  const defaultRegion = getDefaultRegion();
  const defaultSuburb = getDefaultSuburb(defaultRegion);

  const [createForm, setCreateForm] = useState<MeetupFormState>(() =>
    createEmptyMeetupForm(defaultRegion, defaultSuburb),
  );

  const [editForm, setEditForm] = useState<MeetupFormState | null>(null);

  const [matchFilters, setMatchFilters] = useState<MatchFiltersState>(() => ({
    activityType: activityTypeOptions[0],
    region: defaultRegion,
    suburb: defaultSuburb,
    limit: "20",
  }));

  const suburbsForRegion = useMemo(
    () =>
      nzLocations.find((location) => location.region === createForm.region)
        ?.cities ?? [],
    [createForm.region],
  );

  const editSuburbsForRegion = useMemo(() => {
    if (!editForm) return [];
    return (
      nzLocations.find((location) => location.region === editForm.region)
        ?.cities ?? []
    );
  }, [editForm]);

  const matchSuburbsForRegion = useMemo(
    () =>
      nzLocations.find((location) => location.region === matchFilters.region)
        ?.cities ?? [],
    [matchFilters.region],
  );

  function showToast(nextToast: ToastState) {
    setToast(nextToast);
    globalThis.setTimeout(() => setToast(null), 3600);
  }

  async function loadProfileAndMeetups() {
    setIsLoading(true);
    try {
      const data = await getProfile();
      setProfile(data);

      setCreateForm((prev) => ({
        ...prev,
        region: prev.region || data.region || defaultRegion,
        suburb:
          prev.suburb ||
          data.suburb ||
          getDefaultSuburb(data.region || defaultRegion),
      }));

      setMatchFilters((prev) => ({
        ...prev,
        region: prev.region || data.region || defaultRegion,
        suburb:
          prev.suburb || data.suburb || getDefaultSuburb(data.region || defaultRegion),
      }));

      const meetups = await getCreatedMeetups();
      setCreatedMeetups(meetups);

      const joined = await getJoinedMeetups();
      setJoinedMeetups(joined);
      setJoinedMeetupIds(joined.map((meetup) => meetup.id));
    } catch (error) {
      const message = error instanceof Error ? error.message : "Unable to load data.";
      showToast({ tone: "error", message });
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    loadProfileAndMeetups();
  }, []);

  function guardVerified(actionLabel: string) {
    if (!profile?.isVerified) {
      showToast({
        tone: "error",
        message: `You must verify your avatar before ${actionLabel}.`,
      });
      return false;
    }

    return true;
  }

  function guardCanMatch(actionLabel: string) {
    if (!profile?.canMatch || !profile?.isVerified) {
      showToast({
        tone: "error",
        message: `Complete your profile before ${actionLabel}.`,
      });
      return false;
    }

    return true;
  }

  function handleCreateFieldChange<K extends keyof MeetupFormState>(
    key: K,
    value: MeetupFormState[K],
  ) {
    setCreateForm((prev) => ({
      ...prev,
      [key]: value,
    }));
  }

  function handleCreateActivityChange(
    index: number,
    patch: Partial<MeetupFormActivity>,
  ) {
    setCreateForm((prev) => {
      const nextActivities = [...prev.activities];
      nextActivities[index] = { ...nextActivities[index], ...patch };
      return { ...prev, activities: nextActivities };
    });
  }

  function addCreateActivity() {
    setCreateForm((prev) => {
      if (prev.activities.length >= 3) return prev;
      return {
        ...prev,
        activities: [...prev.activities, createActivity()],
      };
    });
  }

  function removeCreateActivity(index: number) {
    setCreateForm((prev) => {
      if (prev.activities.length <= 1) return prev;
      const nextActivities = prev.activities.filter((_, i) => i !== index);
      return { ...prev, activities: nextActivities };
    });
  }

  function validateMeetupForm(form: MeetupFormState) {
    if (!form.title.trim()) {
      showToast({ tone: "error", message: "Title is required." });
      return null;
    }

    if (!form.region.trim()) {
      showToast({ tone: "error", message: "Region is required." });
      return null;
    }

    if (!form.suburb.trim()) {
      showToast({ tone: "error", message: "Suburb is required." });
      return null;
    }

    if (!form.eventDate.trim()) {
      showToast({ tone: "error", message: "Event date is required." });
      return null;
    }

    if (!form.startTime.trim()) {
      showToast({ tone: "error", message: "Start time is required." });
      return null;
    }

    const activityPayloads = buildActivities(form.activities);
    if (activityPayloads.length === 0) {
      showToast({ tone: "error", message: "Add at least one activity." });
      return null;
    }

    return activityPayloads;
  }

  async function handleCreateMeetup(event: SimpleFormEvent) {
    event.preventDefault();

    if (!guardVerified("creating a meetup")) {
      return;
    }

    const activityPayloads = validateMeetupForm(createForm);
    if (!activityPayloads) return;

    setIsSubmitting(true);
    try {
      const payload = {
        title: createForm.title.trim(),
        description: createForm.description.trim() || null,
        region: createForm.region.trim(),
        suburb: createForm.suburb.trim(),
        locationName: createForm.locationName.trim() || null,
        activities: activityPayloads,
        eventDate: createForm.eventDate,
        startTime: normalizeTimeValue(createForm.startTime),
        endTime: createForm.endTime ? normalizeTimeValue(createForm.endTime) : null,
        maxParticipants: Number(createForm.maxParticipants) || 10,
      };

      await createMeetup(payload);
      showToast({ tone: "success", message: "Meetup created." });
      setCreateForm(createEmptyMeetupForm(createForm.region, createForm.suburb));
      await loadProfileAndMeetups();
      setActivePanel("manage");
    } catch (error) {
      const message = error instanceof Error ? error.message : "Unable to create meetup.";
      showToast({ tone: "error", message });
    } finally {
      setIsSubmitting(false);
    }
  }

  function startEditing(meetup: MeetupEventDto) {
    setEditingMeetupId(meetup.id);
    setEditForm({
      title: meetup.title,
      description: meetup.description ?? "",
      region: meetup.region,
      suburb: meetup.suburb,
      locationName: meetup.locationName ?? "",
      eventDate: formatDateInput(meetup.eventDate),
      startTime: formatTimeInput(meetup.startTime),
      endTime: formatTimeInput(meetup.endTime),
      maxParticipants: String(meetup.maxParticipants),
      activities: meetup.activities.map((activity) => ({
        id: createActivityId(),
        name: activity.name,
        description: activity.description ?? "",
        type: activity.type || "Custom",
      })),
    });
  }

  const handleEditFieldChange: MeetupFieldChange = (key, value) => {
    setEditForm((prev) => (prev ? { ...prev, [key]: value } : prev));
  };

  function cancelEdit() {
    setEditingMeetupId(null);
    setEditForm(null);
  }

  function handleEditActivityChange(
    index: number,
    patch: Partial<MeetupFormActivity>,
  ) {
    setEditForm((prev) => {
      if (!prev) return prev;
      const nextActivities = [...prev.activities];
      nextActivities[index] = { ...nextActivities[index], ...patch };
      return { ...prev, activities: nextActivities };
    });
  }

  function addEditActivity() {
    setEditForm((prev) => {
      if (!prev || prev.activities.length >= 3) return prev;
      return {
        ...prev,
        activities: [...prev.activities, createActivity()],
      };
    });
  }

  function removeEditActivity(index: number) {
    setEditForm((prev) => {
      if (!prev || prev.activities.length <= 1) return prev;
      const nextActivities = prev.activities.filter((_, i) => i !== index);
      return { ...prev, activities: nextActivities };
    });
  }

  async function handleUpdateMeetup(meetupId: number) {
    if (!editForm) return;

    const activityPayloads = validateMeetupForm(editForm);
    if (!activityPayloads) return;

    setIsSubmitting(true);
    try {
      const payload = {
        title: editForm.title.trim(),
        description: editForm.description.trim() || null,
        region: editForm.region.trim(),
        suburb: editForm.suburb.trim(),
        locationName: editForm.locationName.trim() || null,
        activities: activityPayloads,
        eventDate: editForm.eventDate,
        startTime: normalizeTimeValue(editForm.startTime),
        endTime: editForm.endTime ? normalizeTimeValue(editForm.endTime) : null,
        maxParticipants: Number(editForm.maxParticipants) || 10,
      };

      await updateMeetup(meetupId, payload);
      showToast({ tone: "success", message: "Meetup updated." });
      setEditingMeetupId(null);
      setEditForm(null);
      await loadProfileAndMeetups();
    } catch (error) {
      const message = error instanceof Error ? error.message : "Unable to update meetup.";
      showToast({ tone: "error", message });
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleDeleteMeetup(meetupId: number) {
    if (!guardVerified("managing meetups")) return;

    setIsSubmitting(true);
    try {
      await deleteMeetup(meetupId);
      showToast({ tone: "success", message: "Meetup deleted." });
      await loadProfileAndMeetups();
    } catch (error) {
      const message = error instanceof Error ? error.message : "Unable to delete meetup.";
      showToast({ tone: "error", message });
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleStatusUpdate(meetupId: number, status: string) {
    if (!guardVerified("updating meetup status")) return;

    setIsSubmitting(true);
    try {
      await updateMeetupStatus(meetupId, status);
      showToast({ tone: "success", message: "Status updated." });
      await loadProfileAndMeetups();
    } catch (error) {
      const message = error instanceof Error ? error.message : "Unable to update status.";
      showToast({ tone: "error", message });
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleApproveParticipant(meetupId: number, participantId: number) {
    if (!guardVerified("approving participants")) return;

    setIsSubmitting(true);
    try {
      await approveParticipant(meetupId, participantId);
      showToast({ tone: "success", message: "Participant approved." });
      await loadProfileAndMeetups();
    } catch (error) {
      const message = error instanceof Error ? error.message : "Unable to approve participant.";
      showToast({ tone: "error", message });
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleMatchSearch(event: SimpleFormEvent) {
    event.preventDefault();

    if (!guardCanMatch("matching meetups")) return;

    if (!matchFilters.region.trim()) {
      showToast({ tone: "error", message: "Region is required for matching." });
      return;
    }

    if (!matchFilters.suburb.trim()) {
      showToast({ tone: "error", message: "Suburb is required for matching." });
      return;
    }

    setIsSubmitting(true);
    try {
      const data = await getMatchedMeetups({
        activityType: matchFilters.activityType,
        suburb: matchFilters.suburb,
        limit: Number(matchFilters.limit) || 20,
      });
      setMatches(data);
      showToast({ tone: "success", message: "Match results refreshed." });
    } catch (error) {
      const message = error instanceof Error ? error.message : "Unable to fetch matches.";
      showToast({ tone: "error", message });
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleJoinMeetup(meetupId: number) {
    if (!guardCanMatch("joining meetups")) return;

    setIsSubmitting(true);
    try {
      await applyMeetup(meetupId);
      setJoinedMeetupIds((prev) =>
        prev.includes(meetupId) ? prev : [...prev, meetupId],
      );
      await loadProfileAndMeetups();
      showToast({ tone: "success", message: "Join request sent." });
    } catch (error) {
      const message = error instanceof Error ? error.message : "Unable to join meetup.";
      showToast({ tone: "error", message });
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleLeaveMeetup(meetupId: number) {
    setIsSubmitting(true);
    try {
      await quitMeetup(meetupId);
      setJoinedMeetupIds((prev) => prev.filter((id) => id !== meetupId));
      await loadProfileAndMeetups();
      showToast({ tone: "success", message: "Meetup left." });
    } catch (error) {
      const message = error instanceof Error ? error.message : "Unable to leave meetup.";
      showToast({ tone: "error", message });
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <ProtectedRoute>
      <main className="min-h-screen bg-[#f7faf4] text-zinc-950 font-sans">
        <div className="relative overflow-hidden">
          <div className="pointer-events-none absolute inset-0">
            <div className="absolute -left-20 top-16 h-56 w-56 rounded-full bg-emerald-200/40 blur-3xl" />
            <div className="absolute right-0 top-0 h-64 w-64 rounded-full bg-amber-200/40 blur-3xl" />
          </div>

          <div className="relative mx-auto flex min-h-screen w-full max-w-6xl flex-col px-5 py-6 sm:px-8 lg:px-10">
            <header className="flex flex-wrap items-center justify-between gap-4">
              <div>
                <Link className="text-lg font-semibold tracking-wide" href="/profile">
                  RealLifeConnections
                </Link>
                <p className="mt-2 text-xs font-semibold uppercase tracking-[0.2em] text-emerald-700">
                  Meetups Hub
                </p>
                <h1 className="mt-3 text-3xl font-bold leading-tight sm:text-4xl">
                  Create, manage, and match from one command center.
                </h1>
              </div>
              <div className="flex items-center gap-3">
                <span className="rounded-full border border-zinc-300 bg-white px-3 py-1 text-xs font-semibold text-zinc-700">
                  {profile?.isVerified ? "Verified" : "Unverified"}
                </span>
                <Link
                  className="rounded-md border border-zinc-300 bg-white px-4 py-2 text-sm font-semibold text-zinc-900 shadow-sm transition hover:border-zinc-500"
                  href="/profile"
                >
                  Back to profile
                </Link>
              </div>
            </header>

            <Toast toast={toast} />

            <div className="mt-10 flex flex-wrap gap-3 rounded-full border border-zinc-200 bg-white/70 p-2 shadow-sm">
              {([
                { key: "create", label: "Create" },
                { key: "manage", label: "Manage" },
                { key: "match", label: "Match" },
              ] as const).map((tab) => (
                <button
                  key={tab.key}
                  type="button"
                  className={`rounded-full px-5 py-2 text-sm font-semibold transition ${
                    activePanel === tab.key
                      ? "bg-emerald-600 text-white shadow"
                      : "text-zinc-700 hover:bg-emerald-50"
                  }`}
                  onClick={() => setActivePanel(tab.key)}
                >
                  {tab.label}
                </button>
              ))}
            </div>

            {isLoading ? (
              <div className="mt-10 rounded-2xl border border-zinc-200 bg-white/80 p-6 shadow-sm">
                <p className="text-sm text-zinc-600">Loading meetups...</p>
              </div>
            ) : null}

            {!isLoading && activePanel === "create" ? (
              <MeetupCreatePanel
                profile={profile}
                createForm={createForm}
                suburbsForRegion={suburbsForRegion}
                isSubmitting={isSubmitting}
                onSubmit={handleCreateMeetup}
                onChangeField={handleCreateFieldChange}
                onChangeActivity={handleCreateActivityChange}
                onAddActivity={addCreateActivity}
                onRemoveActivity={removeCreateActivity}
              />
            ) : null}

            {!isLoading && activePanel === "manage" ? (
              <MeetupManagePanel
                createdMeetups={createdMeetups}
                editingMeetupId={editingMeetupId}
                editForm={editForm}
                editSuburbsForRegion={editSuburbsForRegion}
                isSubmitting={isSubmitting}
                onStartEditing={startEditing}
                onDeleteMeetup={handleDeleteMeetup}
                onStatusUpdate={handleStatusUpdate}
                onApproveParticipant={handleApproveParticipant}
                onUpdateMeetup={handleUpdateMeetup}
                onCancelEdit={cancelEdit}
                onEditFieldChange={handleEditFieldChange}
                onEditActivityChange={handleEditActivityChange}
                onAddEditActivity={addEditActivity}
                onRemoveEditActivity={removeEditActivity}
              />
            ) : null}

            {!isLoading && activePanel === "match" ? (
              <MeetupMatchPanel
                matchFilters={matchFilters}
                matchSuburbs={matchSuburbsForRegion}
                matches={matches}
                joinedMeetups={joinedMeetups}
                joinedMeetupIds={joinedMeetupIds}
                isSubmitting={isSubmitting}
                onSubmit={handleMatchSearch}
                onChangeFilters={(patch) =>
                  setMatchFilters((prev) => ({ ...prev, ...patch }))
                }
                onJoin={handleJoinMeetup}
                onLeave={handleLeaveMeetup}
              />
            ) : null}
          </div>
        </div>
      </main>
    </ProtectedRoute>
  );
}
