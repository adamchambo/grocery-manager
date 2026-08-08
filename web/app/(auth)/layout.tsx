import { AuthShell } from "@/shared/components/layout/auth-shell"

export default function AuthLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  return <AuthShell>{children}</AuthShell>
}
