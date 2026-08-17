import { BrowserRouter } from 'react-router-dom';

import { AppProviders } from './providers';
import { AppRoutes } from '../routes/AppRoutes';

export function App() {
  return (
    <BrowserRouter>
      <AppProviders>
        <AppRoutes />
      </AppProviders>
    </BrowserRouter>
  );
}
