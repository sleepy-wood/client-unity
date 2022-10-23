using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

using Broccoli.Model;
using Broccoli.Factory;

// https://forum.unity.com/threads/building-a-mesh-with-the-job-system-filling-an-array-larger-than-arraylength.572965/
// https://docs.unity3d.com/2020.2/Documentation/ScriptReference/Mesh.MeshData.html
// https://forum.unity.com/threads/build-meshes-with-the-job-system.517423/
// https://forum.unity.com/threads/create-render-mesh-using-job-system.720302/
// https://github.com/keijiro/Voxelman/tree/master/Assets/ECS
// https://forum.unity.com/threads/meshdataarray-and-scheduleparallel.860878/

namespace Broccoli.Builder {
	/// <summary>
	/// Mesh building for branches.
	/// </summary>
	public class BranchMeshBuilder {
		#region Jobs
		/// <summary>
		/// Job structure to process branch skins.
		/// </summary>
		struct BranchJob : IJobParallelFor {
			#region Input
			/// <summary>
			/// Global scale.
			/// </summary>
			public float globalScale;
			/// <summary>
			/// Set the building process to use hard normals.
			/// </summary>
			public bool useHardNormals;
			/// <summary>
			/// Add faces at the base of the mesh.
			/// </summary>
			public bool useMeshCapAtBase;
			/// <summary>
			/// Contains the BRANCH_ID, BRANCH_LENGTH, BRANCH_LENGTH_OFFSET AND SHAPES_OFFSET.
			/// The shapes offset points to the start of the aShapeVertShapePos array for the vertices
			/// corresponding to this branch skin.
			/// </summary>
			public NativeArray<Vector4> bsIdsLengthsLengthOffsetsShapeOffsets;
			/// <summary>
			/// Contains the SEGMENTS_START, SEGMENTS_LENGTHS, VERTICES_START, TRIS_START
			/// </summary>
			public NativeArray<Vector4> segStartLengthVertStartTrisStart;

			/// <summary>
			/// Contains the segments CENTER and POSITION_AT_BRANCH (0-1).
			/// </summary>
			[NativeDisableParallelForRestriction]
			public NativeArray<Vector4> aCentersPos;
			/// <summary>
			/// Contains the segment DIRECTION and POSITION_AT_SKIN (0-1).
			/// </summary>
			[NativeDisableParallelForRestriction]
			public NativeArray<Vector4> aDirectionsPosAtSkins;
			/// <summary>
			/// Contains the segment NORMAL and GIRTH.
			/// </summary>
			[NativeDisableParallelForRestriction]
			public NativeArray<Vector4> aNormalsGirths;
			/// <summary>
			/// Contains the segment BRANCH_ID, STRUCTURE_ID, NUMBER_OF_SEGMENTS and SEGMENT_TYPE.
			/// </summary>
			[NativeDisableParallelForRestriction]
			public NativeArray<Vector4> aIdsStructsSegsSegTypes;
			/// <summary>
			/// Contains the segment BUILDER_TYPE (0 default, 1 trunk, 2 shape).
			/// </summary>
			[NativeDisableParallelForRestriction]
			public NativeArray<int> aBuilders;
			/// <summary>
			/// Contains the segment VERTICES and VERTEX_RADIAL_POS when using shapes,
			/// the vertices are unscaled, unrotated, relative to the center zero,
			/// </summary>
			[NativeDisableParallelForRestriction]
			public NativeArray<Vector4> aShapeVertShapePos;
			#endregion	

			#region Output
			/// <summary>
			/// Vertices for the mesh.
			/// </summary>
			[NativeDisableParallelForRestriction]
			public NativeArray<Vector3> vertices;
			/// <summary>
			/// The triangles array for the mesh.
			/// </summary>
			[NativeDisableParallelForRestriction]
			public NativeArray<int> triangles;
			/// <summary>
			/// Normals for the mesh.
			/// </summary>
			[NativeDisableParallelForRestriction]
			public NativeArray<Vector3> normals;
			/// <summary>
			/// UV information of the mesh.
			/// x: mapping U component.
			/// y: mapping V component.
			/// z: unalloc.
			/// w: unalloc.
			/// </summary>
			[NativeDisableParallelForRestriction]
			public NativeArray<Vector4> uvs;
			/// <summary>
			/// UV3 information of the mesh.
			/// x: vertex x position.
			/// y: vertex y position.
			/// z: vertex z position..
			/// w: vertex z position on the branch origin.
			/// </summary>
			[NativeDisableParallelForRestriction]
			public NativeArray<Vector4> uv3s;
			/// <summary>
			/// UV5 information of the mesh.
			/// x: radial position.
			/// y: global length position.
			/// z: girth.
			/// w: unallocated.
			/// </summary>
			[NativeDisableParallelForRestriction]
			public NativeArray<Vector4> uv5s;
			/// <summary>
			/// UV6 information of the mesh.
			/// x: id of the branch.
			/// y: id of the branch skin.
			/// z: id of the struct.
			/// w: tuned.
			/// </summary>
			[NativeDisableParallelForRestriction]
			public NativeArray<Vector4> uv6s;
			/// <summary>
			/// UV7 information of the mesh.
			/// x, y, z: center.
			/// w: unallocated.
			/// </summary>
			[NativeDisableParallelForRestriction]
			public NativeArray<Vector4> uv7s;
			/// <summary>
			/// UV8 information of the mesh.
			/// x, y, z: direction.
			/// w: unallocated.
			/// </summary>
			[NativeDisableParallelForRestriction]
			public NativeArray<Vector4> uv8s;
			#endregion

			public void Execute (int i) {
				int branchSkinId = (int)bsIdsLengthsLengthOffsetsShapeOffsets [i].x;
				float length = bsIdsLengthsLengthOffsetsShapeOffsets [i].y;
				float lengthOffset = bsIdsLengthsLengthOffsetsShapeOffsets [i].z;
				int shapeVerticesOffset = (int)bsIdsLengthsLengthOffsetsShapeOffsets [i].w;
				int segmentStartAt = (int)segStartLengthVertStartTrisStart [i].x;
				int segmentLength = (int)segStartLengthVertStartTrisStart [i].y;
				int verticesStartAt = (int)segStartLengthVertStartTrisStart [i].z;
				int trisStartAt = (int)segStartLengthVertStartTrisStart [i].w;

				// Send to meshing.
				if (useHardNormals) {

				} else {
					MeshSoftNormals (branchSkinId, length, lengthOffset, 
						segmentStartAt, segmentLength, 
						verticesStartAt, trisStartAt,
						shapeVerticesOffset);
				}
			}

