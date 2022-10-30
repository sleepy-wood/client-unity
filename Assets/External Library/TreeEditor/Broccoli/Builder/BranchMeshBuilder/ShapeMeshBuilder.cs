using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Broccoli.Base;
using Broccoli.Model;

namespace Broccoli.Builder {
	/// <summary>
	/// Gives methods to help create mesh segments using BranchSkin instances.
	/// </summary>
	public class ShapeMeshBuilder : IBranchMeshBuilder {
		#region Class BranchInfo
		/// <summary>
		/// Class containing the information to process the mesh of a branch.
		/// </summary>
		protected class BranchInfo {
			/// <summary>
			/// Modes on how to integrate roots to the trunk mesh.
			/// </summary>
			public enum RootMode {
				/// <summary>
				/// Roots are simulated at the trunk surface.
				/// </summary>
				Pseudo,
				/// <summary>
				/// The trunk mesh integrates with existing roots from the tree structure.
				/// </summary>
				Integration
			}
			/// <summary>
			/// Root mode to integrate to the trunk mesh.
			/// </summary>
			public RootMode rootMode = RootMode.Pseudo;
			public int displacementPoints = 3;
			public float girthAtBase;
			public float girthAtTop;
			public float minScaleAtBase;
			public float maxScaleAtBase;
			public float minAngleVariance;
			public float maxAngleVariance;
			public float range;
			public float twirl;
			public float strength;
			public AnimationCurve scaleCurve;
		}
		#endregion

		#region Vars
		protected Dictionary<int, BranchInfo> _branchInfos = new Dictionary<int, BranchInfo> ();
		public float angleTolerance = 200f;
		float segmentPosition = 0f;
		float tTwirlAngle = 0f;
		public ShapeDescriptorCollection shapeCollection;
		public float shapeScaleMultiplier = 1f;
		public float adherenceToHierarchyScale = 1f;
		public float girthAtHierarchyBase = 1f;
		public float globalScale = 1f;
		#endregion

