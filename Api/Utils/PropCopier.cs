using System.Collections;
using System.Reflection;

namespace API.Utils;

public class PropCopier
{
    public static TTarget Copy<TOrigin, TTarget>(TOrigin origin, TTarget target)
    {
        var originProperties = typeof(TOrigin).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var targetProperties = typeof(TTarget).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var targetPropertiesDictionary = targetProperties.ToDictionary(p => p.Name);

        foreach (var originProp in originProperties)
            if (targetPropertiesDictionary.TryGetValue(originProp.Name, out var targetProp) && targetProp.CanWrite)
            {
                var originValue = originProp.GetValue(origin);

                if (originValue == null || IsSimpleType(originProp.PropertyType))
                {
                    if (originValue is Guid originGuid && originGuid != Guid.Empty)
                        targetProp.SetValue(target, originGuid);
                    else if (originValue != null) targetProp.SetValue(target, originValue);
                }
                else if (typeof(IEnumerable).IsAssignableFrom(originProp.PropertyType) &&
                         originProp.PropertyType != typeof(string))
                {
                    if (originValue is not IEnumerable originCollection ||
                        !targetProp.PropertyType.GetInterfaces().Contains(typeof(ICollection))) continue;
                    var collectionType = targetProp.PropertyType.GetGenericArguments()[0];
                    var targetCollection = Activator.CreateInstance(typeof(List<>).MakeGenericType(collectionType));

                    if (targetCollection is not IList targetList) continue;
                    foreach (var item in originCollection)
                    {
                        var targetItem = Activator.CreateInstance(collectionType);
                        Copy(item, targetItem);
                        targetList.Add(targetItem);
                    }

                    targetProp.SetValue(target, targetCollection);
                }
                else
                {
                    var targetValue = Activator.CreateInstance(targetProp.PropertyType);
                    Copy(originValue, targetValue);
                    targetProp.SetValue(target, targetValue);
                }
            }

        return target;
    }

    private static bool IsSimpleType(Type type)
    {
        return type.IsPrimitive || type.IsValueType || type == typeof(string) || type == typeof(Guid);
    }
}