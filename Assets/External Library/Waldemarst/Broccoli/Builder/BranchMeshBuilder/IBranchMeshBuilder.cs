using System.Collections.Generic;

using UnityEngine;

using Broccoli.Model;

namespace Broccoli.Builder {
	public interface IBranchMeshBuilder {
		#region Vars
		void SetAngleTolerance (float angleTolerance);
		float GetAngleTolerance ();
		void SetGlobalScale (float globalScale);
		float GetGlobalScale ();
		#endregion
		#region Methods
		/// <summary>
		/// Get the branch mesh builder type.
		/// </summary>
		/// <returns>Branch mesh builder type.</returns>
		BranchMeshBuilder.BuilderType GetBuilderType ();
		/// <summary>
		/// Called right after a BranchSkin is created.
		/// </summary>
		/// <param name="rangeIndex">Index of the branch skin range to process.</param>
		/// <param name="branchSkin">BranchSkin instance to process.</param>
		/// <param name="firstBranch">The first branch instance on the BranchSkin instance.</param>
		/// <param name="parentBranchSkin">Parent BranchSkin instance to process.</param>
		/// <param name="parentBranch">The parent branch of the first branch of the BranchSkin instance.</param>
		/// <returns>True if any processing gets done.</returns>
		bool PreprocessBranchSkinRange (
			int rangeIndex, 
			BranchMeshBuilder.BranchSkin branchSkin,
			BroccoTree.Branch firstBranch,
			BranchMeshBuilder.BranchSkin parentBranchSkin = null, 
			BroccoTree.Branch parentBranch = null);
		/// <summary>
		/// Called right after a BranchSkin is created.
		/// </summary>
		/// <param name="mesh">Mesh to process.</param>
		/// <param name="rangeIndex">Index of the branch skin range to process.</param>
		/// <param name="branchSkin">BranchSkin instance to process.</param>
		/// <param name="firstBranch">The first branch instance on the BranchSkin instance.</param>
		/// <param name="parentBranchSkin">Parent BranchSkin instance to process.</param>
		/// <param name="parentBranch">The parent branch of the first branch of the BranchSkin instance.</param>
		/// <returns>True if any processing gets done.</returns>
		Mesh PostprocessBranchSkinRange (
			Mesh mesh,
			int rangeIndex, 
			BranchMeshBuilder.BranchSkin branchSkin,
			BroccoTree.Branch firstBranch,
			BranchMeshBuilder.BranchSkin parentBranchSkin = null, 
			BroccoTree.Branch parentBranch = null);
		/// <summary>
		/// Gets a  positional offset from the center of a branch to its mesh skin surface.
		/// </summary>
		/// <param name="positionAtBranch">Position at branch, from 0 to 1.</param>
		/// <param name="branch">Branch containing the position and belonging to the BranchSkin instance.</param>
		/// <param name="rollAngle">Angle around the branch from the normal of the branch. In radians.</param>
		/// <param name="forward">Forward vector coming from the center of the branch to the surface of the mesh.</param>
		/// <param name="branchSkin">BranchSkin instance the branch belongs to.</param>
		/// <returns></returns>
		Vector3 GetBranchSkinPositionOffset (float positionAtBranch, 
			BroccoTree.Branch branch, 
			float rollAngle, 
			Vector3 forward, 
			BranchMeshBuilder.BranchSkin branchSkin);
		/// <summary>
		/// Gets the number of segments (like polygon sides) as resolution for a branch position.
		/// </summary>
		/// <param name="branchSkin">BranchSkin instance.</param>
		/// <param name="branchSkinPosition">Position along the BranchSkin instance.</param>
		/// <param name="branchAvgGirth">Branch average girth.</param>
		/// <returns>The number polygon sides.</returns>
		int GetNumberOfSegments (BranchMeshBuilder.BranchSkin branchSkin, float branchSkinPosition, float branchAvgGirth);
		Vector3[] GetPolygonAt (
			BranchMeshBuilder.BranchSkin branchSkin,
			int segmentIndex,
			ref List<float> radialPositions,
			float scale,
			float radiusScale = 1f);
		/*
		Vector3[] GetPolygonAt (
			int branchSkinId,
			Vector3 center, 
			Vector3 lookAt, 
			Vector3 normal, 
			float radius, 
			int polygonSides,
			ref List<float> radialPositions,
			float scale,
			float radiusScale = 1f);
		*/
		#endregion
	}
}