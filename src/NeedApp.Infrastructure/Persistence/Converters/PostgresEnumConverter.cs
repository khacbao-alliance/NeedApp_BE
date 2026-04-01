using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace NeedApp.Infrastructure.Persistence.Converters;

internal static class PostgresEnumConverter
{
    public static ValueConverter<TEnum, string> ForEnum<TEnum>() where TEnum : struct, Enum
        => new(
            v => ToSnakeCase(v.ToString()),
            v => FromSnakeCase<TEnum>(v)
        );

    public static ValueConverter<TEnum?, string?> ForNullableEnum<TEnum>() where TEnum : struct, Enum
        => new(
            v => v.HasValue ? ToSnakeCase(v.Value.ToString()) : null,
            v => v != null ? FromSnakeCase<TEnum>(v) : (TEnum?)null
        );

    private static string ToSnakeCase(string value)
        => Regex.Replace(value, "([A-Z])", "_$1").TrimStart('_').ToLower();

    private static TEnum FromSnakeCase<TEnum>(string value) where TEnum : struct, Enum
        => (TEnum)Enum.Parse(typeof(TEnum), value.Replace("_", ""), ignoreCase: true);
}