			public void MeshSoftNormals (
				int branchSkinId,
				float length,
				float lengthOffset,
				int segmentStartAt,
				int segmentLength,
				int verticesStartAt,
				int trisStartAt,
				int shapeVerticesStartAt)
			{
				int segmentVertexStartAt = verticesStartAt;
				int shapeVertexStartAt = shapeVerticesStartAt;
				int segmentIndex;
				int prevSegmentSideCount = 0;
				int currSegmentSideCount = 0;
				int baseIndex = 0;
				int topIndex = 0;
				int trisIndex = 0;
				int segmentType = 0;
				int segmentBuilder = 0;

				// Iterate through the segments assigned at the given segment start index.
				for (int i = segmentStartAt; i < segmentStartAt + segmentLength; i++) {
					// Set the segment index.
					segmentIndex = i;
					segmentBuilder = aBuilders [segmentIndex];
					segmentType = (int)aIdsStructsSegsSegTypes [segmentIndex].w; 
					currSegmentSideCount = (int)aIdsStructsSegsSegTypes [segmentIndex].z;

					SetPolygonAt (
						branchSkinId,
						aIdsStructsSegsSegTypes [segmentIndex],
						aCentersPos [segmentIndex], 
						aDirectionsPosAtSkins [segmentIndex], 
						aNormalsGirths [segmentIndex], 
						lengthOffset + aDirectionsPosAtSkins [segmentIndex].w * length,
						segmentVertexStartAt,
						shapeVertexStartAt,
						segmentBuilder);

					// Add to segmentVertexStartAt.
					segmentVertexStartAt += currSegmentSideCount + 1;

					// Add to shape vertices if the builder is a shape type.
					if (segmentBuilder == 2 && segmentType != 4) { // END_CAP
						shapeVertexStartAt += currSegmentSideCount + 1;
					}

					// Add triangles.
					bool isFirstSegment = false;
					if (useMeshCapAtBase && segmentType == BranchMeshBuilder.BranchSkin.SEGMENT_TYPE_BEGIN_CAP) isFirstSegment = true;
					else if (!useMeshCapAtBase && segmentType == BranchMeshBuilder.BranchSkin.SEGMENT_TYPE_BEGIN) isFirstSegment = true;
					if (prevSegmentSideCount > 0 && 
						!isFirstSegment)
					{
						topIndex = segmentVertexStartAt - currSegmentSideCount - 1;
						baseIndex = topIndex - prevSegmentSideCount - 1;
						// Add triangles when current and previous segment have the same number of sides.
						if (prevSegmentSideCount == currSegmentSideCount) {
							for (int j = 0; j < prevSegmentSideCount; j++) {
								triangles [trisStartAt + trisIndex] = topIndex + j + 1;
								triangles [trisStartAt + trisIndex + 1] = topIndex + j;
								triangles [trisStartAt + trisIndex + 2] = baseIndex + j;
								triangles [trisStartAt + trisIndex + 3] = baseIndex + j + 1;
								triangles [trisStartAt + trisIndex + 4] = topIndex + j + 1;
								triangles [trisStartAt + trisIndex + 5] = baseIndex + j;
								trisIndex += 6;
							}
						} else {
							// Add triangles when one current and previous segment have a different number of sides.
							int aSegmentIndex; // A segment has the fewer number of sides.
							int bSegmentIndex; // B segment has the greater number of sides.
							bool inverse = false;
							if (prevSegmentSideCount > currSegmentSideCount) {
								aSegmentIndex = topIndex; // A segment has the fewer number of sides.
								bSegmentIndex = baseIndex; // B segment has the greater number of sides.
							} else {
								aSegmentIndex = baseIndex; // A segment has the fewer number of sides.
								bSegmentIndex = topIndex; // B segment has the greater number of sides.	
								inverse = true;
							}
							float aSegmentValue = uvs [aSegmentIndex].x; // A segment index radial value.
							float bSegmentValue = uvs [bSegmentIndex].x; // B segment index radial value.
							float halfA = (aSegmentValue + uvs [aSegmentIndex + 1].x) /2f; // half value between segment A index and segment A index + 1.
							while (bSegmentValue < 1) {
								if (bSegmentValue < halfA) {
									// Segment B
									triangles [trisStartAt + trisIndex] = bSegmentIndex;
									if (inverse) {
										triangles [trisStartAt + trisIndex + 1] = aSegmentIndex;
										triangles [trisStartAt + trisIndex + 2] = bSegmentIndex + 1;
									} else {
										triangles [trisStartAt + trisIndex + 1] = bSegmentIndex + 1;
										triangles [trisStartAt + trisIndex + 2] = aSegmentIndex;
									}
									trisIndex += 3;
									bSegmentIndex++;
									bSegmentValue = uvs [bSegmentIndex].x;
								} else {
									// Segment A
									triangles [trisStartAt + trisIndex] = bSegmentIndex;
									if (inverse) {
										triangles [trisStartAt + trisIndex + 2] = aSegmentIndex + 1;
										triangles [trisStartAt + trisIndex + 1] = aSegmentIndex;
									} else {
										triangles [trisStartAt + trisIndex + 1] = aSegmentIndex + 1;
										triangles [trisStartAt + trisIndex + 2] = aSegmentIndex;
									}
									trisIndex += 3;
									aSegmentIndex++;
									aSegmentValue = uvs [aSegmentIndex].x;
									if (aSegmentValue < 1f) {
										halfA = (aSegmentValue + uvs [aSegmentIndex + 1].x) /2f;
									} else {
										halfA = aSegmentValue + 0.2f;
									}
								}
							}
							if (aSegmentValue < 1f) {
								triangles [trisStartAt + trisIndex] = bSegmentIndex;
								if (inverse) {
									triangles [trisStartAt + trisIndex + 2] = aSegmentIndex + 1;
									triangles [trisStartAt + trisIndex + 1] = aSegmentIndex;
								} else {
									triangles [trisStartAt + trisIndex + 1] = aSegmentIndex + 1;
									triangles [trisStartAt + trisIndex + 2] = aSegmentIndex;
								}
								trisIndex += 3;
							}
						}
					}

					prevSegmentSideCount = currSegmentSideCount;
				}
			}
			/// <summary>
			/// Get an array of vertices around a center point with some rotation.
			/// </summary>
			/// <returns>Vertices for the polygon <see cref="System.Collections.Generic.List`1[[UnityEngine.Vector3]]"/> points.</returns>
			/// <param name="center_pos">Center of the polygon.</param>
			/// <param name="direction_posAtSkin">Look at rotation.</param>
			/// <param name="girth">Radius of the polygon.</param>
			/// <param name="polygonSides">Number of sides for the polygon.</param>
			void SetPolygonAt (
				int branchSkinId,
				Vector4 id_struct_seg_segType,
				Vector4 center_pos,
				Vector4 direction_posAtSkin, 
				Vector4 normal_girth,
				float lengthOffset,
				int startIndex,
				int shapeStartIndex,
				int segmentBuilder)
			{
				center_pos *= globalScale;
				float localGirth = normal_girth.w * globalScale;

				Vector3 vertex;
				Vector3 vertexNormal = Vector3.zero;
				float radialPosition;
				int indexPos = 0;
				int polygonSides = (int)id_struct_seg_segType.z;
				int segmentType = (int)id_struct_seg_segType.w;
				if (polygonSides >= 3) {
					float angle = (Mathf.PI * 2) / (float)polygonSides;
					float radialAngle = 1f / (float)polygonSides;
					for (int i = 0; i <= polygonSides; i++) {
						// Calculate vertex and normal.
						if (segmentBuilder == 2 && segmentType != 4) {
							vertex = aShapeVertShapePos [shapeStartIndex + i];
							radialPosition = aShapeVertShapePos [shapeStartIndex + i].w;
						} else {
							vertex = new Vector3 (
								Mathf.Cos (angle * i) * localGirth,
								Mathf.Sin (angle * i) * localGirth,
								0f);
							radialPosition = i * radialAngle;
						}
						//Quaternion rotation = Quaternion.LookRotation (direction_posAtSkin, normal_girth);
						Quaternion rotation = Quaternion.LookRotation (direction_posAtSkin, normal_girth);
						vertex = (rotation * vertex) + (Vector3)center_pos;
						if (!useHardNormals) {
							if (segmentType == BranchMeshBuilder.BranchSkin.SEGMENT_TYPE_END_CAP) {
								vertexNormal = direction_posAtSkin.normalized;
							} else if (segmentType == BranchMeshBuilder.BranchSkin.SEGMENT_TYPE_BEGIN_CAP) {
								vertexNormal = -direction_posAtSkin.normalized;
							} else {
								vertexNormal = (vertex - (Vector3)center_pos).normalized;
							}
						}

						// Assign point and normal values.
						vertices [startIndex + indexPos] = vertex;
						normals [startIndex + indexPos] = vertexNormal;
						uvs [startIndex + indexPos] = new Vector4 (radialPosition, lengthOffset, 0f, 0f);
						uv3s [startIndex + indexPos] = new Vector4 (vertex.x, vertex.y, vertex.z, 0f);
						uv5s [startIndex + indexPos] = new Vector4 (radialPosition, lengthOffset, normal_girth.w, 0f);
						uv6s [startIndex + indexPos] = new Vector4 (id_struct_seg_segType.x, branchSkinId, id_struct_seg_segType.y, 0f);
						uv7s [startIndex + indexPos] = new Vector4 (center_pos.x, center_pos.y, center_pos.z, 0f);
						uv8s [startIndex + indexPos] = new Vector4 (direction_posAtSkin.x, direction_posAtSkin.y, direction_posAtSkin.z, 0f);
						indexPos++;

						if (useHardNormals && (i > 0 || i < polygonSides)) {
							vertices [startIndex + indexPos] = vertex;
							normals [startIndex + indexPos] = vertexNormal;
							indexPos++;
						}
					}
				}
			}
		}
		#endregion

		#region BranchSkinRange class
		/// <summary>
		/// Defines a region along a BranchSkin instance to be processed by an specific mesh builder.
		/// </summary>
		public class BranchSkinRange {
			#region Vars
			/// <summary>
			/// Initial position of the branch skin range, from 0 to 1.
			/// </summary>
			public float from = 0f;
			/// <summary>
			/// Final position of the branch skin range, from 0 to 1.
			/// </summary>
			public float to = 0f;
			/// <summary>
			/// If the range has a capped shape range, the position the bottom cap begins at.
			/// </summary>
			public float bottomCap = 0f;
			/// <summary>
			/// If the range has a capped shape range, the position the top cap ends at.
			/// </summary>
			public float topCap = 1f;
			/// <summary>
			/// Number of preferred subdivisions on this range.
			/// </summary>
			public int subdivisions = 1;
			/// <summary>
			/// Multiply factor to radial resolution.
			/// </summary>
			public float radialResolutionFactor = 1f;
			/// <summary>
			/// Type of builder for this range.
			/// </summary>
			public BuilderType builderType = BuilderType.Default;
			/// <summary>
			/// Used only for ranges describing branches within a branch skin instance.
			/// </summary>
			public int branchId = -1;
			/// <summary>
			/// Id of the shape assigned to this range.
			/// </summary>
			public int shapeId = -1;
			#endregion
		}
		#endregion

		#region BranchSkin class
		/// <summary>
		/// Holds the properties of the mesh around a continium of branches.
		/// Used to have the same continuos mesh for parent branches and
		/// their follow up branches.
		/// </summary>
		public class BranchSkin {
			#region Consts
			public const int SEGMENT_TYPE_BEGIN_CAP	= 0;
			public const int SEGMENT_TYPE_BEGIN		= 1;
			public const int SEGMENT_TYPE_MIDDLE	= 2;
			public const int SEGMENT_TYPE_END		= 3;
			public const int SEGMENT_TYPE_END_CAP	= 4;
			#endregion

			#region Jobs vars
			/// <summary>
			/// Number of total vertices this branch will generate on a mesh.
			/// </summary>
			protected int _totalVertices = 0;
			/// <summary>
			/// Number of total triangles, multiply by 3 to get the total triangles index.
			/// </summary>
			protected int _totalTriangles = 0;
			/// <summary>
			/// Returns the total vertices to be generated on a mesh.
			/// </summary>
			/// <value>Number of vertices for the mesh.</value>
			public int totalVertices {
				get { return _totalVertices; }
			}
			/// <summary>
			/// Return the total triangles, multiply by 3 to get the total triangles index.
			/// </summary>
			/// <value>Total number of triangles.</value>
			public int totalTriangles {
				get { return _totalTriangles; }
			}
			/// <summary>
			/// Starting index for this branch skin when merged on a tree mesh.
			/// </summary>
			public int verticesStartIndex = 0;
			/// <summary>
			/// Set this branch skin meshing to use hard normals.
			/// </summary>
			public bool useHardNormals = false;
			/// <summary>
			/// Add faces at the base of the branchskin.
			/// </summary>
			public bool useMeshCapAtBase = false;
			/// <summary>
			/// How many shape vertices contains this branch skin.
			/// </summary>
			public int totalShapeVertices {
				get { return _shape_vertices_positions.Count; }
			}
			/// <summary>
			/// Center positions and position (0 - 1).
			/// </summary>
			protected List<Vector4> _centers_positions = new List<Vector4> ();
			protected List<Vector4> _directions_posAtSkin = new List<Vector4> ();
			protected List<Vector4> _normals_girths = new List<Vector4> ();
			protected List<Vector4> _ids_structs_segs_segTypes = new List<Vector4> ();
			protected List<Vector4> _shape_vertices_positions = new List<Vector4> ();
			#endregion

