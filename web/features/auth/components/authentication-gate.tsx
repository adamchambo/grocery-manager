"use client"

import { useEffect, useState } from "react"
import { useRouter } from "next/navigation"

import { ApiError } from "@/lib/api/api-client"
import { getApiAccountsMe } from "@/lib/api/generated/accounts/accounts"
import { Button } from "@/shared/components/ui/button"
import { Spinner } from "@/shared/components/ui/spinner"

type AuthenticationGateProps = Readonly<{
  children?: React.ReactNode
  authenticatedRedirectTo?: string
  unauthenticatedRedirectTo?: string
}>

export function AuthenticationGate({ children, authenticatedRedirectTo, unauthenticatedRedirectTo }: AuthenticationGateProps) {
  const router = useRouter()
  const [attempt, setAttempt] = useState(0)
  const [status, setStatus] = useState<"checking" | "ready" | "error">("checking")

  useEffect(() => {
    let cancelled = false

    getApiAccountsMe()
      .then(() => {
        if (cancelled) return
        if (authenticatedRedirectTo) router.replace(authenticatedRedirectTo)
        else setStatus("ready")
      })
      .catch((error: unknown) => {
        if (cancelled) return
        const status = error instanceof ApiError
          ? error.status
          : typeof error === "object" && error !== null && "status" in error && typeof error.status === "number"
            ? error.status
            : undefined
        if (unauthenticatedRedirectTo && (status === 401 || status === undefined)) {
          window.location.replace(unauthenticatedRedirectTo)
          return
        }
        setStatus("error")
      })

    return () => { cancelled = true }
  }, [attempt, authenticatedRedirectTo, router, unauthenticatedRedirectTo])

  if (status === "error") {
    return <main className="flex min-h-svh flex-col items-center justify-center gap-4 px-6 text-center"><p className="text-sm text-muted-foreground">We couldn&apos;t verify your sign-in status.</p><Button variant="outline" onClick={() => { setStatus("checking"); setAttempt((current) => current + 1) }}>Try again</Button></main>
  }

  if (status === "ready") return <>{children}</>

  return <main className="flex min-h-svh items-center justify-center"><Spinner className="size-6" /></main>
}
