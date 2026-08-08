"use client"

import { useEffect } from "react"
import { toast } from "sonner"

export function showErrorToast(error: string, title = "Something went wrong") {
  toast.error(title, { description: error, id: `${title}:${error}` })
}

export function showSuccessToast(message: string) {
  toast.success(message, { id: `success:${message}` })
}

export function useErrorToast(error: string | undefined, title = "Something went wrong") {
  useEffect(() => {
    if (!error) return
    showErrorToast(error, title)
  }, [error, title])
}
