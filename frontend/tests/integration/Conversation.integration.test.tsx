import { describe, it, expect, beforeEach, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import type { ConversationDto, MessageResponseDto } from "../../app/lib/chat-api";
import type { MeetupEventDto } from "../../app/lib/meetup-api";
import type { UserProfile } from "../../app/lib/profile-api";

const useParamsMock = vi.hoisted(() =>
  vi.fn(() => ({ meetupId: "10", userId: "2" })),
);
const getConversationsMock = vi.hoisted(() => vi.fn());
const getMeetupMessagesMock = vi.hoisted(() => vi.fn());
const sendMeetupMessageMock = vi.hoisted(() => vi.fn());
const getMeetupMock = vi.hoisted(() => vi.fn());
const getProfileMock = vi.hoisted(() => vi.fn());

vi.mock("next/navigation", () => ({
  useParams: useParamsMock,
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
  default: ({ toast }: any) =>
    toast ? <div role="status">{toast.message}</div> : null,
}));

vi.mock("../../app/lib/chat-api", () => ({
  getConversations: getConversationsMock,
  getMeetupMessages: getMeetupMessagesMock,
  sendMeetupMessage: sendMeetupMessageMock,
}));

vi.mock("../../app/lib/meetup-api", () => ({
  getMeetup: getMeetupMock,
}));

vi.mock("../../app/lib/profile-api", () => ({
  getProfile: getProfileMock,
}));

import ConversationsPage from "../../app/conversations/page";
import ConversationDetailPage from "../../app/conversations/[meetupId]/[userId]/page";

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
  personality: null,
  interestSelections: [],
};

const meetup: MeetupEventDto = {
  id: 10,
  title: "Board game brunch",
  description: "A relaxed weekend meetup.",
  region: "Auckland",
  suburb: "Auckland City",
  locationName: "Central Library",
  activities: [{ id: 1, name: "Board games", description: null, type: "Cafe" }],
  eventDate: "2026-06-12T00:00:00Z",
  startTime: "10:00:00",
  endTime: null,
  maxParticipants: 6,
  currentParticipants: 2,
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
      status: "Approved",
      joinedAt: "2026-06-02T00:00:00Z",
      isConfirmed: true,
      confirmedAt: null,
    },
  ],
  locationSuggestions: [],
};

const conversations: ConversationDto[] = [
  {
    conversationId: 7,
    meetupEventId: 10,
    meetupTitle: "Board game brunch",
    meetupStatus: "Open",
    otherUserId: 2,
    otherUserName: "Taylor",
    lastMessageAt: "2026-06-02T12:00:00Z",
    isClosed: false,
    isExpired: false,
    endsAt: "2026-06-12T12:00:00Z",
  },
  {
    conversationId: 8,
    meetupEventId: null,
    meetupTitle: "Direct chat",
    meetupStatus: null,
    otherUserId: 3,
    otherUserName: "Morgan",
    lastMessageAt: "2026-06-02T11:00:00Z",
    isClosed: false,
    isExpired: false,
    endsAt: null,
  },
];

const messages: MessageResponseDto[] = [
  {
    id: 1,
    senderId: 2,
    content: "See you near the front desk?",
    createdAt: "2026-06-02T12:00:00Z",
  },
  {
    id: 2,
    senderId: 1,
    content: "Yes, that works.",
    createdAt: "2026-06-02T12:05:00Z",
  },
];

describe("Conversation integration", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    (window.localStorage.getItem as any) = vi.fn().mockReturnValue(null);
    (window.localStorage.setItem as any) = vi.fn();
    useParamsMock.mockReturnValue({ meetupId: "10", userId: "2" });
    getProfileMock.mockResolvedValue(profile);
    getConversationsMock.mockResolvedValue(conversations);
    getMeetupMock.mockResolvedValue(meetup);
    getMeetupMessagesMock.mockResolvedValue(messages);
    sendMeetupMessageMock.mockResolvedValue({ message: "Message sent." });
  });

  it("loads meetup conversations and marks unseen messages as new", async () => {
    render(<ConversationsPage />);

    expect(await screen.findByText("Board game brunch")).toBeInTheDocument();
    expect(screen.getByText("Taylor")).toBeInTheDocument();
    expect(screen.getByText("New")).toBeInTheDocument();
    expect(screen.queryByText("Direct chat")).not.toBeInTheDocument();
  });

  it("refreshes conversations on demand", async () => {
    render(<ConversationsPage />);

    expect(await screen.findByText("Board game brunch")).toBeInTheDocument();
    await userEvent.click(screen.getByRole("button", { name: /Refresh/i }));

    await waitFor(() => {
      expect(getConversationsMock).toHaveBeenCalledTimes(2);
    });
  });

  it("loads a meetup conversation and records it as seen", async () => {
    render(<ConversationDetailPage />);

    expect(await screen.findByText("Chat with Taylor")).toBeInTheDocument();
    expect(screen.getByText("See you near the front desk?")).toBeInTheDocument();
    expect(screen.getByText("Yes, that works.")).toBeInTheDocument();

    await waitFor(() => {
      expect(window.localStorage.setItem).toHaveBeenCalledWith(
        "chat:lastSeen:10:2",
        expect.any(String),
      );
    });
  });

  it("sends a message and refreshes the thread", async () => {
    render(<ConversationDetailPage />);

    const messageBox = await screen.findByPlaceholderText(/Write a message/i);
    await userEvent.type(messageBox, "I'll bring the cards.");
    await userEvent.click(screen.getByRole("button", { name: /^Send$/i }));

    await waitFor(() => {
      expect(sendMeetupMessageMock).toHaveBeenCalledWith({
        receiverId: 2,
        content: "I'll bring the cards.",
        meetupEventId: 10,
      });
    });
    expect(getMeetupMessagesMock).toHaveBeenCalledTimes(2);
    expect(messageBox).toHaveValue("");
  });
});
