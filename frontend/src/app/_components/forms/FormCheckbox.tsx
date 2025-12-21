import clsx from "clsx";
import { FieldValues, RegisterOptions, useFormContext } from "react-hook-form";

type FormCheckboxProps = {
  name: string;
  label?: string;
  opts?: RegisterOptions<FieldValues, string>;

  className?: string;
  labelClassName?: string;
};

export function FormCheckbox({ name, label, className, labelClassName, opts, ...props }: FormCheckboxProps){
  const { register } = useFormContext();

  return (
    <label className="checkbox flex items-center gap-1 cursor-pointer select-none">
      <input
        type="checkbox"
        className={
          clsx(
            "checkbox-input",
            className
          )
        }
        {...props}
        {...register(name, opts)}
      />

      <span className="checkbox-box"></span>

      {label && (
        <span
          className={
            clsx(
              "checkbox-label text-caption text-text-muted",
              labelClassName
            )
          }>
          {label}
        </span>
      )}
    </label>
  );
}
