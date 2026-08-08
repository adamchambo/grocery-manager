"use client"

import { useRouter } from "next/navigation"
import { useState, type FormEvent } from "react"

import { PasswordRequirements } from "@/features/auth/components/password-requirements"
import { getAuthErrorMessage } from "@/features/auth/utilities/get-auth-error-message"
import { postApiAccountsRegister } from "@/lib/api/generated/accounts/accounts"
import { Alert, AlertDescription, AlertTitle } from "@/shared/components/ui/alert"
import { Button } from "@/shared/components/ui/button"
import { Field, FieldError, FieldGroup, FieldLabel } from "@/shared/components/ui/field"
import { Input } from "@/shared/components/ui/input"
import { Spinner } from "@/shared/components/ui/spinner"

export function RegisterForm() {
  const router = useRouter()
  const [password, setPassword] = useState("")
  const [confirmation, setConfirmation] = useState("")
  const [confirmationError, setConfirmationError] = useState<string>()
  const [error, setError] = useState<string>()
  const [isSubmitting, setIsSubmitting] = useState(false)

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    if (password !== confirmation) {
      setConfirmationError("Passwords do not match.")
      return
    }

    setConfirmationError(undefined)
    setError(undefined)
    setIsSubmitting(true)

    const formData = new FormData(event.currentTarget)

    try {
      await postApiAccountsRegister({
        email: String(formData.get("email")),
        password,
      })
      router.replace("/onboarding")
    } catch (submissionError) {
      setError(getAuthErrorMessage(submissionError, "Unable to create your account."))
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <form className="space-y-6" onSubmit={handleSubmit}>
      {error && (
        <Alert variant="destructive">
          <AlertTitle>Account creation failed</AlertTitle>
          <AlertDescription>{error}</AlertDescription>
        </Alert>
      )}
      <FieldGroup className="gap-5">
        <Field>
          <FieldLabel htmlFor="email">Email</FieldLabel>
          <Input id="email" name="email" type="email" autoComplete="email" maxLength={256} disabled={isSubmitting} required />
        </Field>
        <Field>
          <FieldLabel htmlFor="password">Password</FieldLabel>
          <Input
            id="password"
            name="password"
            type="password"
            autoComplete="new-password"
            minLength={6}
            maxLength={100}
            value={password}
            disabled={isSubmitting}
            onChange={(event) => {
              setPassword(event.target.value)
              setConfirmationError(undefined)
            }}
            required
          />
          <PasswordRequirements />
        </Field>
        <Field data-invalid={Boolean(confirmationError)}>
          <FieldLabel htmlFor="confirm-password">Confirm password</FieldLabel>
          <Input
            id="confirm-password"
            name="confirmPassword"
            type="password"
            autoComplete="new-password"
            minLength={6}
            maxLength={100}
            value={confirmation}
            onChange={(event) => {
              setConfirmation(event.target.value)
              setConfirmationError(undefined)
            }}
            aria-invalid={Boolean(confirmationError)}
            aria-describedby={confirmationError ? "confirm-password-error" : undefined}
            disabled={isSubmitting}
            required
          />
          <FieldError id="confirm-password-error">{confirmationError}</FieldError>
        </Field>
      </FieldGroup>
      <Button type="submit" className="w-full" disabled={isSubmitting}>
        {isSubmitting && <Spinner />}
        {isSubmitting ? "Creating account…" : "Create account"}
      </Button>
    </form>
  )
}
