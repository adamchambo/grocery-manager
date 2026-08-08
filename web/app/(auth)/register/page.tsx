import { AuthCard } from "@/features/auth/components/auth-card"
import { RegisterForm } from "@/features/auth/components/register-form"

export default function RegisterPage() {
  return (
    <AuthCard
      title="Create your account"
      description="Start with a pantry you can keep accurate."
      footerText="Already have an account?"
      footerHref="/login"
      footerLinkText="Log in"
    >
      <RegisterForm />
    </AuthCard>
  )
}
