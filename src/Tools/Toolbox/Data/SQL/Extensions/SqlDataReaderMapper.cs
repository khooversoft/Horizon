using System.Reflection;
using Microsoft.Data.SqlClient;

namespace Toolbox.Data;

public static class SqlDataReaderMapper
{
    public static List<T> MapToList<T>(this SqlDataReader reader) where T : class, new()
    {
        var result = new List<T>();
        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                             .Where(p => p.CanWrite)
                             .ToArray();

        // Build a map from column name -> ordinal
        var ordinals = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < reader.FieldCount; i++) ordinals[reader.GetName(i)] = i;

        // Pre-compute property setters aligned to column ordinals (when present)
        var propertyBindings = props
            .Select(p => new
            {
                Property = p,
                Ordinal = ordinals.TryGetValue(GetTargetColumnName(p), out var o) ? o : (int?)null
            })
            .Where(x => x.Ordinal.HasValue)
            .ToArray();

        while (reader.Read())
        {
            var item = new T();
            foreach (var pb in propertyBindings)
            {
                try
                {
                    var ordinal = pb.Ordinal!.Value;
                    object? value = reader.IsDBNull(ordinal) ? null : reader.GetValue(ordinal);
                    if (value == null)
                    {
                        pb.Property.SetValue(item, null);
                        continue;
                    }

                    var targetType = Nullable.GetUnderlyingType(pb.Property.PropertyType) ?? pb.Property.PropertyType;

                    // Handle Enums and safe conversion
                    if (targetType.IsEnum)
                    {
                        value = Enum.ToObject(targetType, value);
                    }
                    else
                    {
                        if (value.GetType() != targetType) value = Convert.ChangeType(value, targetType);
                    }

                    pb.Property.SetValue(item, value);
                }
                catch (Exception ex)
                {
                    string msg = $"Error mapping column '{reader.GetName(pb.Ordinal!.Value)}' to property '{pb.Property.Name}' of type '{pb.Property.PropertyType.FullName}'";
                    throw new InvalidOperationException(msg, ex);
                }
            }

            result.Add(item);
        }
        return result;
    }

    // Supports optional [Column("DbColumnName")] attribute
    private static string GetTargetColumnName(PropertyInfo p)
    {
        var colAttr = p.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.ColumnAttribute>();
        return colAttr?.Name ?? p.Name;
    }
}