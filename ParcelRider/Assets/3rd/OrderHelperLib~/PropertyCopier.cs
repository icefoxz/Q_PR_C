using System.Reflection;

namespace OrderHelperLib;
// 自定义特性，用于属性名映射
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class PropertyMapAttribute : Attribute
{
    /// <summary>
    /// 映射名
    /// </summary>
    public string? MappedPropertyName { get; private set; }
    /// <summary>
    /// 是否忽略映射
    /// </summary>
    public bool Ignore { get; set; }

    public PropertyMapAttribute(string mappedPropertyName)
    {
        MappedPropertyName = mappedPropertyName;
        Ignore = false;
    }

    public PropertyMapAttribute(bool ignore)
    {
        Ignore = ignore;
    }
}
public static class PropertyCopier
{
    public static void CopyPropertiesTo<TTarget, TSource>(TTarget target, TSource source)
    {
        var sourceProps = typeof(TSource).GetProperties()
            .Where(p => p.CanRead && p.GetCustomAttribute<PropertyMapAttribute>()?.Ignore != true)
            .Select(p => new
            {
                SourceProperty = p,
                MappedNames = new[] { p.GetCustomAttribute<PropertyMapAttribute>()?.MappedPropertyName, p.Name }.Where(s => !string.IsNullOrWhiteSpace(s))
            });

        var targetProps = typeof(TTarget).GetProperties()
            .Where(p => p.CanWrite)
            .Select(p => new
            {
                SourceProperty = p,
                MappedNames = new[] { p.GetCustomAttribute<PropertyMapAttribute>()?.MappedPropertyName, p.Name }.Where(s => !string.IsNullOrWhiteSpace(s))
            }).ToList();

        foreach (var sourceProp in sourceProps)
        {
            // Try to get the target property by mapped name first, then by source property name
            var tar = targetProps.FirstOrDefault(t => t.MappedNames.Any(n =>
                sourceProp.MappedNames.Any(s =>
                    n != null && n.Equals(s, StringComparison.InvariantCultureIgnoreCase))));
            if (tar == null) continue; // No matching property found, or property is not writable

            var targetProp = tar.SourceProperty;
            if (!targetProp.PropertyType.IsAssignableFrom(sourceProp.SourceProperty.PropertyType)) continue; // Property types do not match, cannot assign

            // Get value from source and set it on target
            var valueToCopy = sourceProp.SourceProperty.GetValue(source);
            targetProp.SetValue(target, valueToCopy);
        }
    }
}