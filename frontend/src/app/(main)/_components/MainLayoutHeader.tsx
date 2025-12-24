import clsx from "clsx";
import { ReactNode, useEffect, useRef, useState } from "react";

export interface MainLayoutHeaderProps {
    children?: ReactNode,
    className?: string
}

export function MainLayoutHeader({ children, className }: MainLayoutHeaderProps) {
    const [isVisible, setIsVisible] = useState(true);
    const headerRef = useRef<HTMLElement>(null);
    const lastScrollTop = useRef(0);

    useEffect(() => {
        const element = headerRef.current;
        if (!element) {
            return;
        }

        let scrollContainer: HTMLElement | Window | null = null;

        const getScrollParent = (node: HTMLElement | null): HTMLElement | Window => {
            if (!node) {
                return window;
            }

            const overflowY = window.getComputedStyle(node).overflowY;
            const isScrollable = overflowY !== 'visible' && overflowY !== 'hidden';

            if (isScrollable && node.scrollHeight > node.clientHeight) {
                return node;
            }

            if (node.tagName === 'BODY' || node.tagName === 'HTML') {
                return window;
            }

            return getScrollParent(node.parentElement);
        };

        const handleScroll = () => {
            if (!scrollContainer) {
                setIsVisible(true);
                return;
            }

            const currentScrollTop = scrollContainer === window
                ? window.scrollY
                : (scrollContainer as HTMLElement).scrollTop;

            if (currentScrollTop < 0) {
                return;
            }

            const isScrollingUp = currentScrollTop < lastScrollTop.current;
            const diff = Math.abs(currentScrollTop - lastScrollTop.current);

            if (diff > 5) {
                const isAtTop = currentScrollTop < 60;
                const visible = isScrollingUp || isAtTop;
                setIsVisible(visible);
                lastScrollTop.current = currentScrollTop;
            }
        };

        const subscribeToHandleScroll = () => {
            if (scrollContainer) {
                scrollContainer.removeEventListener("scroll", handleScroll);
            }
            scrollContainer = getScrollParent(headerRef.current);
            scrollContainer.addEventListener("scroll", handleScroll);
        }

        const handleResize = () => {
            subscribeToHandleScroll();
            handleScroll();
        }

        window.addEventListener('resize', handleResize);
        subscribeToHandleScroll();

        return () => {
            window.removeEventListener('resize', handleResize);
            if (scrollContainer) {
                scrollContainer.removeEventListener("scroll", handleScroll);
            }
        };
    }, []);

    return (
        <aside
            ref={headerRef}
            className={
                clsx(
                    "sticky top-0 left-0 z-110 w-full min-h-[45px]",
                    "bg-surface/80 backdrop-blur-md shadow-sm border-b border-border/10",
                    "transition-transform duration-300 ease-in-out will-change-transform",
                    !isVisible && "-translate-y-full",
                    className
                )
            }
        >
            {children}
        </aside>
    )
}