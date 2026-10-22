export interface AccessToken { token: string; expiresAt: number }
export interface TokenStore {
  get(): AccessToken | null
  set(value: AccessToken): void
  clear(): void
}
// Replace this adapter to change persistence; consumers never call browser storage directly.
export function createMemoryTokenStore(): TokenStore {
  let value: AccessToken | null = null
  return {
    get: () => value,
    set: (next) => { value = next },
    clear: () => { value = null },
  }
}
export const tokenStore: TokenStore = createMemoryTokenStore()
