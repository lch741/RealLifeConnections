import { describe, it, expect, beforeEach, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import type { AuthResponse } from "../../app/lib/auth-api";

const replaceMock = vi.hoisted(() => vi.fn());
const registerUserMock = vi.hoisted(() => vi.fn());
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
  registerUser: registerUserMock,
  saveAuthSession: saveAuthSessionMock,
}));

import RegisterPage from "../../app/register/page";

describe("Register integration", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    (window.localStorage.getItem as any) = vi.fn().mockReturnValue(null);
  });

  it("submits registration and redirects to profile", async () => {
    const authResponse: AuthResponse = {
      message: "Registration successful.",
      token: "new-token",
      user: {
        email: "sam@example.com",
        userName: "Sam",
      },
    };

    registerUserMock.mockResolvedValue(authResponse);

    render(<RegisterPage />);

    await userEvent.type(screen.getByLabelText(/Email/i), "sam@example.com");
    await userEvent.type(screen.getByLabelText(/Username/i), "Sam");
    await userEvent.type(screen.getByLabelText(/Password/i), "Password123!");

    const regionSelect = screen.getByLabelText(/Region/i) as HTMLSelectElement;
    if (regionSelect.options.length > 0) {
      await userEvent.selectOptions(regionSelect, regionSelect.options[0].value);
    }

    const citySelect = screen.getByLabelText(/City/i) as HTMLSelectElement;
    if (citySelect.options.length > 1) {
      await userEvent.selectOptions(citySelect, citySelect.options[1].value);
    }

    const interestInputs = screen.getAllByLabelText(/Interests/i);
    await userEvent.type(interestInputs[0], "hiking");

    await userEvent.click(
      screen.getByRole("button", { name: /Create account/i })
    );

    await waitFor(() => {
      expect(registerUserMock).toHaveBeenCalled();
    });

    expect(saveAuthSessionMock).toHaveBeenCalledWith(authResponse);
    expect(replaceMock).toHaveBeenCalledWith("/profile");

    expect(
      await screen.findByText(/Registration successful\./i)
    ).toBeInTheDocument();

    const submittedPayload = registerUserMock.mock.calls[0][0];
    expect(submittedPayload).toMatchObject({
      email: "sam@example.com",
      userName: "Sam",
      interestSelections: [
        {
          categoryId: expect.any(Number),
          interests: ["hiking"],
        },
      ],
    });
  });
});
