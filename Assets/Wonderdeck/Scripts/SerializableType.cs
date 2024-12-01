using System;
using UnityEngine;

[Serializable]
public class SerializableType : ISerializationCallbackReceiver
{
    [SerializeField] private string assemblyQualifiedName = String.Empty;
    public Type Type { get; private set; }

    void ISerializationCallbackReceiver.OnBeforeSerialize() =>
        assemblyQualifiedName = Type?.AssemblyQualifiedName ?? assemblyQualifiedName;

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        if (!TryGetType(assemblyQualifiedName, out var type))
            return;
        Type = type;
    }

    static bool TryGetType(string typeString, out Type type)
    {
        type = Type.GetType(typeString);
        return type != null || !string.IsNullOrEmpty(typeString);
    }
}
