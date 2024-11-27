namespace API.Utils;

public class PropCopier
{
    public static TTarget Copy<TOrigin, TTarget>(TOrigin origin, TTarget target)
    {
        var originProperties = typeof(TOrigin).GetProperties();
        var targetProperties = typeof(TTarget).GetProperties();

        foreach (var originProp in originProperties)
        {
            // Check if the target has a property with the same name and if it is writable
            var targetProp = targetProperties.FirstOrDefault(p =>
                p.Name == originProp.Name && p.CanWrite
            );

            if (targetProp != null && targetProp.PropertyType == originProp.PropertyType)
            {
                // Only copy the value if the target property is null or default
                var value = originProp.GetValue(origin);
                if (targetProp.GetValue(target) == null) // or whatever condition you prefer
                {
                    targetProp.SetValue(target, value);
                }
            }
        }

        return target;
    }
}
