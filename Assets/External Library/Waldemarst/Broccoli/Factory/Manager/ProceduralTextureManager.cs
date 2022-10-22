using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

using Broccoli.Pipe;

namespace Broccoli.Manager
{
	/// <summary>
	/// Texture manager.
	/// Creates and manages procedural textures.
	/// </summary>
	public class ProceduralTextureManager {
        #region Vars
        /// <summary>
		/// Relationship between and id and a texture.
		/// </summary>
		private Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D> ();
        /// <summary>
		/// Relationship between and id and a normal texture.
		/// </summary>
		//private Dictionary<string, Texture2D> normalTextures = new Dictionary<string, Texture2D> ();
        #endregion

        #region Singleton
		/// <summary>
		/// Singleton object for the manager.
		/// </summary>
		static ProceduralTextureManager _instance = null;
		/// <summary>
		/// Gets the singleton instance.
		/// </summary>
		/// <returns>The instance.</returns>
		public static ProceduralTextureManager GetInstance () {
			if (_instance == null) {
				_instance = new ProceduralTextureManager ();
			}
			return _instance;
		}
		#endregion

        #region Procedural
        /// <summary>
		/// Creates a procedural texture and adds it to this manager.
		/// </summary>
		/// <returns>Texture added to the manager or null if no file was found.</returns>
		/// <param name="textureId">Texture identifier.</param>
		/// <param name="textureParams">Params for procedural generation.</param>
		public Texture2D CreateTexture (string textureId, string textureParams) {
			/*
            Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            // set the pixel values
            texture.SetPixel (0, 0, new Color(1.0f, 1.0f, 1.0f, 0.5f));
            texture.SetPixel (1, 0, Color.clear);
            texture.SetPixel (0, 1, Color.white);
            texture.SetPixel (1, 1, Color.black);
            // Apply all SetPixel calls
            texture.Apply();
            // Return the texture.
            return texture;
			*/
			Material mat = new Material(Shader.Find("GUI/Text Shader"));
			return RenderGLToTexture (128, 128, mat);
		}
		static Texture2D RenderGLToTexture (int width, int height, Material material ) {
			// get a temporary RenderTexture //
			RenderTexture renderTexture = RenderTexture.GetTemporary( width, height );
		
			// set the RenderTexture as global target (that means GL too)
			RenderTexture.active = renderTexture;
		
			// clear GL //
			GL.Clear( false, true, Color.blue);
		
			// render GL immediately to the active render texture //
			RenderGLStuff( width, height, material );
	
			// read the active RenderTexture into a new Texture2D //
			Texture2D newTexture = new Texture2D( width, height );
			newTexture.ReadPixels( new Rect( 0, 0, width, height ), 0, 0 );
	
			// apply pixels and compress //
			bool applyMipsmaps = false;
			newTexture.Apply( applyMipsmaps );
			bool highQuality = true;
			newTexture.Compress( highQuality );
		
			// clean up after the party //
			RenderTexture.active = null;
			RenderTexture.ReleaseTemporary( renderTexture );
		
			// return the goods //
			return newTexture;
		}
		static void RenderGLStuff (int width, int height, Material material ) {
			material.SetPass( 0 );
			GL.PushMatrix();
			GL.LoadPixelMatrix( 0, width, height, 0 );
			GL.Begin( GL.LINES );
			GL.Color( new Color( 1, 1, 1, 1f ) );
			for( int i=0; i<500; i++ ) GL.Vertex3( Random.value * width, Random.value * height, 0 );
			GL.End();
			GL.PopMatrix();
		}
        #endregion

		#region Vars
		/// <summary>
		/// The last folder created or set on the manager.
		/// </summary>
		public string lastFolder = "";
		/// <summary>
		/// Textures to be saved as normal maps.
		/// </summary>
		private List<string> textureToNormal = new List<string> ();
		/// <summary>
		/// Atlas names.
		/// </summary>
		private List<string> atlases = new List<string> ();
		/// <summary>
		/// Atlases to be saved as normal maps.
		/// </summary>
		private List<string> atlasToNormal = new List<string> ();
		/// <summary>
		/// Assignation of texture id to atlas index.
		/// </summary>
		private Dictionary<string, int> textureToAtlases = new Dictionary<string, int> ();
		/// <summary>
		/// Assignation of rects areas after creation of atlases.
		/// </summary>
		private Dictionary<string, Rect[]> atlasesRects = new Dictionary<string, Rect[]> ();
		/// <summary>
		/// Path to texture asset after saving textures.
		/// </summary>
		private Dictionary<string, string> textureAssetPath = new Dictionary<string, string> ();
		/// <summary>
		/// Path to atlas asset after saving atlases.
		/// </summary>
		private Dictionary<string, string> atlasAssetPath = new Dictionary<string, string> ();
		/// <summary>
		/// List of textures instantiated on this manager.
		/// </summary>
		private List<Texture2D> instantiatedTextures = new List<Texture2D> ();
		/// <summary>
		/// The textures for atlas.
		/// </summary>
		List<Texture2D> texturesForAtlas = new List<Texture2D> ();
		#endregion

		

