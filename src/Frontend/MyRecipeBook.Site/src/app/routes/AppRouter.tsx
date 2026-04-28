import { createBrowserRouter, RouterProvider } from 'react-router-dom';

import { AuthenticatedLayout, PublicLayout } from '@app/layouts';
import { GoogleCallbackPage, LoginPage, RegisterPage } from '@features/auth/pages';
import { DashboardPage } from '@features/dashboard/pages';
import { HomePage } from '@features/home/pages/HomePage';
import { NewRecipePage, RecipeDetailsPage, RecipesPage } from '@features/recipes/pages';
import { ProfilePage } from '@features/user/pages';

const appRouter = createBrowserRouter([
  {
    element: <PublicLayout />,
    children: [
      {
        path: '/',
        element: <HomePage />,
      },
      {
        path: '/login',
        element: <LoginPage />,
      },
      {
        path: '/register',
        element: <RegisterPage />,
      },
      {
        path: '/auth/google/callback',
        element: <GoogleCallbackPage />,
      },
    ],
  },
  {
    element: <AuthenticatedLayout />,
    children: [
      {
        path: '/dashboard',
        element: <DashboardPage />,
      },
      {
        path: '/recipes',
        element: <RecipesPage />,
      },
      {
        path: '/recipes/new',
        element: <NewRecipePage />,
      },
      {
        path: '/recipes/:id',
        element: <RecipeDetailsPage />,
      },
      {
        path: '/profile',
        element: <ProfilePage />,
      },
    ],
  },
]);

export function AppRouter() {
  return <RouterProvider router={appRouter} />;
}
