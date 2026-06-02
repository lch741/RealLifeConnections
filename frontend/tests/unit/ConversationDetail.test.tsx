import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen } from "@testing-library/react";

vi.mock("next/navigation", () => ({
  useParams: () => ({ meetupId: "10", userId: "2" }),
  useRouter: () => ({
    push: vi.fn(),
    replace: vi.fn(),
    back: vi.fn(),
    forward: vi.fn(),
  }),
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

vi.mock("../../app/lib/chat-api", () => ({
  getMeetupMessages: vi.fn(),
  sendMeetupMessage: vi.fn(),
}));

vi.mock("../../app/lib/meetup-api", () => ({
  getMeetup: vi.fn(),
}));

vi.mock("../../app/lib/profile-api", () => ({
  getProfile: vi.fn(),
}));

import ConversationDetailPage from "../../app/conversations/[meetupId]/[userId]/page";
import { getMeetupMessages } from "../../app/lib/chat-api";
import { getMeetup } from "../../app/lib/meetup-api";
import { getProfile } from "../../app/lib/profile-api";

const profile = {
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

const meetup = {
  id: 10,
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
      userId: 2,
      userName: "Participant",
      status: "Approved",
      joinedAt: "2026-06-01T00:00:00Z",
      isConfirmed: true,
      confirmedAt: null,
    },
  ],
  locationSuggestions: [],
} as const;

const messages = [
  {
    id: 1,
    senderId: 2,
    content: "Hello",
    createdAt: "2026-06-02T12:00:00Z",
  },
];

describe("Conversation Detail Page", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    (getProfile as any).mockResolvedValue(profile);
    (getMeetup as any).mockResolvedValue(meetup);
    (getMeetupMessages as any).mockResolvedValue(messages);
  });

  it("should render the meetup title", async () => {
    render(<ConversationDetailPage />);
    expect(await screen.findByText(meetup.title)).toBeInTheDocument();
  });

  it("should render the message content", async () => {
    render(<ConversationDetailPage />);
    expect(await screen.findByText("Hello")).toBeInTheDocument();
  });
});
