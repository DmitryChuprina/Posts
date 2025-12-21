"use client"

import clsx from "clsx";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { ComponentType, SVGProps } from "react";

export interface SettingsLayoutLinkProps {
    className?: string;
    title: string;
    baseHref?: string;
    hrefs: string[];
    Icon?: ComponentType<SVGProps<SVGSVGElement>>;
}

function formatHref(href: string) {
    href = `/${href}/`
    return href.replace(/\/+/g, '/');
}

export default function SettingsLayoutLink({ title, hrefs, baseHref, Icon, className }: SettingsLayoutLinkProps) {
    baseHref = baseHref || '';

    const pathname = formatHref(usePathname());
    const formattedHrefs = hrefs
        .map(h => formatHref(`${baseHref}/${h}`));
    const isActive = formattedHrefs
        .includes(pathname);
    const canonical = formattedHrefs[0];

    return (
        <Link
            className={
                clsx(
                    "flex flex-row",
                    "text-caption text-muted",
                    "hover:text-text hover:font-bold",
                    isActive && "text-text font-bold",
                    className
                )
            }
            href={baseHref + canonical}
        >
            { Icon && <Icon className="size-5! min-w-5 mr-1"></Icon> }
            {title}
        </Link>
    )
}