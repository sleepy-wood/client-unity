using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using Broccoli.Base;

namespace Broccoli.Utils
{
	/// <summary>
	/// GUI texture manager.
	/// </summary>
	public static class GUITextureManager
	{
		#region Vars
		/// <summary>
		/// Element for the GUI element texture.
		/// </summary>
		public enum GUIElement
		{
			Logo,
			LogoBox,
			NodeBgStructure,
			NodeBgMesh,
			NodeBgFunction,
			NodeBgMap,
			NodeBgBranch,
			NodeBgRoot,
			NodeBgSprout,
			NodeBgBarkSprout,
			IconShuffleOn,
			IconShuffleOff
		}
		/// <summary>
		/// Textures paths.
		/// </summary>
		private static Dictionary<GUIElement, string> texturePath = new Dictionary<GUIElement, string> () {
			{GUIElement.Logo, "Broccoli/GUI/broccoli_logo.png"},
			{GUIElement.LogoBox, "Broccoli/GUI/broccoli_logo_simple.png"},
			{GUIElement.NodeBgStructure, "Broccoli/GUI/broccoli_GUI_a.png"},
			{GUIElement.NodeBgMesh, "Broccoli/GUI/broccoli_GUI_a.png"},
			{GUIElement.NodeBgFunction, "Broccoli/GUI/broccoli_GUI_a.png"},
			{GUIElement.NodeBgMap, "Broccoli/GUI/broccoli_GUI_a.png"},
			{GUIElement.NodeBgBranch, "Broccoli/GUI/broccoli_GUI_a.png"},
			{GUIElement.NodeBgRoot, "Broccoli/GUI/broccoli_GUI_a.png"},
			{GUIElement.NodeBgSprout, "Broccoli/GUI/broccoli_GUI_a.png"},
			{GUIElement.NodeBgBarkSprout, "Broccoli/GUI/broccoli_GUI_a.png"},
			{GUIElement.IconShuffleOff, "Broccoli/GUI/broccoli_icons.png"},
			{GUIElement.IconShuffleOn, "Broccoli/GUI/broccoli_icons.png"},
		};
		/// <summary>
		/// Textures crop values.
		/// </summary>
		private static Dictionary<GUIElement, int[]> textureCrop = new Dictionary<GUIElement, int[]> () {
			{GUIElement.Logo, new int[]{0, 0, 246, 109}},
			{GUIElement.LogoBox, new int[]{0, 0, 64,64}},
			{GUIElement.NodeBgStructure, new int[]{0, 0, 64, 64}},
			{GUIElement.NodeBgMesh, new int[]{64, 0, 64, 64}},
			{GUIElement.NodeBgFunction, new int[]{128, 0, 64, 64}},
			{GUIElement.NodeBgMap, new int[]{192, 0, 64, 64}},
			{GUIElement.NodeBgBranch, new int[]{0, 64, 64, 64}},
			{GUIElement.NodeBgRoot, new int[]{192, 64, 64, 64}},
			{GUIElement.NodeBgSprout, new int[]{64, 64, 64, 64}},
			{GUIElement.NodeBgBarkSprout, new int[]{128, 64, 64, 64}},
			{GUIElement.IconShuffleOff, new int[]{0, 0, 32, 32}},
			{GUIElement.IconShuffleOn, new int[]{32, 0, 32, 32}},
		};
		/// <summary>
		/// The loaded textures dictionary.
		/// </summary>
		private static Dictionary<GUIElement, Texture2D> loadedTextures = new Dictionary<GUIElement, Texture2D> ();
		/// <summary>
		/// The requested textures to be loaded, relative to the editor resources path.
		/// </summary>
		private static List<Texture2D> loadedCustomTextures = new List<Texture2D> ();
		/// <summary>
		/// Flag to mark this manager as initialized.
		/// </summary>
		private static bool isInit = false;
		/// <summary>
		/// The new preview button texture.
		/// </summary>
		public static Texture2D newPreviewBtnTexture;
		/// <summary>
		/// The create prefab button texture.
		/// </summary>
		public static Texture2D createPrefabBtnTexture;
		/// <summary>
		/// The info icon texture.
		/// </summary>
		public static Texture2D infoTexture;
		/// <summary>
		/// The warn icon texture.
		/// </summary>
		public static Texture2D warnTexture;
		/// <summary>
		/// The error icon texture.
		/// </summary>
		public static Texture2D errorTexture;
		/// <summary>
		/// The visibility-on texture.
		/// </summary>
		public static Texture2D visibilityOnTexture;
		/// <summary>
		/// The visibility-off texture.
		/// </summary>
		public static Texture2D visibilityOffTexture;
		/// <summary>
		/// The inspect mesh off texture.
		/// </summary>
		public static Texture2D inspectMeshOffTexture;
		/// <summary>
		/// The inspect mesh on texture.
		/// </summary>
		public static Texture2D inspectMeshOnTexture;
		#endregion

