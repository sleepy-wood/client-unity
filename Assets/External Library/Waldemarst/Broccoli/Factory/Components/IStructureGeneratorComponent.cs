using System.Collections;

using UnityEngine;

namespace Broccoli.Component 
{
	/// <summary>
	/// Interface for component that generate a structure.
	/// </summary>
	public interface IStructureGeneratorComponent {
		/// <summary>
		/// The number of available root positions.
		/// </summary>
		int GetAvailableRootPositions ();
		/// <summary>
		/// The unique root position offset when all the root branches have this origin.
		/// </summary>
		Vector3 GetUniqueRootPosition ();
	}
}