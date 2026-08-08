import { AuthCard } from "@/features/auth/components/auth-card"
import { Button } from "@/shared/components/ui/button"
import { Field, FieldLabel } from "@/shared/components/ui/field"
import { Input } from "@/shared/components/ui/input"

export default function ForgotPasswordPage() {
  return (
    <AuthCard
      title="Reset your password"
      description="Enter your email and we’ll send you a reset link."
      footerText="Remembered your password?"
      footerHref="/login"
      footerLinkText="Back to login"
    >
      <form className="space-y-6">
        <Field>
          <FieldLabel htmlFor="email">Email</FieldLabel>
          <Input id="email" name="email" type="email" autoComplete="email" maxLength={256} required />
        </Field>
        <Button type="submit" className="w-full">Send reset link</Button>
      </form>
    </AuthCard>
  )
}