		#region Init
		/// <summary>
		/// Initializes the <see cref="Broccoli.Utils.GUITextureManager"/> class.
		/// </summary>
		static GUITextureManager () {
			#if UNITY_EDITOR
			newPreviewBtnTexture = EditorGUIUtility.FindTexture ("RotateTool On");
			createPrefabBtnTexture = EditorGUIUtility.FindTexture ("xd_Prefab Icon");
			infoTexture = EditorGUIUtility.FindTexture ("console.infoicon");
			warnTexture = EditorGUIUtility.FindTexture ("console.warnicon");
			errorTexture = EditorGUIUtility.FindTexture ("console.erroricon");
			visibilityOnTexture = EditorGUIUtility.FindTexture ("animationvisibilitytoggleon");
			visibilityOffTexture = EditorGUIUtility.FindTexture ("animationvisibilitytoggleoff");
			inspectMeshOnTexture = EditorGUIUtility.FindTexture ("animationvisibilitytoggleoff");
			inspectMeshOffTexture = EditorGUIUtility.FindTexture ("animationvisibilitytoggleon");
			#endif 
		}
		/// <summary>
		/// Inits the manager.
		/// </summary>
		/// <param name="force">If set to <c>true</c> forces the initialization despite the isInit flag.</param>
		public static void Init (bool force = false) {
			if (!isInit || force || GetLogo () == null) {
				LoadTexture ("GUI/broccoli_logo.png", GUIElement.Logo, 0, 0, 246, 109);
				LoadTexture ("GUI/broccoli_logo_simple.png", GUIElement.LogoBox, 0, 0, 64,64);
				LoadTexture ("GUI/broccoli_GUI_a.png", GUIElement.NodeBgStructure, 0, 0, 64, 64);
				LoadTexture ("GUI/broccoli_GUI_a.png", GUIElement.NodeBgMesh, 64, 0, 64, 64);
				LoadTexture ("GUI/broccoli_GUI_a.png", GUIElement.NodeBgFunction, 128, 0, 64, 64);
				LoadTexture ("GUI/broccoli_GUI_a.png", GUIElement.NodeBgMap, 192, 0, 64, 64);
				LoadTexture ("GUI/broccoli_GUI_a.png", GUIElement.NodeBgBranch, 0, 64, 64, 64);
				LoadTexture ("GUI/broccoli_GUI_a.png", GUIElement.NodeBgRoot, 192, 64, 64, 64);
				LoadTexture ("GUI/broccoli_GUI_a.png", GUIElement.NodeBgSprout, 64, 64, 64, 64);
				LoadTexture ("GUI/broccoli_GUI_a.png", GUIElement.NodeBgBarkSprout, 128, 64, 64, 64);
				LoadTexture ("GUI/broccoli_icons.png", GUIElement.IconShuffleOff, 0, 0, 32, 32);
				LoadTexture ("GUI/broccoli_icons.png", GUIElement.IconShuffleOn, 32, 0, 32, 32);
				isInit = true;
			}
		}
		/// <summary>
		/// Clear this instance.
		/// </summary>
		public static void Clear () {
			var loadedTexturesEnumerator = loadedTextures.GetEnumerator ();
			while (loadedTexturesEnumerator.MoveNext ()) {
				UnityEngine.Object.DestroyImmediate (loadedTexturesEnumerator.Current.Value);
			}
			loadedTextures.Clear ();
			var loadedCustomTexturesEnumerator = loadedCustomTextures.GetEnumerator ();
			while (loadedCustomTexturesEnumerator.MoveNext ()) {
				UnityEngine.Object.DestroyImmediate (loadedCustomTexturesEnumerator.Current);
			}
			loadedCustomTextures.Clear ();
			#if UNITY_EDITOR
			EditorUtility.UnloadUnusedAssetsImmediate ();
			#endif
			isInit = false;
		}
		/// <summary>
		/// Loads a custom texture given the path relative to the editor resource path.
		/// </summary>
		/// <param name="path">Path relative to the editor resources folder.</param>
		/// <returns>The index of the texture, if none loaded -1.</returns>
		public static int LoadCustomTexture (string path) {
			int index = -1;
			#if UNITY_EDITOR
			Texture2D texture = null;
			path = ExtensionManager.resourcesPath + path;
			texture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D> (path);
			if (loadedCustomTextures.Contains (texture)) {
				index = loadedCustomTextures.IndexOf (texture);
			} else {
				loadedCustomTextures.Add (texture);
				index = loadedCustomTextures.Count - 1;
			}
			#endif
			return index;
		}
		/// <summary>
		/// Gets a previously loaded custom texture.
		/// </summary>
		/// <param name="index">Id of the texture.</param>
		/// <returns>Loaded custom texture.</returns>
		public static Texture2D GetCustomTexture (int index) {
			if (index >= 0 && index < loadedCustomTextures.Count) {
				return loadedCustomTextures [index];
			}
			return null;
		}
		/// <summary>
		/// Loads a texture given its GUIElement enum value.
		/// </summary>
		/// <returns>The texture.</returns>
		/// <param name="path">Path.</param>
		/// <param name="guiElement">GUI element.</param>
		private static Texture2D LoadTexture (string path, GUIElement guiElement) {
			Texture2D texture = null;
			#if UNITY_EDITOR
			path = ExtensionManager.resourcesPath + path;
			texture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D> (path);
			if (loadedTextures.ContainsKey (guiElement)) {
				if (loadedTextures [guiElement] != null) {
					Texture2D.DestroyImmediate (loadedTextures [guiElement], true);
				}
				loadedTextures.Remove (guiElement);
			}
			loadedTextures.Add (guiElement, texture);
			#endif
			return texture;
		}
		/// <summary>
		/// Loads and crop a texture.
		/// </summary>
		/// <returns>The texture.</returns>
		/// <param name="path">Path.</param>
		/// <param name="guiElement">GUI element.</param>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <param name="width">Width crop.</param>
		/// <param name="height">Height crop.</param>
		private static Texture2D LoadTexture (string path, GUIElement guiElement, int x, int y, int width, int height) {
			Texture2D texture = null;
			#if UNITY_EDITOR
			path = ExtensionManager.resourcesPath + path;
			texture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D> (path);
			texture = TextureUtil.CropTexture (texture, x, y, width, height);
			if (loadedTextures.ContainsKey (guiElement)) {
				if (loadedTextures [guiElement] != null) {
					Texture2D.DestroyImmediate (loadedTextures [guiElement], true);
				}
				loadedTextures.Remove (guiElement);
			}
			loadedTextures.Add (guiElement, texture);
			#endif
			return texture;
		}
		/// <summary>
		/// Gets a texture given its GUIElement enum value.
		/// </summary>
		/// <returns>The texture.</returns>
		/// <param name="textureId">Texture identifier.</param>
		private static Texture2D GetTexture (GUIElement textureId) {
			if (loadedTextures.ContainsKey (textureId)) {
				
				if (loadedTextures [textureId] == null) {
					if (texturePath.ContainsKey (textureId) && textureCrop.ContainsKey (textureId)) {
						LoadTexture (texturePath [textureId], textureId, 
							textureCrop[textureId][0], textureCrop[textureId][1], 
							textureCrop[textureId][2], textureCrop[textureId][3]);
					}
				}
				return loadedTextures [textureId];
			}
			return null;
		}
		#endregion