		#region Interface
		public virtual void SetAngleTolerance (float angleTolerance) {
			//this.angleTolerance = angleTolerance * 2.5f;
		}
		public virtual float GetAngleTolerance () {
			return angleTolerance;
		}
		public virtual void SetGlobalScale (float globalScale) { this.globalScale = globalScale; }
		public virtual float GetGlobalScale () { return this.globalScale; }
		/// <summary>
		/// Get the branch mesh builder type.
		/// </summary>
		/// <returns>Branch mesh builder type.</returns>
		public virtual BranchMeshBuilder.BuilderType GetBuilderType () {
			return BranchMeshBuilder.BuilderType.Shape;
		}
		/// <summary>
		/// Called right after a BranchSkin is created.
		/// </summary>
		/// <param name="rangeIndex">Index of the branch skin range to process.</param>
		/// <param name="branchSkin">BranchSkin instance to process.</param>
		/// <param name="firstBranch">The first branch instance on the BranchSkin instance.</param>
		/// <param name="parentBranchSkin">Parent BranchSkin instance to process.</param>
		/// <param name="parentBranch">The parent branch of the first branch of the BranchSkin instance.</param>
		/// <returns>True if any processing gets done.</returns>
		public virtual bool PreprocessBranchSkinRange (
			int rangeIndex, 
			BranchMeshBuilder.BranchSkin branchSkin, 
			BroccoTree.Branch firstBranch, 
			BranchMeshBuilder.BranchSkin parentBranchSkin = null, 
			BroccoTree.Branch parentBranch = null)
		{
			bool result = false;
			float normalizedPosition = 0;
			// If the selected range index is a Shape type.
			if (rangeIndex < branchSkin.ranges.Count && branchSkin.ranges[rangeIndex].builderType == BranchMeshBuilder.BuilderType.Shape) {
				ShapeDescriptor shape = shapeCollection.shapes[0];
				// For each shape position...
				for (int j = 0; j < shape.positions.Count; j++) {
					// get the normalized position to branch skin...
					if (shape.hasCap) {
						normalizedPosition = NormalizeRangePositionToBranchSkin (shape.positions[j], branchSkin.ranges[rangeIndex], shape);
					} else {
						normalizedPosition = NormalizeRangePositionToBranchSkin (shape.positions[j], branchSkin.ranges[rangeIndex]);
					}
					// and add it to relevant positions.
					branchSkin.AddRelevantPosition (normalizedPosition, 0.01f);
					/*
					bool added = branchSkin.AddRelevantPosition (normalizedPosition, 0f);
					if (added) {
						Debug.Log ("  added...");
					} else {
						Debug.Log ("  not added...");
					}
					*/
				}
				result = true;
			}
			// TODO RE: do this once, not per range.
			// If the branch has a parent...
			if (parentBranchSkin != null) {
				// set the position offset from the parent.
				firstBranch.positionOffset = GetPositionOffset (firstBranch, parentBranchSkin, parentBranch); // Get Surface Skin position.
			}
			return result;
		}
		/// <summary>
		/// Called per branchskin after the main mesh has been processed. Modifies an additional mesh to merge it with the one processed.
		/// </summary>
		/// <param name="mesh">Mesh to process.</param>
		/// <param name="rangeIndex">Index of the branch skin range to process.</param>
		/// <param name="branchSkin">BranchSkin instance to process.</param>
		/// <param name="firstBranch">The first branch instance on the BranchSkin instance.</param>
		/// <param name="parentBranchSkin">Parent BranchSkin instance to process.</param>
		/// <param name="parentBranch">The parent branch of the first branch of the BranchSkin instance.</param>
		/// <returns>True if any processing gets done.</returns>
		public virtual Mesh PostprocessBranchSkinRange (Mesh mesh, int rangeIndex, BranchMeshBuilder.BranchSkin branchSkin, BroccoTree.Branch firstBranch, BranchMeshBuilder.BranchSkin parentBranchSkin = null, BroccoTree.Branch parentBranch = null) {
			return null;
		}
		#endregion

