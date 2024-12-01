using System;
using System.Linq;

public static class TypeExtensions
{
    static Type ResolveGenericType(Type type)
    {
        if (type is not { IsGenericType: true }) return type;
        var genericType = type.GetGenericTypeDefinition();
        return genericType != type ? genericType : type;
    }

    static bool HasAnyInterfaces(Type type, Type interfaceType) => type.GetInterfaces().Any(i => ResolveGenericType(i) == interfaceType);

    public static bool InheritsOrImplements(this Type type, Type baseType)
    {
        type = ResolveGenericType(type);
        baseType = ResolveGenericType(baseType);
        while (type != typeof(object))
        {
            if (baseType == type || HasAnyInterfaces(type, baseType)) return true;
            type = ResolveGenericType(type.BaseType);
            if (type == null) return false;
        }
        return false;
    }
}
