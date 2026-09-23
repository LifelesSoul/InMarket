export interface PendingLogin {
  verifier: string;
  state: string;
  returnTo: string;
}

const PENDING_KEY = 'inmarket.auth.keycloak.pending';

function isPendingLogin(value: unknown): value is PendingLogin {
  if (typeof value !== 'object' || value === null) {
    return false;
  }

  const candidate = value as Record<string, unknown>;

  return (
    typeof candidate.verifier === 'string' &&
    typeof candidate.state === 'string' &&
    typeof candidate.returnTo === 'string'
  );
}

export function savePending(pending: PendingLogin): void {
  sessionStorage.setItem(PENDING_KEY, JSON.stringify(pending));
}

function readPending(): PendingLogin | null {
  try {
    const raw = sessionStorage.getItem(PENDING_KEY);

    if (raw === null) {
      return null;
    }

    const parsed: unknown = JSON.parse(raw);

    return isPendingLogin(parsed) ? parsed : null;
  } catch {
    return null;
  }
}

function clearPending(): void {
  try {
    sessionStorage.removeItem(PENDING_KEY);
  } catch {
    return;
  }
}

export function takePending(): PendingLogin | null {
  const pending = readPending();
  clearPending();

  return pending;
}
