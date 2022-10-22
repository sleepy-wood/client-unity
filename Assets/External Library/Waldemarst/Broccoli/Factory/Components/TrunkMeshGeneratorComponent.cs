using System.Collections.Generic;

using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

using Broccoli.Factory;
using Broccoli.Builder;
using Broccoli.Manager;
using Broccoli.Pipe;
using Broccoli.Model;

namespace Broccoli.Component
{
	/// <summary>
	/// Trunk mesh generator component.
	/// </summary>
	public class TrunkMeshGeneratorComponent : TreeFactoryComponent {
		#region Vars
		TrunkMeshGeneratorElement trunkMeshGeneratorElement;
		NativeArray<Vector3> m_Vertices;
		NativeArray<Vector3> m_Normals;

		Vector3[] m_ModifiedVertices;
		Vector3[] m_ModifiedNormals;
		#endregion

		#region Job
		struct TrunkJob : IJobParallelFor {
			public NativeArray<Vector3> vertices;
			public NativeArray<Vector3> normals;
			/// <summary>
			/// UV5 information of the mesh.
			/// x: radial position.
			/// y: global length position.
			/// z: girth.
			/// w: unallocated.
			/// </summary>
			public NativeArray<Vector4> uv5s;
			/// <summary>
			/// UV6 information of the mesh.
			/// x: id of the branch.
			/// y: id of the branch skin.
			/// z: id of the struct.
			/// w: tuned.
			/// </summary>
			public NativeArray<Vector4> uv6s;
			/// <summary>
			/// UV7 information of the mesh.
			/// x, y, z: center.
			/// w: unallocated.
			/// </summary>
			public NativeArray<Vector4> uv7s;
			/// <summary>
			/// UV8 information of the mesh.
			/// x, y, z: direction.
			/// w: unallocated.
			/// </summary>
			public NativeArray<Vector4> uv8s;

			public int branchSkinId;
			public float maxLength;
			public float minLength;
			public float scaleAtBase;
			[NativeDisableParallelForRestriction]
			public NativeArray<float> baseRadialPositions;
			[NativeDisableParallelForRestriction]
			public NativeArray<Vector2> basePositions;
			[NativeDisableParallelForRestriction]
			public NativeArray<float> scalePoints;
			[NativeDisableParallelForRestriction]
			public NativeArray<float> scalePointVals;
			public float sinTime;
			public float cosTime;
			public float strength;
			public float branchRoll;
			public float twirl;

			public void Execute(int i) {
				if (uv6s[i].y == branchSkinId && 
					uv5s[i].y + 0.01f >= minLength && uv5s[i].y - 0.01f <= maxLength) {
					float absPos = 1f - ((uv5s[i].y - minLength) / (maxLength - minLength));
					float pos = EvalPos (absPos);
					Vector3 branchNormal = Quaternion.AngleAxis (branchRoll * Mathf.Rad2Deg, uv8s[i]) * Vector3.forward;
					Vector2 radialPos = GetRadialPoint (uv5s[i].x);
					Vector3 newVertex = Vector3.Lerp (vertices[i], (Vector3)uv7s[i] + (Quaternion.LookRotation (uv8s[i], branchNormal) * radialPos), pos);
					Quaternion axisRotation = Quaternion.AngleAxis (twirl * Mathf.Rad2Deg * absPos, uv8s[i]);
					vertices[i] = axisRotation * (newVertex - (Vector3)uv7s[i]) + (Vector3)uv7s[i];
					normals[i] = axisRotation * normals[i];
				}
			}
			public Vector2 GetRadialPoint (float radialPosition) {
				if (radialPosition > 0 && radialPosition < 1) {
					int i;
					for (i = 0; i < baseRadialPositions.Length; i++) {
						if (radialPosition < baseRadialPositions [i]) {
							break;
						}
					}
					return basePositions [i];
				} else if (radialPosition == 1) {
					return basePositions [baseRadialPositions.Length - 1];
				} else {
					return basePositions [0];
				}
			}
			public float EvalPos (float pos) {
				for (int i = 0; i < scalePoints.Length; i++) {
					if (pos <= scalePoints [i]) return scalePointVals [i];
				}
				return pos;
			}
		}
		#endregion

		#region Configuration
		/// <summary>
		/// Prepares the parameters to process with this component.
		/// </summary>
		/// <param name="treeFactory">Tree factory.</param>
		/// <param name="useCache">If set to <c>true</c> use cache.</param>
		/// <param name="useLocalCache">If set to <c>true</c> use local cache.</param>
		/// <param name="processControl">Process control.</param>
		protected override void PrepareParams (TreeFactory treeFactory,
			bool useCache = false, 
			bool useLocalCache = false, 
			TreeFactoryProcessControl processControl = null) 
		{
			base.PrepareParams (treeFactory, useCache, useLocalCache, processControl);
		}
		/// <summary>
		/// Gets the changed aspects on the tree for this component.
		/// </summary>
		/// <returns>The changed aspects.</returns>
		public override int GetChangedAspects () {
			return (int)TreeFactoryProcessControl.ChangedAspect.Mesh;
		}
		/// <summary>
		/// Clear this instance.
		/// </summary>
		public override void Clear ()
		{
			base.Clear ();
		}
		#endregion

