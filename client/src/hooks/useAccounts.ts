import { useCallback, useEffect, useState } from 'react'
import { ApiError, getAccounts } from '../api'
import type { Account } from '../types'

export interface UseAccounts {
  accounts: Account[]
  loading: boolean
  error: string | null
  reload: () => Promise<void>
}

/** Loads the accounts on mount and exposes a `reload` to call after a mutation. */
export function useAccounts(): UseAccounts {
  const [accounts, setAccounts] = useState<Account[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const reload = useCallback(async () => {
    setLoading(true)
    try {
      setAccounts(await getAccounts())
      setError(null)
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Could not load accounts.')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    // Fetch-on-mount: synchronising with the API is what an effect is for. The
    // synchronous setLoading(true) inside reload() is intentional (also used on refetch).
    // oxlint-disable-next-line react/set-state-in-effect
    void reload()
  }, [reload])

  return { accounts, loading, error, reload }
}
