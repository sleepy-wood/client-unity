using System.Collections.Generic;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

using Broccoli.Base;

namespace Broccoli.Controller {
	/// <summary>
	/// Controls a Tree Broccoli Instances in Terrains.
	/// </summary>
	public class BroccoTerrainController : MonoBehaviour {
		#region Vars
		/// <summary>
		/// Version of Broccoli Tree Creator issuing this controller.
		/// </summary>
		public string version {
            get { return Broccoli.Base.BroccoliExtensionInfo.version; }
        }
        /// <summary>
        /// Terrain component.
        /// </summary>
        Terrain terrain = null;
        /// <summary>
        /// Keeps track of the material instances to update.
        /// </summary>
        List<Material> _materials = new List<Material> ();
		/// <summary>
		/// Keeps tracl of the material instances parameters.
		/// x: sproutTurbulance
		/// y: sproutSway
		/// z: localWindAmplitude
		/// </summary>
		List<Vector3> _materialParams = new List<Vector3> ();
		/// <summary>
		/// Materials array.
		/// </summary>
		Material[] materials;
		/// <summary>
		/// Material params array.
		/// </summary>
		Vector3[] materialParams;
		bool requiresUpdateWindZoneValues = true;
		private float baseWindAmplitude = 0.2752f;
		public static float globalWindAmplitude = 1f;
		private float valueWindMain = 0f;
		private float valueLeafSwayFactor = 1f;
		private float valueLeafTurbulenceFactor = 1f;
		#endregion

		#region Shader values
		float valueTime = 0f;
		Vector4 valueWindDirection = Vector4.zero;
		Vector4 valueSTWindVector = Vector4.zero;
		Vector4 valueSTWindGlobal = Vector4.zero;
		Vector4 valueSTWindBranch = Vector4.zero;
		Vector4 valueSTWindBranchTwitch = Vector4.zero;
		Vector4 valueSTWindBranchWhip = Vector4.zero;
		Vector4 valueSTWindBranchAnchor = Vector4.zero;
		Vector4 valueSTWindBranchAdherences = Vector4.zero;
		Vector4 valueSTWindTurbulences = Vector4.zero;
		Vector4 valueSTWindLeaf1Ripple = Vector4.zero;
		Vector4 valueSTWindLeaf1Tumble = Vector4.zero;
		Vector4 valueSTWindLeaf1Twitch = Vector4.zero;
		Vector4 valueSTWindLeaf2Ripple = Vector4.zero;
		Vector4 valueSTWindLeaf2Tumble = Vector4.zero;
		Vector4 valueSTWindLeaf2Twitch = Vector4.zero;
		Vector4 valueSTWindFrondRipple = Vector4.zero;
		#endregion

		#region Shader Property Ids
		static int propWindEnabled = 0;
		static int propWindQuality = 0;
		static int propSTWindVector = 0;
		static int propSTWindGlobal = 0;
		static int propSTWindBranch = 0;
		static int propSTWindBranchTwitch = 0;
		static int propSTWindBranchWhip = 0;
		static int propSTWindBranchAnchor = 0;
		static int propSTWindBranchAdherences = 0;
		static int propSTWindTurbulences = 0;
		static int propSTWindLeaf1Ripple = 0;
		static int propSTWindLeaf1Tumble = 0;
		static int propSTWindLeaf1Twitch = 0;
		static int propSTWindLeaf2Ripple = 0;
		static int propSTWindLeaf2Tumble = 0;
		static int propSTWindLeaf2Twitch = 0;
		static int propSTWindFrondRipple = 0;
		#endregion

