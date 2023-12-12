using System;
using System.Reflection;
using Newtonsoft.Json.Linq;

public static class Mapper
{
    public static TDestination JMap<TSource, TDestination>(this TSource source)
    {
        var jObj = JObject.FromObject(source);
        return jObj.ToObject<TDestination>();
    }

    public static TDestination Map<TSource, TDestination>(this TSource source)
    {
        var destination = Activator.CreateInstance<TDestination>();
        var sourceType = typeof(TSource);
        var destinationType = typeof(TDestination);

        foreach (var sourceProperty in sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var destinationProperty = destinationType.GetProperty(sourceProperty.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (destinationProperty == null || !destinationProperty.CanWrite) continue;
            var value = sourceProperty.GetValue(source, null);
            destinationProperty.SetValue(destination, value, null);
        }

        foreach (var sourceField in sourceType.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            var destinationField = destinationType.GetField(sourceField.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (destinationField == null) continue;
            var value = sourceField.GetValue(source);
            destinationField.SetValue(destination, value);
        }

        return destination;
    }
}