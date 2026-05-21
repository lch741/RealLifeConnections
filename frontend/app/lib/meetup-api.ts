import { handleApiError } from "./api-error";

export type ActivityInputPayload = {
  name: string;
  description?: string | null;
  type: string;
  order: number;
};

export type CreateMeetupPayload = {
  title: string;
  description?: string | null;
  region: string;
  suburb: string;
  locationName?: string | null;
  activities: ActivityInputPayload[];
  eventDate: string;
  startTime: string;
  endTime?: string | null;
  maxParticipants: number;
  maxDistanceKm: number;
};

export type UpdateMeetupPayload = Partial<CreateMeetupPayload>;

export type ActivityDto = {
  id: number;
  name: string;
  description?: string | null;
  type: string;
};

export type UserMeetupDto = {
  id: number;
  userId: number;
  userName: string;
  avatarUrl?: string | null;
  status: string;
  joinedAt: string;
  isConfirmed: boolean;
  confirmedAt?: string | null;
};

export type MeetupLocationSuggestionDto = {
  id: number;
  meetupEventId: number;
  suggestedByUserId: number;
  suggestedByUserName: string;
  name: string;
  address?: string | null;
  type: string;
  isChosen: boolean;
  createdAt: string;
};

export type MeetupEventDto = {
  id: number;
  title: string;
  description?: string | null;
  region: string;
  suburb: string;
  locationName?: string | null;
  activities: ActivityDto[];
  eventDate: string;
  startTime: string;
  endTime?: string | null;
  maxParticipants: number;
  currentParticipants: number;
  maxDistanceKm: number;
  status: string;
  confirmedAt?: string | null;
  completedAt?: string | null;
  createdAt: string;
  updatedAt: string;
  creatorId: number;
  creatorName: string;
  participants: UserMeetupDto[];
  locationSuggestions: MeetupLocationSuggestionDto[];
};

export type MeetupMatchDto = {
  meetupId: number;
  title: string;
  description?: string | null;
  region: string;
  suburb: string;
  locationName?: string | null;
  activityName: string;
  eventDate: string;
  startTime: string;
  endTime?: string | null;
  currentParticipants: number;
  maxParticipants: number;
  status: string;
  matchScore: number;
  creatorId: number;
  creatorName: string;
  distanceKm?: number | null;
  timeMatchScore: number;
};

export type ApiMessage = {
  message: string;
};

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_BASE_URL?.replace(/\/$/, "") ??
  "http://localhost:5118";

function getAuthToken() {
  return localStorage.getItem("authToken");
}

function clearAuthSession() {
  localStorage.removeItem("authToken");
  localStorage.removeItem("authUser");
}

async function parseApiResponse(response: Response) {
  const contentType = response.headers.get("content-type") ?? "";
  if (contentType.includes("application/json")) return response.json();
  return response.text();
}

async function request<T>(path: string, options: RequestInit = {}) {
  const token = getAuthToken();
  const headers = new Headers(options.headers ?? undefined);
  const isFormData =
    typeof FormData !== "undefined" && options.body instanceof FormData;

  if (!isFormData && !headers.has("Content-Type")) {
    headers.set("Content-Type", "application/json");
  }

  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers,
  });

  const data = await parseApiResponse(response);

  if (!response.ok) {
    handleApiError(response.status, data, {
      onUnauthorized: clearAuthSession,
      redirectOnUnauthorized: true,
    });
  }

  return data as T;
}

export function createMeetup(payload: CreateMeetupPayload) {
  return request<MeetupEventDto>("/api/meetups", {
    method: "POST",
    body: JSON.stringify(payload),
  });
}

export function getMeetup(meetupId: number) {
  return request<MeetupEventDto>(`/api/meetups/${meetupId}`, {
    method: "GET",
  });
}

export function getCreatedMeetups() {
  return request<MeetupEventDto[]>("/api/meetups/created", {
    method: "GET",
  });
}

export function getJoinedMeetups() {
  return request<MeetupEventDto[]>("/api/meetups/joined", {
    method: "GET",
  });
}

export function getMatchedMeetups(params: {
  activityType: string;
  suburb: string;
  limit?: number;
}) {
  const query = new URLSearchParams({
    activityType: params.activityType,
    suburb: params.suburb,
    limit: String(params.limit ?? 20),
  });

  return request<MeetupMatchDto[]>(`/api/meetups/matched?${query.toString()}`, {
    method: "GET",
  });
}

export function updateMeetup(meetupId: number, payload: UpdateMeetupPayload) {
  return request<MeetupEventDto>(`/api/meetups/${meetupId}`, {
    method: "PUT",
    body: JSON.stringify(payload),
  });
}

export function deleteMeetup(meetupId: number) {
  return request<ApiMessage>(`/api/meetups/${meetupId}`, {
    method: "DELETE",
  });
}

export function updateMeetupStatus(meetupId: number, status: string) {
  const query = new URLSearchParams({ status });
  return request<MeetupEventDto>(`/api/meetups/${meetupId}/status?${query.toString()}`, {
    method: "PATCH",
  });
}

export function applyMeetup(meetupId: number) {
  return request<UserMeetupDto>(`/api/meetups/${meetupId}/apply`, {
    method: "POST",
  });
}

export function quitMeetup(meetupId: number) {
  return request<ApiMessage>(`/api/meetups/${meetupId}/quit`, {
    method: "POST",
  });
}

export function approveParticipant(meetupId: number, participantId: number) {
  return request<UserMeetupDto>(
    `/api/meetups/${meetupId}/approve/${participantId}`,
    {
      method: "POST",
    },
  );
}