		#region Static Constructor
        /// <summary>
        /// Static controller for this class.
        /// </summary>
		static BroccoTerrainController () {
			propWindEnabled = Shader.PropertyToID ("_WindEnabled");
			propWindQuality = Shader.PropertyToID ("_WindQuality");
			propSTWindVector = Shader.PropertyToID ("_ST_WindVector");
			propSTWindVector = Shader.PropertyToID ("_ST_WindVector");
			propSTWindGlobal = Shader.PropertyToID ("_ST_WindGlobal");
			propSTWindBranch = Shader.PropertyToID ("_ST_WindBranch");
			propSTWindBranchTwitch = Shader.PropertyToID ("_ST_WindBranchTwitch");
			propSTWindBranchWhip = Shader.PropertyToID ("_ST_WindBranchWhip");
			propSTWindBranchAnchor = Shader.PropertyToID ("_ST_WindBranchAnchor");
			propSTWindBranchAdherences = Shader.PropertyToID ("_ST_WindBranchAdherences");
			propSTWindTurbulences = Shader.PropertyToID ("_ST_WindTurbulences");
			propSTWindLeaf1Ripple = Shader.PropertyToID ("_ST_WindLeaf1Ripple");
			propSTWindLeaf1Tumble = Shader.PropertyToID ("_ST_WindLeaf1Tumble");
			propSTWindLeaf1Twitch = Shader.PropertyToID ("_ST_WindLeaf1Twitch");
			propSTWindLeaf2Ripple = Shader.PropertyToID ("_ST_WindLeaf2Ripple");
			propSTWindLeaf2Tumble = Shader.PropertyToID ("_ST_WindLeaf2Tumble");
			propSTWindLeaf2Twitch = Shader.PropertyToID ("_ST_WindLeaf2Twitch");
			propSTWindFrondRipple = Shader.PropertyToID ("_ST_WindFrondRipple");
		}
		#endregion

		#region Events
		/// <summary>
		/// Start this instance.
		/// </summary>
		public void Start () {
            // Get the terrain.
            terrain = GetComponent<Terrain> ();
            if (terrain != null) {
				requiresUpdateWindZoneValues = true;
				SetupWind ();
            }
		}
		/// <summary>
		/// Update this instance.
		/// </summary>
		void Update () {
			#if UNITY_EDITOR
			if (EditorApplication.isPlaying) {
				for (int i = 0; i < materials.Length; i++) {
					UpdateSpeedTreeWind (materials [i], materialParams [i]);	
				}
			}
			#else
			for (int i = 0; i < materials.Length; i++) {
					UpdateSpeedTreeWind (materials [i], materialParams [i]);	
				}
			#endif
		}
		#endregion