		#region Textures
        /// <summary>
		/// Add a texture to the manager, if the id exists the texture gets replaced.
		/// </summary>
		/// <param name="textureId">Texture identifier.</param>
		/// <param name="texture">Texture to add to the manager.</param>
		public void AddTexture (string textureId, Texture2D texture) {
            if (textures.ContainsKey (textureId)) {
                RemoveTexture (textureId);
            }
			textures.Add (textureId, texture);
		}
        /*
		/// <summary>
		/// Add a texture to the manager.
		/// </summary>
		/// <returns>Texture added to the manager or null if no file was found.</returns>
		/// <param name="textureId">Texture identifier.</param>
		/// <param name="texturePath">Path to texture file.</param>
		public Texture2D AddTexture (string textureId, string texturePath) {
			Texture2D texture = null;
			byte[] fileData;
			if (File.Exists (texturePath)) {
				texture = new Texture2D (2, 2);
				fileData = File.ReadAllBytes (texturePath);
				texture.LoadImage (fileData);
				AddTexture (textureId, texture);
			}
			return texture;
		}
        */
		/// <summary>
		/// Removes a texture.
		/// </summary>
		/// <returns><c>true</c>, if texture was removed, <c>false</c> otherwise.</returns>
		/// <param name="textureId">Texture identifier.</param>
		public bool RemoveTexture (string textureId) {
			if (textures.ContainsKey (textureId)) {
                Texture2D.DestroyImmediate (textures [textureId]);
				textures.Remove (textureId);
				return true;
			}
			return false;
		}
		/// <summary>
		/// Number of textures in the manager.
		/// </summary>
		/// <returns>The texture count.</returns>
		public int GetTextureCount () {
			return textures.Count;
		}
		/// <summary>
		/// Registers an added texture to be taken as part of an atlas.
		/// </summary>
		/// <returns><c>true</c>, if texture to atlas was added, <c>false</c> otherwise.</returns>
		/// <param name="textureId">Texture identifier.</param>
		/// <param name="atlasId">Atlas identifier.</param>
		public bool RegisterTextureToAtlas (string textureId, string atlasId = "atlas") {
			if (textures.ContainsKey (textureId)) {
				if (!atlases.Contains (atlasId)) {
					atlases.Add (atlasId);
				}
				textureToAtlases.Add (textureId, atlases.IndexOf (atlasId));
				return true;
			}
			return false;
		}
		/// <summary>
		/// Sets the texture to normal map.
		/// </summary>
		/// <returns><c>true</c>, if texture to normal map was set, <c>false</c> otherwise.</returns>
		/// <param name="textureId">Texture identifier.</param>
		public bool SetTextureToNormalMap (string textureId) {
			if (textureToNormal.Contains (textureId)) {
				return true;
			} else if (textures.ContainsKey (textureId)) {
				textureToNormal.Add (textureId);
				return true;
			}
			return false;
		}
		/// <summary>
		/// Sets the atlas to normal map.
		/// </summary>
		/// <returns><c>true</c>, if atlas to normal map was set, <c>false</c> otherwise.</returns>
		/// <param name="atlasId">Atlas identifier.</param>
		public bool SetAtlasToNormalMap (string atlasId = "atlas") {
			if (atlasToNormal.Contains (atlasId)) {
				return true;
			} else if (atlases.Contains (atlasId)) {
				atlasToNormal.Add (atlasId);
				return true;
			}
			return false;
		}
		/// <summary>
		/// Tries to get the main texture assigned to a material.
		/// </summary>
		/// <returns>The main texture assigned to the material, if the method could not find one null is returned.</returns>
		/// <param name="material">Material to inspect.</param>
		/// <param name="makeCopy">If set to <c>true</c> make a copy of the texture.</param>
		public Texture2D GetMainTexture (Material material, bool makeCopy = true) {
			Texture2D tex = null;
			if (material.HasProperty ("_MainTex")) {
				tex = (Texture2D)material.GetTexture ("_MainTex");
			}
			Texture2D dest = null;
			if (tex != null && makeCopy) {
				dest = GetCopy (tex);
				instantiatedTextures.Add (dest);
			} else {
				dest = tex;
			}
			return dest;
		}
		/// <summary>
		/// Tries to get the normal texture assigned to a material.
		/// </summary>
		/// <returns>The normal texture assigned to the material, if the method could not find one null is returned.</returns>
		/// <param name="material">Material to inspect.</param>
		/// <param name="makeCopy">If set to <c>true</c> make a copy of the texture.</param>
		public Texture2D GetNormalTexture (Material material, bool makeCopy = true) {
			Texture2D normalTexture= null;
			if (material.HasProperty ("_BumpSpecMap")) {
				normalTexture = (Texture2D)material.GetTexture ("_BumpSpecMap");
			}
			Texture2D dest = null;
			if (normalTexture == null && material.HasProperty ("_BumpMap")) {
				normalTexture = (Texture2D)material.GetTexture ("_BumpMap");
			}
			if (normalTexture != null && makeCopy) {
				dest = GetCopy (normalTexture);
				if (!instantiatedTextures.Contains (dest)) {
					instantiatedTextures.Add (dest);
				}
			} else {
				dest = normalTexture;
			}
			return dest;
		}
		/// <summary>
		/// Gets a readable copy of a texture.
		/// </summary>
		/// <returns>The readable texture.</returns>
		/// <param name="baseTexture">Base texture.</param>
		public Texture2D GetCopy (Texture2D baseTexture) {
			Texture2D texCopy = null;
			if (baseTexture != null) {
				// Create a temporary RenderTexture of the same size as the texture
				RenderTexture tmpRenderTexture = RenderTexture.GetTemporary (
					baseTexture.width,
					baseTexture.height,
					1,
					RenderTextureFormat.Default,
					RenderTextureReadWrite.Linear);
				// Blit the pixels on texture to the RenderTexture
				Graphics.Blit(baseTexture, tmpRenderTexture);
				// Backup the currently set RenderTexture
				RenderTexture previousRenderTexture = RenderTexture.active;
				// Set the current RenderTexture to the temporary one we created
				RenderTexture.active = tmpRenderTexture;
				// Create a new readable Texture2D to copy the pixels to it
				texCopy = new Texture2D (baseTexture.width, baseTexture.height);
				// Copy the pixels from the RenderTexture to the new Texture
				texCopy.ReadPixels(new Rect(0, 0, tmpRenderTexture.width, tmpRenderTexture.height), 0, 0);
				texCopy.Apply();
				// Reset the active RenderTexture
				RenderTexture.active = previousRenderTexture;
				// Release the temporary RenderTexture
				RenderTexture.ReleaseTemporary (tmpRenderTexture);
				if (!instantiatedTextures.Contains (texCopy)) {
					instantiatedTextures.Add (texCopy);
				}
			}
			return texCopy;
		}
		/// <summary>
		/// Gets the texture asset path.
		/// </summary>
		/// <returns>The texture asset path.</returns>
		/// <param name="textureId">Texture identifier.</param>
		public string GetTextureAssetPath (string textureId) {
			if (textureAssetPath.ContainsKey (textureId)) {
				return textureAssetPath [textureId];
			}
			return string.Empty;
		}
		/// <summary>
		/// Gets the atlas asset path.
		/// </summary>
		/// <returns>The atlas asset path.</returns>
		/// <param name="atlasId">Atlas identifier.</param>
		public string GetAtlasAssetPath (string atlasId = "atlas") {
			if (atlasAssetPath.ContainsKey (atlasId)) {
				return atlasAssetPath [atlasId];
			}
			return string.Empty;
		}
		/// <summary>
		/// Gets the atlas rects.
		/// </summary>
		/// <returns>The atlas rects.</returns>
		/// <param name="atlasId">Atlas identifier.</param>
		public Rect[] GetAtlasRects (string atlasId = "atlas") {
			if (atlasesRects.ContainsKey (atlasId)) {
				return atlasesRects [atlasId];
			}
			return new Rect[0];
		}
		/// <summary>
		/// Clear this instance.
		/// </summary>
		public void Clear () {
			var enumerator = textures.GetEnumerator ();
			while (enumerator.MoveNext ()) {
				UnityEngine.Object.DestroyImmediate (enumerator.Current.Value, true);
			}
			textures.Clear ();
			atlases.Clear ();
			atlasToNormal.Clear ();
			textureToAtlases.Clear ();
			textureToNormal.Clear ();
			atlasesRects.Clear ();
			textureAssetPath.Clear ();
			atlasAssetPath.Clear ();
			for (int i = 0; i < instantiatedTextures.Count; i++) {
				Object.DestroyImmediate (instantiatedTextures[i]);
			}
			instantiatedTextures.Clear ();
			texturesForAtlas.Clear ();
		}
		#endregion

