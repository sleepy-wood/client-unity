using UnityEngine;
using System;
using System.Collections;
using System.Runtime.Serialization;

namespace Broccoli.Serialization {
	/// <summary>
	/// Vector2 serialization surrogate.
	/// </summary>
	public class Vector2Surrogate : ISerializationSurrogate
	{
		/// <summary>
		/// Gets the object data.
		/// </summary>
		/// <param name="obj">Object.</param>
		/// <param name="info">Info.</param>
		/// <param name="context">Context.</param>
		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			var vector = (Vector2)obj;
			info.AddValue("x", vector.x);
			info.AddValue("y", vector.y);
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
			return new Vector2(get("x"), get("y"));
		}
	}
}