using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Object = UnityEngine.Object;

namespace Broccoli.Base
{
	/// <summary>
	/// Extension manager.
	/// </summary>
	public static class ExtensionManager
	{
		#region Vars
		/// <summary>
		/// Init flag, true when the extension is initialized.
		/// </summary>
		private static bool isInit = false;
		/// <summary>
		/// Path used by the extension.
		/// </summary>
		private static string _extensionPath = "";
		/// <summary>
		/// Gets the extension path.
		/// </summary>
		/// <value>The extension path.</value>
		public static string extensionPath { 
			get {
				if (!isInit) {
					Init ();
				}
				return _extensionPath; 
			} 
		}
		/// <summary>
		/// Gets the full extension path.
		/// </summary>
		/// <value>The full extension path.</value>
		public static string fullExtensionPath {
			get {
				return Application.dataPath + 
					ExtensionManager.extensionPath.Replace ("Assets", "");
			}
		}
		/// <summary>
		/// The resources path.
		/// </summary>
		private static string _resourcesPath = "";
		/// <summary>
		/// The resources path.
		/// </summary>
		public static string resourcesPath {
			get {
				if (!isInit) {
					Init ();
				}
				return _resourcesPath;
			}
		}
		public static bool _isURP = false;
		public static bool isURP {
			get { return _isURP; }
		}
		public static bool _isHDRP = false;
		public static bool isHDRP {
			get { return _isHDRP; }
		}
		#if UNITY_EDITOR
		/// <summary>
		/// Asset name used as reference to find the extension path.
		/// </summary>
		private static string uniqueExtensionAsset = "BroccoliExtensionInfo";
		#endif
		#endregion

