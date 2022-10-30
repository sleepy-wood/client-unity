using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Broccoli.Model;

namespace Broccoli.Builder
{
	/// <summary>
	/// Generates data to support the mesh creation of sprouts.
	/// </summary>
	public class SproutMeshMetaBuilder {
		#region Singleton
		/// <summary>
		/// This class singleton instance.
		/// </summary>
		static SproutMeshMetaBuilder _sproutMeshMetaBuilder = null;
		/// <summary>
		/// Gets the singleton instance for this class.
		/// </summary>
		/// <returns>The instance.</returns>
		public static SproutMeshMetaBuilder GetInstance () {
			if (_sproutMeshMetaBuilder == null) {
				_sproutMeshMetaBuilder = new SproutMeshMetaBuilder ();
			}
			return _sproutMeshMetaBuilder;
		}
		#endregion

		#region UVs
		/// <summary>
		/// Crops 0-1 based uvs.
		/// </summary>
		/// <param name="uvs">UVs array.</param>
		/// <param name="x">X offset value.</param>
		/// <param name="y">Y offset value.</param>
		/// <param name="width">Width offset value.</param>
		/// <param name="height">Height offset value.</param>
		/// <param name="step">Rotation for the corner values of the UVs. (0 = 0, 1 = 90, 2 = 180, 3 = -90).</param>
		public void GetCropUVs (ref List<Vector4> uvs, float x, float y, float width, float height, int step = 0) {
			GetCropUVs (ref uvs, 0, uvs.Count, x, y, width, height, step);
		}
		/// <summary>
		/// Crops 0-1 based uvs.
		/// </summary>
		/// <param name="uvs">UVs array.</param>
		/// <param name="startIndex">Start index in the array.</param>
		/// <param name="length">Length of positions in the array.</param>
		/// <param name="x">X offset value.</param>
		/// <param name="y">Y offset value.</param>
		/// <param name="width">Width offset value.</param>
		/// <param name="height">Height offset value.</param>
		/// <param name="step">Rotation for the corner values of the UVs. (0 = 0, 1 = 90, 2 = 180, 3 = -90).</param>
		public void GetCropUVs (ref List<Vector4> uvs, int startIndex, int length, float x, float y, float width, float height, int step = 0) {
			Vector4 pointA = new Vector4 (x, y, 0, 0);
			Vector4 pointB = new Vector4 (x + width, y, 1, 0);
			Vector4 pointC = new Vector4 (x + width, y + height, 1, 1);
			Vector4 pointD = new Vector4 (x, y + height, 0, 1);
			Vector4[] points;
			points = new Vector4[] { pointA, pointB, pointC, pointD };
			for (int i = startIndex; i < startIndex + length; i++) {
				uvs [i] = new Vector4 (Mathf.Lerp (points[0].x, points[1].x, uvs [i].z), 
					Mathf.Lerp (points[0].y, points[2].y, uvs [i].w),
					uvs [i].z,
					uvs [i].w);
			}
		}
		/// <summary>
		/// Gets the UVs for a cropped mapping of a plane.
		/// </summary>
		/// <returns>The cropped plane UVs.</returns>
		/// <param name="planes">Number of planes.</param>
		/// <param name="x">X start value for the cropping. From 0 to 1.</param>
		/// <param name="y">Y start value for the cropping. From 0 to 1.</param>
		/// <param name="width">Width of the cropping. From 0 to 1.</param>
		/// <param name="height">Height of the cropping. From 0 to 1.</param>
		/// <param name="isTwoSided">True if the mesh is two sided.</param>
		/// <param name="step">Rotation for the corner values of the UVs. (0 = 0, 1 = 90, 2 = 180, 3 = -90).</param>
		public Vector4[] GetCropPlaneUVs (int planes, float x, float y, float width, float height, bool isTwoSided, int step = 0) {
			planes = Mathf.Clamp (planes, 1, 3);
			Vector4[] uvs = new Vector4[planes * (isTwoSided?8:4)];
			Vector4 pointA = new Vector4 (x, y, 0, 0);
			Vector4 pointB = new Vector4 (x + width, y, 1, 0);
			Vector4 pointC = new Vector4 (x + width, y + height, 1, 1);
			Vector4 pointD = new Vector4 (x, y + height, 0, 1);
			Vector4[] points;
			if (step == 0) {
				points = new Vector4[] { pointA, pointB, pointC, pointD };
			} else if (step == 1) {
				points = new Vector4[] { pointB, pointC, pointD, pointA };
			} else if (step == 2) {
				points = new Vector4[] { pointC, pointD, pointA, pointB };
			} else {
				points = new Vector4[] { pointD, pointA, pointB, pointC };
			}
			if (isTwoSided) {
				for (int i = 0; i < planes; i++) {
					uvs [0 + i * 8] = points[3];
					uvs [1 + i * 8] = points[0];
					uvs [2 + i * 8] = points[1];
					uvs [3 + i * 8] = points[2];
					uvs [4 + i * 8] = points[3];
					uvs [5 + i * 8] = points[0];
					uvs [6 + i * 8] = points[1];
					uvs [7 + i * 8] = points[2];
				}
			} else {
					for (int i = 0; i < planes; i++) {
					uvs [0 + i * 4] = points[3];
					uvs [1 + i * 4] = points[0];
					uvs [2 + i * 4] = points[1];
					uvs [3 + i * 4] = points[2];
				}
			}
			return uvs;
		}
		/// <summary>
		/// Gets the UVs for a cropped mapping of a plane.
		/// </summary>
		/// <returns>The cropped plane UVs.</returns>
		/// <param name="planes">Number of planes.</param>
		/// <param name="x">X start value for the cropping. From 0 to 1.</param>
		/// <param name="y">Y start value for the cropping. From 0 to 1.</param>
		/// <param name="width">Width of the cropping. From 0 to 1.</param>
		/// <param name="height">Height of the cropping. From 0 to 1.</param>
		/// <param name="step">Rotation for the corner values of the UVs. (0 = 0, 1 = 90, 2 = 180, 3 = -90).</param>
		public Vector4[] GetCropBillboardUVs (float x, float y, float width, float height, int step = 0) {
			Vector4[] uvs = new Vector4[4];
			Vector4 pointA = new Vector4 (x, y, 0, 0);
			Vector4 pointB = new Vector4 (x + width, y, 1, 0);
			Vector4 pointC = new Vector4 (x + width, y + height, 1, 1);
			Vector4 pointD = new Vector4 (x, y + height, 0, 1);
			Vector4[] points;
			if (step == 0) {
				points = new Vector4[] { pointA, pointB, pointC, pointD };
			} else if (step == 1) {
				points = new Vector4[] { pointB, pointC, pointD, pointA };
			} else if (step == 2) {
				points = new Vector4[] { pointC, pointD, pointA, pointB };
			} else {
				points = new Vector4[] { pointD, pointA, pointB, pointC };
			}
			uvs [0] = points [3];
			uvs [1] = points [0];
			uvs [2] = points [1];
			uvs [3] = points [2];
			return uvs;
		}
		/// <summary>
		/// Gets the UVs for a cropped mapping of a plane X.
		/// </summary>
		/// <returns>The cropped plane UVs.</returns>
		/// <param name="x">X start value for the cropping. From 0 to 1.</param>
		/// <param name="y">Y start value for the cropping. From 0 to 1.</param>
		/// <param name="width">Width of the cropping. From 0 to 1.</param>
		/// <param name="height">Height of the cropping. From 0 to 1.</param>
		/// <param name="step">Rotation for the corner values of the UVs. (0 = 0, 1 = 90, 2 = 180, 3 = -90).</param>
		public Vector4[] GetCropPlaneXUVs (float x, float y, float width, float height, bool isTwoSided, int step = 0) { // TODO: convertion from 0-1 to cropped space.
			Vector4[] uvs = new Vector4 [isTwoSided?10:5];
			
			Vector4 pointA = new Vector4 (x, y, 0, 0);
			Vector4 pointB = new Vector4 (x + width, y, 1, 0);
			Vector4 pointC = new Vector4 (x + width, y + height, 1, 1);
			Vector4 pointD = new Vector4 (x, y + height, 0, 1);
			Vector4[] points;
			if (step == 0) {
				points = new Vector4[] { pointA, pointB, pointC, pointD };
			} else if (step == 1) {
				points = new Vector4[] { pointB, pointC, pointD, pointA };
			} else if (step == 2) {
				points = new Vector4[] { pointC, pointD, pointA, pointB };
			} else {
				points = new Vector4[] { pointD, pointA, pointB, pointC };
			}
			uvs [0] = points[3];

			uvs [1] = points[0];
			uvs [2] = points[1];
			uvs [3] = points[2];
			uvs [4] = (points[3] + points[1]) / 2f;
			if (isTwoSided) {
				uvs [5] = points[3];
				uvs [6] = points[0];
				uvs [7] = points[1];
				uvs [8] = points[2];
				uvs [9] = (points[3] + points[1]) / 2f;
			}
			return uvs;
		}
		#endregion

