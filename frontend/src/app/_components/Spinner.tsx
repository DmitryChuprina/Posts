import clsx from "clsx";

export default function Spinner({ className }: { className?: string }){
    return (
        <div className={
            clsx(
                "size-4 rounded-full border-2 border-t-white animate-spin",
                className
            )
        }/>
    );
}