		#region Initialization
		/// <summary>
		/// Init this class.
		/// </summary>
		public static void Init () {
			if (!isInit) {
				SetupBase ();
			}
		}
		/// <summary>
		/// Setups the base of the extension.
		/// </summary>
		public static void SetupBase () {
			if (CheckExtensionPath () && 
				SetPaths ()) {
				CheckRenderPipeline ();
				isInit = true;
			}
		}
		/// <summary>
		/// Checks the extension path.
		/// </summary>
		/// <returns><c>true</c>, if extension path was checked, <c>false</c> otherwise.</returns>
		private static bool CheckExtensionPath () {
			#if UNITY_EDITOR
			Object script = 
				UnityEditor.AssetDatabase.LoadAssetAtPath (_extensionPath + "Base/" + 
					uniqueExtensionAsset + ".cs", typeof(Object));
			if (script == null) {
				string[] assets = UnityEditor.AssetDatabase.FindAssets (uniqueExtensionAsset);
				if (assets.Length > 0) {
					_extensionPath = UnityEditor.AssetDatabase.GUIDToAssetPath (assets [0]);
					int subFolderIndex = _extensionPath.LastIndexOf ("Base/");
					if (subFolderIndex == -1)
						throw new UnityException ("Broccoli Tree Creator: Correct path could not be detected! " +
							"Please correct the _extensionPath variable in ExtensionManager.cs!");
					_extensionPath = _extensionPath.Substring (0, subFolderIndex);
					_resourcesPath = _extensionPath + "Editor/Resources/";
					return true;
				} else {
					throw new UnityException ("Broccoli Tree Creator: Correct path could not be detected! " +
						"Please correct the _extensionPath variable in ExtensionManager.cs!");
				}
			}
			return false;
			#else
			return true;
			#endif
		}
		/// <summary>
		/// Sets the paths for the extension.
		/// </summary>
		/// <returns><c>true</c>, if paths was set, <c>false</c> otherwise.</returns>
		private static bool SetPaths () {
			return true;
		}
		/// <summary>
		/// Checks for installed Render Pipelines.
		/// </summary>
		private static void CheckRenderPipeline () {
			for (int i = 0; i < System.AppDomain.CurrentDomain.GetAssemblies().Length; i++) {
				System.Reflection.Assembly assembly = System.AppDomain.CurrentDomain.GetAssemblies()[i];
				if (assembly.FullName.IndexOf ("Unity.RenderPipelines.Universal") >= 0) {
					_isURP = true;
					break;
				}
			}
			// HDRP SpeedTree8 shaders are available for Unity 2020.3 and HDRP 10.6.0.
			#if UNITY_2020_3_OR_NEWER
			for (int i = 0; i < System.AppDomain.CurrentDomain.GetAssemblies().Length; i++) {
				System.Reflection.Assembly assembly = System.AppDomain.CurrentDomain.GetAssemblies()[i];
				if (assembly.FullName.IndexOf ("Unity.RenderPipelines.HighDefinition") >= 0) {
					_isHDRP = true;
					break;
				}
			}
			#endif
		}
		//MaterialExternalReferences.GetMaterialExternalReferences(target as Material);
		public static ScriptableObject SetMaterialExternalReferences (Material material, ScriptableObject diffusionProfileSettings) {
			ScriptableObject so = null;
			#if UNITY_EDITOR
			System.Type materialExternalReferencesType = System.Type.GetType ("UnityEditor.Rendering.HighDefinition.MaterialExternalReferences, Unity.RenderPipelines.HighDefinition.Editor");
			if (materialExternalReferencesType != null) {
				System.Reflection.BindingFlags bf = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;
				System.Reflection.MethodInfo methodInfo = materialExternalReferencesType.GetMethod ("GetMaterialExternalReferences", bf);
				if (methodInfo != null) {
					ScriptableObject materialExternalRefsSO = (ScriptableObject)methodInfo.Invoke (materialExternalReferencesType, new object[]{material});
				}
			}
			// https://github.com/Unity-Technologies/Graphics/blob/49df5241b040b712d011e3225a63c4d78114d27d/com.unity.render-pipelines.high-definition/Editor/Material/DiffusionProfile/DiffusionProfileMaterialUI.cs#L51
			// https://github.com/Unity-Technologies/Graphics/blob/49df5241b040b712d011e3225a63c4d78114d27d/com.unity.render-pipelines.high-definition/Editor/Material/MaterialExternalReferences.cs#L15
			/*
			foreach (var target in materialEditor.targets)
			{
				MaterialExternalReferences matExternalRefs = MaterialExternalReferences.GetMaterialExternalReferences(target as Material);
				matExternalRefs.SetDiffusionProfileReference(profileIndex, diffusionProfile);
			}
			*/
			#endif
			return so;
		}
		public static float GetHashFromDiffusionProfile (ScriptableObject diffusionProfileSettings) {
			uint hash = 0;
			System.Reflection.BindingFlags bf = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;
			System.Reflection.FieldInfo profileFieldInfo = diffusionProfileSettings.GetType ().GetField ("profile", bf);
			if (profileFieldInfo != null) {
				object profileObj = profileFieldInfo.GetValue ((object)diffusionProfileSettings);
				System.Reflection.FieldInfo hashFieldInfo = profileObj.GetType ().GetField ("hash", bf);
				if (hashFieldInfo != null) { 
					hash = (uint)hashFieldInfo.GetValue (profileObj);
				}
				return ConvertHashToFloat (hash);
			}
			return 0f;
		}
		public static Vector4 GetVector4FromScriptableObject (ScriptableObject so) {
			Vector4 vector = Vector4.zero;
			#if UNITY_EDITOR
			string pathToAsset = UnityEditor.AssetDatabase.GetAssetPath (so);
			string guid = UnityEditor.AssetDatabase.AssetPathToGUID(pathToAsset);
			vector = ConvertGUIDToVector4 (guid);
			#endif
			return vector;
		}
		private static float ConvertHashToFloat (uint hash) {
			float hashFloat = 0f;
			byte[] bytes = System.BitConverter.GetBytes (hash);
			hashFloat = System.BitConverter.ToSingle (bytes,0*sizeof(float));
			return hashFloat;
		}
		private static Vector4 ConvertGUIDToVector4 (string guid)
        {
            byte[] bytes = new byte[16];
            for (int i = 0; i < 16; i++)
                bytes[i] = byte.Parse(guid.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
			Vector4 vect = Vector4.zero;
			vect.x = System.BitConverter.ToSingle(bytes,0*sizeof(float));
			vect.y = System.BitConverter.ToSingle(bytes,1*sizeof(float));
			vect.z = System.BitConverter.ToSingle(bytes,2*sizeof(float));
			vect.w = System.BitConverter.ToSingle(bytes,3*sizeof(float));
			return vect;
        }
		#endregion
	}
}