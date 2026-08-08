import { OnboardingFlow } from "@/features/onboarding/components/onboarding-flow"
import { Brand } from "@/shared/components/layout/brand"

export default function OnboardingPage() {
  return (
    <main className="min-h-svh bg-surface-muted/50">
      <header className="border-b bg-background px-4 py-4 sm:px-6"><div className="mx-auto max-w-5xl"><Brand /></div></header>
      <div className="mx-auto max-w-5xl px-4 py-8 sm:px-6"><OnboardingFlow /></div>
    </main>
  )
}
