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
            <div className="error-icon">!</div>
            <div className="error-message">{message}</div>
        </div>
    );
}