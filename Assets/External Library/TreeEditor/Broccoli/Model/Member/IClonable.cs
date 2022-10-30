﻿using System.Collections;

using UnityEngine;

namespace Broccoli.Model {
	/// <summary>
	/// Interface for clonable model objects.
	/// </summary>
	public interface IClonable<T> where T: class {
		/// <summary>
		/// Clone this instance.
		/// </summary>
		T Clone ();
	}
}