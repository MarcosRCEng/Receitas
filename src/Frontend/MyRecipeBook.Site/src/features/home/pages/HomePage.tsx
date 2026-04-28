import { environment } from '@shared/config/environment';

export function HomePage() {
  return (
    <main className="min-h-screen bg-[#fffaf3] text-slate-900">
      <section className="mx-auto flex min-h-screen w-full max-w-6xl flex-col justify-center px-6 py-16">
        <div className="max-w-2xl">
          <span className="text-sm font-semibold uppercase tracking-wide text-brand-700">
            {environment.appName}
          </span>
          <h1 className="mt-4 text-4xl font-bold leading-tight text-slate-950 sm:text-5xl">
            Sua base frontend esta pronta.
          </h1>
          <p className="mt-5 text-lg leading-8 text-slate-700">
            Aplicacao React, TypeScript, Vite e Tailwind criada para evoluir em modulos,
            mantendo responsabilidades separadas desde o primeiro sprint.
          </p>
          <div className="mt-8 flex flex-wrap gap-3 text-sm font-medium text-slate-700">
            <span className="rounded border border-brand-100 bg-white px-3 py-2">React</span>
            <span className="rounded border border-brand-100 bg-white px-3 py-2">TypeScript</span>
            <span className="rounded border border-brand-100 bg-white px-3 py-2">Vite</span>
            <span className="rounded border border-brand-100 bg-white px-3 py-2">Tailwind</span>
          </div>
        </div>
      </section>
    </main>
  );
}
