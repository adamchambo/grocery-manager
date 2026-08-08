import { AuthCard } from "@/features/auth/components/auth-card"
import { LoginForm } from "@/features/auth/components/login-form"

export default function LoginPage() {
  return (
    <AuthCard
      title="Welcome back"
      description="Log in to manage your pantry and shopping lists."
      footerText="New to Grocery Manager?"
      footerHref="/register"
      footerLinkText="Create an account"
    >
      <LoginForm />
    </AuthCard>
  )
}