			#region Vars
			/// <summary>
			/// Id of this instance, if there are branches then the id of the first branch, otherwise -1.
			/// </summary>
			public int id = -1;
			/// <summary>
			/// The branch identifiers.
			/// </summary>
			protected List<int> _ids = new List<int> ();
			/// <summary>
			/// The structure generator identifiers.
			/// </summary>
			protected List<int> _structIds = new List<int> ();
			/// <summary>
			/// Center positions for each segment.
			/// </summary>
			protected List<Vector3> _centers = new List<Vector3> ();
			/// <summary>
			/// The directions for each segment.
			/// </summary>
			protected List<Vector3> _directions = new List<Vector3> ();
			/// <summary>
			/// The normals.
			/// </summary>
			protected List<Vector3> _normals = new List<Vector3> ();
			/// <summary>
			/// Series of segments on the skin with their number of vertices.
			/// </summary>
			protected List<int> _segments = new List<int> ();
			/// <summary>
			/// Type of the segment: begin, middle, end, endcap.
			/// </summary>
			protected List<int> _segmentTypes = new List<int> ();
			/// <summary>
			/// Girth registered for every segment.
			/// </summary>
			protected List<float> _girths = new List<float> ();
			/// <summary>
			/// The positions in the branch for each segment.
			/// </summary>
			protected List<float> _positions = new List<float> ();
			/// <summary>
			/// The positions in the BranchSkin for each segment.
			/// </summary>
			protected List<float> _positionsAtSkin = new List<float> ();
			/// <summary>
			/// The type of builder for each segment.
			/// </summary>
			/// <typeparam name="BuilderType">Type of builder.</typeparam>
			/// <returns>Type of builder for the segment.</returns>
			protected List<BuilderType> _builders = new List<BuilderType> ();
			/// <summary>
			/// Range of action for mesh builders on this BranchSkin instance.
			/// </summary>
			/// <typeparam name="BranchSkinRange">Definition of the range.</typeparam>
			/// <returns>List of mesh builder ranges.</returns>
			protected List<BranchSkinRange> _builderRanges = new List<BranchSkinRange> ();
			/// <summary>
			/// Ranges branches occupy withing the branch skin instance.
			/// </summary>
			/// <typeparam name="BranchSkinRange">Definition of the range.</typeparam>
			/// <returns>List of branch ranges.</returns>
			protected List<BranchSkinRange> _branchRanges = new List<BranchSkinRange> ();
			/// <summary>
			/// List of relevant positions to be assigned segments to along the branch. Each position ir relative from 0 to 1.
			/// </summary>
			/// <typeparam name="float">Relevant position from 0 to 1 relative to the branch length.</typeparam>
			List<float> _relevantPositions = new List<float> ();
			/// <summary>
			/// Save each relevant position priority on the branch skin.
			/// </summary>
			/// <typeparam name="int">Priority of the position, the higher more piority to replace an existing point.</typeparam>
			List<int> _relevantPositionPriorities = new List<int> ();
			/// <summary>
			/// The minimum polygon sides allowed.
			/// </summary>
			protected int _minPolygonSides = 3;
			/// <summary>
			/// The maximum polygon sides allowed.
			/// </summary>
			protected int _maxPolygonSides = 8;
			/// <summary>
			/// The number of polygon sides used on this mesh.
			/// </summary>
			public int polygonSides = 0;
			/// <summary>
			/// The minimum average girth found on the branches of the tree.
			/// </summary>
			public float minAvgGirth = 0f;
			/// <summary>
			/// The maximum average girth found on the branches of the tree.
			/// </summary>
			public float maxAvgGirth = 0f;
			/// <summary>
			/// Last pointing direction for the segment.
			/// </summary>
			public Vector3 lastDirection = Vector3.zero;
			/// <summary>
			/// True if the skin represents the trunk of the tree.
			/// </summary>
			public bool isTrunk = false;
			/// <summary>
			/// Length of this branch skin.
			/// </summary>
			public float length = 0f;
			/// <summary>
			/// Distance in length for this branch skin from the root of the tree.
			/// </summary>
			public float lengthOffset = 0f;
			/// <summary>
			/// Hierarchy level for this BranchSkin on the tree structure (0 to 1).
			/// </summary>
			public float hierarchyLevel = 0f;
			/// <summary>
			/// Level of hierarchy for this BranchSkin.
			/// </summary>
			public int level = 0;
			/// <summary>
			/// Vertices structure data.
			/// </summary>
			public List<Vector3> vertices = new List<Vector3> ();
			/// <summary>
			/// Counter for polygon faces.
			/// </summary>
			public int faceCount = 0;
			/// <summary>
			/// True if the current faceCount has already been used.
			/// </summary>
			public bool isFaceCountUsed = false;
			/// <summary>
			/// Vertex offset on a mesh.
			/// </summary>
			public int vertexOffset = 0;
			/// <summary>
			/// Start index for the last segment's first vertex added to this instance.
			/// Used as control for soft normals meshing.
			/// </summary>
			public int previousSegmentStartIndex = -1;
			/// <summary>
			/// Dictionary holding every branch Id to their index order.
			/// </summary>
			/// <typeparam name="int">Branch id.</typeparam>
			/// <typeparam name="int">Order in the list of branches of this BranchSkin instance.</typeparam>
			private Dictionary<int, int> _branchIdToIndex = new Dictionary<int, int> ();
			/// <summary>
			/// List holding the start index value for vertices on the mesh for every branch on the BranchSkin instance.
			/// </summary>
			/// <typeparam name="int">Start index of vertices for a branch on its order of listing within the BranchSin instance.</typeparam>
			private List<int> _branchStartVertexIndex = new List<int> ();
            #endregion

            #region Accessors
            public List<int> ids {
				get { return _ids; }
			}
			public List<int> structIds {
				get { return _structIds; }
			}
			public List<Vector3> centers {
				get { return _centers; }
			}
			public List<Vector3> directions {
				get { return _directions; }
			}
			public List<Vector3> normals {
				get { return _normals; }
			}
			public List<int> segments {
				get { return _segments; }
			}
			public List<int> segmentTypes {
				get { return _segmentTypes; }
			}
			public List<float> girths {
				get { return _girths; }
			}
			public List<float> positions {
				get { return _positions; }
			}
			public List<float> positionsAtSkin {
				get { return _positionsAtSkin; }
			}
			public List<BuilderType> builders {
				get { return _builders; }
			}
			public List<BranchSkinRange> ranges {
				get { return _builderRanges; }
			}
			public List<BranchSkinRange> branchRanges {
				get { return _branchRanges; }
			}
			public int minPolygonSides {
				get { return _minPolygonSides; }
			}
			public int maxPolygonSides {
				get { return _maxPolygonSides; }
			}


			public List<Vector4> idsSegs {
				get { return _ids_structs_segs_segTypes; }
			}
			public List<Vector4> centersPos {
				get { return _centers_positions; }
			}
			public List<Vector4> directionsPosAtSkin {
				get { return _directions_posAtSkin; }
			}
			public List<Vector4> normalsGirths {
				get { return _normals_girths; }
			}
			public List<Vector4> shapeVerticesPos {
				get { return _shape_vertices_positions; }
			}
			#endregion

			#region Constructors
			/// <summary>
			/// Initializes a new instance of the <see cref="Broccoli.Builder.BranchMeshBuilder+BranchSkin"/> class.
			/// </summary>
			public BranchSkin () {}
			/// <summary>
			/// Initializes a new instance of the <see cref="Broccoli.Builder.BranchMeshBuilder+BranchSkin"/> class.
			/// </summary>
			/// <param name="minPolygonSides">Minimum polygon sides.</param>
			/// <param name="maxPolygonSides">Max polygon sides.</param>
			/// <param name="minAvgGirth">Minimum avg girth.</param>
			/// <param name="maxAvgGirth">Max avg girth.</param>
			public BranchSkin (int minPolygonSides, int maxPolygonSides, float minAvgGirth, float maxAvgGirth, float lengthOffset, float hierarchyLevel, bool useHardNormals, bool useMeshCapAtBase) {
				this._minPolygonSides = minPolygonSides;
				this._maxPolygonSides = maxPolygonSides;
				this.minAvgGirth = minAvgGirth;
				this.maxAvgGirth = maxAvgGirth;
				this.lengthOffset = lengthOffset;
				this.hierarchyLevel = hierarchyLevel;
				this.useHardNormals = useHardNormals;
				this.useMeshCapAtBase = useMeshCapAtBase;
			}
			#endregion

			#region Range
			/// Adds a BranchSkinRange to the BranchSkin instance. This represents a region
			/// <summary>
			/// along the BranchSkin to be processed by another mesh builder.
			/// </summary>
			/// <param name="range"></param>
			public void AddBuilderRange (BranchSkinRange range) {
				_builderRanges.Add (range);
				_builderRanges.Sort (delegate (BranchSkinRange x, BranchSkinRange y) { return x.from.CompareTo(y.from); });
			}
			/// <summary>
			/// Adds a BranchSkinRange belonging to a branch instance.
			/// </summary>
			/// <param name="range"></param>
			public void AddBranchRange (BranchSkinRange range) {
				_branchRanges.Add (range);
				_branchRanges.Sort (delegate (BranchSkinRange x, BranchSkinRange y) { return x.from.CompareTo(y.from); });
			}
			/// <summary>
			/// Translates a branch skin position to a normalized in range position.
			/// </summary>
			/// <param name="branchSkinPosition">Position on the branch skin.</param>
			/// <param name="builderRange">Range instance if the range matching the position was found, otherwise null.</param>
			/// <returns>In range position.</returns>
			public float TranslateToPositionAtBuilderRange (float branchSkinPosition, out BranchSkinRange builderRange) {
				float rangePosition = branchSkinPosition;
				builderRange = null;
				for (int i = 0; i < _builderRanges.Count; i++) {
					if (branchSkinPosition >= _builderRanges[i].from && branchSkinPosition <= _builderRanges[i].to) {
						builderRange = _builderRanges[i];
						rangePosition = Mathf.InverseLerp (_builderRanges[i].from, _builderRanges[i].to, branchSkinPosition);
						break;
					}
				}
				return rangePosition;
			}
			/// <summary>
			/// Get relevant position bound to a specific branch within the branch skin instance.
			/// </summary>
			/// <param name="branchId">Id of the branch.</param>
			/// <param name="priority">Priority limit.</param>
			/// <param name="normalized">Normalize the position within the branch length.</param>
			/// <returns>List of relevant positions within a branch range.</returns>
			public List<float> GetBranchRelevantPositions (int branchId, int priority = 0, bool normalized = false) {
				List<float> relevantPositions = new List<float> ();
				BranchSkinRange branchSkinRange = null;
				for (int i = 0; i < _branchRanges.Count; i++) {
					if (_branchRanges [i].branchId == branchId) {
						branchSkinRange = _branchRanges [i];
						break;
					}
				}
				if (branchSkinRange != null) {
					for (int i = 0; i < _relevantPositions.Count; i++) {
						if (_relevantPositions [i] >= branchSkinRange.from && 
							_relevantPositions [i] <= branchSkinRange.to &&
							_relevantPositionPriorities [i] <= priority) {
							float relevantPosition = _relevantPositions [i];
							if (normalized) {
								relevantPosition = Mathf.InverseLerp (branchSkinRange.from, branchSkinRange.to, relevantPosition);
							}
							if (relevantPosition > 0 && relevantPosition < 1) {
								relevantPositions.Add (relevantPosition);
							}
						}
					}
				}
				relevantPositions.Sort ();
				return relevantPositions;
			}
			#endregion

