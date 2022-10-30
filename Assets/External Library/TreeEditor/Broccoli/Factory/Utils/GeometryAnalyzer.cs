using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Broccoli.Model;
using Broccoli.HCGL;
using Broccoli.Clipper2Lib;

namespace Broccoli.Utils
{
	/// <summary>
	/// Editor utility class to preview meshes on custom editors.
	/// </summary>
	public class GeometryAnalyzer {
		#region Vars
		/// <summary>
		/// Keeps the branch points when analyzing a tree structure.
		/// </summary>
        public List<Vector3> branchPoints = new List<Vector3> ();
		/// <summary>
		/// Keeps the sprout points when analyzing a tree structure.
		/// </summary>
        public List<Vector3> sproutPoints = new List<Vector3> ();
		/// <summary>
		/// Temp list for branches.
		/// </summary>
		List<BroccoTree.Branch> _branches = new List<BroccoTree.Branch> ();
		/// <summary>
		/// Temp list for sprouts.
		/// </summary>
		List<BroccoTree.Sprout> _sprouts = new List<BroccoTree.Sprout> ();
		public List<Vector3> debugPolyA = new List<Vector3> ();
		public List<Vector3> debugPolyB = new List<Vector3> ();
		public List<Vector3> debugCombinedPoly = new List<Vector3> ();
		#endregion

		#region Debug
		public List<float> debugAngles = new List<float> ();
		public List<Vector3> debugAnglePos = new List<Vector3> ();
		#endregion
		
		#region Singleton
		/// <summary>
		/// Singleton instance.
		/// </summary>
		private static GeometryAnalyzer _instance = null;
		/// <summary>
		/// Get the singleton instance.
		/// </summary>
		/// <returns>Singleton instance.</returns>
		public static GeometryAnalyzer Current () {
			if (_instance == null) {
				_instance = new GeometryAnalyzer ();
			}
			return _instance;
		}
		#endregion

		#region Ops
		/// <summary>
		/// Clear this instance.
		/// </summary>
		public void Clear () {
			branchPoints.Clear ();
			sproutPoints.Clear ();
		}
		#endregion