		#region UV2
		/// <summary>
		/// Get the plane UV2s.
		/// </summary>
		/// <param name="pivotW">Pivot on the width side.</param>
		/// <param name="pivotH">Pivot on the height side.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		/// <param name="isTwoSided">True if the plane has two sides.</param>
		/// <returns></returns>
		public Vector4[] GetPlaneUV2s (float pivotW, float pivotH, float width, float height, bool isTwoSided) {
			Vector4[] uv2sPerPlane = new Vector4[isTwoSided?8:4];
			float sqrtDistance = Mathf.Sqrt (Mathf.Pow (width, 2) + Mathf.Pow (height, 2));
			float w = width / sqrtDistance;
			float h = height / sqrtDistance;
			if (pivotW < 0.33f) {
				if (pivotH < 0.33f) {
					uv2sPerPlane [2] = new Vector4 (0, 0, w, w);
					uv2sPerPlane [3] = new Vector4 (0, 0, 1, 1);
					uv2sPerPlane [0] = new Vector4 (0, 0, h, h);
					if (isTwoSided) {
						uv2sPerPlane [6] = new Vector4 (0, 0, w, w);
						uv2sPerPlane [7] = new Vector4 (0, 0, 1, 1);
						uv2sPerPlane [4] = new Vector4 (0, 0, h, h);
					}
				} else if (pivotH > 0.66f) {
					uv2sPerPlane [2] = new Vector4 (0, 0, 1, 1);
					uv2sPerPlane [1] = new Vector4 (0, 0, h, h);
					uv2sPerPlane [3] = new Vector4 (0, 0, w, w);
					if (isTwoSided) {
						uv2sPerPlane [5] = new Vector4 (0, 0, h, h);
						uv2sPerPlane [6] = new Vector4 (0, 0, 1, 1);
						uv2sPerPlane [7] = new Vector4 (0, 0, w, w);
					}
				} else {
					uv2sPerPlane [2] = new Vector4 (0, 0, w, w);
					uv2sPerPlane [3] = new Vector4 (0, 0, w, w);
					if (isTwoSided) {
						uv2sPerPlane [6] = new Vector4 (0, 0, w, w);
						uv2sPerPlane [7] = new Vector4 (0, 0, w, w);
					}
				}
			} else if (pivotW > 0.66f) {
				if (pivotH < 0.33f) {
					uv2sPerPlane [1] = new Vector4 (0, 0, w, w);
					uv2sPerPlane [0] = new Vector4 (0, 0, 1, 1);
					uv2sPerPlane [3] = new Vector4 (0, 0, h, h);
					if (isTwoSided) {
						uv2sPerPlane [5] = new Vector4 (0, 0, w, w);
						uv2sPerPlane [4] = new Vector4 (0, 0, 1, 1);
						uv2sPerPlane [7] = new Vector4 (0, 0, h, h);
					}
				} else if (pivotH > 0.66f) {
					uv2sPerPlane [0] = new Vector4 (0, 0, w, w);
					uv2sPerPlane [1] = new Vector4 (0, 0, 1, 1);
					uv2sPerPlane [2] = new Vector4 (0, 0, h, h);
					if (isTwoSided) {
						uv2sPerPlane [4] = new Vector4 (0, 0, w, w);
						uv2sPerPlane [5] = new Vector4 (0, 0, 1, 1);
						uv2sPerPlane [6] = new Vector4 (0, 0, h, h);
					}
				} else {
					uv2sPerPlane [0] = new Vector4 (0, 0, w, w);
					uv2sPerPlane [1] = new Vector4 (0, 0, w, w);
					uv2sPerPlane [4] = new Vector4 (0, 0, w, w);
					uv2sPerPlane [5] = new Vector4 (0, 0, w, w);
				}
			} else {
				if (pivotH < 0.33f) {
					uv2sPerPlane [0] = new Vector4 (0, 0, 1, 1);
					uv2sPerPlane [3] = new Vector4 (0, 0, 1, 1);
					if (isTwoSided) {
						uv2sPerPlane [4] = new Vector4 (0, 0, 1, 1);
						uv2sPerPlane [7] = new Vector4 (0, 0, 1, 1);
					}
				} else if (pivotH > 0.66f) {
					uv2sPerPlane [1] = new Vector4 (0, 0, 1, 1);
					uv2sPerPlane [2] = new Vector4 (0, 0, 1, 1);
					if (isTwoSided) {
						uv2sPerPlane [5] = new Vector4 (0, 0, 1, 1);
						uv2sPerPlane [6] = new Vector4 (0, 0, 1, 1);
					}
				} else {
					for (int i = 0; i < (isTwoSided?8:4); i++) {
						uv2sPerPlane [i] = Vector4.one / 2f;
					}
				}
			}
			return uv2sPerPlane;
		}
		/// <summary>
		/// Get the plane X UV2s.
		/// </summary>
		/// <param name="pivotW">Pivot on the width side.</param>
		/// <param name="pivotH">Pivot on the height side.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		/// <param name="isTwoSided">True if the plane has two sides.</param>
		/// <returns></returns>
		public Vector4[] GetPlaneXUV2s (float pivotW, float pivotH, float width, float height, bool isTwoSided) {
			Vector4[] uv2s = new Vector4[isTwoSided?10:5];
			if (pivotH > 0.33f && pivotH < 0.66f && pivotW > 0.33f && pivotW < 0.66f) {
				uv2s [0] = Vector4.one;
				uv2s [1] = Vector4.one;
				uv2s [2] = Vector4.one;
				uv2s [3] = Vector4.one;
				uv2s [4] = Vector3.zero;
				if (isTwoSided) {
					uv2s [5] = Vector4.one;
					uv2s [6] = Vector4.one;
					uv2s [7] = Vector4.one;
					uv2s [8] = Vector4.one;
					uv2s [9] = Vector3.zero;
				}
			} else {
				Vector4[] baseUV2s = GetPlaneUV2s (pivotW, pivotH, width, height, isTwoSided);
				float u = (uv2s[1].z + uv2s[3].z) / 2f;
				float v = (uv2s[1].w + uv2s[3].w) / 2f;
				uv2s [0] = baseUV2s [0];
				uv2s [1] = baseUV2s [1];
				uv2s [2] = baseUV2s [2];
				uv2s [3] = baseUV2s [3];
				uv2s [4] = new Vector4 (u, v, u, v);
				if (isTwoSided) {
					uv2s [5] = baseUV2s [4];
					uv2s [6] = baseUV2s [5];
					uv2s [7] = baseUV2s [6];
					uv2s [8] = baseUV2s [7];
					uv2s [9] = new Vector4 (u, v, u, v);
				}
			}
			return uv2s;
		}
		/// <summary>
		/// Get the billboard UV2s.
		/// </summary>
		/// <param name="pivotW">Pivot on the width side.</param>
		/// <param name="pivotH">Pivot on the height side.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		/// <returns></returns>
		public Vector4[] GetBillboardUV2s (float pivotW, float pivotH, float width, float height) {
			Vector4[] uv2sPerBillboard = new Vector4[4];
			float sqrtDistance = Mathf.Sqrt (Mathf.Pow (width, 2) + Mathf.Pow (height, 2));
			float w = width / sqrtDistance;
			float h = height / sqrtDistance;
			if (pivotW < 0.33f) {
				if (pivotH < 0.33f) {
					uv2sPerBillboard [2] = new Vector4 (0, 0, w, w);
					uv2sPerBillboard [3] = new Vector4 (0, 0, 1, 1);
					uv2sPerBillboard [0] = new Vector4 (0, 0, h, h);
				} else if (pivotH > 0.66f) {
					uv2sPerBillboard [1] = new Vector4 (0, 0, h, h);
					uv2sPerBillboard [2] = new Vector4 (0, 0, 1, 1);
					uv2sPerBillboard [3] = new Vector4 (0, 0, w, w);
				} else {
					uv2sPerBillboard [2] = new Vector4 (0, 0, w, w);
					uv2sPerBillboard [3] = new Vector4 (0, 0, w, w);
				}
			} else if (pivotW > 0.66f) {
				if (pivotH < 0.33f) {
					uv2sPerBillboard [1] = new Vector4 (0, 0, w, w);
					uv2sPerBillboard [0] = new Vector4 (0, 0, 1, 1);
					uv2sPerBillboard [3] = new Vector4 (0, 0, h, h);
				} else if (pivotH > 0.66f) {
					uv2sPerBillboard [0] = new Vector4 (0, 0, w, w);
					uv2sPerBillboard [1] = new Vector4 (0, 0, 1, 1);
					uv2sPerBillboard [2] = new Vector4 (0, 0, h, h);
				} else {
					uv2sPerBillboard [0] = new Vector4 (0, 0, w, w);
					uv2sPerBillboard [1] = new Vector4 (0, 0, w, w);
				}
			} else {
				if (pivotH < 0.33f) {
					uv2sPerBillboard [0] = new Vector4 (0, 0, 1, 1);
					uv2sPerBillboard [3] = new Vector4 (0, 0, 1, 1);
				} else if (pivotH > 0.66f) {
					uv2sPerBillboard [1] = new Vector4 (0, 0, 1, 1);
					uv2sPerBillboard [2] = new Vector4 (0, 0, 1, 1);
				} else {
					for (int i = 0; i < 4; i++) {
						uv2sPerBillboard [i] = Vector4.one / 2f;
					}
				}
			}
			return uv2sPerBillboard;
		}
		#endregion