			#region Structure methods
			/// <summary>
			/// Adds a segment to the skin.
			/// </summary>
			/// <returns><c>true</c>, if segment was added, <c>false</c> otherwise.</returns>
			/// <param name="id">Identifier.</param>
			/// <param name="center">Center.</param>
			/// <param name="direction">Direction.</param>
			/// <param name="normal">Normal.</param>
			/// <param name="numberOfSegments">Number of segments.</param>
			/// <param name="girth">Girth.</param>
			/// <param name="position">Position.</param>
			public bool AddSegment (int id, int structId, Vector3 center, Vector3 direction, Vector3 normal, 
				int numberOfSegments, int segmentType, float girth, float position, float positionAtSkin, BuilderType builder)
			{
				// Adds segment information.
				_ids.Add (id);
				_structIds.Add (structId);
				_centers.Add (center);
				_directions.Add (direction);
				_normals.Add (normal);
				_segments.Add (numberOfSegments);
				_segmentTypes.Add (segmentType);
				_girths.Add (girth);
				_positions.Add (position);
				_positionsAtSkin.Add (positionAtSkin);

				// Adds segment information.
				_centers_positions.Add (new Vector4 (center.x, center.y, center.z, position));
				_directions_posAtSkin.Add (new Vector4 (direction.x, direction.y, direction.z, positionAtSkin));
				_normals_girths.Add (new Vector4 (normal.x, normal.y, normal.z, girth));
				_ids_structs_segs_segTypes.Add (new Vector4 (id, structId, numberOfSegments, segmentType));

				_builders.Add (builder);

				// Add the vertex expected to be generated according to the type of segment and normal type.
				if (segmentType == SEGMENT_TYPE_MIDDLE ||
					segmentType == SEGMENT_TYPE_BEGIN_CAP || segmentType == SEGMENT_TYPE_END_CAP) {
					if (useHardNormals) _totalVertices += numberOfSegments * 4;
					else _totalVertices += numberOfSegments + 1;
				} else if (segmentType == SEGMENT_TYPE_BEGIN || segmentType == SEGMENT_TYPE_END) {
					if (useHardNormals) _totalVertices += numberOfSegments * 2;
					else _totalVertices += numberOfSegments + 1;
				}

				// Add the triangles expected.
				bool isFirstSegment = segmentType == SEGMENT_TYPE_BEGIN;
				if (useMeshCapAtBase) isFirstSegment = segmentType == SEGMENT_TYPE_BEGIN_CAP;
				if (!isFirstSegment) {
					if (segmentType == SEGMENT_TYPE_MIDDLE || segmentType == SEGMENT_TYPE_END) {
						int prevNumberOfSegments = _segments [_segments.Count - 2];
						_totalTriangles += numberOfSegments + prevNumberOfSegments;
					} else { // BEGIN_CAP & END_CAP
						_totalTriangles += numberOfSegments * 2;
					}
				}
				
				return true;
			}
			public void AddShapeSegmentVertex (Vector3 vertex, float radialPosition) {
				_shape_vertices_positions.Add (new Vector4 (vertex.x, vertex.y, vertex.z, radialPosition));
			}
			/// <summary>
			/// Adds a position to the relevant position list by creating an average or replacing with existing positions using a priority.
			/// </summary>
			/// <param name="position">Relative position on the branch skin instance from 0 to 1.</param>
			/// <param name="range">Range on the new position from 0 to 1.</param>
			/// <param name="priority">Priority of the new position.</param>
			public bool AddRelevantPosition (float position, float range, int priority = 0) {
				if (position <= 0f || position >= 1f) return false;
				float minRange = position - (range / 2f);
				float maxRange = position + (range / 2f);
				// Check range with position 0.
				if (minRange < 0f) {
					// Position 0 has top priority, we drop the new point.
					return false;
				}
				// Check range with position 1.
				if (maxRange > 1f) {
					// Position 1 has top priority, we drop the new point.
					return false;
				}
				// If it is the first relevant position getting added.
				if (_relevantPositions.Count == 0) {
					_relevantPositions.Add (position);
					_relevantPositionPriorities.Add (priority);
					return true;
				}
				// Check range with intermediate positions.
				int candidateIndex = -1;
				for (int i = 0; i < _relevantPositions.Count; i++) {
					if (_relevantPositions[i] >= minRange && _relevantPositions[i] <= maxRange) {
						candidateIndex = i;
						break;
					}
				}
				if (candidateIndex < 0) {
					// TODO: add with order.
					int indexToInsert = _relevantPositions.FindLastIndex(e => e < position);
					if (indexToInsert == 0 || indexToInsert == -1) {
						_relevantPositions.Insert (0, position);
						_relevantPositionPriorities.Insert (0, priority);
					} else {
						_relevantPositions.Insert (indexToInsert + 1, position);
						_relevantPositionPriorities.Insert (indexToInsert + 1, priority);
					}
				} else {
					// Merge or drop.
					if (_relevantPositionPriorities[candidateIndex] == priority) {
						// Case of equal priorities: average.
						_relevantPositions [candidateIndex] = (_relevantPositions [candidateIndex] + position) / 2f;
					} else if (priority > _relevantPositionPriorities [candidateIndex]) {
						// Case of higher priority, replace.
						_relevantPositions [candidateIndex] = position;
						_relevantPositionPriorities [candidateIndex] = priority;
					}
					// Case of lower priority, then drop.
				}
				return true;
			}
			/// <summary>
			/// Get the list of relevant positions.
			/// </summary>
			/// <returns>List of relevant positions.</returns>
			public List<float> GetRelevantPositions () {
				return _relevantPositions;
			}
			/// <summary>
			/// Clear structural data for this instance.
			/// </summary>
			public void Clear () {
				vertices.Clear ();
				_branchIdToIndex.Clear ();
				_branchStartVertexIndex.Clear ();
				faceCount = 0;
				isFaceCountUsed = false;
				vertexOffset = 0;
				previousSegmentStartIndex = -1;
				_relevantPositions.Clear ();
				_relevantPositionPriorities.Clear ();
			}
			#endregion

			#region Branch Methods
			/// <summary>
			/// Gets the center point at a position (0 to 1) on this BranchSkin instance.
			/// </summary>
			/// <param name="positionAtSkin">Position on the branch skin.</param>
			/// <param name="firstBranch">First branch on the BranchSkin.</param>
			/// <returns>Center point at position.</returns>
			public Vector3 GetPointAtPosition (float positionAtSkin, BroccoTree.Branch firstBranch) {
				Vector3 point = Vector3.zero;
				BroccoTree.Branch targetBranch = null;
				float targetBranchPosition = TranslateToPositionAtBranch (positionAtSkin, firstBranch, out targetBranch);
				point = targetBranch.GetPointAtPosition (targetBranchPosition);
				return point;
			}
			/// <summary>
			/// Gets the center point at a length position on this BranchSkin instance.
			/// </summary>
			/// <param name="lengthAtSkin">Length distance at branch skin.</param>
			/// <param name="firstBranch">First branch on the BranchSkin instance.</param>
			/// <returns>Center point at position.</returns>
			public Vector3 GetPointAtLength (float lengthAtSkin, BroccoTree.Branch firstBranch) {
				return GetPointAtPosition (lengthAtSkin / length, firstBranch);
			}
			/// <summary>
			/// Gets the direction at a position (0 to 1) on this BranchSkin instance.
			/// </summary>
			/// <param name="positionAtSkin">Position on the branch skin.</param>
			/// <param name="firstBranch">First branch on the BranchSkin.</param>
			/// <returns>Direction at position.</returns>
			public Vector3 GetDirectionAtPosition (float positionAtSkin, BroccoTree.Branch firstBranch) {
				Vector3 direction = Vector3.zero;
				BroccoTree.Branch targetBranch = null;
				float targetBranchPosition = TranslateToPositionAtBranch (positionAtSkin, firstBranch, out targetBranch);
				direction = targetBranch.GetDirectionAtPosition (targetBranchPosition);
				return direction;
			}
			/// <summary>
			/// Gets the normal at a position (0 to 1) on this BranchSkin instance.
			/// </summary>
			/// <param name="positionAtSkin">Position on the branch skin.</param>
			/// <param name="firstBranch">First branch on the BranchSkin.</param>
			/// <returns>Normal at position.</returns>
			public Vector3 GetNormalAtPosition (float positionAtSkin, BroccoTree.Branch firstBranch) {
				Vector3 normal = Vector3.zero;
				BroccoTree.Branch targetBranch = null;
				float targetBranchPosition = TranslateToPositionAtBranch (positionAtSkin, firstBranch, out targetBranch);
				normal = targetBranch.GetNormalAtPosition (targetBranchPosition);
				return normal;
			}
			/// <summary>
			/// Gets the girth value at a position (0 to 1) on this BranchSkin instance.
			/// </summary>
			/// <param name="positionAtSkin">Position on the branch skin.</param>
			/// <param name="firstBranch">First branch on the BranchSkin.</param>
			/// <returns>Girth at position.</returns>
			public float GetGirthAtPosition (float positionAtSkin, BroccoTree.Branch firstBranch) {
				float girth = 0f;
				BroccoTree.Branch targetBranch = null;
				float targetBranchPosition = TranslateToPositionAtBranch (positionAtSkin, firstBranch, out targetBranch);
				girth = targetBranch.GetGirthAtPosition (targetBranchPosition);
				return girth;
			}
			/// <summary>
			/// Gets the girth value at a length distance on this BranchSkin instance.
			/// </summary>
			/// <param name="lengthAtSkin">Length on the branch skin.</param>
			/// <param name="firstBranch">First branch on the BranchSkin.</param>
			/// <returns>Girth at length.</returns>
			public float GetGirthAtLength (float lengthAtSkin, BroccoTree.Branch firstBranch) {
				return GetGirthAtPosition (lengthAtSkin / length, firstBranch);
			}
			/// <summary>
			/// Translates a branch skin position to a position on a branch belonging to this branch skin.
			/// </summary>
			/// <param name="positionAtSkin">Position at BranchSkin.</param>
			/// <param name="firstBranch">First branch instance on this skin.</param>
			/// <param name="branchAtSkin">Out parameter for the branch instance found having the position.</param>
			/// <returns>Position at a branch instance.</returns>
			public float TranslateToPositionAtBranch (
				float positionAtSkin, 
				BroccoTree.Branch firstBranch, 
				out BroccoTree.Branch branchAtSkin) {
				branchAtSkin = null;
				// If the provided first branch instance is indexed as the first range on the branch ranges.
				if (id == firstBranch.id) {
					// If the position < 0
					if (positionAtSkin < 0f) {
						branchAtSkin = firstBranch;
						return (positionAtSkin * length) / firstBranch.length;
					} else {
						float accumLength = 0f;
						float targetLength = length * positionAtSkin;
						BroccoTree.Branch currentBranch = firstBranch;
						BroccoTree.Branch prevBranch = null;
						do {
							if (accumLength + currentBranch.length > targetLength) {
								branchAtSkin = currentBranch;
								return (targetLength - accumLength) / currentBranch.length;
							}
							accumLength += currentBranch.length;
							prevBranch = currentBranch;
							currentBranch = currentBranch.followUp;
						} while (currentBranch != null);
						// If the position is > 1
						branchAtSkin = prevBranch;
						return ((positionAtSkin * length) - (length - branchAtSkin.length)) / branchAtSkin.length;
					}
				}
				return 0f;
			}
			/// <summary>
			/// Translates a BranchSkin position at a child Branch position.
			/// </summary>
			/// <param name="branchSkin">BranchSkin instance to get the position from.</param>
			/// <param name="positionAtSkin">Position at the whole BranchSkin length (0-1).</param>
			/// <param name="firstBranch">First branch at the BranchSkin instance.</param>
			/// <param name="branchAtSkin">Gets the branch instance the asked position was found at, null if the position was not found.</param>
			/// <returns>The position at the child branch.</returns>
			public static float TranslateToPositionAtBranch (
				BranchSkin branchSkin, 
				float positionAtSkin,
				BroccoTree.Branch firstBranch,
				out BroccoTree.Branch branchAtSkin) 
			{
				branchAtSkin = null;
				float accumLength = 0f;
				float targetLength = branchSkin.length * positionAtSkin;
				BroccoTree.Branch currentBranch = firstBranch;
				do {
					if (accumLength + currentBranch.length > targetLength) {
						branchAtSkin = currentBranch;
						return (targetLength - accumLength) / currentBranch.length;
					}
					accumLength += currentBranch.length;
					currentBranch = currentBranch.followUp;
				} while (currentBranch != null);
				return 0;
			}
			/// <summary>
			/// Translates a position belonging to a branch instance within a branch skin, to a branch skin position.
			/// </summary>
			/// <param name="positionAtBranch">Position at the branch at skin.</param>
			/// <param name="branchAtSkin">Branch having the position and belonging to the branch skin.</param>
			/// <param name="firstBranch">First branch at the branch skin.</param>
			/// <param name="branchSkin">BranchSkin instance.</param>
			/// <returns>Position at the BranchSkin instance, if the branch does not belong to the branch skin then the returned value is -1.</returns>
			public static float TranslateToPositionAtBranchSkin (float positionAtBranch, BroccoTree.Branch branchAtSkin, BroccoTree.Branch firstBranch, BranchMeshBuilder.BranchSkin branchSkin) {
				float positionAtBranchSkin = -1;
				float accumLength = 0f;
				BroccoTree.Branch currentBranch = firstBranch;
				do {
					if (currentBranch.id == branchAtSkin.id) {
						return (accumLength + currentBranch.length * positionAtBranch) / branchSkin.length;
					}
					accumLength += currentBranch.length;
					currentBranch = currentBranch.followUp;
				} while (currentBranch != null);
				return positionAtBranchSkin;
			}
			public static float TranslateToPositionAtBranchSkin (float positionAtBranch, int branchId, BranchMeshBuilder.BranchSkin branchSkin) {
				float positionAtBranchSkin = -1f;
				BranchMeshBuilder.BranchSkinRange branchSkinRange = null;
				List<BranchMeshBuilder.BranchSkinRange> branchRanges = branchSkin.branchRanges;
				for (int i = 0; i < branchRanges.Count; i++) {
					if (branchRanges [i].branchId == branchId) {
						branchSkinRange = branchRanges [i];
					}
				}
				if (branchSkinRange != null) {
					positionAtBranchSkin = Mathf.Lerp (branchSkinRange.from, branchSkinRange.to, positionAtBranch);
				}
				return positionAtBranchSkin;
			}
			/// <summary>
			/// Checks if a BranchSkin instance position belong to a branch given its id.
			/// </summary>
			/// <param name="branchSkin">BranchSkin instance.</param>
			/// <param name="positionAtSkin">Position at branch skin.</param>
			/// <param name="firstBranch">First branch at the BranchSkin instance.</param>
			/// <param name="belongingBranchId">Id of the branch to search for.</param>
			/// <param name="inBranchPosition">Translated position on the found branch.</param>
			/// <returns>True if the position belongs to the branch given its id.</returns>
			public static bool PositionBelongsToBranch (
				BranchSkin branchSkin, 
				float positionAtSkin,
				BroccoTree.Branch firstBranch,
				int belongingBranchId, 
				out float inBranchPosition)
			{
				inBranchPosition = 0f;
				float accumLength = 0f;
				float targetLength = branchSkin.length * positionAtSkin;
				BroccoTree.Branch currentBranch = firstBranch;
				do {
					inBranchPosition = (targetLength - accumLength) / currentBranch.length;
					if (inBranchPosition < 0 || inBranchPosition > 1) return false;
					if (accumLength + currentBranch.length > targetLength && currentBranch.id == belongingBranchId) {
						//inBranchPosition = (targetLength - accumLength) / currentBranch.length;
						return true;
					}
					accumLength += currentBranch.length;
					currentBranch = currentBranch.followUp;
				} while (currentBranch != null);
				return false;
			}
			/// <summary>
			/// Registers the start index of vertices belonging to one branch.
			/// </summary>
			/// <param name="branchId">Id of the branch.</param>
			/// <param name="startVertexIndex">Start vertex index.</param>
			public void RegisterBranchStartVertexIndex (int branchId, int startVertexIndex) {
				if (!_branchIdToIndex.ContainsKey (branchId)) {
					_branchIdToIndex.Add (branchId, _branchStartVertexIndex.Count);
					_branchStartVertexIndex.Add (startVertexIndex);
				}
			}
			/// <summary>
			/// Gets the start index and vertex count for a branch given its id on the BranchSkin instance.
			/// </summary>
			/// <param name="branchId">Branch id.</param>
			/// <param name="startIndex">Start index on the mesh vertices.</param>
			/// <param name="vertexCount">Total number of vertices assigned to a branch on this BranchSkin instance.</param>
			public void GetVertexStartAndCount (int branchId, out int startIndex, out int vertexCount) {
				if (_branchIdToIndex.ContainsKey (branchId)) {
					int branchIndex = _branchIdToIndex [branchId];
					startIndex = _branchStartVertexIndex [branchIndex];
					if (branchIndex == _branchStartVertexIndex.Count - 1) {
						vertexCount = vertices.Count - _branchStartVertexIndex [branchIndex];
					} else {
						vertexCount = _branchStartVertexIndex [branchIndex + 1] - _branchStartVertexIndex [branchIndex];
					}
				} else {
					startIndex = 0;
					vertexCount = 0;
				}
			}
			#endregion
		}
		#endregion

