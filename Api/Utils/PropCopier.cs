// namespace API.Utils;

// public class PropCopier
// {
//     public static TTarget Copy<TOrigin, TTarget>(TOrigin origin, TTarget target)
//     {
//         var originProperties = typeof(TOrigin).GetProperties();
//         var targetProperties = typeof(TTarget).GetProperties();

//         foreach (var originProp in originProperties)
//         {
//             // Check if the target has a property with the same name and if it is writable
//             var targetProp = targetProperties.FirstOrDefault(p =>
//                 p.Name == originProp.Name && p.CanWrite
//             );

//             if (targetProp != null && targetProp.PropertyType == originProp.PropertyType)
//             {
//                 // Only copy the value if the target property is null or default
//                 var value = originProp.GetValue(origin);
//                 if (targetProp.GetValue(target) == null) // or whatever condition you prefer
//                 {
//                     targetProp.SetValue(target, value);
//                 }
//             }
//         }

//         return target;
//     }
// }


using System;
using System.Linq;
using System.Reflection;

namespace API.Utils
{
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

            // Create a dictionary for fast lookup of target properties by name
            var targetPropertiesDictionary = targetProperties.ToDictionary(p => p.Name);

            foreach (var originProp in originProperties)
            {
                // Find the target property with the same name
                if (
                    targetPropertiesDictionary.TryGetValue(originProp.Name, out var targetProp)
                    && targetProp.CanWrite
                )
                {
                    // Check if types match or if one is a nullable version of the other
                    if (
                        targetProp.PropertyType == originProp.PropertyType
                        || (
                            Nullable.GetUnderlyingType(targetProp.PropertyType)
                            == originProp.PropertyType
                        )
                    )
                    {
                        var value = originProp.GetValue(origin);

                        // Handle value types like Guid and check for default values
                        if (value != null && IsDefaultValue(value, targetProp.PropertyType))
                        {
                            continue;
                        }

                        targetProp.SetValue(target, value);
                    }
                }
            }

            return target;
        }

        // Check if the value is the default value for its type (null for reference types, or the default for value types)
        private static bool IsDefaultValue(object value, Type type)
        {
            // For reference types, check if the value is null
            if (value == null)
            {
                return true;
            }

            // For value types, check if it is the default (e.g., Guid.Empty for Guid)
            if (type.IsValueType)
            {
                return value.Equals(Activator.CreateInstance(type)); // Default value for that type
            }

            return false; // Not a default value
        }
    }
}
