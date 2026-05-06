import AuthShell from "../auth/AuthShell";
import RegisterForm from "../auth/RegisterForm";

export default function RegisterPage() {
  return (
    <AuthShell
      title="Build a profile that feels like you."
      subtitle="Add your city, interests, and a few personality cues so the matching system can connect you with people who fit your vibe."
      switchLabel="Register"
      switchHref="/login"
      switchText="Login"
      stacked
    >
      <RegisterForm />
    </AuthShell>
  );
}
