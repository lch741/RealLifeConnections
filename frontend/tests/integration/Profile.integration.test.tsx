import { describe, it, expect, beforeEach, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import type { UserProfile } from "../../app/lib/profile-api";

const getProfileMock = vi.hoisted(() => vi.fn());
const updateProfileMock = vi.hoisted(() => vi.fn());
const saveAvatarUploadMock = vi.hoisted(() => vi.fn());
const verifyFaceUploadMock = vi.hoisted(() => vi.fn());

vi.mock("../../app/lib/profile-api", () => ({
  getProfile: getProfileMock,
  updateProfile: updateProfileMock,
  saveAvatarUpload: saveAvatarUploadMock,
  verifyFaceUpload: verifyFaceUploadMock,
}));

import ProfilePage from "../../app/profile/page";

describe("Profile integration", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    (window.localStorage.getItem as any) = vi.fn().mockReturnValue("token");
  });

  it("loads profile data and saves updates", async () => {
    const profileData: UserProfile = {
      id: 1,
      email: "alex@example.com",
      userName: "Alex",
      region: "Auckland",
      suburb: "Auckland City",
      avatarUrl: null,
      gender: "NotToTell",
      age: 28,
      culture: null,
      isVerified: true,
      verificationStatus: "Verified",
      canMatch: true,
      personality: {
        chillToEnergetic: 50,
        talkativeToQuiet: 50,
        plannerToSpontaneous: 50,
        introvertToExtrovert: 50,
        preferredDaysOfWeek: "Weekday",
        preferredTimeOfDay: "Morning",
        preferredDistanceKm: 10,
      },
      interestSelections: [
        {
          categoryId: 1,
          categoryName: "Sports",
          interests: ["hiking"],
        },
      ],
    };

    getProfileMock.mockResolvedValue(profileData);
    updateProfileMock.mockResolvedValue(profileData);

    render(<ProfilePage />);

    expect(await screen.findByText(/Edit your information\./i)).toBeInTheDocument();

    const userNameInput = screen.getByLabelText(/Username/i);
    await userEvent.clear(userNameInput);
    await userEvent.type(userNameInput, "Taylor");

    const interestInputs = screen.getAllByLabelText(/Interests/i);
    await userEvent.clear(interestInputs[0]);
    await userEvent.type(interestInputs[0], "biking");

    await userEvent.click(screen.getByRole("button", { name: /Save changes/i }));

    await waitFor(() => {
      expect(updateProfileMock).toHaveBeenCalled();
    });

    const submittedPayload = updateProfileMock.mock.calls[0][0];
    expect(submittedPayload).toMatchObject({
      userName: "Taylor",
      interestSelections: [
        {
          categoryId: 1,
          interests: ["biking"],
        },
      ],
    });
  });
});
