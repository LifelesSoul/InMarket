import React from 'react'
import ReactDOM from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import App from './App.tsx'
import { Auth0Provider, type AppState } from '@auth0/auth0-react'
import { AuthProvider } from './shared/auth/AuthProvider'
import { MyProfileProvider } from './features/Profile/MyProfileProvider'
import { CALLBACK_PATH } from './shared/auth/keycloak'

const domain = import.meta.env.VITE_AUTH0_DOMAIN;
const clientId = import.meta.env.VITE_AUTH0_CLIENT_ID;
const audience = import.meta.env.VITE_AUTH0_AUDIENCE;

function restorePath(appState?: AppState) {
  window.history.replaceState(null, '', appState?.returnTo ?? window.location.pathname);
  window.dispatchEvent(new PopStateEvent('popstate'));
}

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <Auth0Provider
      domain={domain}
      clientId={clientId}
      skipRedirectCallback={window.location.pathname === CALLBACK_PATH}
      onRedirectCallback={restorePath}
      authorizationParams={{
        redirect_uri: window.location.origin,
        audience: audience,
        scope: 'openid profile email'
      }}
    >
      <BrowserRouter>
        <AuthProvider>
          <MyProfileProvider>
            <App />
          </MyProfileProvider>
        </AuthProvider>
      </BrowserRouter>
    </Auth0Provider>
  </React.StrictMode>,
)
