import { isInteractionRequired, unexpected } from '../errors';
import type { AuthError } from '../errors';
import { createChallenge, createState, createVerifier } from '../pkce';
import { fail } from '../result';
import type { Result } from '../result';
import { buildAuthorizeUrl, exchangeCode } from './strategy';
import type { KeycloakSession, ValidatedConfig } from './types';

const SILENT_CALLBACK_PATH = '/silent-callback.html';
const SILENT_CALLBACK_URI = `${window.location.origin}${SILENT_CALLBACK_PATH}`;

const SILENT_TIMEOUT_MS = 5_000;
const SILENT_MESSAGE_SOURCE = 'inmarket.silent-callback';

function readSilentMessage(data: unknown): string | null {
  if (typeof data !== 'object' || data === null) {
    return null;
  }

  const message = data as Record<string, unknown>;

  return message.source === SILENT_MESSAGE_SOURCE && typeof message.search === 'string'
    ? message.search
    : null;
}

function waitForSilentCallback(url: string): Promise<URLSearchParams | null> {
  return new Promise((resolve) => {
    const frame = document.createElement('iframe');
    frame.style.display = 'none';
    frame.title = 'Silent sign-in';

    let timer = 0;

    function finish(result: URLSearchParams | null): void {
      window.clearTimeout(timer);
      window.removeEventListener('message', onMessage);
      frame.remove();
      resolve(result);
    }

    function onMessage(event: MessageEvent): void {
      if (event.origin !== window.location.origin || event.source !== frame.contentWindow) {
        return;
      }

      const search = readSilentMessage(event.data);

      if (search !== null) {
        finish(new URLSearchParams(search));
      }
    }

    timer = window.setTimeout(() => finish(null), SILENT_TIMEOUT_MS);
    window.addEventListener('message', onMessage);
    frame.src = url;
    document.body.append(frame);
  });
}

export async function restoreSession(
  config: ValidatedConfig,
): Promise<Result<KeycloakSession, AuthError>> {
  let verifier: string;
  let state: string;
  let url: string;

  try {
    verifier = createVerifier();
    state = createState();
    const challenge = await createChallenge(verifier);
    url = buildAuthorizeUrl({ config, redirectUri: SILENT_CALLBACK_URI, state, challenge, prompt: 'none' });
  } catch (err: unknown) {
    return fail(unexpected(err));
  }

  const params = await waitForSilentCallback(url);

  if (params === null) {
    return fail({ kind: 'silent-unavailable', reason: 'timeout' });
  }

  const error = params.get('error');

  if (error !== null) {
    return fail(
      isInteractionRequired(error)
        ? { kind: 'silent-unavailable', reason: error }
        : { kind: 'provider-refused', code: error },
    );
  }

  if (params.get('state') !== state) {
    return fail({ kind: 'state-mismatch' });
  }

  const code = params.get('code');

  if (code === null) {
    return fail({ kind: 'missing-code' });
  }

  return exchangeCode(config, code, verifier, SILENT_CALLBACK_URI);
}
