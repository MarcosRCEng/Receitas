import { Route, Routes } from 'react-router-dom';

import { PublicLayout } from '@app/layouts';
import { LoginPage } from '@features/auth/pages';
import { HomePage } from '@features/home/pages/HomePage';
import { RecipesPage } from '@features/recipes/pages';

export function AppRoutes() {
  return (
    <Routes>
      <Route element={<PublicLayout />}>
        <Route path="/" element={<HomePage />} />
        <Route path="/recipes" element={<RecipesPage />} />
        <Route path="/login" element={<LoginPage />} />
      </Route>
    </Routes>
  );
}
