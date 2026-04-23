// frontend/app/lib/profile-api.ts
export type InterestSelection = {
  categoryId: number;
  interests: string[];
};

export type UserProfile = {
  email: string;
  userName: string;
  bio?: string | null;
  city: string;
  avatarUrl?: string | null;
  isVerified: boolean;
  verificationStatus: string;
  canMatch: boolean;
  interestSelections: Array<{
    categoryId: number;
    categoryName: string;
    interests: string[];
  }>;
};

export type UpdateProfilePayload = {
  userName?: string;
  bio?: string;
  city: string;
  interestSelections: InterestSelection[];
};

export type SaveAvatarPayload = {
  avatarUrl: string;
};

export type VerifyFacePayload = {
  liveCaptureUrl: string;
};

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_BASE_URL?.replace(/\/$/, "") ??
  "http://localhost:5118";

function getAuthToken() {
  return localStorage.getItem("authToken");
}

async function parseApiResponse(response: Response) {
  const contentType = response.headers.get("content-type") ?? "";
  if (contentType.includes("application/json")) return response.json();
  return response.text();
}

async function request<T>(path: string, options: RequestInit = {}) {
  const token = getAuthToken();

  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(options.headers ?? {}),
    },
  });

  const data = await parseApiResponse(response);

  if (!response.ok) {
    const message =
      typeof data === "string"
        ? data
        : data?.message ?? data?.title ?? "Request failed.";
    throw new Error(message);
  }

  return data as T;
}

export function getProfile() {
  return request<UserProfile>("/api/user/profile", {
    method: "GET",
  });
}

export function updateProfile(payload: UpdateProfilePayload) {
  return request<UserProfile>("/api/user/profile", {
    method: "PUT",
    body: JSON.stringify(payload),
  });
}

export function saveAvatar(payload: SaveAvatarPayload) {
  return request<UserProfile>("/api/user/avatar", {
    method: "POST",
    body: JSON.stringify(payload),
  });
}

export function verifyFace(payload: VerifyFacePayload) {
  return request("/api/user/verify-face", {
    method: "POST",
    body: JSON.stringify(payload),
  });
}