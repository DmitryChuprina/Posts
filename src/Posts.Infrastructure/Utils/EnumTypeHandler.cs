using Dapper;
using System.Data;

namespace Posts.Infrastructure.Utils
{
    internal class EnumTypeHandler<TEnum> : SqlMapper.TypeHandler<TEnum>
        where TEnum : struct, Enum
    {
        public override TEnum Parse(object value)
        {
            if (value is string s)
            {
                if (Enum.TryParse(s, ignoreCase: true, out TEnum result))
                {
                    return result;
                }

                throw new ArgumentException($"Cannot parse '{s}' to {typeof(TEnum).Name}");
            }

            if (value is int i)
            {
                return (TEnum)Enum.ToObject(typeof(TEnum), i);
            }

            throw new ArgumentException($"Unsupported value type '{value.GetType()}' for enum {typeof(TEnum).Name}");
        }

        public override void SetValue(IDbDataParameter parameter, TEnum value)
        {
            parameter.Value = Convert.ToInt32(value);
        }
    }
}
