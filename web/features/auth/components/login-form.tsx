"use client"

import Link from "next/link"
import { useRouter } from "next/navigation"
import { useState, type FormEvent } from "react"

import { getApiErrorMessage } from "@/lib/api/get-api-error-message"
import { postApiAccountsLogin } from "@/lib/api/generated/accounts/accounts"
import { Button } from "@/shared/components/ui/button"
import { Field, FieldGroup, FieldLabel } from "@/shared/components/ui/field"
import { Input } from "@/shared/components/ui/input"
import { Spinner } from "@/shared/components/ui/spinner"
import { useErrorToast } from "@/shared/hooks/use-error-toast"

export function LoginForm() {
  const router = useRouter()
  const [error, setError] = useState<string>()
  const [isSubmitting, setIsSubmitting] = useState(false)
  useErrorToast(error, "Login failed")

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setError(undefined)
    setIsSubmitting(true)

    const formData = new FormData(event.currentTarget)

    try {
      await postApiAccountsLogin({
        email: String(formData.get("email")),
        password: String(formData.get("password")),
      })
      router.replace("/app")
    } catch (submissionError) {
      setError(getApiErrorMessage(submissionError, "Unable to log in."))
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <form className="space-y-6" onSubmit={handleSubmit}>
      <FieldGroup className="gap-5">
        <Field>
          <FieldLabel htmlFor="email">Email</FieldLabel>
          <Input id="email" name="email" type="email" autoComplete="email" maxLength={256} disabled={isSubmitting} required />
        </Field>
        <Field>
          <div className="flex items-center justify-between gap-4">
            <FieldLabel htmlFor="password">Password</FieldLabel>
            <Link href="/forgot-password" className="text-sm text-primary hover:underline">
              Forgot password?
            </Link>
          </div>
          <Input id="password" name="password" type="password" autoComplete="current-password" minLength={6} maxLength={100} disabled={isSubmitting} required />
        </Field>
      </FieldGroup>
      <Button type="submit" className="w-full" disabled={isSubmitting}>
        {isSubmitting && <Spinner />}
        {isSubmitting ? "Logging in…" : "Log in"}
      </Button>
    </form>
  )
}
