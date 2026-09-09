import type { Account, TransferResult } from './types'

interface ProblemDetails {
  title?: unknown
  detail?: unknown
}

/** A non-2xx response from the API, carrying the RFC 7807 title and detail. */
export class ApiError extends Error {
  readonly status: number
  readonly title: string
  readonly detail: string | null

  constructor(status: number, title: string, detail: string | null) {
    super(detail ?? title)
    this.name = 'ApiError'
    this.status = status
    this.title = title
    this.detail = detail
  }
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(path, {
    ...init,
    headers: { 'Content-Type': 'application/json', ...init?.headers },
  })

  if (!response.ok) {
    throw await readError(response)
  }

  return (await response.json()) as T
}

async function readError(response: Response): Promise<ApiError> {
  try {
    const problem = (await response.json()) as ProblemDetails
    const title = typeof problem.title === 'string' ? problem.title : response.statusText
    const detail = typeof problem.detail === 'string' ? problem.detail : null
    return new ApiError(response.status, title, detail)
  } catch {
    return new ApiError(response.status, response.statusText, null)
  }
}

export function getAccounts(): Promise<Account[]> {
  return request<Account[]>('/accounts')
}

export function deposit(accountId: string, amount: number): Promise<Account> {
  return request<Account>(`/accounts/${accountId}/deposit`, {
    method: 'POST',
    body: JSON.stringify({ amount }),
  })
}

export function withdraw(accountId: string, amount: number): Promise<Account> {
  return request<Account>(`/accounts/${accountId}/withdraw`, {
    method: 'POST',
    body: JSON.stringify({ amount }),
  })
}

export function transfer(
  fromAccountId: string,
  toAccountId: string,
  amount: number,
): Promise<TransferResult> {
  return request<TransferResult>('/transfers', {
    method: 'POST',
    body: JSON.stringify({ fromAccountId, toAccountId, amount }),
  })
}
