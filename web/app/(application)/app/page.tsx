import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/ui/card";

export default function DashboardPage() {
  return (
    <div className="space-y-6">
      <header>
        <h1 className="text-2xl font-semibold tracking-tight">Dashboard</h1>
        <p className="mt-1 text-sm text-muted-foreground">Your pantry and shopping activity at a glance.</p>
      </header>
      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        {[
          ["Pantry", "Your item summary will appear here."],
          ["Active stocktake", "Resume or begin a stocktake."],
          ["Shopping lists", "Keep active shopping trips close."],
          ["Recent activity", "See your latest inventory update."],
        ].map(([title, description]) => (
          <Card key={title}>
            <CardHeader><CardTitle>{title}</CardTitle></CardHeader>
            <CardContent className="text-sm text-muted-foreground">{description}</CardContent>
          </Card>
        ))}
      </div>
    </div>
  )
}
