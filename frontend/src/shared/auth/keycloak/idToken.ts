import type { AuthUser } from '../types';

function decodeSegment(segment: string): string {
  const normalized = segment.replaceAll('-', '+').replaceAll('_', '/');
  const binary = atob(normalized);
  const bytes = Uint8Array.from(binary, (character) => character.codePointAt(0) ?? 0);

  return new TextDecoder().decode(bytes);
}

export function decodeIdToken(idToken: string): AuthUser {
  const segment = idToken.split('.')[1];

  if (segment === undefined) {
    return {};
  }

  try {
    const claims = JSON.parse(decodeSegment(segment)) as Record<string, unknown>;

    return {
      name: typeof claims.name === 'string' ? claims.name : undefined,
      email: typeof claims.email === 'string' ? claims.email : undefined,
      picture: typeof claims.picture === 'string' ? claims.picture : undefined,
    };
  } catch {
    return {};
  }
}