		#region Processing
		/// <summary>
		/// Process the tree according to the pipeline element.
		/// </summary>
		/// <param name="treeFactory">Parent tree factory.</param>
		/// <param name="useCache">If set to <c>true</c> use cache.</param>
		/// <param name="useLocalCache">If set to <c>true</c> use local cache.</param>
		/// <param name="processControl">Process control.</param>
		public override bool Process (TreeFactory treeFactory, 
			bool useCache = false, 
			bool useLocalCache = false, 
			TreeFactoryProcessControl processControl = null) {
			if (pipelineElement != null && treeFactory != null) {
				// Get the trunk element.
				trunkMeshGeneratorElement = pipelineElement as TrunkMeshGeneratorElement;
				// Prepare the parameters.
				PrepareParams (treeFactory, useCache, useLocalCache, processControl);
				// Get the Trunk mesh builder.
				BranchMeshBuilder branchMeshBuilder = BranchMeshBuilder.GetInstance ();
				TrunkMeshBuilder trunkMeshBuilder = (TrunkMeshBuilder) branchMeshBuilder.GetBranchMeshBuilder (BranchMeshBuilder.BuilderType.Trunk);
				// Process each branch skin.
				if (trunkMeshBuilder != null) {

					var enumerator = trunkMeshBuilder.branchInfos.GetEnumerator();
					while (enumerator.MoveNext()) {
						int branchSkinId = enumerator.Current.Key;

						Mesh mesh = treeFactory.meshManager.GetMesh (MeshManager.MeshData.Type.Branch);

						// Mark mesh as dynamic.
						mesh.MarkDynamic ();

						// Create job and set variables.
						TrunkJob trunkJob = new TrunkJob ();
						trunkJob.branchSkinId = branchSkinId;
						trunkJob.maxLength = enumerator.Current.Value.rangeLength;
						trunkJob.minLength = 0f;
						trunkJob.scaleAtBase = enumerator.Current.Value.scaleAtBase;
						trunkJob.sinTime = Mathf.Sin(Time.time);
						trunkJob.cosTime = Mathf.Cos(Time.time);
						trunkJob.strength = 0.4f;
						trunkJob.branchRoll = enumerator.Current.Value.branchRoll;
						trunkJob.twirl = Random.Range (trunkMeshGeneratorElement.minDisplacementTwirl, trunkMeshGeneratorElement.maxDisplacementTwirl);

						// Define scale points. This way we export the scale cuve to the job system.
						int scaleCurveRes = 25;
						float scaleCurveStep = 1f / scaleCurveRes;
						List<float> scalePoints = new List<float> ();
						List<float> scalePointVals = new List<float> ();
						for (int i = 0; i < scaleCurveRes + 1; i++) {
							scalePoints.Add (i * scaleCurveStep);
							scalePointVals.Add (trunkMeshGeneratorElement.scaleCurve.Evaluate (i * scaleCurveStep));
						}
						trunkJob.scalePoints = new NativeArray<float> (scalePoints.ToArray (), Allocator.TempJob);
						trunkJob.scalePointVals = new NativeArray<float> (scalePointVals.ToArray (), Allocator.TempJob);

						BezierCurve baseCurve = trunkMeshBuilder.baseCurves [branchSkinId];
						trunkJob.baseRadialPositions = new NativeArray<float> (baseCurve.points.Count, Allocator.TempJob);
						trunkJob.basePositions = new NativeArray<Vector2> (baseCurve.points.Count, Allocator.TempJob);
						Quaternion roll = Quaternion.AngleAxis (enumerator.Current.Value.branchRoll * Mathf.Rad2Deg, Vector3.forward);
						for (int i = 0; i < baseCurve.points.Count; i++) {
							CurvePoint cp = baseCurve.points [i];
							trunkJob.baseRadialPositions [i] = cp.relativePosition;
							trunkJob.basePositions [i] = roll * cp.position;
						}

						m_Vertices = new NativeArray<Vector3>(mesh.vertices, Allocator.TempJob);
						m_Normals = new NativeArray<Vector3>(mesh.normals, Allocator.TempJob);
						m_ModifiedVertices = new Vector3[m_Vertices.Length];
						m_ModifiedNormals = new Vector3[m_Vertices.Length];
						trunkJob.vertices = m_Vertices;
						trunkJob.normals = m_Normals;

						List<Vector4> uv5s = new List<Vector4> ();
						mesh.GetUVs (4, uv5s);
						trunkJob.uv5s = new NativeArray<Vector4> (uv5s.ToArray (), Allocator.TempJob);
						List<Vector4> uv6s = new List<Vector4> ();
						mesh.GetUVs (5, uv6s);
						trunkJob.uv6s = new NativeArray<Vector4> (uv6s.ToArray (), Allocator.TempJob);
						List<Vector4> uv7s = new List<Vector4> ();
						mesh.GetUVs (6, uv7s);
						trunkJob.uv7s = new NativeArray<Vector4> (uv7s.ToArray (), Allocator.TempJob);
						List<Vector4> uv8s = new List<Vector4> ();
						mesh.GetUVs (7, uv8s);
						trunkJob.uv8s = new NativeArray<Vector4> (uv8s.ToArray (), Allocator.TempJob);

						// Execute job.
						JobHandle uvJobHandle = trunkJob.Schedule (uv5s.Count, 64);

						// Complete job.
						uvJobHandle.Complete ();

						trunkJob.vertices.CopyTo (m_ModifiedVertices);
						trunkJob.normals.CopyTo (m_ModifiedNormals);

						mesh.vertices = m_ModifiedVertices;
						mesh.normals = m_ModifiedNormals;

						// Dispose.
						trunkJob.vertices.Dispose ();
						trunkJob.normals.Dispose ();
						trunkJob.uv5s.Dispose ();
						trunkJob.uv6s.Dispose ();
						trunkJob.uv7s.Dispose ();
						trunkJob.uv8s.Dispose ();
						trunkJob.scalePoints.Dispose ();
						trunkJob.scalePointVals.Dispose ();
						trunkJob.baseRadialPositions.Dispose ();
						trunkJob.basePositions.Dispose ();
					}
				}
				return true;
			}
			return false;
		}
		#endregion
	}
}