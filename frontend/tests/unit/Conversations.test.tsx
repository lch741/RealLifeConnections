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

vi.mock("../../app/lib/chat-api", () => ({
  getConversations: vi.fn(),
}));

vi.mock("../../app/lib/profile-api", () => ({
  getProfile: vi.fn(),
}));

import ConversationsPage from "../../app/conversations/page";
import { getConversations } from "../../app/lib/chat-api";
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

const conversations = [
  {
    conversationId: 1,
    meetupEventId: 10,
    meetupTitle: "Meetup",
    meetupStatus: "Open",
    otherUserId: 2,
    otherUserName: "Participant",
    lastMessageAt: "2026-06-02T12:00:00Z",
    isClosed: false,
    isExpired: false,
    endsAt: null,
  },
];

describe("Conversations Page", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    (getProfile as any).mockResolvedValue(profile);
    (getConversations as any).mockResolvedValue(conversations);
  });

  it("should render the conversations header", () => {
    render(<ConversationsPage />);
    expect(
      screen.getByRole("heading", { name: /Meetup chats/i }),
    ).toBeInTheDocument();
  });

  it("should render the top bar", () => {
    render(<ConversationsPage />);
    expect(screen.getByTestId("top-bar")).toBeInTheDocument();
  });
});
