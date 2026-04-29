import { md5 } from 'js-md5'

export function gravatarUrl(email: string | undefined, size = 80): string {
  if (!email) return `https://www.gravatar.com/avatar/?s=${size}&d=mp`
  const hash = md5(email.trim().toLowerCase())
  return `https://www.gravatar.com/avatar/${hash}?s=${size}&d=mp`
}
