"use client";

import SettingsLayoutLink, { SettingsLayoutLinkProps } from "./_components/SettingsLayoutLink";

import ProfileIcon from "@/public/profile.svg";
import LockIcon from "@/public/lock.svg";

export default function SettingsLayout({ children }: { children: React.ReactNode }) {
    const menu: SettingsLayoutLinkProps[] = [
        { title: 'Profile', hrefs: ['/', '/profile'], Icon: ProfileIcon },
        { title: 'Security', hrefs: ['/security'], Icon: LockIcon }
    ];

    return (
        <div className="items-start flex flex-row w-full justify-center">
            <div className="flex-1 md:min-w-md max-w-2xl pb-4">
                <div className=" border-border sm:border-x border-b bg-surface w-full">
                    {children}
                </div>
            </div>
            <div className="hidden sm:block sticky pl-5 pt-6 w-[100px]">
                <nav>
                    <ul>
                        {
                            menu.map((item, idx) => (
                                <li key={idx}>
                                    <SettingsLayoutLink 
                                        {...item}
                                        baseHref="/settings"
                                        className="mb-1"
                                    ></SettingsLayoutLink>
                                </li>
                            ))
                        }
                    </ul>
                </nav>
            </div>
        </div>
    )
}