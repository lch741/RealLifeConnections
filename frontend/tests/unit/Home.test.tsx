import { describe, it, expect, beforeEach, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import Home from "../../app/page";

describe("Home Page", () => {
  beforeEach(() => {
    // Reset mocks before each test
    vi.clearAllMocks();
  });

  it("should render the Home page with redirecting text", () => {
    (window.localStorage.getItem as any) = vi.fn().mockReturnValue(null);
    render(<Home />);
    expect(screen.getByText(/Redirecting.../i)).toBeInTheDocument();
  });

  it("should have main container with correct styling", () => {
    (window.localStorage.getItem as any) = vi.fn().mockReturnValue(null);
    const { container } = render(<Home />);
    const main = container.querySelector("main");
    expect(main).toHaveClass("min-h-screen", "bg-[#f8faf7]");
  });

  it("should render a div with redirecting message", () => {
    (window.localStorage.getItem as any) = vi.fn().mockReturnValue(null);
    render(<Home />);
    const messageDiv = screen.getByText(/Redirecting.../i);
    expect(messageDiv).toHaveClass("text-sm", "text-zinc-600");
  });
});

