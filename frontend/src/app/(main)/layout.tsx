import Header from "@/components/main/layout/Header";

export default function MainLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="main-layout">
      <Header />
      {children}
    </div>
  );
}