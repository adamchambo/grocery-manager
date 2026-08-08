import { ApplicationShell } from "@/shared/components/layout/application-shell";

export default function ApplicationLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return <ApplicationShell>{children}</ApplicationShell>;
}
