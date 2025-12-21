import { ComponentType, SVGProps } from "react";
import { usePathname } from "next/navigation";
import Popover from "../../_components/Popover";
import Link from "next/link";
import clsx from "clsx";

import './LeftSidePanelLink.css'

export interface LeftSidePanelLinkProps {
    className?: string;
    href: string;
    title: string;
    expanded: boolean;
    Icon: ComponentType<SVGProps<SVGSVGElement>>;
}

export default function LeftSidePanelLink({ className, href, title, expanded, Icon }: LeftSidePanelLinkProps) {
    const pathname = usePathname();

    return (
        <Popover
            popover={title}
            position="right"
            popoverClassName="ml-0!"
            className={
                clsx(
                    "left-panel_link-container",
                    className,
                    pathname === href && 'active',
                    expanded && 'expanded'
                )
            }>
            <Link
                href={href}
                className="left-panel_link">
                <Icon className="left-panel_link-icon"></Icon>
                <span className="left-panel_link-title">{title}</span>
            </Link>
        </Popover>
    )
}