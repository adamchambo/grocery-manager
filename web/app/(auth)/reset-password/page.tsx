import { AuthCard } from "@/features/auth/components/auth-card"
import { PasswordRequirements } from "@/features/auth/components/password-requirements"
import { Button } from "@/shared/components/ui/button"
import { Field, FieldGroup, FieldLabel } from "@/shared/components/ui/field"
import { Input } from "@/shared/components/ui/input"

export default function ResetPasswordPage() {
  return (
    <AuthCard
      title="Choose a new password"
      description="Your reset link securely identifies this request."
      footerText="Return to"
      footerHref="/login"
      footerLinkText="Login"
    >
      <form className="space-y-6">
        <FieldGroup className="gap-5">
          <Field>
            <FieldLabel htmlFor="new-password">New password</FieldLabel>
            <Input id="new-password" name="newPassword" type="password" autoComplete="new-password" minLength={6} maxLength={100} required />
            <PasswordRequirements />
          </Field>
          <Field>
            <FieldLabel htmlFor="confirm-password">Confirm password</FieldLabel>
            <Input id="confirm-password" name="confirmPassword" type="password" autoComplete="new-password" minLength={6} maxLength={100} required />
          </Field>
        </FieldGroup>
        <Button type="submit" className="w-full">Reset password</Button>
      </form>
    </AuthCard>
  )
}
