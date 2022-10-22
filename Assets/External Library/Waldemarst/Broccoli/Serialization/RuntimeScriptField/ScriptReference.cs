using UnityEngine;

namespace Broccoli.Serialization
{
    /// <summary>
    /// Inherit this to create a field where you can assign a script at edit time, and get access to the script's
    /// type at runtime!
    /// </summary>
    /// <typeparam name="T">Type restriction for the script type. You can only assign scripts of the type T to the field</typeparam>
    public class ScriptReference<T> : ScriptReference_Base where T : UnityEngine.Component {
        public T AddTo(GameObject gameObject)
        {
            return (T) gameObject.AddComponent(script);
        }

        public T AddTo(UnityEngine.Component component)
        {
            return (T) component.gameObject.AddComponent(script);
        }
    }
}