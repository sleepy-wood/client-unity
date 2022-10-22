using UnityEngine;
using System;
using System.Collections;
using System.Runtime.Serialization;

namespace Broccoli.Serialization {
	/// <summary>
	/// Vector3 serialization surrogate.
	/// </summary>
	public class Vector3Surrogate : ISerializationSurrogate
	{
		/// <summary>
		/// Gets the object data.
		/// </summary>
		/// <param name="obj">Object.</param>
		/// <param name="info">Info.</param>
		/// <param name="context">Context.</param>
		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			var vector = (Vector3)obj;
			info.AddValue("x", vector.x);
			info.AddValue("y", vector.y);
			info.AddValue("z", vector.z);
		}
		/// <summary>
		/// Sets the object data.
		/// </summary>
		/// <returns>The object data.</returns>
		/// <param name="obj">Object.</param>
		/// <param name="info">Info.</param>
		/// <param name="context">Context.</param>
		/// <param name="selector">Selector.</param>
		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			Func<string, float> get = name => (float)info.GetValue(name, typeof(float));
			return new Vector3(get("x"), get("y"), get("z"));
		}
	}
}