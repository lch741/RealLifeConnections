import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen } from "@testing-library/react";

vi.mock("next/navigation", () => ({
  useRouter: () => ({
    push: vi.fn(),
    replace: vi.fn(),
    back: vi.fn(),
    forward: vi.fn(),
  }),
  useSearchParams: () => new URLSearchParams(),
}));

vi.mock("@/components/ProtectedRoute", () => ({
  default: ({ children }: any) => children,
}));

vi.mock("@/components/TopBar", () => ({
  default: () => <div data-testid="top-bar" />,
}));

vi.mock("@/components/Toast", () => ({
  default: () => null,
}));

vi.mock("../../app/lib/meetup-api", () => ({
  applyMeetup: vi.fn(),
  approveParticipant: vi.fn(),
  createMeetup: vi.fn(),
  deleteMeetup: vi.fn(),
  getCreatedMeetups: vi.fn(),
  getJoinedMeetups: vi.fn(),
  getMatchedMeetups: vi.fn(),
  rejectParticipant: vi.fn(),
  updateMeetup: vi.fn(),
  updateMeetupStatus: vi.fn(),
  quitMeetup: vi.fn(),
}));

vi.mock("../../app/lib/profile-api", () => ({
  getProfile: vi.fn(),
}));

import MeetupHubPage from "../../app/meetups/page";
import { getCreatedMeetups, getJoinedMeetups } from "../../app/lib/meetup-api";
import { getProfile } from "../../app/lib/profile-api";

const baseProfile = {
  id: 1,
  email: "user@example.com",
  userName: "user",
  region: "TestRegion",
  suburb: "TestSuburb",
  isVerified: true,
  verificationStatus: "approved",
  canMatch: true,
  interestSelections: [],
} as const;

const baseMeetup = {
  id: 1,
  title: "Meetup title",
  description: "Description",
  region: "TestRegion",
  suburb: "TestSuburb",
  locationName: "Test Location",
  activities: [
    { id: 1, name: "Coffee", description: "", type: "Cafe" },
  ],
  eventDate: "2026-06-02T10:00:00Z",
  startTime: "10:00:00",
  endTime: "12:00:00",
  maxParticipants: 5,
  currentParticipants: 1,
  maxDistanceKm: 20,
  status: "Open",
  confirmedAt: null,
  completedAt: null,
  createdAt: "2026-06-01T00:00:00Z",
  updatedAt: "2026-06-01T00:00:00Z",
  creatorId: 1,
  creatorName: "Host",
  participants: [
    {
      id: 1,
      userId: 1,
      userName: "Host",
      status: "Approved",
      joinedAt: "2026-06-01T00:00:00Z",
      isConfirmed: true,
      confirmedAt: null,
    },
  ],
  locationSuggestions: [],
} as const;

describe("Meetups Page", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    (getProfile as any).mockResolvedValue(baseProfile);
    (getCreatedMeetups as any).mockResolvedValue([baseMeetup]);
    (getJoinedMeetups as any).mockResolvedValue([baseMeetup]);
  });

  it("should render the meetups hub header", () => {
    render(<MeetupHubPage />);
    expect(screen.getByText(/Meetups Hub/i)).toBeInTheDocument();
  });

  it("should render the top bar", () => {
    render(<MeetupHubPage />);
    expect(screen.getByTestId("top-bar")).toBeInTheDocument();
  });

  it("should render the tab buttons", () => {
    render(<MeetupHubPage />);
    expect(screen.getByText("Create")).toBeInTheDocument();
    expect(screen.getByText("Manage")).toBeInTheDocument();
    expect(screen.getByText("Match")).toBeInTheDocument();
  });
});