		#region Traversing and Analyzing
		/// <summary>
		/// Gets positions from the branches of a tree.
		/// </summary>
		/// <param name="tree">Tree to inspect.</param>
		/// <param name="relativePosition">Relative position on each branch.</param>
		/// <param name="hierarchyLevel">Hierarchy level on the tree structure.</param>
		/// <param name="isAdditive">If <c>true</c>, the points found get added to an already existing list of point.</param>
		public void GetBranchPositions (
			BroccoTree tree, 
			float relativePosition, 
			int hierarchyLevel = -1, 
			bool isAdditive = true)
		{
			_branches.Clear ();
			if (hierarchyLevel < 0) {
				_branches = tree.GetDescendantBranches ();
			} else {
				_branches = tree.GetDescendantBranches (hierarchyLevel);
			}
			if (!isAdditive) {
				branchPoints.Clear ();
			}
			for (int i = 0; i < _branches.Count; i++) {
				branchPoints.Add (_branches [i].GetPointAtPosition (relativePosition));
			}
		}
		/// <summary>
		/// Gets positions from the terminal branches of a tree.
		/// </summary>
		/// <param name="tree">Tree to inspect.</param>
		/// <param name="relativePosition">Relative position on each branch.</param>
		/// <param name="isAdditive">If <c>true</c>, the points found get added to an already existing list of point.</param>
		public void GetTerminalBranchPositions (
			BroccoTree tree, float relativePosition, bool isAdditive = true)
		{
			GetBranchPositions (tree, relativePosition, tree.GetOffspringLevel () - 1, isAdditive);
		}
		/// <summary>
		/// Gets positions from the base branches of a tree.
		/// </summary>
		/// <param name="tree">Tree to inspect.</param>
		/// <param name="relativePosition">Relative position on each branch.</param>
		/// <param name="hierarchyLevel">Hierarchy level on the tree structure.</param>
		/// <param name="isAdditive">If <c>true</c>, the points found get added to an already existing list of point.</param>
		public void GetBaseBranchPositions (
			BroccoTree tree, float relativePosition, bool isAdditive = true)
		{
			GetBranchPositions (tree, relativePosition, 0, isAdditive);
		}
		/// <summary>
		/// Gets the positions from the sprouts of a tree.
		/// </summary>
		/// <param name="tree">Tree to inspect.</param>
		/// <param name="branchHierarchyLevel">Hierarchy level of the branches on the tree structure.</param>
		/// <param name="isAdditive">If <c>true</c>, the points found get added to an already existing list of point.</param>
		public void GetSproutPositions (
			BroccoTree tree,
			int branchHierarchyLevel = -1,
			bool isAdditive = true)
		{
			_branches.Clear ();
			_sprouts.Clear ();
			if (branchHierarchyLevel < 0) {
				_branches = tree.GetDescendantBranches ();
			} else {
				_branches = tree.GetDescendantBranches (branchHierarchyLevel);
			}
			if (!isAdditive) {
				sproutPoints.Clear ();
			}
			for (int i = 0; i < _branches.Count; i++) {
				_sprouts = _branches [i].sprouts;
				for (int j = 0; j < _sprouts.Count; j++) {
					if (_sprouts [j].meshHeight > 0f) {
						sproutPoints.Add (
							_branches [i].GetPointAtPosition (
								_sprouts [j].position) + 
							_sprouts [j].sproutDirection.normalized * _sprouts [j].meshHeight);
					}
				}
			}
		}
		/// <summary>
		/// Gets the position of sprouts from a list of branches.
		/// </summary>
		/// <param name="branches">List of branches.</param>
		/// <param name="isAdditive">If <c>true</c>, the points found get added to an already existing list of point.</param>
		public void GetSproutPositions (List<BroccoTree.Branch> branches, float lengthScale = 1f, bool isAdditive = true) {
			if (!isAdditive) {
				sproutPoints.Clear ();
			}
			for (int i = 0; i < branches.Count; i++) {
				_sprouts = branches [i].sprouts;
				for (int j = 0; j < _sprouts.Count; j++) {
					if (_sprouts [j].meshHeight > 0f) {
						sproutPoints.Add (
							branches [i].GetPointAtPosition (
								_sprouts [j].position) + 
							_sprouts [j].sproutDirection.normalized * _sprouts [j].meshHeight * lengthScale);
					}
				}
			}
		}
		/// <summary>
		/// Gets the position of sprouts from a branch.
		/// </summary>
		/// <param name="branch">Branch.</param>
		/// <param name="isAdditive">If <c>true</c>, the points found get added to an already existing list of point.</param>
		public void GetSproutPositions (BroccoTree.Branch branch, float lengthScale = 1f, bool isAdditive = true) {
			if (!isAdditive) {
				sproutPoints.Clear ();
			}
			_sprouts = branch.sprouts;
			for (int j = 0; j < _sprouts.Count; j++) {
				if (_sprouts [j].meshHeight > 0f) {
					sproutPoints.Add (
						branch.GetPointAtPosition (
							_sprouts [j].position) + 
						_sprouts [j].sproutDirection.normalized * _sprouts [j].meshHeight * lengthScale);
				}
			}
		}
		/// <summary>
		/// Gets the base and terminal points of a filtered section of the tree.
		/// </summary>
		/// <param name="tree">Tree to get the outline from.</param>
		/// <param name="includes">List of included branches guids.</param>
		/// <param name="excludes">List of excluded branches guids.</param>
		/// <param name="isAdditive">If <c>true</c>, the points found get added to an already existing list of point.</param>
		/// <returns>Filtered branches.</returns>
		public List<BroccoTree.Branch> GetOutlinePoints (
			BroccoTree tree,
			List<System.Guid> includes,
			List<System.Guid> excludes, 
			bool isAdditive = true) 
		{
			_branches = GetFilteredBranches (tree, includes, excludes);
			if (!isAdditive) {
				branchPoints.Clear ();
			}
			int minLevel = -1;
			BroccoTree.Branch branch;
			for (int i = 0; i < _branches.Count; i++) {
				branch = _branches [i];
				if (minLevel == -1 || branch.GetLevel () == minLevel) {
					minLevel = branch.GetLevel ();
					// Add base point.
					branchPoints.Add (branch.GetPointAtPosition (0f));
				} else if (branch.branches.Count == 0) {
					// Add branch terminal points.
					branchPoints.Add (branch.GetPointAtPosition (1f));
				}
			}
			return _branches;
		}
		/// <summary>
		/// Gets the base and terminal points of a branch.
		/// </summary>
		/// <param name="tree">Tree to get the outline from.</param>
		/// <param name="isAdditive">If <c>true</c>, the points found get added to an already existing list of point.</param>
		/// <returns>Filtered branches.</returns>
		public List<BroccoTree.Branch> GetOutlinePoints (
			BroccoTree.Branch targetBranch,
			bool isAdditive = true) 
		{
			_branches.Clear ();
			_branches.Add (targetBranch);
			_branches = targetBranch.GetDescendantBranches ();
			if (!isAdditive) {
				branchPoints.Clear ();
			}
			int minLevel = -1;
			BroccoTree.Branch workingBranch;
			for (int i = 0; i < _branches.Count; i++) {
				workingBranch = _branches [i];
				if (minLevel == -1 || workingBranch.GetLevel () == minLevel) {
					minLevel = workingBranch.GetLevel ();
					// Add base point.
					branchPoints.Add (workingBranch.GetPointAtPosition (0f));
				} else if (workingBranch.branches.Count == 0) {
					// Add branch terminal points.
					branchPoints.Add (workingBranch.GetPointAtPosition (1f));
				}
			}
			return _branches;
		}
		/// <summary>
		/// Get the inner points (points at base) of a selection of branches.
		/// </summary>
		/// <param name="tree">Tree to get the outline from.</param>
		/// <param name="includes">List of included branches guids.</param>
		/// <param name="excludes">List of excluded branches guids.</param>
		/// <param name="isAdditive">If <c>true</c>, the points found get added to an already existing list of point.</param>
		/// <returns>Filtered branches.</returns>
		public List<BroccoTree.Branch> GetInnerPoints (
			BroccoTree tree,
			List<System.Guid> includes,
			List<System.Guid> excludes,
			bool isAdditive = true)
		{
			_branches = GetFilteredBranches (tree, includes, excludes);
			if (!isAdditive) {
				branchPoints.Clear ();
			}
			if (_branches.Count > 0) {
				int minLevel = _branches [0].GetLevel ();
				for (int i = 0; i < _branches.Count; i++) {
					if (_branches [i].GetLevel () > minLevel + 1) {
						branchPoints.Add (_branches [i].GetPointAtPosition (1f));
					}
				}
			}
			return _branches;
		}
		/*
		public List<BroccoTree.Branch GetChildrenBranches (BroccoTree.Branch branch, List<V) {
		}
		*/
		/// <summary>
		/// Filters branches on a tree.
		/// </summary>
		/// <param name="branch">Branch to inspect.</param>
		/// <param name="includes">List of Guids of branches to include.</param>
		/// <param name="excludes">List of Guids of branches to exclude.</param>
		public List<BroccoTree.Branch> GetFilteredBranches (
			BroccoTree tree,
			List<System.Guid> includes,
			List<System.Guid> excludes) 
		{
			List<BroccoTree.Branch> filteredBranches = new List<BroccoTree.Branch> ();
			for (int i = 0; i < tree.branches.Count; i++) {
				GetFilteredBranchesRecursive (tree.branches [i], filteredBranches, includes, excludes, false);
			}
			return filteredBranches;
		}
		/// <summary>
		/// Filters recursively a list of branches on a tree.
		/// </summary>
		/// <param name="branch">Branch to inspect.</param>
		/// <param name="branches">Branches added from the filter.</param>
		/// <param name="includes">List of Guids of branches to include.</param>
		/// <param name="excludes">List of Guids of branches to exclude.</param>
		/// <param name="isInInclude"><c>True</c> if the branch belongs to a hierarchy already included.</param>
		/// <param name="depth">Recursion depth to prevent stack overflow.</param>
		private void GetFilteredBranchesRecursive (
			BroccoTree.Branch branch, 
			List<BroccoTree.Branch> branches, 
			List<System.Guid> includes,
			List<System.Guid> excludes,
			bool isInInclude,
			int depth = 0) 
		{
			if (depth > 500) return;
			if (includes == null || includes.Count == 0) {
				isInInclude = true;
			}
			if (!isInInclude) {
				// See if it should be included.
				if (includes == null || includes.Contains (branch.guid)) {
					branches.Add (branch);
					isInInclude = true;
				}
				for (int i = 0; i < branch.branches.Count; i++) {
					GetFilteredBranchesRecursive (branch.branches [i], branches, 
						includes, excludes, isInInclude, depth + 1);
				}
			} else {
				// See if it excludes.
				if (excludes == null || !excludes.Contains (branch.guid)) {
					branches.Add (branch);
					for (int i = 0; i < branch.branches.Count; i++) {
						GetFilteredBranchesRecursive (branch.branches [i], branches, 
							includes, excludes, isInInclude, depth + 1);
					}
				}
			}
		}
		#endregion

