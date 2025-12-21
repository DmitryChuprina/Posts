import clsx from "clsx";
import { useEffect } from "react";
import { UseFormReturn } from "react-hook-form";

interface FormErrorAlertProps {
  message?: string | null;
  setMessage?: (msg: string | null) => void,
  className?: string;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  form?: UseFormReturn<any>
}

export function FormApiErrorAlert({ message, setMessage, className, form }: FormErrorAlertProps) {
  useEffect(() => {
    if (!form || !message || !setMessage) {
      return;
    }
    const subscription = form.watch(() => {
      setMessage(null);
    });
    return () => subscription.unsubscribe();
  }, [form, message, setMessage])

  if (!message) {
    return null;
  }

  return (
    <div
      className={clsx(
        "flex items-center gap-3 w-full p-3 border",
        "bg-surface",
        "border-danger/30",
        "text-danger",
        "rounded",
        className
      )}
    >
      <svg
        xmlns="http://www.w3.org/2000/svg"
        viewBox="0 0 20 20"
        fill="currentColor"
        className="w-5 h-5 shrink-0 text-danger"
      >
        <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
      </svg>

      <span className="text-sm font-medium">
        {message}
      </span>
    </div>
  );
}