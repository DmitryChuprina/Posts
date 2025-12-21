import { HTMLInputTypeAttribute } from "react";
import { useFormContext, useFormState } from "react-hook-form";
import { FormError } from "./FormError";
import clsx from "clsx";


export type FormInputProps = {
  name: string,
  label?: string,
  placeholder?: string,
  type?: Extract<HTMLInputTypeAttribute, 'text' | 'email' | 'password'>,
  required?: boolean,
  textarea?: boolean,

  className?: string,
  groupClassName?: string,
  labelClassName?: string,
  errorClassName?: string
}

export function FormInput({ name, label, required, className, type, groupClassName, errorClassName, placeholder, textarea, ...props }: FormInputProps) {
  const { register } = useFormContext();
  const { errors } = useFormState({ name: name });
  const error = errors?.[name]?.message as string | undefined;

  const controlClassName = clsx(
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
  )

  return (
    <div className={clsx("flex flex-col gap-1 w-full relative", groupClassName)}>
      {label && (
        <label
          {...(name ? { htmlFor: name } : {})}
          className="text-[.9rem] text-text-muted"
        >
          {label}
          {required && (<span className="text-danger">&nbsp;*</span>)}
        </label>
      )}
      <div className="relative w-full h-full">
        {
          textarea ?
            <textarea
              {...(name ? { id: name } : {})}
              {...props}
              placeholder={placeholder}
              className={controlClassName}
              {...register(name)}
            ></textarea> :
            <input
              {...(name ? { id: name } : {})}
              {...props}
              placeholder={placeholder}
              type={type || 'text'}
              className={controlClassName}
              {...register(name)} />
        }
        <FormError message={error} className={clsx(errorClassName, 'absolute! top-1/2 -translate-y-1/2 right-2')}></FormError>
      </div>
    </div>
  );
}
