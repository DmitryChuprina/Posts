import clsx from "clsx";
import './AuthError.css';

interface AuthErrorProps {
    message?: string;
    className?: string;
}

export function AuthError({ message, className }: AuthErrorProps) {
    if (!message) return null;

    return (
        <div className={clsx("error-group", className)}>
            <svg
                xmlns="http://www.w3.org/2000/svg"
                viewBox="0 0 20 20"
                fill="currentColor"
                className="w-5 h-5 shrink-0 error-icon"
            >
                <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
            </svg>
            <div className="error-message">{message}</div>
        </div>
    );
}