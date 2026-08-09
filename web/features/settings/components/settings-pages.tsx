"use client"

import Link from "next/link"
import { useRouter } from "next/navigation"
import { useEffect, useState, type FormEvent } from "react"
import { ChevronRightIcon, LogOutIcon, MoonIcon, PackageIcon, StoreIcon, UserRoundIcon } from "lucide-react"

import { getApiErrorMessage } from "@/lib/api/get-api-error-message"
import { getApiAccountsMe, postApiAccountsLogout, putApiAccountsMe, putApiAccountsMePassword } from "@/lib/api/generated/accounts/accounts"
import { getApiPantriesCurrentRoutine, putApiPantriesCurrentRoutine } from "@/lib/api/generated/pantries/pantries"
import type { AccountResponse, ShoppingRoutineResponse } from "@/lib/api/generated/models"
import { ThemeToggle } from "@/shared/components/layout/theme-toggle"
import { Button } from "@/shared/components/ui/button"
import { Card, CardAction, CardContent, CardDescription, CardHeader, CardTitle } from "@/shared/components/ui/card"
import { Field, FieldGroup, FieldLabel } from "@/shared/components/ui/field"
import { Input } from "@/shared/components/ui/input"
import { Spinner } from "@/shared/components/ui/spinner"
import { showErrorToast, showSuccessToast } from "@/shared/hooks/use-error-toast"

export function SettingsHome() {
  const router = useRouter()
  const [isSigningOut, setIsSigningOut] = useState(false)

  async function signOut() {
    setIsSigningOut(true)
    try {
      await postApiAccountsLogout()
      router.replace("/login")
      router.refresh()
    } catch (error) {
      showErrorToast(getApiErrorMessage(error, "Unable to sign out. Please try again."), "Sign out failed")
      setIsSigningOut(false)
    }
  }

  return <div className="mx-auto max-w-3xl space-y-6"><header><h1 className="text-2xl font-semibold tracking-tight">Settings</h1><p className="mt-1 text-sm text-muted-foreground">Keep your shopping routine simple.</p></header><div className="space-y-3"><SettingsLink href="/app/settings/account" icon={UserRoundIcon} title="Profile" description="Email address and password" /><SettingsLink href="/app/settings/routine" icon={StoreIcon} title="Shopping routine" description="Where you shop and how often" /><SettingsLink href="/app/pantry/locations" icon={PackageIcon} title="Stock areas" description="Pantry, fridge, freezer, and any custom areas" /><Card><CardHeader><div className="flex items-center gap-3"><span className="rounded-lg bg-muted p-2 text-muted-foreground"><MoonIcon className="size-4" /></span><div><CardTitle>Appearance</CardTitle><CardDescription>Choose light or dark mode.</CardDescription></div></div><CardAction><ThemeToggle /></CardAction></CardHeader></Card><Card><CardHeader><CardTitle>Account</CardTitle><CardDescription>Sign out on this device.</CardDescription><CardAction><Button variant="outline" onClick={() => void signOut()} disabled={isSigningOut}>{isSigningOut && <Spinner />}<LogOutIcon />{isSigningOut ? "Signing out…" : "Sign out"}</Button></CardAction></CardHeader></Card></div></div>
}

export function ProfileSettings() {
  const [account, setAccount] = useState<AccountResponse>()
  const [isSavingEmail, setIsSavingEmail] = useState(false)
  const [isChangingPassword, setIsChangingPassword] = useState(false)

  useEffect(() => { getApiAccountsMe().then(setAccount).catch((error) => showErrorToast(getApiErrorMessage(error, "Unable to load your profile."), "Profile unavailable")) }, [])

  async function saveEmail(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!account) return
    setIsSavingEmail(true)
    try {
      const updated = await putApiAccountsMe({ email: String(new FormData(event.currentTarget).get("email")) })
      setAccount(updated)
      showSuccessToast("Email updated")
    } catch (error) { showErrorToast(getApiErrorMessage(error, "Unable to update your email."), "Email not updated") } finally { setIsSavingEmail(false) }
  }

  async function changePassword(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const form = event.currentTarget
    const data = new FormData(form)
    const newPassword = String(data.get("newPassword"))
    if (newPassword !== String(data.get("confirmPassword"))) { showErrorToast("Your new password and confirmation must match.", "Password not changed"); return }
    setIsChangingPassword(true)
    try {
      await putApiAccountsMePassword({ currentPassword: String(data.get("currentPassword")), newPassword })
      form.reset()
      showSuccessToast("Password changed")
    } catch (error) { showErrorToast(getApiErrorMessage(error, "Unable to change your password."), "Password not changed") } finally { setIsChangingPassword(false) }
  }

  if (!account) return <SettingsLoading title="Profile" />
  return <div className="mx-auto max-w-2xl space-y-6"><PageHeader title="Profile" description="Manage your sign-in details." /><Card><CardHeader><CardTitle>Email address</CardTitle><CardDescription>This is the email you use to sign in.</CardDescription></CardHeader><CardContent><form onSubmit={saveEmail}><FieldGroup><Field><FieldLabel htmlFor="email">Email</FieldLabel><Input id="email" name="email" type="email" autoComplete="email" defaultValue={account.email} maxLength={256} required disabled={isSavingEmail} /></Field><Button type="submit" className="w-fit" disabled={isSavingEmail}>{isSavingEmail && <Spinner />}{isSavingEmail ? "Saving…" : "Save email"}</Button></FieldGroup></form></CardContent></Card><Card><CardHeader><CardTitle>Password</CardTitle><CardDescription>Use at least 6 characters, including uppercase, lowercase, a number, and a special character.</CardDescription></CardHeader><CardContent><form onSubmit={changePassword}><FieldGroup className="gap-4"><Field><FieldLabel htmlFor="currentPassword">Current password</FieldLabel><Input id="currentPassword" name="currentPassword" type="password" autoComplete="current-password" minLength={6} maxLength={100} required disabled={isChangingPassword} /></Field><Field><FieldLabel htmlFor="newPassword">New password</FieldLabel><Input id="newPassword" name="newPassword" type="password" autoComplete="new-password" minLength={6} maxLength={100} required disabled={isChangingPassword} /></Field><Field><FieldLabel htmlFor="confirmPassword">Confirm new password</FieldLabel><Input id="confirmPassword" name="confirmPassword" type="password" autoComplete="new-password" minLength={6} maxLength={100} required disabled={isChangingPassword} /></Field><Button type="submit" className="w-fit" disabled={isChangingPassword}>{isChangingPassword && <Spinner />}{isChangingPassword ? "Changing…" : "Change password"}</Button></FieldGroup></form></CardContent></Card></div>
}

