import { Brand } from "@/shared/components/layout/brand"

export function AuthShell({ children }: Readonly<{ children: React.ReactNode }>) {
  return (
    <main className="grid min-h-svh bg-background lg:grid-cols-[minmax(20rem,0.8fr)_1.2fr]">
      <section className="hidden bg-primary p-10 text-primary-foreground lg:flex lg:flex-col lg:justify-between">
        <Brand className="text-primary-foreground" />
        <div className="max-w-md space-y-4">
          <p className="text-sm font-medium uppercase tracking-[0.18em] opacity-75">
            Shop with less guesswork
          </p>
          <p className="text-4xl font-semibold leading-tight">
            Know what you have, what you need, and what to buy next.
          </p>
        </div>
        <p className="text-sm opacity-75">Your pantry stays private to your account.</p>
      </section>

      <section className="flex min-h-svh flex-col p-5 sm:p-8">
        <Brand className="lg:hidden" />
        <div className="flex flex-1 items-center justify-center py-10">
          <div className="w-full max-w-md">{children}</div>
        </div>
      </section>
    </main>
  )
}
