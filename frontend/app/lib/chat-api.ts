import { handleApiError } from "./api-error";

export type MessageResponseDto = {
  id: number;
  senderId: number;
  content: string;
  createdAt: string;
};

export type ConversationDto = {
  conversationId: number;
  meetupEventId?: number | null;
  meetupTitle?: string | null;
  meetupStatus?: string | null;
  otherUserId: number;
  otherUserName?: string | null;
  lastMessageAt: string;
  isClosed: boolean;
  isExpired: boolean;
  endsAt?: string | null;
};

export type SendMessagePayload = {
  receiverId: number;
  content: string;
  meetupEventId?: number | null;
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

export function getMeetupMessages(meetupId: number, otherUserId: number) {
  return request<MessageResponseDto[]>(
    `/api/chat/meetups/${meetupId}/${otherUserId}`,
    {
      method: "GET",
    },
  );
}

export function getConversations() {
  return request<ConversationDto[]>("/api/chat/conversations", {
    method: "GET",
  });
}

export function sendMeetupMessage(payload: SendMessagePayload) {
  return request<ApiMessage>("/api/chat/send", {
    method: "POST",
    body: JSON.stringify(payload),
  });
}