		#region Wind
        /// <summary>
        /// Setup the wind on Tree Prototype materals found on this terrain
        /// and add there materials to an array to update the wind on each frame.
        /// </summary>
		//private void SetupWind (BroccoTreeController treeController) {
		private void SetupWind () {
			//SetupTreeController (treeController);
			// Get all the materials on the tree prefabs, initializes 
			// them and saves them to and array.
			GameObject treePrefab;
			BroccoTreeController[] treeControllers;
			for (int i = 0; i < terrain.terrainData.treePrototypes.Length; i++) {
				treePrefab = terrain.terrainData.treePrototypes [i].prefab;
				if (treePrefab != null) {
					treeControllers = treePrefab.GetComponentsInChildren<BroccoTreeController> ();
					foreach (BroccoTreeController treeController in treeControllers) {
						// Setup instances of tree controller according to the controller.
						SetupTreeController (treeController);
					}
				}
			}
			materials = _materials.ToArray ();
			materialParams = _materialParams.ToArray ();
			_materials.Clear ();
			_materialParams.Clear ();
		}
        /// <summary>
        /// Setup materials in instances with BroccoTreeController.
        /// </summary>
        /// <param name="treeController"></param>
        private void SetupTreeController (BroccoTreeController treeController) {
            Renderer renderer = treeController.gameObject.GetComponent<Renderer> ();
            Material material;
            if (renderer != null && 
                (treeController.shaderType == BroccoTreeController.ShaderType.SpeedTree8OrCompatible ||
                treeController.shaderType == BroccoTreeController.ShaderType.SpeedTree7OrCompatible))
            {
				if (requiresUpdateWindZoneValues) {
					GetWindZoneValues (treeController);
					requiresUpdateWindZoneValues = false;
				}
                for (int i = 0; i < renderer.sharedMaterials.Length; i++) {
                    material = renderer.sharedMaterials [i];
                    bool isWindEnabled = treeController.windQuality != BroccoTreeController.WindQuality.None;

                    if (treeController.shaderType == BroccoTreeController.ShaderType.SpeedTree8OrCompatible) {
                        material.DisableKeyword ("_WINDQUALITY_NONE");
                        material.DisableKeyword ("_WINDQUALITY_FASTEST");
                        material.DisableKeyword ("_WINDQUALITY_FAST");
                        material.DisableKeyword ("_WINDQUALITY_BETTER");
                        material.DisableKeyword ("_WINDQUALITY_BEST");
                        material.DisableKeyword ("_WINDQUALITY_PALM");
                        if (isWindEnabled) {
                            switch (treeController.windQuality) {
                                case BroccoTreeController.WindQuality.None:
                                    material.EnableKeyword ("_WINDQUALITY_NONE");
                                    break;
                                case BroccoTreeController.WindQuality.Fastest:
                                    material.EnableKeyword ("_WINDQUALITY_FASTEST");
                                    break;
                                case BroccoTreeController.WindQuality.Fast:
                                    material.EnableKeyword ("_WINDQUALITY_FAST");
                                    break;
                                case BroccoTreeController.WindQuality.Better:
                                    material.EnableKeyword ("_WINDQUALITY_BETTER");
                                    break;
                                case BroccoTreeController.WindQuality.Best:
                                    material.EnableKeyword ("_WINDQUALITY_BEST");
                                    break;
                                case BroccoTreeController.WindQuality.Palm:
                                    material.EnableKeyword ("_WINDQUALITY_PALM");
                                    break;
                            }
                        }
                    } else if (isWindEnabled) {
                        if (treeController.windQuality != BroccoTreeController.WindQuality.None) {
                            material.EnableKeyword ("ENABLE_WIND");
                        } else {
                            material.DisableKeyword ("ENABLE_WIND");
                        }
                    }
                    // Set the material wind properties.
					valueSTWindGlobal.z = 0.0655f;
					valueSTWindGlobal.w = 1.728f;
                    material.SetFloat (propWindEnabled, (isWindEnabled?1f:0f));
                    material.SetFloat (propWindQuality, (float)treeController.windQuality);
					// STWindVector
					valueSTWindVector = valueWindDirection;
					material.SetVector (propSTWindVector, valueSTWindVector);
                    // STWindBranchTwitch (0.6, 0.1, 0.8, 0.3)
                    valueSTWindBranchTwitch = new Vector4 (0.603f, 0.147f, 0.75f, 0.3f);
                    material.SetVector (propSTWindBranchTwitch, valueSTWindBranchTwitch);
                    // STWindBranchWhip
                    valueSTWindBranchWhip = new Vector4 (0.0f, 0.0f, 0.0f, 0.0f);
                    material.SetVector (propSTWindBranchWhip, valueSTWindBranchWhip);
                    // STWindBranchAnchor
                    valueSTWindBranchAnchor = new Vector4 (0.034f, 0.4773f, 0.878f, 11.081f);
                    material.SetVector (propSTWindBranchAnchor, valueSTWindBranchAnchor);
                    // STWindBranchAdherences
                    valueSTWindBranchAdherences = new Vector4 (0.09295f, 0.1f, 0f, 0f);
                    material.SetVector (propSTWindBranchAdherences, valueSTWindBranchAdherences);
                    // STWindTurbulences
                    valueSTWindTurbulences = new Vector4 (0.7f, 0.3f, 0f, 0f);
                    material.SetVector (propSTWindTurbulences, valueSTWindTurbulences);

                    if (!_materials.Contains (material) && isWindEnabled) {
                        _materials.Add (material);
						_materialParams.Add (new Vector3 (treeController.sproutTurbulance, treeController.sproutSway, treeController.localWindAmplitude));
                    }
                }
            }
        }
		/// <summary>
		/// Updates the values of materials.
		/// </summary>
		/// <param name="material">Material</param>
		/// <param name="windParams">Wind parameters, x: sproutTurbulance, y: sproutSway, z: localWindAmplitude.</param>
		private void UpdateSpeedTreeWind (Material material, Vector3 windParams) {
			#if UNITY_EDITOR
			valueTime = (EditorApplication.isPlaying)?Time.time:(float)EditorApplication.timeSinceStartup;
			#else
			valueTime = Time.time;
			#endif
			valueSTWindGlobal.x = valueTime * 0.36f;
			valueSTWindGlobal.y = baseWindAmplitude * windParams.z * globalWindAmplitude * valueWindMain;
			material.SetVector (propSTWindGlobal, valueSTWindGlobal);
			// STWindBranch
			valueSTWindBranch = new Vector4 (valueTime * 0.65f, 0.4102f, valueTime * 1.5f, 0f) * windParams.z; // Branch swaying
			material.SetVector (propSTWindBranch, valueSTWindBranch);
			// STWindLeaf1Ripple (time * 3.2, 0, 0.5, 0)
			valueSTWindLeaf1Ripple = new Vector4 (valueTime * 3.18f, 0.044f, 0.5f, 0f) * windParams.x; // Leaf Ripple
			material.SetVector (propSTWindLeaf1Ripple, valueSTWindLeaf1Ripple);
			// STWindLeaf2Ripple (time * 4.7, 0, 0.5, 0)
			valueSTWindLeaf2Ripple = new Vector4 (valueTime * 4.7f, 0f, 0.5f, 0f) * windParams.x; // Leaf Ripple
			material.SetVector (propSTWindLeaf2Ripple, valueSTWindLeaf2Ripple);
			// STWindLeaf1Tumble (time, 0.1, 0.1, 0.1)
			valueSTWindLeaf1Tumble = new Vector4 (valueTime * 0.84f, 0.1298f, 0.11403f, 0.11f) * valueLeafSwayFactor * windParams.y; // EFFECT SPROUT SWAY
			material.SetVector (propSTWindLeaf1Tumble, valueSTWindLeaf1Tumble);
			// STWindLeaf2Tumble (time, 0.1, 0.1, 0.1)
			valueSTWindLeaf2Tumble = new Vector4 (valueTime, 0.035f, 0.035f, 0.5f) * valueLeafSwayFactor * windParams.y; // EFFECT SPROUT SWAY
			material.SetVector (propSTWindLeaf2Tumble, valueSTWindLeaf2Tumble);
			// STWindLeaf1Twitch (0.3, 0.3, time * 1.5, 0.0)
			valueSTWindLeaf1Twitch = new Vector4 (0.3315f, 0.3246f, valueTime * 1.56f, 0f) * valueLeafTurbulenceFactor * windParams.y; // EFFECT SPROUT TURBULENCE
			material.SetVector (propSTWindLeaf1Twitch, valueSTWindLeaf1Twitch);
			// STWindLeaf2Twitch (0, 33.3, time / 1.5, 0.0)
			valueSTWindLeaf2Twitch = new Vector4 (0.01745f, 33.3333f, valueTime * 0.31f, 12.896f) * valueLeafTurbulenceFactor * windParams.y; // EFFECT SPROUT TURBULENCE
			material.SetVector (propSTWindLeaf2Twitch, valueSTWindLeaf2Twitch);
			// STWindFrondRipple (time * -40, 1.2, 10.3, 0)
			valueSTWindFrondRipple = new Vector4 (valueTime * -40.5f, 1.2192f, 10.34f, 0.0f);
			material.SetVector (propSTWindFrondRipple, valueSTWindFrondRipple);
		}
		/// <summary>
		/// Update params related to the first detected directional wind zone.
		/// </summary>
		/// <param name="treeController">Tree controller.</param>
		public void GetWindZoneValues (BroccoTreeController treeController) {
			bool isST8 = treeController.shaderType == BroccoTreeController.ShaderType.SpeedTree8OrCompatible;
			valueWindDirection = new Vector4 (1f, 0f, 0f, 0f);
			WindZone[] windZones = FindObjectsOfType<WindZone> ();
			for (int i = 0; i < windZones.Length; i++) {
				if (windZones [i].gameObject.activeSelf && windZones[i].mode == WindZoneMode.Directional) {
					valueWindMain = windZones [i].windMain;
					valueWindDirection = new Vector4 (windZones [i].transform.forward.x, windZones [i].transform.forward.y, windZones [i].transform.forward.z, 1f);
					valueLeafSwayFactor = (isST8?0.4f:1f) * windZones [i].windMain;
					valueLeafTurbulenceFactor = (isST8?0.4f:1f) * windZones [i].windTurbulence;
					break;
				}
			}
		}
		#endregion
	}
}