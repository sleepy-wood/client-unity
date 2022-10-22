using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;

using Broccoli.Manager;
using Broccoli.Base;

namespace Broccoli.Builder
{
	public class BillboardBuilder {
		#region Vars
		/// <summary>
		/// Layer to move the object when rendering with the camera.
		/// </summary>
		private const int PREVIEW_LAYER = 22;
		/// <summary>
		/// Number of shots to take around the target.
		/// </summary>
		public int imageCount = 8;
		/// <summary>
		/// The size of the texture.
		/// </summary>
		public Vector2 textureSize = new Vector2(1024, 1024);
		/// <summary>
		/// Padding pixels for the billboard atlas.
		/// </summary>
		int paddingPixels = 3;
		/// <summary>
		/// Number of rows on the target texture.
		/// </summary>
		private int rows = 1;
		/// <summary>
		/// Number of columns on the target texture.
		/// </summary>
		private int columns = 1;
		/// <summary>
		/// The target bounds.
		/// </summary>
		public Bounds targetBounds;
		/// <summary>
		/// Mesh bounds domain offset from the base of the trunk (0,0,0 position at mesh domain).
		/// </summary>
		public float meshTargetYOffset= 0f;
		/// <summary>
		/// The width of the target.
		/// </summary>
		private float targetWidth = 1f;
		/// <summary>
		/// The height of the target.
		/// </summary>
		private float targetHeight = 1f;
		/// <summary>
		/// The target center.
		/// </summary>
		private Vector3 targetCenter = Vector3.zero;
		/// <summary>
		/// The target aspect ratio.
		/// </summary>
		private float targetAspect = 1f;
		/// <summary>
		/// The UV coords of the generated images.
		/// </summary>
		public List<Vector4> imageCoords = new List<Vector4> ();
		/// <summary>
		/// The billboard camera.
		/// </summary>
		public Camera billboardCamera;
		/// <summary>
		/// Game object containing the billboard camera.
		/// </summary>
		public GameObject billboardCameraGameObject;
		/// <summary>
		/// The billboard game object.
		/// </summary>
		GameObject billboardGameObject;
		/// <summary>
		/// The billboard asset.
		/// </summary>
		BillboardAsset billboardAsset = null;
		/// <summary>
		/// The billboard renderer.
		/// </summary>
		BillboardRenderer billboardRenderer = null;
		/// <summary>
		/// The billboard mesh.
		/// </summary>
		Mesh billboardMesh = null;
		/// <summary>
		/// The billboard material.
		/// </summary>
		public Material billboardMaterial;
		/// <summary>
		/// The billboard texture path.
		/// </summary>
		public string billboardTexturePath = "Assets/billboardTexture.png";
		/// <summary>
		/// The billboard normal texture path.
		/// </summary>
		public string billboardNormalTexturePath = "Assets/billboardNormalsTexture.png";
		/// <summary>
		/// The billboard texture.
		/// </summary>
		Texture2D billboardTexture = null;
		/// <summary>
		/// The billboard normal texture.
		/// </summary>
		Texture2D billboardNormalTexture = null;
		#endregion

		#region Singleton
		/// <summary>
		/// This class singleton.
		/// </summary>
		static BillboardBuilder _billboardBuilder = null;
		/// <summary>
		/// Gets a builder instance.
		/// </summary>
		/// <returns>Singleton instance.</returns>
		public static BillboardBuilder GetInstance () {
			if (_billboardBuilder == null) {
				_billboardBuilder = new BillboardBuilder ();
			}
			return _billboardBuilder;
		}
		#endregion