		#region Vars
		/// <summary>
		/// Enumeration for the types of builders known.
		/// </summary>
		public enum BuilderType {
			Default = 0,
			Trunk = 1,
			Shape = 2,
			Welding = 3
		}
		/// <summary>
		/// The minimum polygon sides to use for meshing.
		/// </summary>
		public int minPolygonSides = 3;
		/// <summary>
		/// The maximum polygon sides to use for meshing.
		/// </summary>
		public int maxPolygonSides = 8;
		/// <summary>
		/// The segment angle.
		/// </summary>
		public float segmentAngle = 0f;
		/// <summary>
		/// Use hard normals on the mesh.
		/// </summary>
		public bool useHardNormals = false;
		/// <summary>
		/// Use triangles to 
		/// </summary>
		public bool useMeshCapAtBase = false;
		/// <summary>
		/// The global scale.
		/// </summary>
		float _globalScale = 1f;
		public float globalScale {
			get { return _globalScale; }
			set {
				_globalScale = value;
				foreach (var branchMeshBuilder in _branchMeshBuilders) {
					branchMeshBuilder.Value.SetGlobalScale (_globalScale);
				}
			}
		}
		/// <summary>
		/// Used to get the branches curve points. The lower the value more resolution, the higher lower resolution.
		/// </summary>
		public float branchAngleTolerance = 5f;
		/// <summary>
		/// Limit level of BranchSkin hierarchy to apply average normals to.
		/// </summary>
		public int averageNormalsLevelLimit = 0;
		/// <summary>
		/// The base radius scale factor.
		/// </summary>
		//float baseRadiusScaleFactor = 0.80f;
		/// <summary>
		/// Dictionary with initial branch skins, the instance will try to take them from here before
		/// creating a new one for a branch.
		/// </summary>
		/// <typeparam name="int">Branch id.</typeparam>
		/// <typeparam name="BranchSkin">BranchSkin instance.</typeparam>
		/// <returns>Dictionary of SkinBranch instances.</returns>
		protected Dictionary<int, BranchSkin> idToBranchSkin = new Dictionary<int, BranchSkin> ();
		/// <summary>
		/// Reference to the first Branch on every BranchSkin instance generated by this builder.
		/// </summary>
		/// <typeparam name="int">Id of the BranchSkin instance.</typeparam>
		/// <typeparam name="BroccoTree.Branch">Branch instance.</typeparam>
		/// <returns>First branch on a BranchSkin instance.</returns>
		protected Dictionary<int, BroccoTree.Branch> idToFirstBranch = new Dictionary<int, BroccoTree.Branch> ();
		/// <summary>
		/// The branch skins generated or processed by this instance.
		/// </summary>
		public List<BranchSkin> branchSkins = new List<BranchSkin> ();
		/// <summary>
		/// Dictionary of mesh builder instances used to mesh the branch skins.
		/// </summary>
		protected Dictionary<BuilderType, IBranchMeshBuilder> _branchMeshBuilders = new Dictionary<BuilderType, IBranchMeshBuilder> ();
		/// <summary>
		/// Vertex counter.
		/// </summary>
		public int vertexCount = 0;
		/// <summary>
		/// Maximum length to expect from the three.
		/// </summary>
		public float treeMaxLength = 0f;
		/// <summary>
		/// Maximum girth to expect from the tree.
		/// </summary>
		public float treeMaxGirth = 0f;
		/// <summary>
		/// Maximum girth to expect from the tree.
		/// </summary>
		public float treeMinGirth = 0f;
		/// <summary>
		/// Maximum average girth found on the branches.
		/// </summary>
		public float treeMaxAvgGirth = 0f;
		/// <summary>
		/// Minimum average girth found on the branches.
		/// </summary>
		public float treeMinAvgGirth = 0f;
		/// <summary>
		/// Hold count of the number of vertices created on the processed mesh.
		/// </summary>
		public int verticesGenerated { get; private set; }
		/// <summary>
		/// Hold count of the number of triangles created on the processed mesh.
		/// </summary>
		public int trianglesGenerated { get; private set; }
		/// <summary>
		/// The first and last segment vertices pairs.
		/// </summary>
		Dictionary<int, int> firstLastSegmentVertices = new Dictionary<int, int> ();
		/// <summary>
		/// Vertices to use on the mesh construction.
		/// </summary>
		List<Vector3> meshVertices = new List<Vector3> ();
		/// <summary>
		/// Triangles to use on the mesh construction.
		/// </summary>
		List<int> meshTriangles = new List<int> ();
		/// <summary>
		/// Normals to use on the mesh construction.
		/// </summary>
		List<Vector3> meshNormals = new List<Vector3> ();
		/// <summary>
		/// Temp var to hold base vertices.
		/// </summary>
		List<Vector3> baseVertices = new List<Vector3> ();
		/// <summary>
		/// Temp var to hold top vertices.
		/// </summary>
		List<Vector3> topVertices = new List<Vector3> ();
		#endregion

		#region Singleton
		/// <summary>
		/// This class singleton.
		/// </summary>
		static BranchMeshBuilder _treeMeshFactory = null;
		/// <summary>
		/// Gets a builder instance.
		/// </summary>
		/// <returns>Singleton instance.</returns>
		public static BranchMeshBuilder GetInstance() {
			if (_treeMeshFactory == null) {
				_treeMeshFactory = new BranchMeshBuilder ();
			}
			return _treeMeshFactory;
		}
		#endregion

		#region Constructor
		public BranchMeshBuilder () {
			DefaultMeshBuilder defaultMeshBuilder = new DefaultMeshBuilder ();
			defaultMeshBuilder.SetGlobalScale (_globalScale);
			_branchMeshBuilders.Add (BuilderType.Default, defaultMeshBuilder);
		}
		#endregion