		#region Accessors
		/// <summary>
		/// Gets the logo texture.
		/// </summary>
		/// <returns>The logo texture.</returns>
		public static Texture2D GetLogo () {
			return GetTexture (GUIElement.Logo);
		}
		/// <summary>
		/// Gets the logo box texture.
		/// </summary>
		/// <returns>The logo box texture.</returns>
		public static Texture2D GetLogoBox () {
			return GetTexture (GUIElement.LogoBox);
		}
		/// <summary>
		/// Gets the node background structure texture.
		/// </summary>
		/// <returns>The node background structure texture.</returns>
		public static Texture2D GetNodeBgStructure () {
			return GetTexture (GUIElement.NodeBgStructure);
		}
		/// <summary>
		/// Gets the node background mesh texture.
		/// </summary>
		/// <returns>The node background mesh texture.</returns>
		public static Texture2D GetNodeBgMesh () {
			return GetTexture (GUIElement.NodeBgMesh);
		}
		/// <summary>
		/// Gets the node background function texture.
		/// </summary>
		/// <returns>The node background function texture.</returns>
		public static Texture2D GetNodeBgFunction () {
			return GetTexture (GUIElement.NodeBgFunction);
		}
		/// <summary>
		/// Gets the node background map texture.
		/// </summary>
		/// <returns>The node background map texture.</returns>
		public static Texture2D GetNodeBgMap () {
			return GetTexture (GUIElement.NodeBgMap);
		}
		/// <summary>
		/// Gets the node background branch texture.
		/// </summary>
		/// <returns>The node background branch texture.</returns>
		public static Texture2D GetNodeBgBranch () {
			return GetTexture (GUIElement.NodeBgBranch);
		}
		/// <summary>
		/// Gets the node background root texture.
		/// </summary>
		/// <returns>The node background root texture.</returns>
		public static Texture2D GetNodeBgRoot () {
			return GetTexture (GUIElement.NodeBgRoot);
		}
		/// <summary>
		/// Gets the node background sprout texture.
		/// </summary>
		/// <returns>The node background sprout texture.</returns>
		public static Texture2D GetNodeBgSprout () {
			return GetTexture (GUIElement.NodeBgSprout);
		}
		/// <summary>
		/// Gets the node background bark sprout texture.
		/// </summary>
		/// <returns>The node background bark sprout texture.</returns>
		public static Texture2D GetNodeBgTrunk () {
			return GetTexture (GUIElement.NodeBgBarkSprout);
		}
		/// <summary>
		/// Gets the shuffle-off icon.
		/// </summary>
		/// <returns>The shuffle-off icon.</returns>
		public static Texture2D GetIconShuffleOff () {
			return GetTexture (GUIElement.IconShuffleOff);
		}
		/// <summary>
		/// Gets the shuffle-on icon.
		/// </summary>
		/// <returns>The shuffle-on icon.</returns>
		public static Texture2D GetIconShuffleOn () {
			return GetTexture (GUIElement.IconShuffleOn);
		}
		#endregion
	}
}