import { Outlet } from 'react-router-dom';

export function PublicLayout() {
  return (
    <div className="min-h-screen bg-[#fffaf3] text-slate-900">
      <Outlet />
    </div>
  );
}
