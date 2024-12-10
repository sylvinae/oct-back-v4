using System.Reflection;

namespace API.Utils;

public class PropCopier
{
    public static TTarget Copy<TOrigin, TTarget>(TOrigin origin, TTarget target)
    {
        var originProperties = typeof(TOrigin).GetProperties(
            BindingFlags.Public | BindingFlags.Instance
        );
        var targetProperties = typeof(TTarget).GetProperties(
            BindingFlags.Public | BindingFlags.Instance
        );

        var targetPropertiesDictionary = targetProperties.ToDictionary(p => p.Name);

        foreach (var originProp in originProperties)
            if (
                targetPropertiesDictionary.TryGetValue(originProp.Name, out var targetProp)
                && targetProp.CanWrite
            )
                if (
                    targetProp.PropertyType == originProp.PropertyType
                    || Nullable.GetUnderlyingType(targetProp.PropertyType)
                    == originProp.PropertyType
                )
                {
                    var value = originProp.GetValue(origin);

                    if (value != null && IsDefaultValue(value, targetProp.PropertyType)) continue;

                    targetProp.SetValue(target, value);
                }

        return target;
    }

    private static bool IsDefaultValue(object value, Type type)
    {
        return type.IsValueType && value.Equals(Activator.CreateInstance(type));
    }
}