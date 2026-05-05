import { NavLink, Outlet } from 'react-router-dom';

const navItems = [
  { label: 'Dashboard', to: '/dashboard' },
  { end: true, label: 'Receitas', to: '/recipes' },
  { label: 'Nova receita', to: '/recipes/new' },
  { label: 'Perfil', to: '/profile' },
];

export function AuthenticatedLayout() {
  return (
    <div className="min-h-screen bg-slate-50 text-slate-900">
      <header className="sticky top-0 z-10 border-b border-slate-200 bg-white/95 backdrop-blur">
        <div className="mx-auto flex w-full max-w-7xl flex-col gap-4 px-4 py-4 sm:px-6 lg:flex-row lg:items-center lg:justify-between lg:px-8">
          <div className="min-w-0">
            <span className="block truncate text-lg font-bold text-brand-800">MyRecipeBook</span>
            <p className="text-sm text-slate-600">Área autenticada</p>
          </div>
          <nav className="flex gap-2 overflow-x-auto pb-1 lg:hidden" aria-label="Navegação principal">
            {navItems.map((item) => (
              <NavLink
                className={({ isActive }) =>
                  `whitespace-nowrap rounded-md px-3 py-2 text-sm font-semibold transition focus:outline-none focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2 ${
                    isActive ? 'bg-brand-100 text-brand-800' : 'text-slate-700 hover:bg-slate-100 hover:text-slate-950'
                  }`
                }
                key={item.to}
                end={item.end}
                to={item.to}
              >
                {item.label}
              </NavLink>
            ))}
          </nav>
        </div>
      </header>
      <div className="mx-auto grid w-full max-w-7xl gap-6 px-4 py-6 sm:px-6 sm:py-8 lg:grid-cols-[220px_1fr] lg:gap-8 lg:px-8">
        <aside className="hidden lg:block">
          <nav className="sticky top-28 space-y-1" aria-label="Navegação principal">
            {navItems.map((item) => (
              <NavLink
                className={({ isActive }) =>
                  `block rounded-md px-3 py-2 text-sm font-semibold transition focus:outline-none focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2 ${
                    isActive ? 'bg-brand-100 text-brand-800' : 'text-slate-700 hover:bg-white hover:text-slate-950'
                  }`
                }
                key={item.to}
                end={item.end}
                to={item.to}
              >
                {item.label}
              </NavLink>
            ))}
          </nav>
        </aside>
        <main className="min-w-0">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
