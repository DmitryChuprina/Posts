export default function AuthLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="min-h-screen w-full flex items-center justify-center bg-bg text-text px-8">
      <div
        className="
          w-full max-w-md
          bg-surface
          border border-muted/30
          rounded-[var(--radius)]
          p-4
          shadow-[0_4px_16px_rgba(0,0,0,0.06)]
        "
      >
        {children}
      </div>
    </div>
  );
}