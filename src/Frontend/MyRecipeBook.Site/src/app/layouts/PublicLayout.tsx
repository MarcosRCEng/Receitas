import { Outlet } from 'react-router-dom';

export function PublicLayout() {
  return (
    <div className="min-h-screen bg-[#fffaf3] text-slate-900">
      <div className="mx-auto flex min-h-screen w-full max-w-6xl flex-col px-4 py-6 sm:px-6 lg:px-8">
        <header className="flex items-center justify-between py-2">
          <span className="text-lg font-bold text-brand-700">MyRecipeBook</span>
          <span className="hidden text-sm font-medium text-slate-500 sm:inline">Receitas em ordem</span>
        </header>
        <div className="flex flex-1">
          <Outlet />
        </div>
      </div>
    </div>
  );
}
