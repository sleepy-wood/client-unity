using System.Collections;
using System.Collections.Generic;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

using Broccoli.Base;
using Broccoli.Model;
using Broccoli.Pipe;
using Broccoli.Utils;

namespace Broccoli.Utils
{
	/// <summary>
	/// Utility class for trees. 
	/// Includes tree processing helping methods and
	/// gizmo/handle drawing methods.
	/// </summary>
	public class TreeEditorUtils {
		#region Vars
		#endregion

		#region Sprout Drawing
		/// <summary>
		/// Draws tree sprouts as gizmos.
		/// </summary>
		/// <param name="tree">Tree.</param>
		/// <param name="sproutGroups">Sprout groups.</param>
		/// <param name="treeOrigin">Tree origin.</param>
		/// <param name="scale">Scale.</param>
		public static void DrawTreeSprouts (BroccoTree tree, SproutGroups sproutGroups, 
			Vector3 treeOrigin, float scale = 1f) 
		{
			if (tree != null) {
				for (int i = 0; i < tree.branches.Count; i++) {
					DrawSprouts (tree.branches[i], sproutGroups, Vector3.zero, treeOrigin, scale);
				}
			}
		}
		/// <summary>
		/// Draw sprouts of a branch as gizmos.
		/// </summary>
		/// <param name="branch">Branch.</param>
		/// <param name="sproutGroups">Sprout groups.</param>
		/// <param name="origin">Origin.</param>
		/// <param name="treeOrigin">Tree origin.</param>
		/// <param name="scale">Scale.</param>
		static void DrawSprouts (BroccoTree.Branch branch, SproutGroups sproutGroups, 
			Vector3 origin, Vector3 treeOrigin, float scale = 1f) 
		{
			#if UNITY_EDITOR
			for (int i = 0; i < branch.sprouts.Count; i++) {
				// Draw direction.
				Gizmos.color = sproutGroups.GetSproutGroupColor (branch.sprouts[i].groupId);
				Vector3 dest1 = branch.sprouts[i].inGirthPosition + 
					branch.sprouts[i].sproutDirection * HandleUtility.GetHandleSize (branch.sprouts[i].inGirthPosition) * 
					GlobalSettings.sproutGizmoLength;
				Gizmos.DrawLine (branch.sprouts[i].inGirthPosition * scale + treeOrigin, dest1 * scale + treeOrigin);
				// Draw normal.
				Gizmos.color = Color.white;
				Vector3 dest2 = dest1 + 
					branch.sprouts[i].sproutNormal * HandleUtility.GetHandleSize (branch.sprouts[i].inGirthPosition) * 
					GlobalSettings.sproutGizmoLength;
				Gizmos.DrawLine (dest1 * scale + treeOrigin, dest2 * scale + treeOrigin);

			}
			for (int i = 0; i < branch.branches.Count; i++) {
				Vector3 childBranchOrigin = branch.branches[i].origin;
				DrawSprouts (branch.branches[i], sproutGroups, childBranchOrigin, treeOrigin, scale);
			}
			#endif
		}
		#endregion