		#region Mesh Building
		/// <summary>
		/// Meshs the tree.
		/// </summary>
		/// <returns>The tree.</returns>
		/// <param name="tree">Tree.</param>
		public Mesh MeshTree (BroccoTree tree) { // TODO: cut
			Clear ();
			tree.RecalculateNormals ();
			tree.UpdateGirth ();
			AnalyzeTree (tree);
			Mesh treeMesh = new Mesh();
                        treeMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

                        TreeFactory.GetActiveInstance ().BeginColliderUsage ();

			// Preprocess each branchskin and add its segments.
			for (int i = 0; i < tree.branches.Count; i++) {
				BranchSkin mainBranchSkin = GetOrCreateBranchSkin (tree.branches[i], 0, 0f, 0f);
				PreprocessBranchSkin (mainBranchSkin, tree.branches [i]);
				SkinBranch (tree.branches[i], mainBranchSkin, 0f);
			}

			//treeMesh = MeshBranchSkins ();
			treeMesh = MeshBranchSkinsJob ();

			// Postprocess each branchskin and add its segments.
			List<Mesh> meshes = new List<Mesh> ();
			BroccoTree.Branch firstBranch;
			for (int i = 0; i < branchSkins.Count; i++) {
				firstBranch = idToFirstBranch[branchSkins[i].id];
				BranchSkin parentBranchSkin = firstBranch.parent!=null?idToBranchSkin [firstBranch.parent.id]:null;
				BroccoTree.Branch parentBranch = null;
				if (parentBranchSkin!= null) {
					parentBranch = idToFirstBranch [parentBranchSkin.id];
				}
				PostprocessBranchSkin (
					meshes, branchSkins[i], 
					firstBranch, 
					parentBranchSkin, 
					parentBranch);
			}
			// Combine meshes.
			if (meshes.Count > 0) {
				Mesh newMesh = new Mesh ();
				CombineInstance[] iMeshes = new CombineInstance[meshes.Count + 1];
				Matrix4x4 matrix= Broccoli.Factory.TreeFactory.GetActiveInstance().gameObject.transform.worldToLocalMatrix;
				iMeshes[0] = new CombineInstance ();
				iMeshes[0].mesh = treeMesh;
				iMeshes[0].transform = matrix * Broccoli.Factory.TreeFactory.GetActiveInstance().gameObject.transform.localToWorldMatrix;
				for (int i = 1; i < meshes.Count + 1; i++) {
					iMeshes[i] = new CombineInstance ();
					iMeshes[i].mesh = meshes[i - 1];
					iMeshes[i].transform = matrix * Broccoli.Factory.TreeFactory.GetActiveInstance().gameObject.transform.localToWorldMatrix;
				}
				newMesh.CombineMeshes (iMeshes);
				treeMesh = newMesh;
			}

			TreeFactory.GetActiveInstance ().EndColliderUsage ();

			return treeMesh;
		}
		/// <summary>
		/// Check if there is an instance of a mesh builder type already registered.
		/// </summary>
		/// <param name="builderType">Mesh builder type.</param>
		/// <returns>True is there is a mesh builder already registered.</returns>
		public bool ContainsMeshBuilder (BuilderType builderType) {
			return _branchMeshBuilders.ContainsKey (builderType);
		}
		/// <summary>
		/// Gets the branch mesh builder of a given type.
		/// </summary>
		/// <param name="builderType">Type of branch mesh builder.</param>
		/// <returns>Builder of the type specified, if not registered then it returns a default branch mesh builder.</returns>
		public IBranchMeshBuilder GetBranchMeshBuilder (BuilderType builderType) {
			if (_branchMeshBuilders.ContainsKey (builderType)) {
				return _branchMeshBuilders [builderType];
			}
			return _branchMeshBuilders [BuilderType.Default];
		}
		/// <summary>
		/// /Add a mesh builder of a builder type.
		/// </summary>
		/// <param name="branchMeshBuilder">Branch mesh builder.</param>
		/// <returns>True if no other mesh builder of the same builder type was present.</returns>
		public bool AddMeshBuilder (IBranchMeshBuilder branchMeshBuilder) {
			if (ContainsMeshBuilder (branchMeshBuilder.GetBuilderType ())) {
				return false;
			}
			_branchMeshBuilders.Add (branchMeshBuilder.GetBuilderType (), branchMeshBuilder);
			branchMeshBuilder.SetGlobalScale (_globalScale);
			return true;
		}
		/// <summary>
		/// Clear all the mesh builders registered on this instance.
		/// </summary>
		public void ClearMeshBuilders () {
			_branchMeshBuilders.Clear ();
			DefaultMeshBuilder defaultMeshBuilder = new DefaultMeshBuilder ();
			defaultMeshBuilder.SetGlobalScale (_globalScale);
			_branchMeshBuilders.Add (BuilderType.Default, defaultMeshBuilder);
		}
		#endregion

