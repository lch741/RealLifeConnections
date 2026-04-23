import { handleApiError } from "./api-error";

export type InterestSelection = {
  categoryId: number;
  interests: string[];
};

export type RegisterPayload = {
  email: string;
  userName: string;
  password: string;
  city?: string;
  bio?: string;
  profileImageUrl?: string;
  interestSelections: InterestSelection[];
};

export type LoginPayload = {
  email: string;
  password: string;
};

export type AuthUser = {
  id?: number;
  email: string;
  userName: string;
  city?: string;
  bio?: string | null;
  profileImageUrl?: string | null;
};

export type AuthResponse = {
  message: string;
  token: string;
  user: AuthUser;
};

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_BASE_URL?.replace(/\/$/, "") ??
  "http://localhost:5118";

async function parseApiResponse(response: Response) {
  const contentType = response.headers.get("content-type") ?? "";

  if (contentType.includes("application/json")) {
    return response.json();
  }

  return response.text();
}

async function requestAuth<TPayload>(
  path: "/api/user/login" | "/api/user/register",
  payload: TPayload,
) {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(payload),
  });

  const data = await parseApiResponse(response);

  if (!response.ok) {
    handleApiError(response.status, data);
  }

  return data as AuthResponse;
}

export function loginUser(payload: LoginPayload) {
  return requestAuth("/api/user/login", payload);
}

export function registerUser(payload: RegisterPayload) {
  return requestAuth("/api/user/register", payload);
}

export function saveAuthSession(auth: AuthResponse) {
  localStorage.setItem("authToken", auth.token);
  localStorage.setItem("authUser", JSON.stringify(auth.user));
}