		#region Billboard Creation
		/// <summary>
		/// Generates the billboard asset.
		/// </summary>
		/// <param name="target">Target.</param>
		/// <param name="isST8">True if the mesh generated uses a cross plane mesh.</param>
		public GameObject GenerateBillboardAsset (GameObject target, bool isST8) {
			Clear ();
			billboardGameObject = new GameObject ();
			#if UNITY_EDITOR
			int originalLayer = target.layer;
			SetLayerRecursively (target.transform);
			CalculateGrid (targetAspect);
			SetupTarget (target);
			SetupBillboardCamera (target);

			// Render without SRP, save the render pipeline to a temp var, then assign it back after rendering
			RenderPipelineAsset renderPipeline = GraphicsSettings.renderPipelineAsset;
			GraphicsSettings.renderPipelineAsset = null;

			// Render
			/*
			if (CreateBillboardTexture (billboardTexturePath, billboardTexture, GetUnlitShader ())) {
				CreateBillboardTexture (billboardNormalTexturePath, billboardNormalTexture, GetNormalShader (), true);
				if (!isST8) {
					SetupBillboardObjectST7 (billboardGameObject);
					SetupBillboardAssetST7 ();
				} else {
					SetupBillboardObjectST8 (billboardGameObject);
				}
			}
			*/
			MeshRenderer renderer= target.GetComponent<MeshRenderer> ();
			Material[] unlitMaterials = MaterialManager.GetUnlitMaterials (renderer.sharedMaterials);
			Material[] normalMaterials = GetNormalMaterials (renderer.sharedMaterials);
			MeshRenderer meshRenderer = target.GetComponent<MeshRenderer> ();

			if (CreateBillboardTexture (billboardTexturePath, billboardTexture, unlitMaterials, meshRenderer, Color.clear)) {
				CreateBillboardTexture (billboardNormalTexturePath, billboardNormalTexture, normalMaterials, meshRenderer, new Color (0.5f, 0.5f, 1f, 1f));
				if (!isST8) {
					SetupBillboardObjectST7 (billboardGameObject);
					SetupBillboardAssetST7 ();
				} else {
					SetupBillboardObjectST8 (billboardGameObject);
				}
			}

			// Assign back the render pipeline. TODO: assign back using a try-catch block
			GraphicsSettings.renderPipelineAsset = renderPipeline;

			SetLayerRecursively (target.transform, originalLayer);
			return billboardGameObject;
			#else
			return null;
			#endif
		}
		/// <summary>
		/// Clear this instance.
		/// </summary>
		public void Clear () {
			// Remove camera.
			if (billboardCamera != null)
				Object.DestroyImmediate (billboardCamera);
			if (billboardCameraGameObject != null)
				Object.DestroyImmediate (billboardCameraGameObject);
			// Remove billboard objects.
			if (billboardRenderer != null)
				Object.DestroyImmediate (billboardRenderer);
			if (billboardMesh != null)
				Object.DestroyImmediate (billboardMesh);
			if (billboardAsset != null)
				Object.DestroyImmediate (billboardAsset);
			if (billboardMaterial != null)
				Object.DestroyImmediate (billboardMaterial);
			// Remove billboard game object.
			if (billboardGameObject != null)
				Object.DestroyImmediate (billboardGameObject);
			//Object.DestroyImmediate (billboardTexture);
			//Object.DestroyImmediate (billboardNormalTexture);
		}
		/// <summary>
		/// Gets the image coordinates, ready after generating a billboard.
		/// </summary>
		/// <returns>The image coords.</returns>
		public List<Vector4> GetImageCoords () {
			return imageCoords;
		}
		/// <summary>
		/// Generates the billboard.
		/// </summary>
		/// <returns><c>true</c>, if billboard texture was created, <c>false</c> otherwise.</returns>
		/// <param name="texturePath">Texture path.</param>
		/// <param name="targetTexture">Target texture.</param>
		/// <param name="shader">Shader.</param>
		private bool CreateBillboardTexture (string texturePath, Texture2D targetTexture, Material[] materials, MeshRenderer meshRenderer, Color bgColor) {
			#if UNITY_EDITOR
			if (string.IsNullOrEmpty (texturePath)) {
				texturePath = billboardTexturePath;
			}
			if (targetTexture == null) {
				targetTexture = billboardTexture;
			}

			imageCoords.Clear ();

			Vector2 frameResolution = new Vector2 (textureSize.x / (float)columns, textureSize.y / (float)rows);
			RenderTexture renderTexture = null;
			Texture2D mainTexture;
			Color[] pixels;

			// Create the billboard atlas texture.
			mainTexture = new Texture2D ((int)textureSize.x, (int)textureSize.y, TextureFormat.ARGB32, false);
			// Apply transparent pixels as background.
			pixels = Enumerable.Repeat (bgColor, mainTexture.height * mainTexture.width).ToArray();
			mainTexture.SetPixels (pixels);
			mainTexture.Apply();

			float angleStep = 360f / (float)imageCount;
			int _col = 0;
			int _row = 0;
			// Get the UV per frame (0-1).
			float widthUV = (frameResolution.x - paddingPixels) / textureSize.x;
			float heightUV = (frameResolution.y - paddingPixels) / textureSize.y;
			float leftUV, topUV;

			// Save original materials.
			Material[] sharedMaterials = meshRenderer.sharedMaterials;
			meshRenderer.sharedMaterials = materials;

			// Render image on every angle.
			billboardCamera.transform.RotateAround (targetCenter, Vector3.up, -135);
			for (int i = 0; i < imageCount; i++) {
				// Set the camera around the tree.
				if (i > 0) {
					billboardCamera.transform.RotateAround (targetCenter, Vector3.up, -angleStep);
				}
				RenderTexture.active = null;
				billboardCamera.targetTexture = null;
				billboardCamera.clearFlags = CameraClearFlags.SolidColor;
				billboardCamera.backgroundColor = bgColor;
				// Create the texture to render the tree frame.
				renderTexture = RenderTexture.GetTemporary ((int)frameResolution.x - paddingPixels, (int)frameResolution.y - paddingPixels, 24, RenderTextureFormat.ARGB32);
				RenderTexture.active = renderTexture;
				billboardCamera.targetTexture = renderTexture;

				// Render texture.
				billboardCamera.Render ();

				mainTexture.ReadPixels(new Rect(0, 0, frameResolution.x - paddingPixels, frameResolution.y - paddingPixels), 
					(int)(_col * frameResolution.x), 
					(int)(_row * frameResolution.y));
				mainTexture.Apply();

				leftUV = (_col * frameResolution.x) / textureSize.x;
				topUV = (_row * frameResolution.y) / textureSize.y;
				imageCoords.Add (new Vector4 (leftUV, topUV, widthUV, heightUV));

				_col++;
				if (_col >= columns) {
					_col = 0;
					_row++;
				}

				RenderTexture.ReleaseTemporary (renderTexture);
			}
			billboardCamera.transform.RotateAround (targetCenter, Vector3.up, -angleStep);
			RenderTexture.active = null;

			// Return original materials.
			meshRenderer.sharedMaterials = sharedMaterials;

			var bytes = mainTexture.EncodeToPNG();

			File.WriteAllBytes (texturePath, bytes);
			targetTexture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D> (texturePath);
			AssetDatabase.ImportAsset (texturePath);

			Object.DestroyImmediate (mainTexture);
			#endif
			return true;
		}
		/// <summary>
		/// Generates the billboard.
		/// </summary>
		/// <returns><c>true</c>, if billboard texture was created, <c>false</c> otherwise.</returns>
		/// <param name="texturePath">Texture path.</param>
		/// <param name="targetTexture">Target texture.</param>
		/// <param name="shader">Shader.</param>
		private bool CreateBillboardTexture (string texturePath, Texture2D targetTexture, Shader shader = null, bool isNormal = false) {
			#if UNITY_EDITOR
			if (string.IsNullOrEmpty (texturePath)) {
				texturePath = billboardTexturePath;
			}
			if (targetTexture == null) {
				targetTexture = billboardTexture;
			}

			imageCoords.Clear ();

			Vector2 frameResolution = new Vector2 (textureSize.x / (float)columns, textureSize.y / (float)rows);
			RenderTexture renderTexture = null;
			Texture2D mainTexture;
			Color[] pixels;
			if (isNormal) {
				mainTexture = new Texture2D ((int)textureSize.x, (int)textureSize.y, TextureFormat.RGB24, false);
				pixels = Enumerable.Repeat(new Color (0.5f, 0.5f, 1f), mainTexture.height * mainTexture.width).ToArray();
			} else {
				mainTexture = new Texture2D ((int)textureSize.x, (int)textureSize.y, TextureFormat.ARGB32, false);
				pixels = Enumerable.Repeat(Color.clear, mainTexture.height * mainTexture.width).ToArray();
			}
			mainTexture.SetPixels (pixels);
			mainTexture.Apply();

			float angleStep = 360f / (float)imageCount;
			int _col = 0;
			int _row = 0;
			float widthUV = frameResolution.x / textureSize.x;
			float heightUV = frameResolution.y / textureSize.y;
			float leftUV, topUV;

			// Render image on every angle.
			for (int i = 0; i < imageCount; i++) {
				if (i > 0) {
					billboardCamera.transform.RotateAround (targetCenter, Vector3.up, -angleStep);
				}
				RenderTexture.active = null;
				billboardCamera.targetTexture = null;
				billboardCamera.clearFlags = CameraClearFlags.SolidColor;
				if (isNormal) {
					billboardCamera.backgroundColor = new Color (0.5f, 0.5f, 1f);
				} else {
					billboardCamera.backgroundColor = Color.clear;
				}
				renderTexture = RenderTexture.GetTemporary ((int)frameResolution.x, (int)frameResolution.y, 16, RenderTextureFormat.ARGB32);
				RenderTexture.active = renderTexture;
				billboardCamera.targetTexture = renderTexture;

				if (shader != null) {
					// https://forum.unity.com/threads/set-shader-property-by-renderwithshader-oder-replacement.29747/
					billboardCamera.RenderWithShader (shader, "");
				} else {
					billboardCamera.Render ();
				}

				mainTexture.ReadPixels(new Rect(0, 0, frameResolution.x, frameResolution.y), 
					(int)(_col * frameResolution.x), 
					(int)(_row * frameResolution.y));
				mainTexture.Apply();

				leftUV = (_col * frameResolution.x) / textureSize.x;
				topUV = (_row * frameResolution.y) / textureSize.y;
				imageCoords.Add (new Vector4 (leftUV, topUV, widthUV, heightUV));

				_col++;
				if (_col >= columns) {
					_col = 0;
					_row++;
				}

				RenderTexture.ReleaseTemporary (renderTexture);
			}
			billboardCamera.transform.RotateAround (targetCenter, Vector3.up, -angleStep);
			RenderTexture.active = null;

			var bytes = mainTexture.EncodeToPNG();

			File.WriteAllBytes (texturePath, bytes);
			targetTexture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D> (texturePath);
			AssetDatabase.ImportAsset (texturePath);

			Object.DestroyImmediate (mainTexture);
			#endif
			return true;
		}
		Material[] GetNormalMaterials (Material[] originalMaterials) {
			Shader normalShader = Shader.Find ("Hidden/Broccoli/Billboard Normals");
			Material[] normalMaterials = new Material[originalMaterials.Length];
			for (int i = 0; i < originalMaterials.Length; i++) {
				normalMaterials [i] = new Material (normalShader);
				if (originalMaterials[i].HasProperty ("_MainTex")) normalMaterials [i].SetTexture ("_MainTex", originalMaterials[i].GetTexture ("_MainTex"));
				if (originalMaterials[i].HasProperty ("_BumpTex")) normalMaterials [i].SetTexture ("_BumpTex", originalMaterials[i].GetTexture ("_BumpTex"));
			}
			return normalMaterials;
		}
		#endregion

