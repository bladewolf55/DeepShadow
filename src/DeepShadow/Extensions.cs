using System;
using System.Collections;
using System.Reflection;

namespace DeepShadow
{
    public static class Extensions
    {
        //https://stackoverflow.com/questions/3569811/how-to-know-if-a-propertyinfo-is-a-collection

        
        public static bool IsParentPrincipal(this PropertyInfo propInfo, object value)
        {
            return value != null && !propInfo.PropertyType.Name.Contains("String") && propInfo.PropertyType.IsClass && !propInfo.IsNonStringEnumerable();
        }

        public static bool IsChildCollection(this PropertyInfo propInfo, object value)
        {
            return value != null && !propInfo.PropertyType.Name.Contains("String") && propInfo.IsNonStringEnumerable();
        }

        private static bool IsNonStringEnumerable(this PropertyInfo propInfo)
        {
            return propInfo != null && propInfo.PropertyType.IsNonStringEnumerable();
        }

        private static bool IsNonStringEnumerable(this object instance)
        {
            return instance != null && instance.GetType().IsNonStringEnumerable();
        }

        private static bool IsNonStringEnumerable(this Type type)
        {
            if (type == null || type == typeof(string))
                return false;
            return typeof(IEnumerable).IsAssignableFrom(type);
        }

        //clf
        public static bool IsValueOrStringEnumerable(this Type type)
        {
            if (type == null) return false;
            if (type.IsArray) return true;
            if (type.IsConstructedGenericType & hasGenericValueTypeArgument(type)) return true;
            return false;
        }

        public static bool hasGenericValueTypeArgument(this Type type)
        {
            var types = type.GetGenericArguments();
            if (types != null && types.Length == 1 )
            {
                Type t = types[0];
                if (t.IsValueType || t == typeof(string))
                {
                    return true;
                }
            }
            return false;
        }
    }

}
