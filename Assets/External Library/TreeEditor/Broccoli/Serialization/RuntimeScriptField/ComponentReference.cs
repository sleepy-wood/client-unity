using System;

namespace Broccoli.Serialization {
    /// <summary>
    /// You can assign a script that references a Component with this type, and add it at runtime.
    /// </summary>
    [Serializable]
    public class ComponentReference : ScriptReference<UnityEngine.Component> { }
}