		#region Setup Billboard Objects
		/// <summary>
		/// Setups the billboard objects.
		/// </summary>
		/// <param name="container">Container object.</param>
		private void SetupBillboardObjectST7 (GameObject container) {
			#if UNITY_EDITOR
			billboardRenderer = container.GetComponent<BillboardRenderer> ();
			if (billboardRenderer == null) {
				billboardRenderer = container.AddComponent<BillboardRenderer> ();
				billboardRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
			}
			billboardAsset = new BillboardAsset ();
			billboardAsset.name = "billboard";
			billboardMaterial = new Material (MaterialManager.billboardShader);
			billboardMaterial.name = "billboard";
			AssetDatabase.ImportAsset (billboardTexturePath);
			AssetDatabase.ImportAsset (billboardNormalTexturePath);
			billboardTexture = AssetDatabase.LoadAssetAtPath<Texture2D> (billboardTexturePath);
			if (billboardTexture != null) {
				TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath (billboardTexturePath);
				importer.alphaIsTransparency = true;
				importer.SaveAndReimport ();
			}
			billboardNormalTexture = AssetDatabase.LoadAssetAtPath<Texture2D> (billboardNormalTexturePath);
			if (billboardNormalTexture != null) {
				TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath (billboardNormalTexturePath);
				importer.textureType = TextureImporterType.NormalMap;
				importer.SaveAndReimport ();
			}
			billboardMaterial.SetTexture ("_MainTex", billboardTexture);
			billboardMaterial.SetTexture ("_BumpMap", billboardNormalTexture);
			billboardMaterial.SetColor ("_Color", new Color (0.9f, 0.9f, 0.9f));
			billboardMaterial.SetColor ("_HueVariation", new Color (1f, 1f, 1f, 0f));
			billboardMaterial.SetFloat ("_Cutoff", 0.5f);
			billboardMaterial.EnableKeyword ("EFFECT_BUMP");
			billboardMaterial.EnableKeyword ("EFFECT_HUE_VARIATION");

			// Set other material values according to the billboard shader version (ST7 or ST8)
			if (MaterialManager.billboardShaderType == MaterialManager.LeavesShaderType.SpeedTree7OrSimilar) {
				billboardMaterial.SetFloat ("_WindQuality", 1f);
			} else {
				//Debug.Log ("Going to use billboard shader 8.");
			}
			#endif
		}
		/// <summary>
		/// Setups the billboard objects.
		/// </summary>
		/// <param name="container">Container object.</param>
		private void SetupBillboardObjectST8 (GameObject container) {
			#if UNITY_EDITOR
			//container.transform.rotation = Quaternion.Euler (0f, 135, 0);
			MeshFilter billboardMeshFilter = container.AddComponent<MeshFilter> ();
			MeshRenderer billboardMeshRenderer = container.AddComponent<MeshRenderer> ();
			billboardMesh = CreateBillboardMesh ();
			billboardMeshFilter.sharedMesh = billboardMesh;
			billboardMaterial = new Material (MaterialManager.billboardShader);
			billboardMaterial.name = "billboard";
			AssetDatabase.ImportAsset (billboardTexturePath);
			AssetDatabase.ImportAsset (billboardNormalTexturePath);
			billboardTexture = AssetDatabase.LoadAssetAtPath<Texture2D> (billboardTexturePath);
			if (billboardTexture != null) {
				TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath (billboardTexturePath);
				importer.alphaIsTransparency = true;
				importer.SaveAndReimport ();
			}
			billboardNormalTexture = AssetDatabase.LoadAssetAtPath<Texture2D> (billboardNormalTexturePath);
			if (billboardNormalTexture != null) {
				TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath (billboardNormalTexturePath);
				importer.textureType = TextureImporterType.NormalMap;
				importer.SaveAndReimport ();
			}
			billboardMaterial.SetTexture ("_MainTex", billboardTexture);
			billboardMaterial.SetTexture ("_BumpMap", billboardNormalTexture);
			billboardMaterial.SetColor ("_Color", new Color (1f, 1f, 1f, 0.55f));
			billboardMaterial.SetFloat ("_Glossiness", 0f);
			billboardMaterial.DisableKeyword ("EFFECT_EXTRA_TEX");
			billboardMaterial.SetFloat ("EFFECT_EXTRA_TEX", 0f);
			billboardMaterial.SetFloat ("_NormalMapKwToggle", 1f);
			billboardMaterial.SetFloat ("EFFECT_BILLBOARD", 1f);
			billboardMaterial.SetFloat ("_BillboardKwToggle", 1f);
			billboardMaterial.EnableKeyword ("EFFECT_BUMP");
			billboardMaterial.EnableKeyword ("EFFECT_HUE_VARIATION");
			billboardMaterial.EnableKeyword ("EFFECT_BILLBOARD");
			billboardMaterial.DisableKeyword ("_DOUBLESIDED_ON");

			// Set other material values according to the billboard shader version (ST7 or ST8)
			if (MaterialManager.billboardShaderType == MaterialManager.LeavesShaderType.SpeedTree8OrSimilar) {
				billboardMaterial.SetFloat ("_WindQuality", 1f);
			} else {
				//Debug.Log ("Going to use billboard shader 8.");
			}
			#endif
		}
		Vector3[] GetPlaneVertices () {
			Vector3[] points = new Vector3[4];
			float wA = -targetWidth / 2f;
			float wB = targetWidth / 2f;
			float hA = 0f;
			float hB = targetHeight;

			points[0] = new Vector3 (wA, hB); // 0
			points[1] = new Vector3 (wA, hA); // 1
			points[2] = new Vector3 (wB, hA); // 2
			points[3] = new Vector3 (wB, hB); // 3

			return points;
		}
		Vector3[] GetPlaneNormals () {
			Vector3[] normals = new Vector3[4];
			/*
			normals[0] = Vector3.forward; // 0
			normals[1] = Vector3.forward; // 1
			normals[2] = Vector3.forward; // 2
			normals[3] = Vector3.forward; // 3
			*/
			normals[0] = Vector3.back; // 0
			normals[1] = Vector3.back; // 1
			normals[2] = Vector3.back; // 2
			normals[3] = Vector3.back; // 3
			
			return normals;
		}
		/// <summary>
		/// Rotates an array of points.
		/// </summary>
		/// <returns>The points rotated.</returns>
		/// <param name="pointsToRotate">Points to rotate.</param>
		/// <param name="angle">Rotation angle.</param>
		Vector3[] RotatePoints (Vector3[] pointsToRotate, float angle) {
			for (int i = pointsToRotate.Length - 1; i >= 0; i--) {
				pointsToRotate [i] = Quaternion.AngleAxis (angle, Vector3.up) * pointsToRotate [i];
			}
			return pointsToRotate;
		}
		public Mesh CreateBillboardMesh () {
			List<Vector3> vertices = new List<Vector3> ();
			List<Vector3> normals = new List<Vector3> ();
			List<int> triangles = new List<int> ();
			List<Vector2> uvs = new List<Vector2> ();
			Vector4 imageCoord;
			Vector3[] planePoints = GetPlaneVertices ();
			Vector3[] planeNormals = GetPlaneNormals ();
			float angleStep = 360f / (float)imageCount;
			Mesh planesMesh = new Mesh ();
			// Iterate through the planes.
			for (int i = 0; i < imageCount; i ++) {
				Vector3[] _vertices = (Vector3[])planePoints.Clone();
				Vector3[] _normals = (Vector3[])planeNormals.Clone();
				// Rotate if it has an angle.
				if (i > 0) {
					vertices.AddRange (RotatePoints(_vertices, angleStep * i));
					normals.AddRange (RotatePoints(_normals, angleStep * i));
				} else {
					vertices.AddRange (_vertices);
					normals.AddRange (_normals);
				}
				triangles.Add ((i * 4) + 2); //2
				triangles.Add ((i * 4) + 1); //1
				triangles.Add (i * 4); //0
				triangles.Add ((i * 4) + 3); //3
				triangles.Add ((i * 4) + 2); //2
				triangles.Add (i * 4); //0				
			}
			//for (int i = 0; i < imageCount; i ++) {
			for (int i = imageCount - 1; i >= 0; i --) {
				imageCoord = imageCoords [i];
				uvs.Add (new Vector2 (imageCoord.x + imageCoord.z, imageCoord.y + imageCoord.w));
				uvs.Add (new Vector2 (imageCoord.x + imageCoord.z, imageCoord.y));
				uvs.Add (new Vector2 (imageCoord.x, imageCoord.y));
				uvs.Add (new Vector2 (imageCoord.x, imageCoord.y + imageCoord.w));
			}
			planesMesh.vertices = vertices.ToArray ();
			planesMesh.normals = normals.ToArray ();
			planesMesh.triangles = triangles.ToArray ();
			planesMesh.SetUVs (0, uvs);
			planesMesh.RecalculateTangents ();
			return planesMesh;
		}
		/// <summary>
		/// Gets the billboard asset.
		/// </summary>
		/// <returns>The billboard asset.</returns>
		/// <param name="clone">If set to <c>true</c> clone.</param>
		public BillboardAsset GetBillboardAsset (bool clone = false) {
			if (clone) {
				return Object.Instantiate<BillboardAsset> (billboardAsset);
			}
			return billboardAsset;
		}
		/// <summary>
		/// Gets the billboard material.
		/// </summary>
		/// <returns>The billboard material.</returns>
		/// <param name="clone">If set to <c>true</c> clone.</param>
		public Material GetBillboardMaterial (bool clone = false) {
			if (clone) {
				return Object.Instantiate <Material> (billboardMaterial);
			}
			return billboardMaterial;
		}
		/// <summary>
		/// Gets the billboard mesh.
		/// </summary>
		/// <returns>The billboard mesh.</returns>
		/// <param name="clone">If set to <c>true</c> clone.</param>
		public Mesh GetBillboardMesh (bool clone = false) {
			if (clone) {
				return Object.Instantiate <Mesh> (billboardMesh);
			}
			return billboardMesh;
		}
		/// <summary>
		/// Gets the billboard texture.
		/// </summary>
		/// <returns>The billboard texture.</returns>
		public Texture2D GetBillboardTexture () {
			return billboardTexture;
		}
		/// <summary>
		/// Gets the billboard normal texture.
		/// </summary>
		/// <returns>The billboard normal texture.</returns>
		public Texture2D GetBillboardNormalTexture () {
			return billboardNormalTexture;
		}
		/// <summary>
		/// Setups the billboard asset.
		/// </summary>
		private void SetupBillboardAssetST7 () {
			billboardAsset.SetImageTexCoords (imageCoords);
			billboardAsset.material = billboardMaterial;
			billboardAsset.bottom = 0f;
			billboardAsset.height = targetHeight;
			billboardAsset.width = targetWidth;
			billboardAsset.SetVertices (GetVertices ());
			billboardAsset.SetIndices (GetIndices ());
			billboardRenderer.billboard = billboardAsset;
		}
		/// <summary>
		/// Gets the billboard mesh vertices.
		/// </summary>
		/// <returns>The vertices.</returns>
		private List<Vector2> GetVertices () {
			List<Vector2> vertices = new List<Vector2> ();
			vertices.Add (new Vector2 (0, 0));
			vertices.Add (new Vector2 (1, 0));
			vertices.Add (new Vector2 (0, 1));
			vertices.Add (new Vector2 (1, 1));
			return vertices;
		}
		/// <summary>
		/// Gets the billboard mesh indices.
		/// </summary>
		/// <returns>The indices.</returns>
		private List<ushort> GetIndices () {
			List<ushort> indices = new List<ushort> ();
			indices.Add (0);
			indices.Add (2);
			indices.Add (1);
			indices.Add (2);
			indices.Add (3);
			indices.Add (1);
			return indices;
		}
		#endregion

