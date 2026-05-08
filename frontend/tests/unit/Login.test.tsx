import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen } from "@testing-library/react";
import LoginPage from "../../app/login/page";

// Mock the components
vi.mock("../../app/auth/AuthShell", () => ({
  default: ({ title, subtitle, switchLabel, switchHref, switchText, children }: any) => (
    <div data-testid="auth-shell">
      <h1>{title}</h1>
      <p>{subtitle}</p>
      <a href={switchHref}>{switchText}</a>
      <span>{switchLabel}</span>
      <div data-testid="auth-shell-children">{children}</div>
    </div>
  ),
}));

vi.mock("../../app/auth/LoginForm", () => ({
  default: () => <div data-testid="login-form">Login Form Component</div>,
}));

describe("Login Page", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should render the LoginPage component", () => {
    render(<LoginPage />);
    expect(screen.getByTestId("auth-shell")).toBeInTheDocument();
  });

  it("should display the correct title", () => {
    render(<LoginPage />);
    expect(screen.getByText(/Welcome back to your real-life circle/i)).toBeInTheDocument();
  });

  it("should display the correct subtitle", () => {
    render(<LoginPage />);
    expect(
      screen.getByText(/Sign in to keep building your profile/i)
    ).toBeInTheDocument();
  });

  it("should render the LoginForm component", () => {
    render(<LoginPage />);
    expect(screen.getByTestId("login-form")).toBeInTheDocument();
  });

  it("should have a link to register page", () => {
    render(<LoginPage />);
    const registerLink = screen.getByRole("link", { name: /Register/i });
    expect(registerLink).toHaveAttribute("href", "/register");
  });

  it("should have Login label", () => {
    render(<LoginPage />);
    expect(screen.getByText("Login")).toBeInTheDocument();
  });

  it("should display switch text to Register", () => {
    render(<LoginPage />);
    expect(screen.getByText("Register")).toBeInTheDocument();
  });
});