		#region Processing
		/// <summary>
		/// Simplify a convex hull series a points merging points within an angle threshold
		/// while keeping the contained hull.
		/// </summary>
		/// <param name="points">Points of the convex hull, ordered from first to last.</param>
		/// <param name="thresholdAngle">Threshold angle in degrees.</param>
		public List<Vector3> SimplifyConvexHullYZ (List<Vector3> points, float thresholdAngle = 15f) {
			if (points.Count < 3) return points;
			int a = 0;
			int b = 1;
			bool isNext = true;
			float thetaDiff;
			List<Vector3> newPoints = new List<Vector3> ();
			newPoints.Add (points [a]);
			Vector3 vA;
			Vector3 vB;
			float xIntersect = points [b].x;
			while (b + 1 < points.Count) {
				vA = points [a + 1] - points [a];
				vB = points [b + 1] - points [b];
				// α = arccos[ (xa * xb + ya * yb) / (√(xa2 + ya2) * √(xb2 + yb2)) ]
				thetaDiff = Mathf.Acos ((vA.z * vB.z + vA.y * vB.y) / 
					(Mathf.Sqrt (vA.z * vA.z + vA.y * vA.y) * Mathf.Sqrt(vB.z * vB.z + vB.y * vB.y)));
				thetaDiff = thetaDiff * Mathf.Rad2Deg;
				if (thetaDiff > thresholdAngle || b + 1 == points.Count - 1) {
					//newPoints.Add (points [a]);
					if (isNext) {
						newPoints.Add (points [b]);
					} else {
						Vector3 pA = points [a];
						Vector3 lA = (points [a + 1] - points [a]).normalized;
						Vector3 pB = points [b + 1];
						Vector3 lB = (points [b] - points [b + 1]).normalized;
						Vector3 cross = points [a + 1];
						bool intersects = LineLineIntersectionYZ (out cross, pA, lA, pB, lB, xIntersect);
						newPoints.Add (cross);
					}
					a = b;
					isNext = true;
					b = a + 1;
					xIntersect = points[b].x;
				} else {
					b++;
					xIntersect = (xIntersect + points[b].x) / 2f;
					isNext = false;
				}
			}
			newPoints.Add (points [b]);
			// Run simplify on reverse.
			points.Clear ();
			points.AddRange (newPoints);
			newPoints.Clear ();
			a = points.Count - 1;
			b = a - 1;
			isNext = true;
			newPoints.Add (points [a]);
			xIntersect = points [b].x;
			while (b - 1 >= 0) {
				vA = points [a - 1] - points [a];
				vB = points [b - 1] - points [b];
				// α = arccos[ (xa * xb + ya * yb) / (√(xa2 + ya2) * √(xb2 + yb2)) ]
				thetaDiff = Mathf.Acos ((vA.z * vB.z + vA.y * vB.y) / 
					(Mathf.Sqrt (vA.z * vA.z + vA.y * vA.y) * Mathf.Sqrt(vB.z * vB.z + vB.y * vB.y)));
				thetaDiff = thetaDiff * Mathf.Rad2Deg;
				if (thetaDiff > thresholdAngle || b - 1 == 0) {
					if (isNext) {
						newPoints.Add (points [b]);
					} else {
						Vector3 pA = points [a];
						Vector3 lA = (points [a - 1] - points [a]).normalized;
						Vector3 pB = points [b - 1];
						Vector3 lB = (points [b] - points [b - 1]).normalized;
						Vector3 cross = points [a - 1];
						bool intersects = LineLineIntersectionYZ (out cross, pA, lA, pB, lB, xIntersect);
						newPoints.Add (cross);
					}
					a = b;
					isNext = true;
					b = a - 1;
					xIntersect = points[b].x;
				} else {
					b--;
					xIntersect = (xIntersect + points[b].x) / 2f;
					isNext = false;
				}
			}
			newPoints.Add (points [b]);
			newPoints.Reverse ();
			return newPoints;
		}
		/// <summary>
		/// Combines a list of overlapping convex hulls into one single hull (convex or non-convex).
		/// </summary>
		/// <param name="convexHulls">List of convex hull points.</param>
		/// <returns>Combined hull.</returns>
		public List<Vector3> CombineConvexHullsYZ (List<List<Vector3>> convexHulls) {
			List<Vector3> hull = new List<Vector3> ();
			//debugCombinedPoly = GreinerHormann.CombinePolygonsYZ (debugPolyA, debugPolyB);
			List<List<Point64>> subj = new List<List<Point64>>();
			for (int i = 0; i < convexHulls.Count; i++) {
				List<Point64> poly = new List<Point64> ();
				for (int j = 0; j < convexHulls[i].Count; j++) {
					poly.Add (new Point64 (
						convexHulls[i][j].z * 10000, 
						convexHulls[i][j].y * 10000, 
						convexHulls[i][j].x * 10000));
				}
				subj.Add (poly);
			}
			/*
			List<List<Point64>> clip = new List<List<Point64>>();
			subj.Add(Clipper.MakePath(new int[] { 100, 50, 10, 79, 65, 2, 65, 98, 10, 21 }));
			clip.Add(Clipper.MakePath(new int[] { 98, 63, 4, 68, 77, 8, 52, 100, 19, 12 }));
			*/
			//List<List<Point64>> solution = Clipper.Intersect(subj, clip, FillRule.NonZero);
			List<List<Point64>> solution = Clipper.Union (subj, FillRule.Positive);
			//solution = Clipper.InflatePaths (solution, 0.4f, Broccoli.Clipper2Lib.JoinType.Miter, Broccoli.Clipper2Lib.EndType.Polygon);
			//solution = Clipper.RamerDouglasPeucker (solution, 0.125);
			for (int i = 0; i < solution.Count; i++) {
				for (int j = 0; j < solution[i].Count; j++) {
					hull.Add (new Vector3 (solution[i][j].Z / 10000f, solution[i][j].Y / 10000f, solution[i][j].X / 10000f));
				}
			}
			//debugCombinedPoly = hull;
			return hull;
		}
		/// <summary>
		/// Shifts the convex hull points to begin at the point closest to vector zero.
		/// </summary>
		/// <param name="points">List of points.</param>
		/// <returns>Shifted points.</returns>
		public List<Vector3> ShiftConvexHullPoint (List<Vector3> points) {
			List<Vector3> shiftPoints = new List<Vector3> ();
			float distanceToZero = -1f;
			float _distanceToZero = -1f;
			int shiftIndex = 0;
			for (int i = 0; i < points.Count; i++) {
				_distanceToZero = Vector3.Distance (Vector3.zero, points [i]);
				if (distanceToZero == -1f || _distanceToZero < distanceToZero) {
					distanceToZero = _distanceToZero;
					shiftIndex = i;
				}
			}
			for (int i = shiftIndex; i < points.Count; i++) {
				shiftPoints.Add (points [i]);
			}
			for (int i = 0; i < shiftIndex; i++) {
				shiftPoints.Add (points [i]);
			}
			return shiftPoints;
		}
		public Bounds GetOBBFromPolygon (List<Vector3> points, out float obbAngle) {
			obbAngle = 0;
			int searchLimit = 100;
			Bounds bounds = GeometryUtility.CalculateBounds (points.ToArray (), Matrix4x4.identity);
			float obbArea = Mathf.Abs (bounds.max.z - bounds.min.z) * Mathf.Abs (bounds.max.y - bounds.min.y);
			int i = 0;
			List<Vector3> rotPoints;
			Bounds rotBounds;
			float rotArea;
			float theta = 0f;
			while (theta <= 180f && i + 1 < points.Count && searchLimit > 0) {
				theta = Mathf.Atan2 (points [i + 1].y - points [i].y, points [i + 1].z - points [i].z);
				theta = (theta/Mathf.PI * 180) + (theta > 0 ? 0 : 360);
				rotPoints = RotateAroundX (points, theta);
				rotBounds = GeometryUtility.CalculateBounds (rotPoints.ToArray (), Matrix4x4.identity);
				rotArea = Mathf.Abs (rotBounds.max.z - rotBounds.min.z) * Mathf.Abs (rotBounds.max.y - rotBounds.min.y);
				if (rotArea < obbArea) {
					obbArea = rotArea;
					obbAngle = theta;
					bounds = rotBounds;
				}
				i++;
				searchLimit--;
			}
			return bounds;
		}
		public List<Vector3> RotateAroundX (List<Vector3> sourcePoints, float degrees) {
			List<Vector3> rotatedPoints = new List<Vector3> ();
			Quaternion rotation = Quaternion.AngleAxis (degrees, Vector3.right);
			for (int i = 0; i < sourcePoints.Count; i++) {
				rotatedPoints.Add (rotation * sourcePoints [i]);
			}
			return rotatedPoints;
		}
		public List<int> DelaunayTriangulationYZ (List<Vector3> polygonPoints) {
			return _Delaunay.ProcessYZ (polygonPoints);
		}
		public List<int> DelaunayConstrainedTriangulationYZ (List<Vector3> polygonPoints, int lastConvexPointIndex) {
			return _Delaunay.ProcessConstrainedYZ (polygonPoints, lastConvexPointIndex);
		}
		public List<Vector3> QuickHullYZ (List<Vector3> candidatePoints, bool includeColinearPoints = false) {
			return _ConvexHull.ProcessQuickHullYZ (candidatePoints, includeColinearPoints);
		}
		#endregion

		#region Utils
		public bool LineLineIntersectionYZ (out Vector3 intersection, Vector3 linePoint1,
        Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2, float xValue){
			float linePoint1X = linePoint1.x;
			linePoint1.x = 0;
			linePoint2.x = 0;
			lineVec1.x = 0;
			float lineVec2X = lineVec2.x;
			lineVec2.x = 0;

			Vector3 lineVec3 = linePoint2 - linePoint1;
			Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
			Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

			float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

			//is coplanar, and not parallel
			if( Mathf.Abs(planarFactor) < 0.0001f 
					&& crossVec1and2.sqrMagnitude > 0.0001f)
			{
				float s = Vector3.Dot(crossVec3and2, crossVec1and2) 
						/ crossVec1and2.sqrMagnitude;
				linePoint1.x = linePoint1X; 
				lineVec1.x = lineVec2X;
				intersection = linePoint1 + (lineVec1 * s);
				intersection.x = xValue;
				return true;
			}
			else
			{
				intersection = Vector3.zero;
				return false;
			}
		}
		#endregion
	}
}