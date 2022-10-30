using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Serialization;

namespace Broccoli.Pipe {
	/// <summary>
	/// Class with directives to construct meshes for sprouts.
	/// </summary>
	[System.Serializable]
	public class SproutMesh {
		#region Vars
		/// <summary>
		/// Sprout group identifier.
		/// </summary>
		public int groupId = 0;
		/// <summary>
		/// Active subgroups for this sprout mesh.
		/// </summary>
		[System.NonSerialized]
		public int[] subgroups = new int[0];
		/// <summary>
		/// Modes available to mesh sprouts.
		/// </summary>
		public enum MeshingMode {
			Shape = 0,
			//Mesh = 1,
			BranchCollection = 2
		}
		/// <summary>
		/// Mode to mesh sprouts.
		/// </summary>
		public MeshingMode meshingMode = MeshingMode.Shape;
		/// <summary>
		/// Modes available for the shape mode.
		/// </summary>
		public enum ShapeMode
		{
			Plane = 0,
			Cross = 1,
			Tricross = 2,
			//Billboard = 3, // Deprecated, mode for Unity Tree Creator
			Mesh = 4,
			PlaneX = 5,
			GridPlane = 6
		}
		/// <summary>
		/// Mode for the sprout shape mesh.
		/// </summary>
		[FormerlySerializedAs("mode")]
		public ShapeMode shapeMode = ShapeMode.Plane;
		/// <summary>
		/// The horizontal alignment (perpendicular to gravity) for sprouts at the base of the parent branch.
		/// </summary>
		public float horizontalAlignAtBase = 0f;
		/// <summary>
		/// The horizontal alignment (perpendicular to gravity) for sprouts at the top of the parent branch.
		/// </summary>
		public float horizontalAlignAtTop = 0f;
		/// <summary>
		/// The horizontal alignment curve, from base (left) to top (right).
		/// </summary>
		public AnimationCurve horizontalAlignCurve = AnimationCurve.Linear (0f, 0f, 1f, 1f);
		/// <summary>
		/// The scale of the sprout mesh at the base of the parent branch.
		/// </summary>
		public float scaleAtBase = 1f;
		/// <summary>
		/// The scale of the sprout mesh at the top of the parent branch.
		/// </summary>
		public float scaleAtTop = 1f;
		/// <summary>
		/// The scale curve.
		/// </summary>
		public AnimationCurve scaleCurve = AnimationCurve.Linear (0f, 0f, 1f, 1f);
		/// <summary>
		/// Mesh created for internal use on building process.
		/// </summary>
		[System.NonSerialized]
		public Mesh processedMesh = null;
		/// <summary>
		/// Plane is double sided.
		/// </summary>
		public bool isTwoSided = true;
		/// <summary>
		/// Branch Collection asset, used to load sprout meshes to populate the tree.
		/// </summary>
		public ScriptableObject branchCollection = null;
		#endregion

		#region Planes mode
		/// <summary>
		/// Width of the sprout mesh.
		/// </summary>
		public float width = 1f;
		/// <summary>
		/// Height of the sprout mesh.
		/// </summary>
		public float height = 1f;
		/// <summary>
		/// The x coordinate on the mesh to be the sprout point of origin.
		/// </summary>
		public float pivotX = 0.5f;
		/// <summary>
		/// The y coordinate on the mesh to be the sprout point of origin.
		/// </summary>
		public float pivotY = 0f;
		/// <summary>
		/// If true then height is adjusted to the assigned texture dimension ratio.
		/// </summary>
		public bool overrideHeightWithTexture = false;
		/// <summary>
		/// If true then the texture mapping is checked against other sprouts coming from
		/// the same texture atlas; a scale is applied according to its size on the atlas
		/// relative to the biggest mapping on that particular atlas.
		/// </summary>
		public bool includeScaleFromAtlas = false;
		#endregion

		#region Mesh mode
		/// <summary>
		/// The mesh game object to copy the mesh from.
		/// </summary>
		public GameObject meshGameObject;
		/// <summary>
		/// The mesh scale.
		/// </summary>
		public Vector3 meshScale = Vector3.one;
		/// <summary>
		/// The mesh rotation.
		/// </summary>
		public Vector3 meshRotation = Vector3.zero;
		/// <summary>
		/// Offset from the mesh center.
		/// </summary>
		public Vector3 meshOffset = Vector3.zero;
		#endregion

