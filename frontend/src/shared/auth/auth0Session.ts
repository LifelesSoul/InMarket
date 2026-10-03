import { GenericError, type Auth0ContextInterface } from '@auth0/auth0-react';
import { errorMessage, isInteractionRequired } from './errors';
import type { AuthError } from './errors';
import { fail, ok, type Result } from './result';
import type { ProviderSession } from './session';
import { toAuthUser } from './user';

export function createAuth0Session(
    auth0: Auth0ContextInterface,
    onExpired: () => void,
): Result<ProviderSession, AuthError> {
    const user = toAuthUser(auth0.user ?? {});

    if (!user.ok) {
        return user;
    }

    return ok({
        provider: 'Auth0',
        user: user.value,
        async getAccessToken() {
            try {
                return ok(await auth0.getAccessTokenSilently());
            } catch (err: unknown) {
                if (err instanceof GenericError && isInteractionRequired(err.error)) {
                    onExpired();
                    return fail({ kind: 'session-expired' });
                }

                return fail({ kind: 'provider-sdk', message: errorMessage(err) });
            }
        },
        logout() {
            auth0.logout({ logoutParams: { returnTo: window.location.origin } });
        },
    });
}
