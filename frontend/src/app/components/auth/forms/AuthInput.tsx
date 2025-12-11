import { HTMLInputTypeAttribute } from "react";
import { useFormContext, useFormState } from "react-hook-form";
import { AuthError } from "./AuthError";
import clsx from "clsx";


export type AuthInputProps = {
  name: string,
  label?: string,
  placeholder?: string,
  type?: Extract<HTMLInputTypeAttribute, 'text' | 'email' | 'password'>,
  required?: boolean,

  className?: string,
  groupClassName?: string,
  labelClassName?: string,
  errorClassName?: string
}

export function AuthInput({ name, label, required, className, type, groupClassName, errorClassName, placeholder, ...props }: AuthInputProps) {
  const { register } = useFormContext();
  const { errors } = useFormState({ name: name });
  const error = errors?.[name]?.message as string | undefined;

  return (
    <div className={clsx("flex flex-col gap-1 w-full relative", groupClassName)}>
      {label && (
        <label
          {...(name ? { htmlFor: name } : {})}
          className="text-[.9rem] text-text-muted"
        >
          {label}
          { required && (<span className="text-danger">&nbsp;*</span>)  }
        </label>
      )}
      <div className="relative w-full h-full">
        <input
          {...(name ? { id: name } : {})}
          {...props}
          placeholder={placeholder}
          type={type || 'text'}
          className={clsx(
            "w-full",
            "input input-md",
            "rounded",
            "bg-surface border",
            error ? 'border-danger/30' : 'border-muted/30',
            "text-text placeholder:text-muted",
            "focus:outline-none focus:ring-2",
            error ? 'focus:ring-danger-hover' : 'focus:ring-primary-hover',
            "transition-all duration-fast",
            className
          )}
          {...register(name)} />
          <AuthError message={error} className={clsx(errorClassName, 'absolute! top-1/2 -translate-y-1/2 right-2')}></AuthError>
      </div>
    </div>
  );
}
