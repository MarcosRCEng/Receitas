import { Link, NavLink, Outlet } from 'react-router-dom';

const navLinkClasses = ({ isActive }: { isActive: boolean }) =>
  `rounded-md px-2.5 py-2 text-slate-700 transition hover:bg-white hover:text-brand-800 focus:outline-none focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2 ${
    isActive ? 'bg-white text-brand-800 shadow-sm' : ''
  }`;

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
          <nav className="flex flex-wrap items-center gap-2 text-sm font-semibold" aria-label="Navegacao publica">
            <NavLink className={navLinkClasses} to="/" end>
              Início
            </NavLink>
            <NavLink className={navLinkClasses} to="/recipes">
              Receitas
            </NavLink>
            <NavLink className={navLinkClasses} to="/login">
              Entrar
            </NavLink>
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
