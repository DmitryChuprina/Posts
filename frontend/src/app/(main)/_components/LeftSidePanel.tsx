"use client";

import Link from "next/link";
import Image from "next/image";
import clsx from "clsx";
import Popover from "../../_components/Popover";
import LeftSidePanelLink from "./LeftSidePanelLink";
import { ISessionUser } from "@/lib/stores/session";
import { ComponentType, SVGProps, useMemo, useState } from "react";
import { clientStorage } from "@/lib/client-api";
import { NoSSR } from "@/_components/NoSsr";
import { signOutAction } from "@/lib/actions/auth";
import ProfileIcon from "../../_components/ProfileIcon";

import ArrowIcon from "@/public/arrow.svg";
import ProfileIconSvg from "@/public/profile.svg";

import SignInIcon from "@/public/sign-in.svg";
import HomeIcon from "@/public/menu/home.svg";
import SearchIcon from "@/public/menu/search.svg";
import ChatIcon from "@/public/menu/chat.svg";

import './LeftSidePanel.css';

interface IMenuItem {
    Icon: ComponentType<SVGProps<SVGSVGElement>>;
    title: string;
    href: string;
}

interface LeftSidePanelProps {
    user: ISessionUser | null
    defaultExpanded: boolean;
    expandedStoreKey: string
}

export const getMenu = (user?: { username: string } | null): IMenuItem[] => {
    const baseMenu = [
        { title: 'Home', href: '/', Icon: HomeIcon },
        { title: 'Search', href: '/search', Icon: SearchIcon },
    ];

    if (!user) {
        return baseMenu;
    }

    return [
        ...baseMenu,
        { title: 'Chat', href: '/chat', Icon: ChatIcon },
        { title: 'Profile', href: `/profile/${user.username}`, Icon: ProfileIconSvg },
    ];
};

export default function LeftSidePanel({ user, defaultExpanded, expandedStoreKey }: LeftSidePanelProps) {
    const menu = useMemo(() => getMenu(user), [user]);
    const [expanded, setExpanded] = useState(!!defaultExpanded)

    const fullname = user && [user?.firstName, user?.lastName]
        .map(part => part && part.trim())
        .filter(part => !!part)
        .join(' ');

    const handleMenuExpandedChange = (val: boolean) => {
        setExpanded(val);
        clientStorage.set(expandedStoreKey, `${val}`)
    }

    return (
        <div className="left-panel-container">
            <div
                className={
                    clsx(
                        "left-panel",
                        expanded && 'expanded'
                    )
                }>
                <div className="left-panel_header">
                    <Link className="left-panel_header-logo" href="/">
                        <Image src="/logo.svg" alt="Posts" width={50} height={50}></Image>
                    </Link>
                    <NoSSR>
                        <button className="left-panel_header-toogle" onClick={() => handleMenuExpandedChange(!expanded)}>
                            <ArrowIcon className="icon"></ArrowIcon>
                        </button>
                    </NoSSR>
                </div>
                <nav>
                    <ul>
                        {
                            menu.map((b, idx) => (
                                <li key={idx} className="w-full">
                                    <LeftSidePanelLink
                                        {...b}
                                        expanded={expanded}
                                    ></LeftSidePanelLink>
                                </li>
                            ))
                        }
                    </ul>
                </nav>
                <div className="left-panel_profile">
                    {
                        user ?
                            (
                                <Popover
                                    trigger="click"
                                    position="right-bottom"
                                    popover={(
                                        <div className="flex flex-col w-[200px] gap-2">
                                            <Link className="btn btn-outline" href="/settings">Settings</Link>
                                            <button className="btn btn-primary" onClick={signOutAction}>Sign out</button>
                                        </div>
                                    )}
                                    className="left-panel_profile-details"
                                >
                                    <div className="max-w-full flex flex-row overflow-hidden">
                                        <ProfileIcon file={user.profileImage} className="size-10 min-w-10"></ProfileIcon>
                                        <div className="left-panel_profile-details-titles">
                                            <span className="left-panel_profile-details-fullname">{fullname || '...'}</span>
                                            <span className="left-panel_profile-details-username">{user.username}</span>
                                        </div>
                                    </div>
                                </Popover>

                            ) :
                            (
                                <div className="mb-2">
                                    <LeftSidePanelLink
                                        title="Sign In"
                                        href="/sign-in"
                                        Icon={SignInIcon}
                                        expanded={expanded}
                                    ></LeftSidePanelLink>
                                </div>
                            )
                    }
                </div>
            </div>
        </div>
    )
}