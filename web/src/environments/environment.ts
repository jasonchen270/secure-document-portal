declare global {
  interface Window { __API_URL__?: string; }
}

export const environment = {
  production: false,
  apiUrl: (typeof window !== 'undefined' && window.__API_URL__) || '/api'
};
