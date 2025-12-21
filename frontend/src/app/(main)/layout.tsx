import LeftSidePanel from "@/(main)/_components/LeftSidePanel";
import { getSession, getStorage } from "@/lib/server-api";

import './layout.css';



export default async function MainLayout({ children }: { children: React.ReactNode }) {
  const storage = await getStorage();
  const session = await getSession();
  const sessionUser = await session.getUser();

  const menuExpandedStoreKey = 'menu-expanded';
  const menuExpanded = storage.get(menuExpandedStoreKey) !== 'false';

  return (
    <div className="main-layout">
      <LeftSidePanel 
        user={sessionUser} 
        defaultExpanded={menuExpanded} 
        expandedStoreKey={menuExpandedStoreKey}>
      </LeftSidePanel>
      <div className="main-layout_content">
        {children}
      </div>
    </div>
  );
}