import { describe, it, expect, beforeEach, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import type { MeetupEventDto, MeetupMatchDto } from "../../app/lib/meetup-api";
import type { UserProfile } from "../../app/lib/profile-api";

const pushMock = vi.hoisted(() => vi.fn());
const searchParamsMock = vi.hoisted(() => vi.fn(() => new URLSearchParams()));
const useParamsMock = vi.hoisted(() => vi.fn(() => ({ meetupId: "10" })));
const applyMeetupMock = vi.hoisted(() => vi.fn());
const approveParticipantMock = vi.hoisted(() => vi.fn());
const createMeetupMock = vi.hoisted(() => vi.fn());
const deleteMeetupMock = vi.hoisted(() => vi.fn());
const getCreatedMeetupsMock = vi.hoisted(() => vi.fn());
const getJoinedMeetupsMock = vi.hoisted(() => vi.fn());
const getMatchedMeetupsMock = vi.hoisted(() => vi.fn());
const getMeetupMock = vi.hoisted(() => vi.fn());
const rejectParticipantMock = vi.hoisted(() => vi.fn());
const updateMeetupMock = vi.hoisted(() => vi.fn());
const updateMeetupStatusMock = vi.hoisted(() => vi.fn());
const quitMeetupMock = vi.hoisted(() => vi.fn());
const getProfileMock = vi.hoisted(() => vi.fn());

vi.mock("next/navigation", () => ({
  useParams: useParamsMock,
  useRouter: () => ({
    push: pushMock,
    replace: vi.fn(),
    back: vi.fn(),
    forward: vi.fn(),
  }),
  useSearchParams: searchParamsMock,
}));

vi.mock("@/components/ProtectedRoute", () => ({
  default: ({ children }: any) => children,
}));

vi.mock("@/components/TopBar", () => ({
  default: () => <div data-testid="top-bar" />,
}));

vi.mock("@/components/Toast", () => ({
  default: ({ toast }: any) =>
    toast ? <div role="status">{toast.message}</div> : null,
}));

vi.mock("../../app/lib/meetup-api", () => ({
  applyMeetup: applyMeetupMock,
  approveParticipant: approveParticipantMock,
  createMeetup: createMeetupMock,
  deleteMeetup: deleteMeetupMock,
  getCreatedMeetups: getCreatedMeetupsMock,
  getJoinedMeetups: getJoinedMeetupsMock,
  getMatchedMeetups: getMatchedMeetupsMock,
  getMeetup: getMeetupMock,
  rejectParticipant: rejectParticipantMock,
  updateMeetup: updateMeetupMock,
  updateMeetupStatus: updateMeetupStatusMock,
  quitMeetup: quitMeetupMock,
}));

vi.mock("../../app/lib/profile-api", () => ({
  getProfile: getProfileMock,
}));

import MeetupHubPage from "../../app/meetups/page";
import MeetupDetailPage from "../../app/meetups/[meetupId]/page";

const profile: UserProfile = {
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
    preferredDistanceKm: 15,
  },
  interestSelections: [],
};

const hostedMeetup: MeetupEventDto = {
  id: 10,
  title: "Board game brunch",
  description: "A relaxed weekend meetup.",
  region: "Auckland",
  suburb: "Auckland City",
  locationName: "Central Library",
  activities: [{ id: 1, name: "Board games", description: null, type: "Cafe" }],
  eventDate: "2026-06-12T00:00:00Z",
  startTime: "10:00:00",
  endTime: "12:00:00",
  maxParticipants: 6,
  currentParticipants: 1,
  maxDistanceKm: 15,
  status: "Open",
  confirmedAt: null,
  completedAt: null,
  createdAt: "2026-06-01T00:00:00Z",
  updatedAt: "2026-06-01T00:00:00Z",
  creatorId: 1,
  creatorName: "Alex",
  participants: [
    {
      id: 1,
      userId: 1,
      userName: "Alex",
      status: "Approved",
      joinedAt: "2026-06-01T00:00:00Z",
      isConfirmed: true,
      confirmedAt: null,
    },
    {
      id: 2,
      userId: 2,
      userName: "Taylor",
      status: "Pending",
      joinedAt: "2026-06-02T00:00:00Z",
      isConfirmed: false,
      confirmedAt: null,
    },
  ],
  locationSuggestions: [
    {
      id: 1,
      meetupEventId: 10,
      suggestedByUserId: 2,
      suggestedByUserName: "Taylor",
      name: "Quiet Corner",
      address: "1 Queen Street",
      type: "Cafe",
      isChosen: false,
      createdAt: "2026-06-02T00:00:00Z",
    },
  ],
};

const matchedMeetup: MeetupMatchDto = {
  meetupId: 20,
  title: "Coffee walk",
  description: "Coffee first, walk after.",
  region: "Auckland",
  suburb: "Auckland City",
  locationName: "Harbour Cafe",
  activityName: "Coffee",
  eventDate: "2026-06-20T00:00:00Z",
  startTime: "09:30:00",
  endTime: "11:00:00",
  currentParticipants: 1,
  maxParticipants: 4,
  status: "Open",
  matchScore: 87,
  creatorId: 3,
  creatorName: "Morgan",
  distanceKm: 3,
  timeMatchScore: 90,
};

