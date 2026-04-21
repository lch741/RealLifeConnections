import AuthShell from "../auth/AuthShell";
import RegisterForm from "../auth/RegisterForm";

export default function RegisterPage() {
  return (
    <AuthShell
      title="Create a profile that starts with shared interests."
      subtitle="Register with your city and a few interests so the matching backend has enough context to find relevant people."
      switchLabel="Register"
      switchHref="/login"
      switchText="Login"
    >
      <RegisterForm />
    </AuthShell>
  );
}
