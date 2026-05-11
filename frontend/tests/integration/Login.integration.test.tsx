import { describe, it, expect, beforeEach, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import type { AuthResponse } from "../../app/lib/auth-api";

const replaceMock = vi.hoisted(() => vi.fn());
const loginUserMock = vi.hoisted(() => vi.fn());
const saveAuthSessionMock = vi.hoisted(() => vi.fn());

vi.mock("next/navigation", () => ({
  useRouter: () => ({
    replace: replaceMock,
    push: vi.fn(),
    back: vi.fn(),
    forward: vi.fn(),
  }),
}));

vi.mock("../../app/lib/auth-api", () => ({
  loginUser: loginUserMock,
  saveAuthSession: saveAuthSessionMock,
}));

import LoginPage from "../../app/login/page";

describe("Login integration", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    (window.localStorage.getItem as any) = vi.fn().mockReturnValue(null);
  });

  it("submits login and redirects to profile", async () => {
    const authResponse: AuthResponse = {
      message: "Login successful.",
      token: "test-token",
      user: {
        email: "alex@example.com",
        userName: "Alex",
      },
    };

    loginUserMock.mockResolvedValue(authResponse);

    render(<LoginPage />);

    await userEvent.type(screen.getByLabelText(/Email/i), "alex@example.com");
    await userEvent.type(screen.getByLabelText(/Password/i), "Password123!");

    await userEvent.click(screen.getByRole("button", { name: /Sign in/i }));

    await waitFor(() => {
      expect(loginUserMock).toHaveBeenCalledWith({
        email: "alex@example.com",
        password: "Password123!",
      });
    });

    expect(saveAuthSessionMock).toHaveBeenCalledWith(authResponse);
    expect(replaceMock).toHaveBeenCalledWith("/profile");
    expect(
      await screen.findByText(/Login successful\./i)
    ).toBeInTheDocument();
  });
});