		#region Colors
		/// <summary>
		/// Gets an array of color values.
		/// </summary>
		/// <returns>Colors array.</returns>
		/// <param name="length">Length of the array.</param>
		public Color[] GetColor (Color baseColor, int length) {
			Color[] colors = new Color[length];
			for (int i = 0; i < length; i++) {
				colors [i] = baseColor;
			}
			return colors;
		}
		#endregion

		#region Tangents
		/// <summary>
		/// Recalculates tangents for a mesh.
		/// </summary>
		/// <param name="mesh">Mesh.</param>
		public void RecalculateTangents(Mesh mesh)
		{
			int triangleCount = mesh.triangles.Length;
			int vertexCount = mesh.vertices.Length;

			Vector3[] tan1 = new Vector3[vertexCount];
			Vector3[] tan2 = new Vector3[vertexCount];
			Vector4[] tangents = new Vector4[vertexCount];

			Vector3[] meshVertices = mesh.vertices;
			int[] meshTriangles = mesh.triangles;
			List<Vector4> meshUVs = new List<Vector4> ();
			mesh.GetUVs (0, meshUVs);

			Vector2 w1 = Vector2.zero;
			Vector2 w2 = Vector2.zero;
			Vector2 w3 = Vector2.zero;

			for(long a = 0; a < triangleCount; a+=3)
			{
				long i1 = meshTriangles[a+0];
				long i2 = meshTriangles[a+1];
				long i3 = meshTriangles[a+2];
				Vector3 v1 = meshVertices[i1];
				Vector3 v2 = meshVertices[i2];
				Vector3 v3 = meshVertices[i3];
				if (meshUVs.Count > 0) {
					w1 = meshUVs [(int)i1];
					w2 = meshUVs [(int)i2];
					w3 = meshUVs [(int)i3];
				}
				float x1 = v2.x - v1.x;
				float x2 = v3.x - v1.x;
				float y1 = v2.y - v1.y;
				float y2 = v3.y - v1.y;
				float z1 = v2.z - v1.z;
				float z2 = v3.z - v1.z;
				float s1 = w2.x - w1.x;
				float s2 = w3.x - w1.x;
				float t1 = w2.y - w1.y;
				float t2 = w3.y - w1.y;
				float r = 1.0f / (s1 * t2 - s2 * t1);
				Vector3 sdir = new Vector3 ((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
				Vector3 tdir = new Vector3 ((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);
				tan1[i1] += sdir;
				tan1[i2] += sdir;
				tan1[i3] += sdir;
				tan2[i1] += tdir;
				tan2[i2] += tdir;
				tan2[i3] += tdir;
			}

			Vector3[] meshNormals = mesh.normals;
			for (long a = 0; a < vertexCount; ++a)
			{
				Vector3 n = meshNormals[a];
				Vector3 t = tan1[a];
				Vector3 tmp = (t - n * Vector3.Dot(n, t)).normalized;
				tangents[a] = new Vector4(tmp.x, tmp.y, tmp.z);
				tangents[a].w = (Vector3.Dot (Vector3.Cross(n, t), tan2[a]) < 0.0f) ? -1.0f : 1.0f;
			}
			mesh.tangents = tangents;
		}
		/// <summary>
		/// Set the mesh tangents to zero.
		/// </summary>
		/// <param name="mesh">Mesh.</param>
		public void TangentsToZero (Mesh mesh) {
			int i = mesh.vertices.Length;
			Vector4[] tangents = new Vector4[i];
			for (int j = 0; j < i; j++) {
				tangents[j] = new Vector4 (1, 0, 0, 0);
			}
			mesh.tangents = tangents;
		}
		#endregion
	}
}