		#region Setup Target and Camera
		/// <summary>
		/// Setups the target.
		/// </summary>
		/// <returns>The target.</returns>
		/// <param name="target">Target.</param>
		private void SetupTarget (GameObject target) {
			targetBounds = target.GetComponent<Renderer>().bounds;

			targetCenter = target.transform.position;
			targetCenter.y = targetBounds.center.y;

			Vector3 maxGround = targetBounds.max;
			maxGround.y = targetBounds.min.y;
			targetWidth = Vector3.Distance (target.transform.position, maxGround);
			targetWidth *= 2f;

			targetHeight = targetBounds.size.y;

			targetAspect = targetWidth / targetHeight;

			//Calculate target offset from mesh domain.
			meshTargetYOffset = 0f;
			MeshFilter meshFilter = target.GetComponent<MeshFilter>();
			if (meshFilter != null) {
				meshTargetYOffset = meshFilter.sharedMesh.bounds.min.y;
			}
		}
		/// <summary>
		/// Calculates the number of rows and columns on the grid using the
		/// aspect radio of the target and the size of the texture.
		/// </summary>
		/// <param name="targetAspect">Target aspect ratio (width/height).</param>
		private void CalculateGrid (float targetAspect) {
			float textureAspect = textureSize.x / textureSize.y;
			float k = textureAspect * targetAspect;
			float rowsSqrt = Mathf.Sqrt (imageCount * k);
			float colsSqrt = Mathf.Sqrt (imageCount / k);
			rows = Mathf.RoundToInt (rowsSqrt);
			columns = Mathf.RoundToInt (colsSqrt);
			if (rows < 1)
				rows = 1;
			if (columns < 1)
				columns = 1;
			if (rows * columns < imageCount) {
				rows++;
			}
		}
		/// <summary>
		/// Setups the billboard camera.
		/// </summary>
		/// <param name="target">Target.</param>
		private void SetupBillboardCamera (GameObject target) {
			if (billboardCamera != null) {
				Object.DestroyImmediate (billboardCamera);
			}
			if (billboardCameraGameObject != null) {
				Object.DestroyImmediate (billboardCameraGameObject);
				billboardCameraGameObject = null;
			}

			billboardCameraGameObject = new GameObject ("BillboardCameraContainer");
			billboardCamera = billboardCameraGameObject.AddComponent<Camera> ();
			//billboardCamera.CopyFrom (Camera.main);
			// Set camera properties
			billboardCamera.cameraType = CameraType.Preview;
			billboardCamera.clearFlags = CameraClearFlags.Color;
			//billboardCamera.clearFlags = CameraClearFlags.Depth;
			billboardCamera.backgroundColor = new Color (0.5f, 0.5f, 0.5f, 0f);
			
			billboardCamera.cullingMask = 1 << PREVIEW_LAYER;
			billboardCamera.orthographic = true;
			billboardCamera.enabled = false;
			billboardCamera.orthographicSize = targetHeight / 2;
			billboardCamera.aspect = targetAspect;

			// Positioning the camera
			float num = targetBounds.extents.magnitude;
			billboardCamera.nearClipPlane = num * 0.1f;
			billboardCamera.farClipPlane = num * 2.2f;
			Vector3 angleVector = new Vector3 (1f, 0f, 0f);
			billboardCameraGameObject.transform.position = targetCenter + angleVector.normalized * num;
			billboardCameraGameObject.transform.LookAt (targetCenter);
		}
		/// <summary>
		/// Sets the layer of an object recursively.
		/// </summary>
		/// <param name="obj">Object.</param>
		private static void SetLayerRecursively (Transform obj, int layer = -1) {
			if (layer < 0)
				layer = PREVIEW_LAYER;
			obj.gameObject.layer = layer;
			for( int i = 0; i < obj.childCount; i++ )
				SetLayerRecursively( obj.GetChild( i ) );
		}
		#endregion
	}
}