describe("Meetup integration", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    (window.localStorage.getItem as any) = vi.fn().mockReturnValue("token");
    searchParamsMock.mockReturnValue(new URLSearchParams());
    useParamsMock.mockReturnValue({ meetupId: "10" });
    getProfileMock.mockResolvedValue(profile);
    getCreatedMeetupsMock.mockResolvedValue([hostedMeetup]);
    getJoinedMeetupsMock.mockResolvedValue([]);
    getMatchedMeetupsMock.mockResolvedValue([matchedMeetup]);
    getMeetupMock.mockResolvedValue(hostedMeetup);
    applyMeetupMock.mockResolvedValue({ message: "Join request sent." });
    approveParticipantMock.mockResolvedValue(hostedMeetup.participants[1]);
    createMeetupMock.mockResolvedValue(hostedMeetup);
    deleteMeetupMock.mockResolvedValue({ message: "Meetup deleted." });
    updateMeetupStatusMock.mockResolvedValue({
      ...hostedMeetup,
      status: "Confirmed",
    });
    updateMeetupMock.mockResolvedValue(hostedMeetup);
    quitMeetupMock.mockResolvedValue({ message: "Meetup left." });
    rejectParticipantMock.mockResolvedValue({ message: "Rejected." });
  });

  it("creates a meetup and switches to the manage panel", async () => {
    render(<MeetupHubPage />);

    expect(await screen.findByText(/Plan a new experience\./i)).toBeInTheDocument();

    await userEvent.type(screen.getByLabelText(/^Title$/i), "Sunset sketch walk");
    await userEvent.type(
      screen.getAllByLabelText(/^Description$/i)[0],
      "Bring a notebook and meet new people.",
    );
    await userEvent.type(screen.getByLabelText(/Location name/i), "Viaduct");
    await userEvent.type(screen.getByLabelText(/Event date/i), "2026-06-30");
    await userEvent.type(screen.getByLabelText(/Start time/i), "18:00");
    await userEvent.type(screen.getByLabelText(/End time/i), "19:30");
    await userEvent.clear(screen.getByLabelText(/Max participants/i));
    await userEvent.type(screen.getByLabelText(/Max participants/i), "8");
    await userEvent.type(screen.getByLabelText(/^Name$/i), "Sketching");
    await userEvent.selectOptions(screen.getByLabelText(/^Type$/i), "Park");

    await userEvent.click(screen.getByRole("button", { name: /Create meetup/i }));

    await waitFor(() => {
      expect(createMeetupMock).toHaveBeenCalledWith(
        expect.objectContaining({
          title: "Sunset sketch walk",
          description: "Bring a notebook and meet new people.",
          locationName: "Viaduct",
          eventDate: "2026-06-30",
          startTime: "18:00:00",
          endTime: "19:30:00",
          maxParticipants: 8,
          maxDistanceKm: 15,
          activities: [
            {
              name: "Sketching",
              description: null,
              type: "Park",
              order: 1,
            },
          ],
        }),
      );
    });

    expect(await screen.findByText(/Meetup created\./i)).toBeInTheDocument();
    expect(await screen.findByText(/Control your hosted events\./i)).toBeInTheDocument();
  });

  it("finds matching meetups and sends a join request", async () => {
    searchParamsMock.mockReturnValue(new URLSearchParams("tab=match"));

    render(<MeetupHubPage />);

    expect(await screen.findByText(/Find meetups tailored to your vibe\./i)).toBeInTheDocument();

    await userEvent.click(screen.getByRole("button", { name: /Find matches/i }));

    expect(await screen.findByText("Coffee walk")).toBeInTheDocument();
    await userEvent.click(screen.getByRole("button", { name: /^Join$/i }));

    await waitFor(() => {
      expect(applyMeetupMock).toHaveBeenCalledWith(20);
    });
    expect(await screen.findByText(/Join request sent\./i)).toBeInTheDocument();
  });

  it("loads meetup details and lets the host approve a pending participant", async () => {
    render(<MeetupDetailPage />);

    expect(
      (await screen.findAllByRole("heading", { name: "Board game brunch" }))[0],
    ).toBeInTheDocument();
    expect(screen.getByText("Taylor")).toBeInTheDocument();

    expect(screen.getByText("Pending")).toBeInTheDocument();
    await userEvent.click(screen.getByRole("button", { name: /Approve/i }));

    await waitFor(() => {
      expect(approveParticipantMock).toHaveBeenCalledWith(10, 2);
    });
    expect(await screen.findByText(/Participant approved\./i)).toBeInTheDocument();
  });

  it("updates meetup status and deletes the meetup from the detail page", async () => {
    render(<MeetupDetailPage />);

    expect(await screen.findByLabelText(/Update status/i)).toBeInTheDocument();
    await userEvent.selectOptions(screen.getByLabelText(/Update status/i), "Confirmed");

    await waitFor(() => {
      expect(updateMeetupStatusMock).toHaveBeenCalledWith(10, "Confirmed");
    });

    await userEvent.click(screen.getByRole("button", { name: /Delete meetup/i }));

    await waitFor(() => {
      expect(deleteMeetupMock).toHaveBeenCalledWith(10);
    });
    expect(pushMock).toHaveBeenCalledWith("/meetups");
  });
});