export function RoutineSettings() {
  const [routine, setRoutine] = useState<ShoppingRoutineResponse>()
  const [isSaving, setIsSaving] = useState(false)
  useEffect(() => { getApiPantriesCurrentRoutine().then(setRoutine).catch((error) => showErrorToast(getApiErrorMessage(error, "Unable to load your shopping routine."), "Routine unavailable")) }, [])

  async function save(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!routine) return
    const data = new FormData(event.currentTarget)
    const interval = Number(data.get("shoppingIntervalDays"))
    if (!Number.isFinite(interval) || interval <= 0) { showErrorToast("Enter a shopping interval greater than zero.", "Routine not saved"); return }
    setIsSaving(true)
    try {
      const updated = await putApiPantriesCurrentRoutine({ primaryShopName: String(data.get("primaryShopName")).trim() || null, shoppingIntervalDays: interval, version: routine.version })
      setRoutine(updated)
      showSuccessToast("Shopping routine updated")
    } catch (error) { showErrorToast(getApiErrorMessage(error, "Unable to save your shopping routine."), "Routine not saved") } finally { setIsSaving(false) }
  }

  if (!routine) return <SettingsLoading title="Shopping routine" />
  return <div className="mx-auto max-w-2xl space-y-6"><PageHeader title="Shopping routine" description="Your list uses this interval to allow for what you use before the next shop." /><Card><CardHeader><CardTitle>Routine</CardTitle><CardDescription>Buffers stay on individual items, where their quantities make sense.</CardDescription></CardHeader><CardContent><form onSubmit={save}><FieldGroup className="gap-5"><Field><FieldLabel htmlFor="primaryShopName">Primary shop <span className="font-normal text-muted-foreground">(optional)</span></FieldLabel><Input id="primaryShopName" name="primaryShopName" maxLength={120} defaultValue={routine.primaryShopName ?? ""} placeholder="e.g. Woolworths" disabled={isSaving} /></Field><Field><FieldLabel htmlFor="shoppingIntervalDays">Shopping interval (days)</FieldLabel><Input id="shoppingIntervalDays" name="shoppingIntervalDays" type="number" inputMode="decimal" min="0.001" step="1" defaultValue={routine.shoppingIntervalDays} required disabled={isSaving} /></Field><Button type="submit" className="w-fit" disabled={isSaving}>{isSaving && <Spinner />}{isSaving ? "Saving…" : "Save routine"}</Button></FieldGroup></form></CardContent></Card></div>
}

function SettingsLink({ href, icon: Icon, title, description }: Readonly<{ href: string; icon: typeof UserRoundIcon; title: string; description: string }>) { return <Link href={href} className="block rounded-2xl focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"><Card className="gap-0 py-0 transition-colors hover:border-primary/25 hover:bg-accent/25"><CardContent className="flex items-center gap-3 p-5"><span className="rounded-lg bg-muted p-2 text-muted-foreground"><Icon className="size-4" /></span><div className="min-w-0 flex-1"><p className="font-medium">{title}</p><p className="mt-0.5 text-sm text-muted-foreground">{description}</p></div><ChevronRightIcon className="size-4 text-muted-foreground" /></CardContent></Card></Link> }
function PageHeader({ title, description }: Readonly<{ title: string; description: string }>) { return <header><Link href="/app/settings" className="text-sm text-muted-foreground hover:text-foreground">Settings</Link><h1 className="mt-2 text-2xl font-semibold tracking-tight">{title}</h1><p className="mt-1 text-sm text-muted-foreground">{description}</p></header> }
function SettingsLoading({ title }: Readonly<{ title: string }>) { return <div className="mx-auto max-w-2xl"><PageHeader title={title} description="Loading your settings…" /><div className="flex justify-center py-16"><Spinner className="size-5" /></div></div> }
