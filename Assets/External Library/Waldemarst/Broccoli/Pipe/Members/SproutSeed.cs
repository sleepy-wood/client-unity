using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Broccoli.Pipe {
	[System.Serializable]
	/// <summary>
	/// Sprout seed holding parameters to generate sprouts.
	/// </summary>
	public class SproutSeed {
		/// <summary>
		/// The sprout group identifier.
		/// </summary>
		public int groupId = 0;
		/// <summary>
		/// Clone this instance.
		/// </summary>
		public SproutSeed Clone () {
			SproutSeed clone = new SproutSeed();
			clone.groupId = groupId;
			return clone;
		}
	}
}