		#region Skin BranchSkin
		/// <summary>
		/// Clears the BranchSkin instances used as reference to begin building the branch meshes.
		/// </summary>
		public void ClearReferenceBranchSkins () {
			idToBranchSkin.Clear ();
			idToFirstBranch.Clear ();
		}
		/// <summary>
		/// Get a BranchSkin instance from the reference dictionary or creates a new one adding it to the dictionary.
		/// </summary>
		/// <param name="branchId">Branch id.</param>
		/// <param name="level">Hierarchy level for the BranchSkin.</param>
		/// <param name="lengthOffset">Length from the tree root.</param>
		/// <param name="hierarchyLevel">Level of hierarchy of the first branch..</param>
		/// <returns>BranchSkin instance.</returns>
		public BranchSkin GetOrCreateBranchSkin (BroccoTree.Branch firstBranch, int level, float lengthOffset, float hierarchyLevel) {
			if (idToBranchSkin.ContainsKey (firstBranch.id)) {
				return idToBranchSkin [firstBranch.id];
			}
			BranchSkin branchSkin = new BranchSkin (minPolygonSides, maxPolygonSides, treeMinAvgGirth, treeMaxAvgGirth, lengthOffset, hierarchyLevel, useHardNormals, useMeshCapAtBase);
			branchSkin.id = firstBranch.id;
			branchSkin.level = level;
			idToFirstBranch.Add (firstBranch.id, firstBranch);

			// Set branch skin length.
			BroccoTree.Branch currentBranch = firstBranch;
			float branchSkinLength = 0f;
			do {
				branchSkinLength += currentBranch.length;
				currentBranch = currentBranch.followUp;
			} while (currentBranch != null);
			branchSkin.length = branchSkinLength;

			// Add first branch id to branch skin relationship.
			// Add the following branches.
			idToBranchSkin.Add (firstBranch.id, branchSkin);
			BroccoTree.Branch followUpBranch = firstBranch.followUp;
			while (followUpBranch != null) {
				idToBranchSkin.Add (followUpBranch.id, branchSkin);
				followUpBranch = followUpBranch.followUp;
			}

			// Set branch skin branch ranges.
			currentBranch = firstBranch;
			branchSkinLength = 0f;
			bool isFirstBranch = true;
			WeldingMeshBuilder weldingMeshBuilder = null;
			if (ContainsMeshBuilder (BuilderType.Welding)) {
				weldingMeshBuilder = (WeldingMeshBuilder) GetBranchMeshBuilder (BuilderType.Welding);
			}
			do {
				// Add range for welding if active.
				if (weldingMeshBuilder != null && 
					((weldingMeshBuilder.useBranchWelding && !currentBranch.isRoot) || (weldingMeshBuilder.useRootWelding && currentBranch.isRoot)) &&
					isFirstBranch && 
					!currentBranch.isTrunk &&
					hierarchyLevel <= weldingMeshBuilder.branchWeldingHierarchyRange)
				{
					WeldingMeshBuilder.BranchInfo branchInfo = weldingMeshBuilder.RegisterBranchInfo (currentBranch);
					BranchSkinRange branchSkinWeldingRange = new BranchSkinRange ();
					branchSkinWeldingRange.builderType = BuilderType.Welding;
					branchSkinWeldingRange.from = 0f;
					branchSkinWeldingRange.to = branchInfo.weldingDistance / branchSkin.length;
					if (branchSkinWeldingRange.to > 1f) branchSkinWeldingRange.to = 1f- 0.01f;
					branchSkinWeldingRange.branchId = currentBranch.id;
					branchSkin.AddBuilderRange (branchSkinWeldingRange);
					isFirstBranch = false;
				}
				BranchSkinRange branchSkinRange = new BranchSkinRange ();
				branchSkinRange.from = branchSkinLength / branchSkin.length;
				branchSkinLength += currentBranch.length;
				branchSkinRange.to = branchSkinLength / branchSkin.length;
				branchSkinRange.branchId = currentBranch.id;
				currentBranch = currentBranch.followUp;
				branchSkin.AddBranchRange (branchSkinRange);
			} while (currentBranch != null);

			/*
			// Set branch skin branch ranges.
			currentBranch = firstBranch;
			branchSkinLength = 0f;
			do {
				BranchSkinRange branchSkinRange = new BranchSkinRange ();
				branchSkinRange.from = branchSkinLength / branchSkin.length;
				branchSkinLength += currentBranch.length;
				branchSkinRange.to = branchSkinLength / branchSkin.length;
				branchSkinRange.branchId = currentBranch.id;
				currentBranch = currentBranch.followUp;
				branchSkin.AddBranchRange (branchSkinRange);
			} while (currentBranch != null);
			*/

			return branchSkin;
		}
		/// <summary>
		/// Gets an existing BranchSkin instance given its id.
		/// </summary>
		/// <param name="id">Id of the BranchSkin instance.</param>
		/// <returns>BranchSkin instance if found, otherwise null.</returns>
		public BranchSkin GetBranchSkin (int id) {
			if (idToBranchSkin.ContainsKey (id)) {
				return idToBranchSkin [id];
			}
			return null;
		}
		/// <summary>
		/// Preprocessing method to call right after a BranchSkin is created. Processing gets mainly done by
		/// the BranchSkin builders per range.
		/// </summary>
		/// <param name="branchSkin"></param>
		/// <param name="firstBranch"></param>
		protected void PreprocessBranchSkin (BranchSkin branchSkin, BroccoTree.Branch firstBranch, BranchSkin parentBranchSkin = null, BroccoTree.Branch parentBranch = null) {
			// Get every builder per range on the BranchSkin instance.
			for (int i = 0; i < branchSkin.ranges.Count; i++) {
				IBranchMeshBuilder branchMeshBuilder = GetBuilder (branchSkin.ranges [i].builderType);
				if (branchMeshBuilder != null) {
					branchMeshBuilder.PreprocessBranchSkinRange (i, branchSkin, firstBranch, parentBranchSkin, parentBranch);
				}
			}
			// Set position offset for each sprout in branch.
			BroccoTree.Branch currentBranch = firstBranch;
			bool upperLimitReached;
			do {
				for (int i = 0; i < currentBranch.sprouts.Count; i++) {
					IBranchMeshBuilder branchMeshBuilder = GetBuilderAtPosition (branchSkin, 
						BranchSkin.TranslateToPositionAtBranchSkin (currentBranch.sprouts [i].position, currentBranch, firstBranch, branchSkin),
						out upperLimitReached);
					if (branchMeshBuilder != null) {
						currentBranch.sprouts [i].positionOffset = branchMeshBuilder.GetBranchSkinPositionOffset (currentBranch.sprouts [i].position, 
							currentBranch, currentBranch.sprouts [i].rollAngle, currentBranch.sprouts [i].forward, branchSkin);
					}
				}
				currentBranch = currentBranch.followUp;
			} while (currentBranch != null);
		}
		protected void PostprocessBranchSkin (
			List<Mesh> meshes,
			BranchSkin branchSkin, 
			BroccoTree.Branch firstBranch, 
			BranchSkin parentBranchSkin = null, 
			BroccoTree.Branch parentBranch = null) 
		{
			Mesh meshToAdd;
			// Get every builder per range on the BranchSkin instance.
			for (int i = 0; i < branchSkin.ranges.Count; i++) {
				IBranchMeshBuilder branchMeshBuilder = GetBuilder (branchSkin.ranges [i].builderType);
				if (branchMeshBuilder != null) {
					meshToAdd = branchMeshBuilder.PostprocessBranchSkinRange (null, i, branchSkin, firstBranch, parentBranchSkin, parentBranch);
					if (meshToAdd != null) {
						meshes.Add (meshToAdd);
					}
				}
			}
		}
		/// <summary>
		/// Skins the branch.
		/// </summary>
		/// <param name="branch">Branch.</param>
		/// <param name="branchSkin">Branch skin.</param>
		void SkinBranch (BroccoTree.Branch branch, BranchSkin branchSkin, float consumedLength, bool isFollowup = false) {
			//Skin Base
			if (consumedLength == 0) {
				if (branch.parent == null) {
					branchSkin.isTrunk = true;
				}
				consumedLength = SkinBranchBase (branch, branchSkin);
				if (consumedLength < 0) {
					return;
				}
			}

			// Skin Middle
			consumedLength = SkinBranchMiddleBody (branch, consumedLength, branchSkin, isFollowup);
			
			if (branch.followUp != null) {
				SkinBranch (branch.followUp, branchSkin, consumedLength, true);
			}

			if (branch.followUp == null) {
				// Skin Tip
				//SkinBranchTip (branch, branchSkin);
				branchSkins.Add (branchSkin);
			}

			for (int i = 0; i < branch.branches.Count; i++) {
				if (branch.branches[i] != branch.followUp) {
					float lengthOffset = branchSkin.lengthOffset + consumedLength - (branch.length * (1f - branch.branches[i].position));
					BranchSkin childBranchSkin = GetOrCreateBranchSkin (branch.branches[i],
						branchSkin.level + 1, 
						lengthOffset,
						lengthOffset / treeMaxLength);
					PreprocessBranchSkin (childBranchSkin, branch.branches [i], branchSkin, branch);
					branch.branches [i].Update ();
					SkinBranch (branch.branches[i], childBranchSkin, 0f);
				}
			}
		}
		/// <summary>
		/// Add mesh vertices for a branch base.
		/// </summary>
		/// <returns>The branch base.</returns>
		/// <param name="branch">Branch.</param>
		/// <param name="branchSkin">Branch skin.</param>
		protected virtual float SkinBranchBase (BroccoTree.Branch branch, BranchSkin branchSkin) {
			bool upperLimitReached;
			IBranchMeshBuilder branchMeshBuilder = GetBuilderAtPosition (branchSkin, 0, out upperLimitReached);
			float girth = branch.GetGirthAtPosition (0f);
			int numberOfSegments = branchMeshBuilder.GetNumberOfSegments (branchSkin, 0f, girth);
			Vector3 directionAtPosition = branch.GetDirectionAtPosition (0);
			Vector3 normalAtPosition = branch.GetNormalAtPosition (0);
			if (useMeshCapAtBase) {
				branchSkin.AddSegment (
					branch.id,
					branch.helperStructureLevelId, 
					branch.origin, 
					directionAtPosition, 
					normalAtPosition, 
					numberOfSegments,
					BranchSkin.SEGMENT_TYPE_BEGIN_CAP,
					0f, // Girth
					0f,
					0f,
					branchMeshBuilder.GetBuilderType ());	
			}
			branchSkin.AddSegment (
				branch.id,
				branch.helperStructureLevelId, 
				branch.origin, 
				directionAtPosition, 
				normalAtPosition, 
				numberOfSegments,
				BranchSkin.SEGMENT_TYPE_BEGIN,
				girth, 
				0f,
				0f,
				branchMeshBuilder.GetBuilderType ());
			branchSkin.lastDirection = branch.direction;
			return 0f;
		}
		/// <summary>
		/// Add mesh vertices for a branch middle body.
		/// </summary>
		/// <param name="branch">Branch.</param>
		/// <param name="consumedLength">Consumed length so far on this branch.</param>
		/// <param name="branchSkin">Branch skin.</param>
		protected virtual float SkinBranchMiddleBody (BroccoTree.Branch branch, float consumedLength, BranchSkin branchSkin, bool isFollowup = false) {
			List<float> inBranchRelevantPositions = new List<float> ();
			// Add Branches position to relevance list.
			for (int i = 0; i < branch.branches.Count; i++) {
				if (!branch.branches[i].isRoot) {
					inBranchRelevantPositions.Add (branch.branches[i].position);
				}
			}
			// Add parent skin surface segment.
			if (branchSkin.level > 0 && branchSkin.level <= averageNormalsLevelLimit && branch.parent != null && !isFollowup) {
				float girthAtParent = branch.parent.GetGirthAtPosition (branch.position) / branch.length;
				inBranchRelevantPositions.Add (girthAtParent);
			}

			/*
			// Add relevant positions from the BranchSkin if they belong to this branch.
			float inBranchPosition = 0f;
			List<float> bsRelevantPositions = branchSkin.GetRelevantPositions ();
			for (int i = 0; i < bsRelevantPositions.Count; i++) {
				if (BranchSkin.PositionBelongsToBranch (branchSkin, bsRelevantPositions[i], idToFirstBranch [branchSkin.id], branch.id, out inBranchPosition)) {
					inBranchRelevantPositions.Add (inBranchPosition);
				}	
			}
			*/

			// Add the branch break position if present.
			if (branch.isBroken) {
				inBranchRelevantPositions.Add (branch.breakPosition);
			}

			for (int i = 0; i < inBranchRelevantPositions.Count; i++) {
				//branchSkin.AddRelevantPosition (BranchSkin.TranslateToPositionAtBranchSkin (relevantPositions [i], branch.id, branchSkin), 0.01f, 1);
				branchSkin.AddRelevantPosition (BranchSkin.TranslateToPositionAtBranchSkin (inBranchRelevantPositions [i], branch.id, branchSkin), 0.1f, 1);
			}

			// Order relevance list
			//relevantPositions.Sort ();
			// Add positions to relevance list.
			List<float> branchRelevantPositions = branchSkin.GetBranchRelevantPositions (branch.id, 3, true);

			branchRelevantPositions.Sort ();

			List<CurvePoint> curvePoints = branch.curve.GetPoints (branchAngleTolerance, branchRelevantPositions);


			IBranchMeshBuilder branchMeshBuilder;
			bool upperLimitReached = false;
			for (int i = (isFollowup?0:1); i < curvePoints.Count; i++) {
				//float curvePointPosition = (float)(curvePoints[i].lengthPosition + consumedLength) / branchSkin.length;
				float curvePointPosition = (float)((curvePoints[i].relativePosition * branch.curve.length) + consumedLength) / branchSkin.length;
				branchMeshBuilder = GetBuilderAtPosition (branchSkin, (float)curvePointPosition, out upperLimitReached, !upperLimitReached);
				float girth = branch.GetGirthAtPosition (curvePoints[i].relativePosition);
				int numberOfSegments = branchMeshBuilder.GetNumberOfSegments (branchSkin, curvePointPosition, girth);
				Vector3 avgDirection = curvePoints[i].forward;
				if (!branch.isBroken || curvePoints[i].relativePosition <= branch.breakPosition + 0.001f) {
					// Set type of segment to add.
					int segmentType;
					if (isFollowup && i == 0) { segmentType = BranchSkin.SEGMENT_TYPE_BEGIN; }
					else if (i == curvePoints.Count - 1) { 
						segmentType = BranchSkin.SEGMENT_TYPE_END;
					} else {
						segmentType = BranchSkin.SEGMENT_TYPE_MIDDLE;
					}
					// Add the segment.
					branchSkin.AddSegment (branch.id, branch.helperStructureLevelId, branch.GetPointAtPosition (curvePoints[i].relativePosition), avgDirection,
						curvePoints [i].normal, numberOfSegments, segmentType, girth, curvePoints[i].relativePosition, 
						curvePointPosition, branchMeshBuilder.GetBuilderType ());
					// Add final cap if necessary. Dont include welding.
					if (i == curvePoints.Count - 1 && branch.followUp == null) {
						BuilderType builderType = branchMeshBuilder.GetBuilderType ();
						if (builderType == BuilderType.Welding) builderType = BuilderType.Default;
						branchSkin.AddSegment (branch.id, branch.helperStructureLevelId, branch.GetPointAtPosition (curvePoints[i].relativePosition), avgDirection,
							curvePoints [i].normal, numberOfSegments, BranchSkin.SEGMENT_TYPE_END_CAP, 0.001f, curvePoints[i].relativePosition, 
							curvePointPosition, builderType);
					}
				}
				branchSkin.lastDirection = curvePoints[i].tangent;
			}
			consumedLength += branch.length;
			return consumedLength;
		}
		#endregion

		#region Skin BranchSkinRange
		/// <summary>
		/// Get a BranchSkinRange instance from a BranchSkin instance if present at the given branch skin position.
		/// </summary>
		/// <param name="branchSkin">BranchSkin instance.</param>
		/// <param name="position">Position in the branch skin (from 0 to 1).</param>
		/// <returns>Branch skin range if found, null otherwise.</returns>
		BranchSkinRange GetRangeAtPosition (BranchSkin branchSkin, float position) {
			for (int i = 0; i < branchSkin.ranges.Count; i++) {
				if (position >= branchSkin.ranges [i].from && position <= branchSkin.ranges [i].to) {
					return branchSkin.ranges [i];
				}
			}
			return null;
		}
		/// <summary>
		/// Get the type of builder given a branchskin position.
		/// </summary>
		/// <param name="branchSkin">BranchSkin class.</param>
		/// <param name="position">Posiion on the branch skin.</param>
		/// <param name="upperLimitReached">Return true if the position is at the upper limit.</param>
		/// <param name="useInclusiveUpperLimit">Option to check for lower or equal than on upper limit instead of user only lower than.</param>
		/// <returns>Builder type assigned to the branch skin position.</returns>
		BuilderType GetBuilderTypeAtPosition (BranchSkin branchSkin, float position, out bool upperLimitReached, bool useInclusiveUpperLimit = true) {
			// If there are no ranges, everything gets processed with the default mesh builder.
			upperLimitReached = false;
			bool withinRange = false;
			for (int i = 0; i < branchSkin.ranges.Count; i++) {
				if (useInclusiveUpperLimit) {
					withinRange = position >= branchSkin.ranges [i].from && position <= branchSkin.ranges [i].to;
				} else {
					withinRange = position >= branchSkin.ranges [i].from && position < branchSkin.ranges [i].to;
				}
				if (withinRange ||
					(Mathf.Approximately (position, branchSkin.ranges [i].from) || Mathf.Approximately (position, branchSkin.ranges [i].to))) {
					upperLimitReached = Mathf.Approximately (position, branchSkin.ranges [i].to);
					return branchSkin.ranges [i].builderType;
				}
			}
			return BuilderType.Default;
		}
		/// <summary>
		/// Gets the mesh builder assigned to a builder type.
		/// </summary>
		/// <param name="builderType">Builder type.</param>
		/// <returns>Builder assigned to a builder type.</returns>
		IBranchMeshBuilder GetBuilder (BuilderType builderType) {
			if (_branchMeshBuilders.ContainsKey (builderType)) {
				return _branchMeshBuilders [builderType];
			}
			return null;
		}
		/// <summary>
		/// Gets the mesh builder assigned at a branchskin position.
		/// </summary>
		/// <param name="branchSkin">BranchSkin class.</param>
		/// <param name="position">Position on the branch skin.</param>
		/// <param name="upperLimitReached">Return true if the position is at the upper limit.</param>
		/// <param name="useInclusiveUpperLimit">Option to check for lower or equal than on upper limit instead of user only lower than.</param>
		/// <returns>Builder assigned to the branch skin position.</returns>
		IBranchMeshBuilder GetBuilderAtPosition (BranchSkin branchSkin, float position, out bool upperLimitReached, bool useInclusiveUpperLimit = true) {
			BuilderType builderType = GetBuilderTypeAtPosition (branchSkin, position, out upperLimitReached, useInclusiveUpperLimit);
			_branchMeshBuilders [builderType].SetAngleTolerance (branchAngleTolerance);
			return _branchMeshBuilders [builderType];
		}
		#endregion
		
