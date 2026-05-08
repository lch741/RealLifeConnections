import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen } from "@testing-library/react";
import RegisterPage from "../../app/register/page";

// Mock the components
vi.mock("../../app/auth/AuthShell", () => ({
  default: ({ title, subtitle, switchLabel, switchHref, switchText, stacked, children }: any) => (
    <div data-testid="auth-shell">
      <h1>{title}</h1>
      <p>{subtitle}</p>
      <a href={switchHref}>{switchText}</a>
      <span>{switchLabel}</span>
      {stacked && <div data-testid="stacked">stacked</div>}
      <div data-testid="auth-shell-children">{children}</div>
    </div>
  ),
}));

vi.mock("../../app/auth/RegisterForm", () => ({
  default: () => <div data-testid="register-form">Register Form Component</div>,
}));

describe("Register Page", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should render the RegisterPage component", () => {
    render(<RegisterPage />);
    expect(screen.getByTestId("auth-shell")).toBeInTheDocument();
  });

  it("should display the correct title", () => {
    render(<RegisterPage />);
    expect(screen.getByText(/Build a profile that feels like you/i)).toBeInTheDocument();
  });

  it("should display the correct subtitle", () => {
    render(<RegisterPage />);
    expect(screen.getByText(/Add your city, interests, and a few personality cues/i)).toBeInTheDocument();
  });

  it("should render the RegisterForm component", () => {
    render(<RegisterPage />);
    expect(screen.getByTestId("register-form")).toBeInTheDocument();
  });

  it("should have a link to login page", () => {
    render(<RegisterPage />);
    const loginLink = screen.getByRole("link", { name: /Login/i });
    expect(loginLink).toHaveAttribute("href", "/login");
  });

  it("should have Register label", () => {
    render(<RegisterPage />);
    expect(screen.getByText("Register")).toBeInTheDocument();
  });

  it("should display switch text to Login", () => {
    render(<RegisterPage />);
    expect(screen.getByText(/Login/i)).toBeInTheDocument();
  });

  it("should have stacked layout prop", () => {
    render(<RegisterPage />);
    expect(screen.getByTestId("stacked")).toBeInTheDocument();
  });

  it("should render with proper structure", () => {
    const { container } = render(<RegisterPage />);
    const authShell = container.querySelector('[data-testid="auth-shell"]');
    expect(authShell).toBeTruthy();
  });
});