		#region Vertices
		public virtual Vector3[] GetPolygonAt (
			BranchMeshBuilder.BranchSkin branchSkin,
			int segmentIndex,
			ref List<float> radialPositions,
			float scale,
			float radiusScale = 1f)
		{
			// BranchInfo class contains information to process trunk mesh.
			/*
			if (_branchInfos.ContainsKey (branchSkin.id)) {
				BranchInfo branchInfo = _branchInfos [branchSkin.id];
				segmentPosition = branchInfo.scaleCurve.Evaluate (Mathf.InverseLerp (0f, branchInfo.range, branchSkin.positionsAtSkin [segmentIndex]));
				tTwirlAngle = branchInfo.twirl * 2f * Mathf.PI;
			}
			*/
			segmentPosition = branchSkin.positionsAtSkin [segmentIndex];
			BranchMeshBuilder.BranchSkinRange range;
			segmentPosition = branchSkin.TranslateToPositionAtBuilderRange (segmentPosition, out range);
			ShapeDescriptor shape = shapeCollection.GetShape (range.shapeId);

			Vector3[] polygon = GetPolygonAt (range, shape, branchSkin.id, branchSkin.centers [segmentIndex], branchSkin.directions [segmentIndex], branchSkin.normals [segmentIndex],
				branchSkin.girths [segmentIndex], branchSkin.segments [segmentIndex], ref radialPositions, scale, radiusScale);
			//Debug.Log ("GetPolygonAt " + segmentIndex + ", segments: " + polygon.Length);
			return polygon;
		}
		/// <summary>
		/// Get an array of vertices around a center point with some rotation.
		/// </summary>
		/// <param name="branchSkinId">Id of the branch.</param>
		/// <param name="center">Center of the polygon</param>
		/// <param name="lookAt">Look at rotation.</param>
		/// <param name="normal"></param>
		/// <param name="radius">Radius of the polygon.</param>
		/// <param name="polygonSides">Number of sides for the polygon.</param>
		/// <param name="scale">Global scale to apply to the polygon.</param>
		/// <param name="radialPositions"></param>
		/// <param name="isBase"></param>
		/// <param name="radiusScale"></param>
		/// <returns>Vertices for the polygon <see cref="System.Collections.Generic.List`1[[UnityEngine.Vector3]]"/> points.</returns>
		Vector3[] GetPolygonAt (
			BranchMeshBuilder.BranchSkinRange range,
			ShapeDescriptor shape,
			int branchSkinId,
			Vector3 center, 
			Vector3 lookAt, 
			Vector3 normal, 
			float radius, 
			int polygonSides,
			ref List<float> radialPositions,
			float scale,
			float radiusScale = 1f)
		{
			center *= scale;
			radius *= scale * radiusScale;
			
			Vector3 [] polygonVertices = new Vector3 [0];
			BezierCurve bezierCurve;
			bezierCurve = GetShapeSegment (shapeCollection, NormalizedShapePositionToShapePosition (segmentPosition, range, shape));
			//bezierCurve = GetShapeSegment (shapeCollection, segmentPosition);
			
				 // TODO Optimize
			float tAngle = 0f;
			float cos, sin;

			if (polygonSides >= 3) {
				List<CurvePoint> points;
				if (GlobalSettings.experimentalMergeCurvePointsByDistanceEnabled) {
					points = BezierCurve.MergeCurvePointsByDistance (bezierCurve.GetPoints (angleTolerance), 0.05f);
				} else {
					points = bezierCurve.GetPoints (angleTolerance);
				}
				//List<CurvePoint> points = bezierCurve.GetPoints (angleTolerance);
				polygonVertices = new Vector3 [points.Count - 1];
				for (int i = 0; i < points.Count - 1; i++) {
					//Debug.Log ("Point " + i + ", pos: " + points[i].relativePosition);
					//Vector3 point = points[i].position * scale;
					Vector3 point = points[i].position;
					radialPositions.Add (points[i].relativePosition);
					
					//Add rotation.
					tAngle = Mathf.Lerp (tTwirlAngle, 0f, segmentPosition);
					//Debug.Log ("Segment position: " + segmentPosition);

					cos = Mathf.Cos (tAngle);
					sin = Mathf.Sin (tAngle);
					//point = new Vector3 (cos * point.x - sin * point.y, sin * point.x + cos * point.y);
					point = new Vector3 (cos * point.x - sin * point.z, sin * point.x + cos * point.z);

					Quaternion rotation = Quaternion.LookRotation (lookAt, normal);
					point = rotation * point;
					//polygonVertices [i] = point * Mathf.Lerp (girthAtHierarchyBase, radius, adherenceToHierarchyScale) * shapeScaleMultiplier + center;
					//polygonVertices [i] = point * radius * shapeScaleMultiplier + center;
					//polygonVertices [i] = point * Mathf.Lerp (girthAtHierarchyBase * scale, radius, adherenceToHierarchyScale) * shapeScaleMultiplier + center;
					polygonVertices [i] = point * Mathf.Lerp (girthAtHierarchyBase * globalScale, radius, adherenceToHierarchyScale) * shapeScaleMultiplier + center;
				}
			} else {
				Debug.LogError ("Polygon sides is expected to be equal or greater than 3.");
			}
			return polygonVertices;
		}
		/// <summary>
		/// Gets the number of segments (like polygon sides) as resolution for a branch position.
		/// </summary>
		/// <param name="branchSkin">BranchSkin instance.</param>
		/// <param name="branchSkinPosition">Position along the BranchSkin instance.</param>
		/// <param name="branchAvgGirth">Branch average girth.</param>
		/// <returns>The number polygon sides.</returns>
		public virtual int GetNumberOfSegments (BranchMeshBuilder.BranchSkin branchSkin, float branchSkinPosition, float branchAvgGirth) {
			float tPosition = 0;
			tPosition = branchSkinPosition;
			BranchMeshBuilder.BranchSkinRange range;
			tPosition = branchSkin.TranslateToPositionAtBuilderRange (tPosition, out range);
			ShapeDescriptor shape = shapeCollection.GetShape (range.shapeId);
			float tNormalizedPosition = NormalizedShapePositionToShapePosition (tPosition, range, shape);
			BezierCurve bezierCurve = GetShapeSegment (shapeCollection, tNormalizedPosition);
			List<CurvePoint> points;
			if (GlobalSettings.experimentalMergeCurvePointsByDistanceEnabled) {
				points = BezierCurve.MergeCurvePointsByDistance (bezierCurve.GetPoints (angleTolerance), 0.05f);
			} else {
				points = bezierCurve.GetPoints (angleTolerance);
			}
			Vector3 point;
			float scaleFactor;
			for (int i = 0; i < points.Count; i++) {
				scaleFactor = Mathf.Lerp (girthAtHierarchyBase * globalScale, points[i].girth * globalScale, adherenceToHierarchyScale) * shapeScaleMultiplier;
				point = new Vector3 (points[i].position.x * scaleFactor, points[i].position.z * scaleFactor, 0f);
				branchSkin.AddShapeSegmentVertex (point, points[i].relativePosition);
			}
			//Debug.Log ("BS: " + branchSkin.id + ", BSPsos: " + branchSkinPosition + ", rangePos: " + tPosition + ", shapePos: " + tNormalizedPosition);
			//Debug.Log ("ShapeMeshBuilder: number of segments: " + (points.Count - 1));
			return points.Count - 1;
		}
		#endregion