		#region Texture IO
		/// <summary>
		/// Gets or creates a folder.
		/// </summary>
		/// <returns>The or create folder.</returns>
		/// <param name="parentFolder">Parent folder.</param>
		/// <param name="newFolderName">New folder name.</param>
		public string GetOrCreateFolder (string parentFolder, string newFolderName) {
			lastFolder = "";
			#if UNITY_EDITOR
			if (!AssetDatabase.IsValidFolder (parentFolder)) {
				throw new UnityException ("Broccoli Tree Creator: Parent folder does not exist (" + parentFolder + ").");
			} else if (AssetDatabase.IsValidFolder (parentFolder + "/" + newFolderName)) {
				lastFolder = parentFolder + "/" + newFolderName;
				return lastFolder;
			}
			AssetDatabase.CreateFolder (parentFolder, newFolderName);
			#endif
			lastFolder = parentFolder + "/" + newFolderName;
			return lastFolder;
		}
		/// <summary>
		/// Saves a texture to a file.
		/// </summary>
		/// <param name="texture">Texture.</param>
		/// <param name="filename">Filename.</param>
		public void SaveTextureToFile (Texture2D texture, string filename) {
			#if UNITY_EDITOR
			System.IO.File.WriteAllBytes (filename, texture.EncodeToPNG());
			AssetDatabase.ImportAsset (filename);
			#endif
		}
		/// <summary>
		/// Saves textures to assets.
		/// </summary>
		/// <param name="includeAtlases">If set to <c>true</c> include atlases.</param>
		/// <param name="folderPath">Folder path.</param>
		public void SaveTexturesToAssets (bool includeAtlases = false, string folderPath = "") {
			#if UNITY_EDITOR
			if (string.IsNullOrEmpty (folderPath))
				folderPath = lastFolder;
			if (!AssetDatabase.IsValidFolder(folderPath))
				throw new UnityException ("Broccoli Tree Creator: Path specified to create the mipmap file is not valid (" + 
					folderPath + ").");
			var enumerator = textures.GetEnumerator ();
			string textureId;
			while (enumerator.MoveNext ()) {
				textureId = enumerator.Current.Key;
				if (!textureToAtlases.ContainsKey (textureId)) {
					string texturePath = folderPath + "/" + textureId + ".png";
					SaveTextureToFile (textures[textureId], texturePath);
					textureAssetPath.Add (textureId, texturePath);
					if (textureToNormal.Contains (textureId)) {
						TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath (texturePath);
						importer.textureType = TextureImporterType.NormalMap;
						importer.SaveAndReimport ();
					}
				}
			}
			if (includeAtlases) {
				SaveAtlasesToAssets (folderPath);
			}
			#endif
		}
		/// <summary>
		/// Saves atlases to assets.
		/// </summary>
		/// <param name="folderPath">Folder path.</param>
		/// <param name="maximumAtlasSize">Maximum atlas size.</param>
		public void SaveAtlasesToAssets (string folderPath = "", int maximumAtlasSize = 512) {
			#if UNITY_EDITOR
			if (string.IsNullOrEmpty (folderPath))
				folderPath = lastFolder;
			if (!AssetDatabase.IsValidFolder(folderPath))
				throw new UnityException ("Broccoli Tree Creator: Path specified to create the mipmap file is not valid (" + 
					folderPath + ").");
			for (int i = 0; i < atlases.Count; i++) {
				texturesForAtlas.Clear ();
				var enumerator = textureToAtlases.GetEnumerator ();
				string textureId;
				while (enumerator.MoveNext ()) {
					textureId = enumerator.Current.Key;
					if (textureToAtlases [textureId] == i) {
						texturesForAtlas.Add (textures [textureId]);
					}
				}
				if (texturesForAtlas.Count > 0) {
					Texture2D atlas = new Texture2D (2, 2);
					atlasesRects.Add (atlases[i], atlas.PackTextures (texturesForAtlas.ToArray (), 2, maximumAtlasSize, false));
					atlas.alphaIsTransparency = true;
					string texturePath = folderPath + "/" + atlases [i] + ".png";
					SaveTextureToFile (atlas, texturePath);
					atlasAssetPath.Add (atlases [i], texturePath);
					TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath (texturePath);
					if (atlasToNormal.Contains (atlases[i])) {
						importer.textureType = TextureImporterType.NormalMap;
					}
					importer.alphaIsTransparency = true;
					importer.SaveAndReimport ();
					instantiatedTextures.Add (atlas);
				}
				texturesForAtlas.Clear ();
			}
			#endif
		}
		#endregion
	}
}