import { Link, Outlet } from 'react-router-dom';

export function PublicLayout() {
  return (
    <div className="min-h-screen bg-[#fffaf3] text-slate-900">
      <div className="mx-auto flex min-h-screen w-full max-w-6xl flex-col px-4 py-4 sm:px-6 sm:py-6 lg:px-8">
        <header className="flex flex-col gap-3 py-2 sm:flex-row sm:items-center sm:justify-between">
          <Link
            className="w-fit rounded-md text-lg font-bold text-brand-800 focus:outline-none focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2"
            to="/"
          >
            MyRecipeBook
          </Link>
          <nav className="flex flex-wrap items-center gap-2 text-sm font-semibold" aria-label="Navegação pública">
            <Link
              className="rounded-md px-2.5 py-2 text-slate-700 transition hover:bg-white hover:text-brand-800 focus:outline-none focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2"
              to="/recipes"
            >
              Receitas
            </Link>
            <Link
              className="rounded-md px-2.5 py-2 text-slate-700 transition hover:bg-white hover:text-brand-800 focus:outline-none focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2"
              to="/login"
            >
              Entrar
            </Link>
          </nav>
        </header>
        <main className="flex flex-1">
          <Outlet />
        </main>
        <footer className="py-6 text-sm text-slate-600">Receitas em ordem, sem pressa e sem bagunça.</footer>
      </div>
    </div>
  );
}
