import { createBrowserRouter } from 'react-router-dom';

import { HomePage } from '@features/home/pages/HomePage';

export const appRouter = createBrowserRouter([
  {
    path: '/',
    element: <HomePage />,
  },
]);