		#region Billboard mode
		/// <summary>
		/// If true then the billboard mesh is placed at the point of origin of the sprout.
		/// </summary>
		public bool billboardAtOrigin = false;
		/// <summary>
		/// The billboard rotation at top of the parent branch.
		/// </summary>
		public float billboardRotationAtTop = 0f;
		/// <summary>
		/// The billboard rotation at top of the parent branch.
		/// </summary>
		public float billboardRotationAtBase = 0f;
		/// <summary>
		/// The billboard rotation curve, from base (left) to top (right).
		/// </summary>
		public AnimationCurve billboardRotationCurve = AnimationCurve.Linear (0f, 0f, 1f, 1f);
		#endregion

		#region Plane X
		/// <summary>
		/// Depth of the plane.
		/// </summary>
		public float depth = 0f; //TODO: add atBase, atTop and curve.
		/// <summary>
		/// The size of the inner plane.
		/// </summary>
		public float innerPlaneSize = 1f;
		#endregion

		#region Grid Plane mode
		/// <summary>
		/// Number of segments for the width of the plane.
		/// </summary>
		[Range (1, 10)]
		public int resolutionWidth = 1;
		/// <summary>
		/// Number of segments for the height of the plane.
		/// </summary>
		[Range (1,10)]
		public int resolutionHeight = 1;
		/// <summary>
		/// The gravity bending (perpendicular to gravity) for sprouts at the base of the parent branch.
		/// </summary>
		public float gravityBendingAtBase = 0f;
		/// <summary>
		/// The gravity bending (perpendicular to gravity) for sprouts at the top of the parent branch.
		/// </summary>
		public float gravityBendingAtTop = 0f;
		/// <summary>
		/// The gravity bending curve, from base (left) to top (right).
		/// </summary>
		public AnimationCurve gravityBendingCurve = AnimationCurve.Linear (0f, 0f, 1f, 1f);
		/// <summary>
		/// Additional multiplier factor for gravity bending to apply in the middle of the sprout mesh.
		/// </summary>
		[Range (-0.5f, 0.5f)]
		public float gravityBendingMultiplierAtMiddle = 0f;
		/// <summary>
		/// Gravity bending on the sides along the length of the sprout mesh at the base or the parent branch.
		/// </summary>
		[Range(-1,1)]
		public float sideGravityBendingAtBase = 0f;
		/// <summary>
		/// Gravity bending on the sides along the length of the sprout mesh at the top or the parent branch.
		/// </summary>
		[Range(-1,1)]
		public float sideGravityBendingAtTop = 0f;
		/// <summary>
		/// Distribution of the side gravity bending on a sprout mesh.
		/// </summary>
		public AnimationCurve sideGravityBendingShape = AnimationCurve.Linear (0f, 0f, 1f, 1f);
		#endregion

		#region Cloning
		/// <summary>
		/// Clone this instance.
		/// </summary>
		public SproutMesh Clone () {
			SproutMesh clone = new SproutMesh();
			clone.groupId = groupId;
			clone.meshingMode = meshingMode;
			clone.shapeMode = shapeMode;
			clone.meshGameObject = meshGameObject;
			clone.meshScale = meshScale;
			clone.meshRotation = meshRotation;
			clone.meshOffset = meshOffset;
			clone.billboardAtOrigin = billboardAtOrigin;
			clone.billboardRotationAtTop = billboardRotationAtTop;
			clone.billboardRotationAtBase = billboardRotationAtBase;
			clone.billboardRotationCurve = new AnimationCurve (billboardRotationCurve.keys);
			clone.depth = depth;
			clone.innerPlaneSize = innerPlaneSize;
			clone.resolutionWidth = resolutionWidth;
			clone.resolutionHeight = resolutionHeight;
			clone.gravityBendingAtTop = gravityBendingAtTop;
			clone.gravityBendingAtBase = gravityBendingAtBase;
			clone.gravityBendingCurve = new AnimationCurve (gravityBendingCurve.keys);
			clone.width = width;
			clone.height = height;
			clone.overrideHeightWithTexture = overrideHeightWithTexture;
			clone.includeScaleFromAtlas = includeScaleFromAtlas;
			clone.horizontalAlignAtBase = horizontalAlignAtBase;
			clone.horizontalAlignAtTop = horizontalAlignAtTop;
			clone.horizontalAlignCurve = new AnimationCurve (horizontalAlignCurve.keys);
			clone.scaleAtBase = scaleAtBase;
			clone.scaleAtTop = scaleAtTop;
			clone.scaleCurve = new AnimationCurve (scaleCurve.keys);
			clone.gravityBendingMultiplierAtMiddle = gravityBendingMultiplierAtMiddle;
			clone.sideGravityBendingAtBase = sideGravityBendingAtBase;
			clone.sideGravityBendingAtTop = sideGravityBendingAtTop;
			clone.sideGravityBendingShape = new AnimationCurve (sideGravityBendingShape.keys);
			clone.branchCollection = branchCollection;
			return clone;
		}
		#endregion
	}
}