import { Link } from 'react-router-dom';

import { FeatureCard } from '../components/FeatureCard';

const features = [
  {
    accent: '01',
    title: 'Organize suas receitas',
    description: 'Mantenha suas receitas favoritas salvas e fáceis de encontrar.',
  },
  {
    accent: '02',
    title: 'Busque por ingredientes',
    description: 'Encontre ideias com base no que você já tem em casa.',
  },
  {
    accent: '03',
    title: 'Salve favoritas',
    description: 'Marque receitas importantes para acessar rapidamente.',
  },
  {
    accent: '04',
    title: 'Pronto para IA',
    description: 'Estrutura preparada para futuras sugestões inteligentes.',
  },
];

const primaryLinkClasses =
  'inline-flex min-h-11 items-center justify-center rounded-md bg-brand-600 px-5 py-3 text-sm font-semibold text-white shadow-sm transition hover:bg-brand-700 focus:outline-none focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2';

const secondaryLinkClasses =
  'inline-flex min-h-11 items-center justify-center rounded-md border border-slate-200 bg-white px-5 py-3 text-sm font-semibold text-slate-800 shadow-sm transition hover:border-brand-200 hover:text-brand-700 focus:outline-none focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2';

export function HomePage() {
  return (
    <main className="w-full space-y-20 pb-10 pt-8 sm:space-y-24 sm:pb-14 sm:pt-12">
      <section className="grid items-center gap-10 lg:grid-cols-[1.05fr_0.95fr]">
        <div className="max-w-2xl">
          <span className="text-sm font-semibold uppercase tracking-wide text-brand-700">MYRECIPEBOOK</span>
          <h1 className="mt-4 text-4xl font-bold leading-tight text-slate-950 sm:text-5xl lg:text-6xl">
            Organize suas receitas em um só lugar.
          </h1>
          <p className="mt-5 text-lg leading-8 text-slate-700">
            Cadastre, encontre e planeje suas receitas favoritas com uma experiência simples, rápida e preparada para
            evoluir com inteligência artificial.
          </p>
          <div className="mt-8 flex flex-col gap-3 sm:flex-row">
            <Link className={primaryLinkClasses} to="/recipes">
              Explorar receitas
            </Link>
            <Link className={secondaryLinkClasses} to="/register">
              Criar minha conta
            </Link>
          </div>
        </div>

        <div className="rounded-lg border border-orange-100 bg-white p-4 shadow-xl shadow-orange-100/70 sm:p-5">
          <div className="rounded-md bg-brand-50 p-4 sm:p-5">
            <div className="flex items-center justify-between gap-4 border-b border-orange-100 pb-4">
              <div>
                <p className="text-sm font-semibold uppercase text-brand-700">Planejamento</p>
                <h2 className="mt-1 text-2xl font-bold text-slate-950">Semana saborosa</h2>
              </div>
              <span className="shrink-0 rounded-md bg-white px-3 py-2 text-sm font-semibold text-brand-700 shadow-sm">
                12 receitas
              </span>
            </div>
            <div className="mt-5 space-y-3">
              {['Risoto de legumes', 'Frango ao molho', 'Bolo de cenoura'].map((recipe, index) => (
                <div key={recipe} className="flex items-center justify-between rounded-md bg-white px-4 py-3 shadow-sm">
                  <div>
                    <span className="text-sm font-semibold text-slate-800">{recipe}</span>
                    <p className="mt-1 text-xs text-slate-500">Receita favorita</p>
                  </div>
                  <span className="flex h-8 w-8 items-center justify-center rounded-md bg-orange-100 text-sm font-bold text-brand-700">
                    {index + 1}
                  </span>
                </div>
              ))}
            </div>
            <div className="mt-5 grid grid-cols-2 gap-3">
              <div className="rounded-md bg-white p-4 shadow-sm">
                <p className="text-2xl font-bold text-slate-950">28</p>
                <p className="mt-1 text-sm text-slate-500">ingredientes</p>
              </div>
              <div className="rounded-md bg-white p-4 shadow-sm">
                <p className="text-2xl font-bold text-slate-950">4</p>
                <p className="mt-1 text-sm text-slate-500">listas salvas</p>
              </div>
            </div>
          </div>
        </div>
      </section>

      <section>
        <div className="max-w-2xl">
          <span className="text-sm font-semibold uppercase tracking-wide text-brand-700">Benefícios</span>
          <h2 className="mt-3 text-3xl font-bold text-slate-950 sm:text-4xl">Tudo para cozinhar com menos atrito.</h2>
        </div>
        <div className="mt-8 grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {features.map((feature) => (
            <FeatureCard
              key={feature.title}
              accent={feature.accent}
              description={feature.description}
              title={feature.title}
            />
          ))}
        </div>
      </section>

      <section className="rounded-lg bg-slate-950 px-6 py-10 text-white shadow-xl shadow-slate-200 sm:px-10">
        <div className="flex flex-col gap-6 lg:flex-row lg:items-center lg:justify-between">
          <div className="max-w-2xl">
            <h2 className="text-3xl font-bold">Sua cozinha mais organizada.</h2>
            <p className="mt-3 text-base leading-7 text-slate-300">
              Comece com suas receitas favoritas e evolua para uma rotina mais simples de planejamento.
            </p>
          </div>
          <Link className={`${primaryLinkClasses} bg-white text-slate-950 hover:bg-brand-50`} to="/register">
            Começar agora
          </Link>
        </div>
      </section>
    </main>
  );
}