		#region Mesh Branch
		/// <summary>
		/// Builds a mesh from this clas branchSkins.
		/// </summary>
		/// <returns>Mesh object.</returns>
		protected Mesh MeshBranchSkinsJob () {
			// Count total number of segments and total vertices.
			int totalBranchSkins = branchSkins.Count;
			int totalSegments = 0;
			int totalVertices = 0;
			int totalTriangles = 0;
			int totalShapeVertices = 0;

			for (int i = 0; i < totalBranchSkins; i++) {
				totalSegments += branchSkins [i].segments.Count;
				totalVertices += branchSkins [i].totalVertices;
				totalTriangles += branchSkins [i].totalTriangles;
				totalShapeVertices += branchSkins [i].totalShapeVertices;
			}

			// Create BranchJob for processing multiple branch skin instances.
			BranchJob _branchJob = new BranchJob () {
				useHardNormals = useHardNormals,
				useMeshCapAtBase = useMeshCapAtBase,
				globalScale = globalScale,
				bsIdsLengthsLengthOffsetsShapeOffsets = new NativeArray<Vector4> (totalBranchSkins, Allocator.TempJob),
				segStartLengthVertStartTrisStart = new NativeArray<Vector4> (totalBranchSkins, Allocator.TempJob),
				aIdsStructsSegsSegTypes = new NativeArray<Vector4> (totalSegments, Allocator.TempJob),
				aCentersPos = new NativeArray<Vector4> (totalSegments, Allocator.TempJob),
				aDirectionsPosAtSkins = new NativeArray<Vector4> (totalSegments, Allocator.TempJob),
				aNormalsGirths = new NativeArray<Vector4> (totalSegments, Allocator.TempJob),
				aBuilders = new NativeArray<int> (totalSegments, Allocator.TempJob),
				aShapeVertShapePos = new NativeArray<Vector4> (totalShapeVertices, Allocator.TempJob),

				vertices = new NativeArray<Vector3> (totalVertices, Allocator.TempJob),
				normals = new NativeArray<Vector3> (totalVertices, Allocator.TempJob),
				uvs = new NativeArray<Vector4> (totalVertices, Allocator.TempJob),
				uv3s = new NativeArray<Vector4> (totalVertices, Allocator.TempJob),
				uv5s = new NativeArray<Vector4> (totalVertices, Allocator.TempJob),
				uv6s = new NativeArray<Vector4> (totalVertices, Allocator.TempJob),
				uv7s = new NativeArray<Vector4> (totalVertices, Allocator.TempJob),
				uv8s = new NativeArray<Vector4> (totalVertices, Allocator.TempJob),

				triangles = new NativeArray<int> (totalTriangles * 3, Allocator.TempJob)
			};

			// Initialize start index vars and total vertices count var.
			int _segmentStartIndex = 0;
			int _verticesStartIndex = 0;
			int _trisStartIndex = 0;
			int _shapeVerticesStartIndex = 0;
			BranchSkin _branchSkin;

			// Iterate through each branch skin.
			for (int i = 0; i < branchSkins.Count; i++) {
				// Select branch skin.
				_branchSkin = branchSkins [i];

				// Set branch skin values for the job.
				_branchJob.bsIdsLengthsLengthOffsetsShapeOffsets [i] = new Vector4 (_branchSkin.id, _branchSkin.length, _branchSkin.lengthOffset, _shapeVerticesStartIndex);
				_branchJob.segStartLengthVertStartTrisStart [i] = new Vector4 (_segmentStartIndex, _branchSkin.segments.Count, _verticesStartIndex, _trisStartIndex);

				// Iterate through segents to add them to the job.
				for (int j = 0; j < _branchSkin.segments.Count; j++) {
					_branchJob.aIdsStructsSegsSegTypes [_segmentStartIndex + j] = _branchSkin.idsSegs [j];
					_branchJob.aCentersPos [_segmentStartIndex + j] = _branchSkin.centersPos [j];
					_branchJob.aDirectionsPosAtSkins [_segmentStartIndex + j] = _branchSkin.directionsPosAtSkin [j];
					_branchJob.aNormalsGirths [_segmentStartIndex + j] = _branchSkin.normalsGirths [j];
					_branchJob.aBuilders [_segmentStartIndex + j] = (int)_branchSkin.builders [j];
				}

				// Add BranchSkin shape vertices.
				for (int j = 0; j < _branchSkin.shapeVerticesPos.Count; j++) {
					_branchJob.aShapeVertShapePos [_shapeVerticesStartIndex + j] = _branchSkin.shapeVerticesPos [j];
				}
				_shapeVerticesStartIndex += _branchSkin.totalShapeVertices;

				_segmentStartIndex += _branchSkin.segments.Count;
				_verticesStartIndex += _branchSkin.totalVertices;
				_trisStartIndex += _branchSkin.totalTriangles * 3;
			}

			// Execute the branch jobs.
			JobHandle _branchJobHandle = _branchJob.Schedule (totalBranchSkins, 12);

			// Complete the job.
			_branchJobHandle.Complete();

			// Build the mesh.
			Mesh mesh = new Mesh ();
			Vector3[] _vertices = new Vector3 [totalVertices];
			Vector3[] _normals = new Vector3 [totalVertices];
			Vector4[] _uvs = new Vector4 [totalVertices];
			Vector4[] _uv3s = new Vector4 [totalVertices];
			Vector4[] _uv5s = new Vector4 [totalVertices];
			Vector4[] _uv6s = new Vector4 [totalVertices];
			Vector4[] _uv7s = new Vector4 [totalVertices];
			Vector4[] _uv8s = new Vector4 [totalVertices];
			int[] _triangles = new int [totalTriangles * 3];

			_branchJob.vertices.CopyTo (_vertices);
        	_branchJob.normals.CopyTo (_normals);
			_branchJob.triangles.CopyTo (_triangles);
			_branchJob.uvs.CopyTo (_uvs);
			_branchJob.uv3s.CopyTo (_uv3s);
			_branchJob.uv5s.CopyTo (_uv5s);
			_branchJob.uv6s.CopyTo (_uv6s);
			_branchJob.uv7s.CopyTo (_uv7s);
			_branchJob.uv8s.CopyTo (_uv8s);

			mesh.vertices = _vertices;
			mesh.normals = _normals;
			mesh.triangles = _triangles;
			mesh.SetUVs (0, new List<Vector4>(_uvs));
			mesh.SetUVs (2, new List<Vector4>(_uv3s));
			mesh.SetUVs (4, new List<Vector4>(_uv5s));
			mesh.SetUVs (5, new List<Vector4>(_uv6s));
			mesh.SetUVs (6, new List<Vector4>(_uv7s));
			mesh.SetUVs (7, new List<Vector4>(_uv8s));
			/*
			List<Color> colors = new List<Color> ();
			for (int i = 0; i < _vertices.Length; i++) {
				colors.Add (new Color (1f, 1f, 1f, Random.Range(0f,1f)));
			}
			mesh.SetColors (colors);
			*/

			// Dispose allocated memory.
			_branchJob.bsIdsLengthsLengthOffsetsShapeOffsets.Dispose ();
			_branchJob.segStartLengthVertStartTrisStart.Dispose ();

			_branchJob.aIdsStructsSegsSegTypes.Dispose ();
			_branchJob.aCentersPos.Dispose ();
			_branchJob.aDirectionsPosAtSkins.Dispose ();
			_branchJob.aNormalsGirths.Dispose ();
			_branchJob.aBuilders.Dispose ();
			_branchJob.aShapeVertShapePos.Dispose ();

			_branchJob.vertices.Dispose ();
			_branchJob.normals.Dispose ();
			_branchJob.uvs.Dispose ();
			_branchJob.uv3s.Dispose ();
			_branchJob.uv5s.Dispose ();
			_branchJob.uv6s.Dispose ();
			_branchJob.uv7s.Dispose ();
			_branchJob.uv8s.Dispose ();
			_branchJob.triangles.Dispose ();

			// Return mesh.
			return mesh;
		}
		#endregion

		#region Geometry processing
		/// <summary>
		/// Traverses a tree branch structure and collects information about
		/// the girth on its branches.
		/// </summary>
		/// <param name="tree">Tree.</param>
		protected void AnalyzeTree (BroccoTree tree) {
			// Get max length on all the tree.
			treeMaxLength = tree.GetMaxLength ();

			List<BroccoTree.Branch> branches = tree.GetDescendantBranches ();
			for (int i = 0; i < branches.Count; i++) {
				float baseGirth = branches[i].GetGirthAtPosition (0f);
				float mediumGirth = branches[i].GetGirthAtPosition (0f);
				float topGirth = branches[i].GetGirthAtPosition (0f);
				float avgGirth = (baseGirth + mediumGirth + topGirth) / 3f;

				// Check avg girth.
				if (treeMinAvgGirth == -1f || avgGirth < treeMinAvgGirth) {
					treeMinAvgGirth = avgGirth;
				}
				if (avgGirth > treeMaxAvgGirth) {
					treeMaxAvgGirth = avgGirth;
				}

				// Check against minGirth.
				if (treeMinGirth == -1 || baseGirth < treeMinGirth) {
					treeMinGirth = baseGirth;
				}
				if (mediumGirth < treeMinGirth) {
					treeMinGirth = mediumGirth;
				}
				if (topGirth < treeMinGirth) {
					treeMinGirth = topGirth;
				}

				// Check against maxGirth.
				if (baseGirth > treeMaxGirth) {
					treeMaxGirth = baseGirth;
				}
				if (mediumGirth > treeMaxGirth) {
					treeMaxGirth = mediumGirth;
				}
				if (topGirth > treeMaxGirth) {
					treeMaxGirth = topGirth;
				}
			}
		}
		#endregion

		#region Maintenance
		/// <summary>
		/// Clear this instance variables.
		/// </summary>
		public virtual void Clear () {
			vertexCount = 0;
			verticesGenerated = 0;
			trianglesGenerated = 0;
			treeMaxLength = 0f;
			treeMinGirth = -1f;
			treeMaxGirth = -1f;
			treeMinAvgGirth = -1f;
			treeMaxAvgGirth = -1f;
			branchSkins.Clear ();
		}
		#endregion
	}
}