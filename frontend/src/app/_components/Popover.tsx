"use client";

import clsx from "clsx";
import { useState } from "react";
import { NoSSR } from "./NoSsr";

import './Popover.css';

interface PopoverProps {
    children: React.ReactNode,
    popover: React.ReactNode | string,
    className?: string,
    popoverClassName?: string,
    trigger?: 'click' | 'hover',
    position?: 'top' | 'bottom' | 'left' | 'right' | 'left-bottom' | 'right-bottom';
}

export default function Popover({ children, popover, position = 'left-bottom', trigger = 'hover', className, popoverClassName }: PopoverProps) {
    const [showed, setShowed] = useState(false);
    const handleMouseEnter = () => {
        if (trigger !== 'hover') {
            return;
        }

        setShowed(true);
    };

    const handleMouseLeave = () => {
        if (trigger !== 'hover') {
            return;
        }

        setShowed(false);
    };

    const handleClick = () => {
        if (trigger !== 'click') {
            return;
        }
        setShowed(!showed);
    };

    return (
        <div
            className={clsx("popover-container", className)}
            onMouseEnter={handleMouseEnter}
            onMouseLeave={handleMouseLeave}
        >
            <NoSSR>
                {
                    showed && trigger === 'click' && (
                        <div className="popover-backdrop" onClick={handleClick}></div>
                    )
                }
                <div
                    className={clsx(
                        "popover",
                        `popover_${position}`,
                        popoverClassName,
                        showed ? 'showed' : ''
                    )}
                >
                    {
                        typeof popover === 'string' ?
                            (<span>{popover}</span>) :
                            (popover)
                    }
                </div>
            </NoSSR>
            <div
                className="popover-container"
                onClick={handleClick}
            >
                {children}
            </div>
        </div>
    );
}