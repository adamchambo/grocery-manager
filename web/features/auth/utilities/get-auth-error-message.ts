import { ApiError } from "@/lib/api/api-client"

type ProblemDetails = {
  detail?: unknown
  errors?: Record<string, string[]>
}

export function getAuthErrorMessage(error: unknown, fallback: string) {
  if (!(error instanceof ApiError)) {
    return "Unable to reach the server. Please try again."
  }

  const details = error.details as ProblemDetails | null

  if (typeof details?.detail === "string") {
    return details.detail
  }

  const validationMessage = details?.errors
    ? Object.values(details.errors).flat().at(0)
    : undefined

  return validationMessage ?? fallback
}
