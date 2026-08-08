import Link from "next/link"

import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/shared/components/ui/card"

type AuthCardProps = Readonly<{
  title: string
  description: string
  children: React.ReactNode
  footerText: string
  footerHref: string
  footerLinkText: string
}>

export function AuthCard({
  title,
  description,
  children,
  footerText,
  footerHref,
  footerLinkText,
}: AuthCardProps) {
  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-2xl">{title}</CardTitle>
        <CardDescription>{description}</CardDescription>
      </CardHeader>
      <CardContent>{children}</CardContent>
      <CardFooter className="justify-center border-t text-sm text-muted-foreground">
        {footerText}&nbsp;
        <Link href={footerHref} className="font-medium text-primary hover:underline">
          {footerLinkText}
        </Link>
      </CardFooter>
    </Card>
  )
}