		#region Bezier Curve
		/// <summary>
		/// Get the segment bezier curve corresponding to a shape positions.
		/// </summary>
		/// <param name="shapeDescriptorCollection">Collection of shapes to select the shape from.</param>
		/// <param name="segmentPosition">Position form 0 to 1 in the length of the shape.</param>
		/// <returns>Segment bezier curve.</returns>
		public BezierCurve GetShapeSegment (ShapeDescriptorCollection shapeDescriptorCollection, float shapeSegmentPosition) {
			BezierCurve shapeSegment;
			ShapeDescriptor shape = shapeDescriptorCollection.shapes [0];
			int shapeSegmentIndex;
			for (shapeSegmentIndex = 0; shapeSegmentIndex < shape.positions.Count - 1; shapeSegmentIndex++) {
				if (shape.positions [shapeSegmentIndex + 1] >= shapeSegmentPosition) {
					break;
				}
			}
			float newSegmentIndex = Mathf.InverseLerp (shape.positions[shapeSegmentIndex], shape.positions[shapeSegmentIndex + 1], shapeSegmentPosition);
			shapeSegment = BezierCurve.Lerp (shape.segments[shapeSegmentIndex], shape.segments[shapeSegmentIndex + 1], newSegmentIndex); // TODO: optimize with cache for lerp curves.
			return shapeSegment;
		}
		Vector3 GetPositionOffset (BroccoTree.Branch branch, BranchMeshBuilder.BranchSkin parentBranchSkin, BroccoTree.Branch parentBranch) {
			return GetBranchSkinPositionOffset (branch.position, parentBranch, branch.rollAngle, branch.forward, parentBranchSkin);
		}
		public virtual Vector3 GetBranchSkinPositionOffset (float positionAtBranch, BroccoTree.Branch branch, float rollAngle, Vector3 forward, BranchMeshBuilder.BranchSkin branchSkin) {
			Vector3 positionOffset = Vector3.zero;
			float positionAtBranchSkin = BranchMeshBuilder.BranchSkin.TranslateToPositionAtBranchSkin (positionAtBranch, branch.id, branchSkin);
			if (positionAtBranchSkin >= 0) {
				BranchMeshBuilder.BranchSkinRange range;
				float segmentPosition = branchSkin.TranslateToPositionAtBuilderRange (positionAtBranchSkin, out range);
				BezierCurve bezierCurve = GetShapeSegment (shapeCollection, segmentPosition);
				//float shapeRadius = bezierCurve.GetPositionAt (0.75f).magnitude;
				float shapeAnglePosition = (rollAngle + Mathf.PI / 2f) / (Mathf.PI * 2f);
				shapeAnglePosition %= 1;
				float shapeRadius = bezierCurve.GetPositionAt (shapeAnglePosition).magnitude;
				float girthRadius = branch.GetGirthAtPosition (positionAtBranch);
				//positionOffset = forward.normalized * shapeRadius * Mathf.Lerp (girthAtHierarchyBase, girthRadius, adherenceToHierarchyScale) * shapeScaleMultiplier;
				//positionOffset = forward.normalized * shapeRadius * shapeScaleMultiplier;
				//positionOffset = forward.normalized * shapeRadius * Mathf.Lerp (girthAtHierarchyBase * globalScale, girthRadius, adherenceToHierarchyScale) * shapeScaleMultiplier * globalScale;
				positionOffset = forward.normalized * Mathf.Lerp (girthAtHierarchyBase * shapeRadius, girthRadius * shapeRadius, adherenceToHierarchyScale) * shapeScaleMultiplier;
			}
			return positionOffset * 0.85f;
		}
		float NormalizeRangePositionToBranchSkin (float shapePosition, BranchMeshBuilder.BranchSkinRange range) {
			return Mathf.Lerp (range.from, range.to, shapePosition);
		}
		float NormalizeRangePositionToBranchSkin (
			float shapePosition, 
			BranchMeshBuilder.BranchSkinRange range, 
			ShapeDescriptor shape)
		{
			// Normalize positions according to
			// range length
			// girth at top
			// girth at bottom
			float newShapePosition = shapePosition;
			if (shapePosition < 0f) {
				// Shape position is at bottom cap.
				newShapePosition = Mathf.Lerp (0f, range.bottomCap, Mathf.InverseLerp (shape.minBottomCapPos, 0f, shapePosition));
			} else if (shapePosition > 1f) {
				// Shape position is at top cap.
				newShapePosition = Mathf.Lerp (range.topCap, 1f, Mathf.InverseLerp (1f, shape.maxTopCapPos, shapePosition));
			} else {
				// Shape position is at middle range.
				newShapePosition = Mathf.Lerp (range.bottomCap, range.topCap, shapePosition);
			}
			return newShapePosition;
		}
		float NormalizedShapePositionToShapePosition (float normalizedShapePosition, BranchMeshBuilder.BranchSkinRange range, ShapeDescriptor shape) {
			float shapePosition = normalizedShapePosition;
			if (normalizedShapePosition < range.bottomCap) {
				float bottomPos = Mathf.InverseLerp (0f, range.bottomCap, normalizedShapePosition);
				shapePosition = Mathf.Lerp (shape.minBottomCapPos, 0f, bottomPos);
			} else if (normalizedShapePosition > range.topCap) {
				float topPos = Mathf.InverseLerp (range.topCap, 1f, normalizedShapePosition);
				shapePosition = Mathf.Lerp (1f, shape.maxTopCapPos, topPos);
			} else {
				shapePosition = Mathf.InverseLerp (range.bottomCap, range.topCap, normalizedShapePosition);
			}
			return shapePosition;
		}
		float GetGirthAtBranchSkinPosition (float position, BranchMeshBuilder.BranchSkin branchSkin, BroccoTree.Branch firstBranch, BranchMeshBuilder.BranchSkinRange range) {
			float radius = branchSkin.GetGirthAtPosition (position, firstBranch);
			return Mathf.Lerp (girthAtHierarchyBase, radius, adherenceToHierarchyScale) * shapeScaleMultiplier;
		}
		#endregion
	}
}