		#region Branch Structure Levels Drawing
		/// <summary>
		/// Draws branches assigned to a specific structure level as handles.
		/// </summary>
		/// <param name="structureLevelId">Structure level identifier.</param>
		/// <param name="tree">Tree.</param>
		/// <param name="offset">Offset.</param>
		/// <param name="scale">Scale.</param>
		public static void DrawTreeBranchesForStructureLevel (int structureLevelId, BroccoTree tree, 
			Vector3 offset, float scale = 1f) 
		{
			if (tree != null) {
				for (int i = 0; i < tree.branches.Count; i++) {
					BroccoTree.Branch branch = tree.branches [i];
					DrawBranchForStructureLevel (structureLevelId, branch, branch.origin, offset, scale);
				}
			}
		}
		/// <summary>
		/// Draws a branch from a structure level as handle line.
		/// </summary>
		/// <param name="structureLevelId">Structure level identifier.</param>
		/// <param name="branch">Branch.</param>
		/// <param name="origin">Origin.</param>
		/// <param name="offset">Offset.</param>
		/// <param name="scale">Scale.</param>
		static void DrawBranchForStructureLevel (int structureLevelId, BroccoTree.Branch branch, 
			Vector3 origin, Vector3 offset, float scale = 1f) 
		{
			#if UNITY_EDITOR
			/*
			if (branch.helperStructureLevelId == structureLevelId) {
				Vector3 destination;
				List<BroccoTree.Branch.BendPoint> bendPoints = branch.GetBendPoints ();
				for (int i = 0; i < bendPoints.Count; i++) {
					destination = branch.GetPointAtPosition (bendPoints[i].position); // TODO: cache BendPoint point.
					Handles.DrawLine (origin * scale + offset, destination * scale + offset);
					origin = destination;
				}
				// Draw last line of branch.
				destination = branch.destination;
				Handles.DrawLine (origin * scale + offset, destination * scale + offset);
			}
			for (int i = 0; i < branch.branches.Count; i++) {
				Vector3 childBranchOrigin = branch.branches[i].origin;
				DrawBranchForStructureLevel (structureLevelId, branch.branches[i], childBranchOrigin, offset, scale);
			}
			*/
			if (branch.helperStructureLevelId == structureLevelId) {
				BezierCurveDraw.DrawCurve (branch.curve, offset + (branch.positionFromRoot * scale), scale, Color.white);
			}
			for (int i = 0; i < branch.branches.Count; i++) {
				Vector3 childBranchOrigin = branch.branches[i].origin;
				DrawBranchForStructureLevel (structureLevelId, branch.branches[i], childBranchOrigin, offset, scale);
			}
			#endif
		}
		#endregion

		#region Sprout Structure Levels Drawing
		/// <summary>
		/// Draws sprouts assigned to a structure level as hadles.
		/// </summary>
		/// <param name="structureLevelId">Structure level identifier.</param>
		/// <param name="tree">Tree.</param>
		/// <param name="treeOrigin">Tree origin.</param>
		/// <param name="scale">Scale.</param>
		public static void DrawTreeSproutsForStructureLevel (int structureLevelId, BroccoTree tree, 
			Vector3 treeOrigin, float scale = 1f) 
		{
			if (tree != null) {
				for (int i = 0; i < tree.branches.Count; i++) {
					DrawSproutsForStructureLevel (structureLevelId, tree.branches[i], Vector3.zero, treeOrigin, scale);
				}
			}
		}
		/// <summary>
		/// Draws sprouts on a branch from a structure level as handles.
		/// </summary>
		/// <param name="structureLevelId">Structure level identifier.</param>
		/// <param name="branch">Branch.</param>
		/// <param name="origin">Origin.</param>
		/// <param name="treeOrigin">Tree origin.</param>
		/// <param name="scale">Scale.</param>
		static void DrawSproutsForStructureLevel (int structureLevelId, BroccoTree.Branch branch, 
			Vector3 origin, Vector3 treeOrigin, float scale = 1f) 
		{
			#if UNITY_EDITOR
			for (int i = 0; i < branch.sprouts.Count; i++) {
				if (branch.sprouts[i].helperStructureLevelId == structureLevelId) {
					// Draw sprout direction.
					Vector3 dest = branch.sprouts[i].inGirthPosition + branch.sprouts[i].sproutDirection * 
						HandleUtility.GetHandleSize (branch.sprouts[i].inGirthPosition) * 
						GlobalSettings.sproutGizmoLength;
					Handles.DrawLine (branch.sprouts[i].inGirthPosition * scale + treeOrigin, dest * scale + treeOrigin);
					// Draw sprout normal.
					Vector3 dest2 = dest + 
						(branch.sprouts[i].sproutNormal * 0.5f) * HandleUtility.GetHandleSize (branch.sprouts[i].inGirthPosition) * 
						GlobalSettings.sproutGizmoLength;
					Handles.DrawLine (dest * scale + treeOrigin, dest2 * scale + treeOrigin);
				}
			}
			for (int i = 0; i < branch.branches.Count; i++) {
				Vector3 childBranchOrigin = branch.branches[i].origin;
				DrawSproutsForStructureLevel (structureLevelId, branch.branches[i], childBranchOrigin, treeOrigin, scale);
			}
			#endif
		}
		public static Vector3 ToTreeSpace (Vector3 target, Vector3 treeOrigin, float scale = 1) {
			return target * scale + treeOrigin;
		}
		#endregion
	}
}