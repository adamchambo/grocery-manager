import { AuthenticationGate } from "@/features/auth/components/authentication-gate"
import { ApplicationShell } from "@/shared/components/layout/application-shell"

export default function ApplicationLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return <AuthenticationGate unauthenticatedRedirectTo="/login"><ApplicationShell>{children}</ApplicationShell></AuthenticationGate>
}
