using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Broccoli.Base;

namespace Broccoli.Pipe {
	/// <summary>
	/// Position for tree.
	/// </summary>
	[System.Serializable]
	public class Position {
		/// <summary>
		/// Absolute root position.
		/// </summary>
		public Vector3 rootPosition = Vector3.zero;
		/// <summary>
		/// If true the root direction for the tree is override.
		/// </summary>
		public bool overrideRootDirection = false;
		/// <summary>
		/// Overrided root direction.
		/// </summary>
		public Vector3 rootDirection = GlobalSettings.againstGravityDirection;
		/// <summary>
		/// True is this position is enabled.
		/// </summary>
		public bool enabled = true;
		/// <summary>
		/// Initializes a new instance of the <see cref="Broccoli.Pipe.Position"/> class.
		/// </summary>
		public Position () {}
		/// <summary>
		/// Initializes a new instance of the <see cref="Broccoli.Pipe.Position"/> class.
		/// </summary>
		/// <param name="rootPosition">Root position.</param>
		/// <param name="rootDirection">Root direction.</param>
		/// <param name="overrideRootDirection">If set to <c>true</c> override root direction.</param>
		public Position (Vector3 rootPosition, Vector3 rootDirection, bool overrideRootDirection) {
			this.rootPosition = rootPosition;
			this.rootDirection = rootDirection;
			this.overrideRootDirection = overrideRootDirection;
		}
		/// <summary>
		/// Clone this instance.
		/// </summary>
		public Position Clone () {
			Position clone = new Position ();
			clone.rootPosition = rootPosition;
			clone.overrideRootDirection = overrideRootDirection;
			clone.rootDirection = rootDirection;
			clone.enabled = enabled;
			return clone;
		}
	}
}