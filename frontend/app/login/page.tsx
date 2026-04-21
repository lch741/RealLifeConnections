import AuthShell from "../auth/AuthShell";
import LoginForm from "../auth/LoginForm";

export default function LoginPage() {
  return (
    <AuthShell
      title="Welcome back to your real-life circle."
      subtitle="Sign in to keep building your profile, verify your face, and continue matching with people who share your interests."
      switchLabel="Login"
      switchHref="/register"
      switchText="Register"
    >
      <LoginForm />
    </AuthShell>
  );
}
