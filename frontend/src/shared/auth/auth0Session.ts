import type { Auth0ContextInterface } from '@auth0/auth0-react';
import type { AuthError } from './errors';
import { ok, type Result } from './result';
import type { ProviderSession } from './session';
import { toAuthUser } from './user';

export function createAuth0Session(auth0: Auth0ContextInterface): Result<ProviderSession, AuthError> {
    const user = toAuthUser(auth0.user);

    if (!user.ok) {
        return user;
    }

    return ok({
        provider: 'Auth0',
        user: user.value,
        async getAccessToken() {
            try {
                return await auth0.getAccessTokenSilently();
            }   catch {
                return null;
            }
        },
        logout() {
            auth0.logout({ logoutParams: { returnTo: window.location.origin } });
        },
    });
}
