import type { Auth0ContextInterface } from '@auth0/auth0-react';
import type { ProviderSession } from './session';

export function createAuth0Session(auth0: Auth0ContextInterface): ProviderSession {
    return {
        provider: 'Auth0',
        user: {
            name: auth0.user?.name,
            email: auth0.user?.email,
            picture: auth0.user?.picture,
        },
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
    };
}
