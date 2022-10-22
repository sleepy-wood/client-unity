using System.Collections;
using System.Collections.Generic;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

using Broccoli.Base;
using Broccoli.Pipe;
using Broccoli.Model;
using Broccoli.Controller;
using Broccoli.Component;
using Broccoli.Manager;

/// <summary>
/// Working classes that generate the tree asset from the pipeline of instruction nodes.
/// Includes manager classes.
/// </summary>
namespace Broccoli.Factory
{
	using Pipeline = Broccoli.Pipe.Pipeline;
	/// <summary>
	/// Main factory class. Process the pipeline of instruction nodes and creates the components of a
	/// tree using managers for meshes, materials and textures. It produces a final tree asset using the AssetManager class.
	/// </summary>
	[AddComponentMenu("Broccoli/Factory/TreeFactory")]
	[ExecuteInEditMode]
	public class TreeFactory : MonoBehaviour {
		#region Delegates and Events
		public delegate bool ProcessPipelineDelegate (Broccoli.Pipe.Pipeline pipeline, 
			BroccoTree tree, 
			int lodIndex,
			PipelineElement referenceElement = null, 
			bool useCache = false, 
			bool forceNewTree = false);
		public delegate bool ProcessPrefabDelegate (Broccoli.Pipe.Pipeline pipeline, BroccoTree tree, string path);
		public delegate void OnLODEvent (GameObject lodGameObject);
		public ProcessPipelineDelegate onBeforeProcessPipeline;
		public ProcessPipelineDelegate onBeforeProcessPipelinePreview;
		public ProcessPrefabDelegate onBeforeEndPrefabCommit;
		public ProcessPrefabDelegate onEndPrefabCommit;
		public ProcessPipelineDelegate onProcessPipeline;
		public ProcessPipelineDelegate onProcessPipelinePreview;
		public OnLODEvent onLODReady;
		#endregion

		#region Vars
		/// <summary>
		/// Preview mode for the preview trees.
		/// </summary>
		public enum PreviewMode {
			Textured,
			Colored
		}
		/// <summary>
		/// Prefab texture mode.
		/// Original: use the texture as provided.
		/// Separated: create atlases for sprouts and branches if necessary.
		/// Unique: create a global atlas for sprouts and branches textures if necessary.
		/// </summary>
		public enum PrefabTextureMode {
			Original,
			Atlas
		}
		/// <summary>
		/// Texture size.
		/// </summary>
		public enum TextureSize
		{
			_128px,
			_256px,
			_512px,
			_1024px,
			_2048px
		}
		/// <summary>
		/// The tree factory preferences.
		/// </summary>
		public TreeFactoryPreferences treeFactoryPreferences = new TreeFactoryPreferences ();
		/// <summary>
		/// Forces preview with colored materials.
		/// </summary>
		public bool forcePreviewModeColored = false;
		/// <summary>
		/// Types of processing done by the factory.
		/// </summary>
		public enum ProcessType {
			Preview = 0,
			Runtime = 1,
			Prefab = 2
		}
		/// <summary>
		/// Current type of processing.
		/// </summary>
		public ProcessType processType = ProcessType.Preview;
		/*
		/// <summary>
		/// Last pass on pipeline processing.
		/// </summary>
		int lastPass = 0;
		*/
		/// <summary>
		/// Indicates whether a progress bar should be display when a 
		/// prefab is being created..
		/// </summary>
		[System.NonSerialized]
		public bool showPrefabProgressBar = false;
		/// <summary>
		/// Estimate value of the prefab creation process.
		/// </summary>
		[System.NonSerialized]
		public int prefabProcessWeight = 0;
		/// <summary>
		/// Completed value of the prefab creation process value.
		/// </summary>
		[System.NonSerialized]
		public int prefabProcessWeightCompleted = 0;
		/// <summary>
		/// Description for a current step on the prefab creation process.
		/// </summary>
		[System.NonSerialized]
		public string prefabProcessAction = "";
		/// <summary>
		/// The local pipeline filepath.
		/// </summary>
		[System.NonSerialized]
		public string localPipelineFilepath = "";
		/// <summary>
		/// The tree used for previewing pipelines.
		/// </summary>
		[System.NonSerialized]
		BroccoTree _previewTree = null;
		/// <summary>
		/// Rendes visible the preview tree.
		/// </summary>
		public bool buildPreviewTreeVisible = true;
		/// <summary>
		/// Number of tries to get the preview tree when it is null.
		/// </summary>
		int getPreviewTreeTries = 2;
		/// <summary>
		/// Pipeline containing the definition on how to build a tree.
		/// </summary>
		[SerializeField]
		Broccoli.Pipe.Pipeline _localPipeline;
		/// <summary>
		/// Log queue for relevant events on the factory.
		/// </summary>
		public Queue<LogItem> log = new Queue<LogItem> (3);
		/// <summary>
		/// True if this instance has been initialized.
		/// </summary>
		bool isInit = false;
		/// <summary>
		/// Last value of undo count registered from a processed pipeline.
		/// </summary>
		public int lastUndoProcessed = 0;
		/// <summary>
		/// The seconds to wait to update the pipeline.
		/// </summary>
		private float secondsToUpdatePipeline = 0;
		/*
		/// <summary>
		/// The seconds to second pass preview.
		/// </summary>
		private float secondsToSecondPass = 0;
		/// <summary>
		/// The second pass reference element.
		/// </summary>
		private PipelineElement secondPassReferenceElement = null;
		*/
		/// <summary>
		/// The editor delta time.
		/// </summary>
		double editorDeltaTime = 0f;
		#if UNITY_EDITOR
		/// <summary>
		/// The last time since startup.
		/// </summary>
		double lastTimeSinceStartup = 0f;
		#endif
		/// <summary>
		/// If greater than 0 the factory is allowed to reprocess an 
		/// existing pipeline if the former data was lost.
		/// </summary>
		int firstReprocess = 2;
		/// <summary>
		/// The path to the last created prefab.
		/// </summary>
		public string lastPrefabPath = "";
		/// <summary>
		/// Counter for the number of spawned trees.
		/// </summary>
		int spawnCount = 0;
		/// <summary>
		/// Mesh collider component used to process branch intersection operations.
		/// </summary>
		MeshCollider _meshCollider;
		#endregion

		#region Managers
		/// <summary>
		/// The component manager.
		/// </summary>
		ComponentManager _componentManager = new ComponentManager ();
		/// <summary>
		/// The mesh manager.
		/// </summary>
		MeshManager _meshManager = new MeshManager ();
		/// <summary>
		/// The texture manager.
		/// </summary>
		TextureManager _textureManager = new TextureManager ();
		/// <summary>
		/// The material manager.
		/// </summary>
		MaterialManager _materialManager = new MaterialManager ();
		/// <summary>
		/// The asset manager.
		/// </summary>
		AssetManager _assetManager = new AssetManager ();
		/*
		/// <summary>
		/// Gets the last pipeline processed pass.
		/// </summary>
		/// <value>The last processed pass.</value>
		public int lastProcessedPass {
			get { return lastPass; }
		}
		*/
		#endregion

		#region Subfactories
		/// <summary>
		/// The sprout subfactory.
		/// </summary>
		SproutSubfactory _sproutFactory = null;
		#endregion

		#region Accessors
		/// <summary>
		/// Acess to the preview tree.
		/// </summary>
		/// <value>The preview tree.</value>
		public BroccoTree previewTree {
			get {
				if (_previewTree == null && getPreviewTreeTries > 0) {
					InitPreviewTree ();
					ProcessPipelinePreview (null, true, true);
					getPreviewTreeTries--;
				}
				return _previewTree;
			}
			set { _previewTree = value; }
		}
		/// <summary>
		/// Access to the local pipeline.
		/// </summary>
		/// <value>The local pipeline.</value>
		public Broccoli.Pipe.Pipeline localPipeline {
			get { return _localPipeline; }
			set {
				_localPipeline = value;
				ValidatePipeline (_localPipeline);
			}
		}
		/// <summary>
		/// Access to the component manager.
		/// </summary>
		/// <value>The component manager.</value>
		public ComponentManager componentManager {
			get { return _componentManager; }
		}
		/// <summary>
		/// Access to the mesh manager.
		/// </summary>
		/// <value>The mesh manager.</value>
		public MeshManager meshManager {
			get { return _meshManager; }
		}
		/// <summary>
		/// Access to the texture manager.
		/// </summary>
		/// <value>The texture manager.</value>
		public TextureManager textureManager {
			get { return _textureManager; }
		}
		/// <summary>
		/// Access to the materials manager.
		/// </summary>
		/// <value>The material manager.</value>
		public MaterialManager materialManager {
			get { return _materialManager; }
		}
		/// <summary>
		/// Access to the asset manager.
		/// </summary>
		/// <value>The asset manager.</value>
		public AssetManager assetManager {
			get { return _assetManager; }
		}
		/// <summary>
		/// Checks if the current process is to generate a preview tree.
		/// </summary>
		/// <returns>True if the process type is a preview tree.</returns>
		public bool isPreviewProcess {
			get { return this.processType == ProcessType.Preview; }
		}
		/// <summary>
		/// Checks if the current process is to generate a tree at runtime.
		/// </summary>
		/// <returns>True if the process type is a tree at runtime.</returns>
		public bool isRuntimeProcess {
			get { return this.processType == ProcessType.Runtime; }
		}
		/// <summary>
		/// Checks if the current process is to generate a prefab tree.
		/// </summary>
		/// <returns>True if the process type is a prefab tree.</returns>
		public bool isPrefabProcess {
			get { return this.processType == ProcessType.Prefab; }
		}
		#endregion

		#region Singleton
		/// <summary>
		/// Singleton.
		/// </summary>
		static TreeFactory _treeFactory = null;
		/// <summary>
		/// Singleton accessor.
		/// </summary>
		/// <returns>The instance.</returns>
		public static TreeFactory GetActiveInstance () {
			return _treeFactory;
		}
		/// <summary>
		/// Set this class singleton.
		/// </summary>
		/// <param name="treeFactory">Tree factory instance to set as singleton for the class.</param>
		public static void SetActiveInstance (TreeFactory treeFactory) {
			_treeFactory = treeFactory;
		}
		#endregion

		#region Editor extension
		#if UNITY_EDITOR
		[MenuItem("GameObject/Broccoli/Tree Factory", false, 10)]
		static void CreateTreeFactoryGameObject (MenuCommand menuCommand)
		{
			// Create a custom game object
			GameObject go = new GameObject("Broccoli Tree Factory");
			// Ensure it gets reparented if this was a context click (otherwise does nothing)
			GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
			go.AddComponent<Broccoli.Factory.TreeFactory> ();
			Selection.activeObject = go;
			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty (
				UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene ());
		}
		#endif
		#endregion

		#region Runtime
		/// <summary>
		/// Gets a TreeFactory object.
		/// </summary>
		/// <returns>The factory.</returns>
		public static TreeFactory GetFactory () {
			GameObject go = new GameObject ("Broccoli Tree Factory");
			TreeFactory treeFactory = go.AddComponent<Broccoli.Factory.TreeFactory> ();
			return treeFactory;
		}
		/// <summary>
		/// Loads a pipeline from an asset path.
		/// </summary>
		/// <returns>The pipeline if valid, otherwise null.</returns>
		/// <param name="pathToResource">Path to resource pipeline file. 
		/// (ex: Assets/Resources/MyPath/MyPipeline.asset would be MyPath/MyPipeline).</param>
		public Pipeline LoadPipeline (string pathToResource) {
			if (_localPipeline != null) {
				UnloadAndClearPipeline ();
				_localPipeline = null;
			}
			Pipeline loadedPipeline = null;;
			if (string.IsNullOrEmpty(pathToResource)) {
				Debug.LogWarning ("Could not load pipeline, path to pipeline asset is empty or null.");
			} else {
				loadedPipeline = Resources.Load<Pipeline> (pathToResource);
				if (loadedPipeline == null) {
					Debug.LogWarning ("Could not load pipeline at: " + pathToResource + ". Please make sure " +
						"the file exists and you are using a Resources valid file path format.");
				} else {
					loadedPipeline.OnAfterDeserialize ();
					LoadPipeline (loadedPipeline.Clone (), false);
					ValidatePipeline ();
				}
			}
			return _localPipeline;
		}
		/// <summary>
		/// Spawns vegetation from a pipeline.
		/// </summary>
		public GameObject Spawn () {
			_localPipeline.GenerateSeed ();
			return Spawn (_localPipeline.seed);
		}
		/// <summary>
		/// Spawns vegetation from a pipeline given a seed.
		/// </summary>
		/// <param name="seed">Seed.</param>
		public GameObject Spawn (int seed) {
			GameObject treeGO;
			if (_localPipeline != null) {
				/*
				int pass = 2;
				if (_localPipeline.treeFactoryPreferences.prefabStrictLowPoly) {
					pass = 1;
				}
				*/
				processType = ProcessType.Runtime;
				_localPipeline.seed = seed;
				InitPreviewTree ();
				ProcessPipeline (_localPipeline, _previewTree, 
					treeFactoryPreferences.previewLODIndex, null, true, false);
				treeGO = _previewTree.obj;
				treeGO.transform.parent = null;
				treeGO.name = "BroccoTree_" + spawnCount;
				treeGO.hideFlags = HideFlags.None;
				spawnCount++;
				_previewTree = new BroccoTree ();
				#if UNITY_EDITOR
				DestroyImmediate (_previewTree.obj);
				int childs = treeGO.transform.childCount;
				for (int i = 0; i < childs; i++) {
					DestroyImmediate (treeGO.transform.GetChild (i).gameObject);
				}
				#else
				Destroy (_previewTree.obj);
				int childs = treeGO.transform.childCount;
				for (int i = 0; i < childs; i++) {
				Destroy (treeGO.transform.GetChild (i).gameObject);
				}
				#endif
			} else {
				treeGO = new GameObject ();
				Debug.LogWarning ("Could not find a valid pipeline to spawn a Game Object.");
			}
			return treeGO;
		}
		#endregion

		#region Messages
		private static string MSG_PIPELINE_EMPTY = "Pipeline is empty. A full traversable pipeline is needed to create trees.";
		private static string MSG_PIPELINE_NO_SOURCE = "Pipeline has no source element.";
		private static string MSG_PIPELINE_NO_SINK = "Pipeline has no sink element connected.";
		private static string MSG_PIPELINE_MULTI_ELEMENT = "Some element appear more than one (allowed) time on a pipeline.";
		private static string MSG_PIPELINE_MULTIPLE = "More than one pipeline has been found for this factory." +
			"Please make sure only one completely connected pipeline is available.";
		private static string MSG_PIPELINE_INVALID_CONNECTION = "Some invalid connections found, please correct them.";
		private static string MSG_MESHDATA_UNASSIGNED = "Found some submeshes without data associated to them on the final merged mesh.";
		private static string MSG_MATERIALS_UNASSIGNED = "Some submeshes (sprout or branch) have no material associated.";
		private static string MSG_CUSTOM_MATERIAL_NO_WINDZONE = "This tree have custom materials. WindZone support depends on the material itself.";
		#endregion

		#region Events
		/// <summary>
		/// Raises the enable event.
		/// </summary>
		public void OnEnable () {
			Init ();
			#if UNITY_EDITOR
			EditorApplication.update += OnEditorUpdate;
			#endif
		}
		/// <summary>
		/// Raises the disable event.
		/// </summary>
		public void OnDisable () {
			#if UNITY_EDITOR
			EditorApplication.update -= OnEditorUpdate;
			#endif
		}
		/// <summary>
		/// Raises the editor update event.
		/// </summary>
		void OnEditorUpdate () {
			if (secondsToUpdatePipeline > 0) {
				SetEditorDeltaTime();
				secondsToUpdatePipeline -= (float) editorDeltaTime;
				if (secondsToUpdatePipeline < 0) {
					ProcessPipelinePreview (null, true);
					secondsToUpdatePipeline = 0;
				}
			}
			/*
			if (secondsToSecondPass > 0) {
				SetEditorDeltaTime();
				secondsToSecondPass -= (float) editorDeltaTime;
				if (secondsToSecondPass < 0) {
					ProcessPipeline (_localPipeline, _previewTree, secondPassReferenceElement, true, 2);
					secondsToSecondPass = 0;
				}
			}
			*/
		}
		/// <summary>
		/// Sets the editor delta time.
		/// </summary>
		private void SetEditorDeltaTime ()
		{
			#if UNITY_EDITOR
			if (lastTimeSinceStartup == 0f)
			{
				lastTimeSinceStartup = EditorApplication.timeSinceStartup;
			}
			editorDeltaTime = EditorApplication.timeSinceStartup - lastTimeSinceStartup;
			lastTimeSinceStartup = EditorApplication.timeSinceStartup;
			#endif
		}
		/// <summary>
		/// Sets the instance as the accessible singleton.
		/// </summary>
		public void SetInstanceAsActive () {
			TreeFactory._treeFactory = this;
		}
		/// <summary>
		/// Start this instance.
		/// </summary>
		private void Start () {
			Init ();
		}
		/// <summary>
		/// Update this instance.
		/// </summary>
		private void Update () {
			if (this.transform.hasChanged) {
				if (_localPipeline != null)
					_localPipeline.origin = this.transform.position;
				/*
				if (this.transform.eulerAngles != Vector3.zero) {
					this.transform.eulerAngles = Vector3.zero;
				}
				*/
				if (this.transform.localScale != Vector3.one) {
					this.transform.localScale = Vector3.one;
				}
			}
		}
		/// <summary>
		/// Raises the destroy event.
		/// </summary>
		void OnDestroy () {
			if (_localPipeline != null) {
				DestroyImmediate (_localPipeline);
			}
			DestroyPreviewTree ();
			DestroySproutFactory ();
			treeFactoryPreferences = null;
			log.Clear ();
		}
		/// <summary>
		/// Init this instance.
		/// </summary>
		public void Init () {
			if (!isInit) {
				if (_localPipeline == null) {
					_localPipeline = ScriptableObject.CreateInstance<Pipeline> ();
				}
				ValidatePipeline ();
				ExtensionManager.Init ();
				isInit = true;
			}
		}
		/// <summary>
		/// Raises the application quit event.
		/// </summary>
		public void OnApplicationQuit () {}
		#endregion

		#region Sprout Factory
		/// <summary>
		/// Initialize the preview tree.
		/// </summary>
		public SproutSubfactory GetSproutFactory () {
			if (_sproutFactory == null) {
				_sproutFactory = new SproutSubfactory ();
			}
			if (!_sproutFactory.HasValidTreeFactory ()) {
				Transform sproutFactoryTransform = this.transform.Find ("sproutFactory");
				if (!sproutFactoryTransform) {
					GameObject sproutFactoryObj = new GameObject ();
					sproutFactoryObj.name = "sproutFactory";
					sproutFactoryObj.transform.SetParent (this.transform);
					sproutFactoryObj.transform.localPosition = Vector3.zero;
					TreeFactory sproutTreeFactory = sproutFactoryObj.AddComponent<TreeFactory> ();
					sproutTreeFactory.buildPreviewTreeVisible = GlobalSettings.showPreviewTreeInHierarchy;
					_sproutFactory.Init (sproutTreeFactory);
					if (GlobalSettings.showSproutLabTreeFactoryInHierarchy) {
						sproutFactoryObj.hideFlags = HideFlags.None;
					} else {
						sproutFactoryObj.hideFlags = HideFlags.HideInHierarchy;
					}
				} else {
					TreeFactory sproutTreeFactory = sproutFactoryTransform.gameObject.GetComponent<TreeFactory> ();
					sproutTreeFactory.buildPreviewTreeVisible = GlobalSettings.showPreviewTreeInHierarchy;
					_sproutFactory.Init (sproutTreeFactory);
					if (GlobalSettings.showSproutLabTreeFactoryInHierarchy) {
						sproutFactoryTransform.gameObject.hideFlags = HideFlags.None;
					} else {
						sproutFactoryTransform.gameObject.hideFlags = HideFlags.HideInHierarchy;
					}
				}
			}
			return _sproutFactory;
		}
		/// <summary>
		/// Destroys the preview tree.
		/// </summary>
		public void DestroySproutFactory () {
			if (_sproutFactory != null) {
				_sproutFactory.Clear ();
				Transform sproutFactoryTransform = this.transform.Find ("sproutFactory");
				if (sproutFactoryTransform != null) {
					DestroyImmediate (sproutFactoryTransform.gameObject);
				}
				_sproutFactory = null;
			}
		}
		#endregion

		#region Preview Tree
		/// <summary>
		/// Initialize the preview tree.
		/// </summary>
		private Broccoli.Model.BroccoTree InitPreviewTree () {
			if (_previewTree == null) {
				_previewTree = new BroccoTree ();
				DestroyImmediate (_previewTree.obj);
			} else {
				Transform previewTreeTransform = this.transform.Find ("previewTree");
				if (!previewTreeTransform) {
					if (_previewTree.obj == null) {
						_previewTree.obj = new GameObject ();
					} else {
						CleanTreeGameObject (_previewTree);
					}
					_previewTree.obj.name = "previewTree";
					_previewTree.obj.transform.SetParent (this.transform);
					_previewTree.obj.transform.localPosition = Vector3.zero;
					BroccoTreeController broccoTreeController = _previewTree.obj.AddComponent<Broccoli.Controller.BroccoTreeController> ();
					broccoTreeController.shaderType = (BroccoTreeController.ShaderType)MaterialManager.leavesShaderType;
					broccoTreeController.windScale = treeFactoryPreferences.factoryScale;

					// Set Wind
					WindEffectElement windEffectElement= (WindEffectElement)localPipeline.GetElement (PipelineElement.ClassType.WindEffect, true);
					if (windEffectElement && windEffectElement.isActive) {
						broccoTreeController.localWindAmplitude = windEffectElement.windAmplitude;
						broccoTreeController.sproutTurbulance = windEffectElement.sproutTurbulence;
						broccoTreeController.sproutSway = windEffectElement.sproutSway;
					}

					if (GlobalSettings.showPreviewTreeInHierarchy) {
						_previewTree.obj.hideFlags = HideFlags.None;
					} else {
						_previewTree.obj.hideFlags = HideFlags.HideInHierarchy;
					}
					_previewTree.obj.SetActive (buildPreviewTreeVisible);
				} else if (_previewTree.obj == null || previewTreeTransform != _previewTree.obj.transform) {
					CleanTreeGameObject (_previewTree);
					_previewTree.obj = previewTreeTransform.gameObject;
					if (GlobalSettings.showPreviewTreeInHierarchy) {
						_previewTree.obj.hideFlags = HideFlags.None;
					} else {
						_previewTree.obj.hideFlags = HideFlags.HideInHierarchy;
					}
					_previewTree.obj.SetActive (buildPreviewTreeVisible);
				}
			}
			return _previewTree;
		}
		/// <summary>
		/// Destroys the preview tree.
		/// </summary>
		public void DestroyPreviewTree () {
			if (_previewTree != null) {
				_previewTree.Clear ();
				CleanTreeGameObject (_previewTree);
				if (_previewTree != null && _previewTree.obj != null) {
					DestroyImmediate (_previewTree.obj.GetComponent<Mesh>(), true);
					DestroyImmediate (_previewTree.obj.GetComponent<MeshRenderer>(), true);
					DestroyImmediate (_previewTree.obj.GetComponent<BroccoTreeController>(), true);
					DestroyImmediate (_previewTree.obj, true);
				}
				_previewTree = null;
				textureManager.Clear ();
				meshManager.Clear ();
				materialManager.Clear ();
			}
		}
		/// <summary>
		/// Cleans the preview tree game object.
		/// It destroys all children objects, materials and cleans the mesh.
		/// </summary>
		private void CleanTreeGameObject (BroccoTree treeToClean) {
			if (treeToClean != null && treeToClean.obj != null) {
				Mesh mesh = treeToClean.obj.GetComponent<Mesh> ();
				if (mesh != null) {
					mesh.Clear (false);
				}
				if (treeToClean.obj.transform.childCount > 0) {
					int i = 0;
					GameObject[] allChildren = new GameObject [treeToClean.obj.transform.childCount];

					var transformEnumerator = treeToClean.obj.transform.GetEnumerator ();
					while (transformEnumerator.MoveNext ()) {
						var child = (Transform)transformEnumerator.Current;
						allChildren [i] = child.gameObject;
						i += 1;
					}

					for (int j = 0; j < allChildren.Length; j++) {
						DestroyChildren (allChildren[j].gameObject);
						DestroyImmediate (allChildren[j].gameObject);
					}
				}
			}
		}
		/// <summary>
		/// Recursive destruction of children game objects.
		/// </summary>
		/// <param name="parent">Parent.</param>
		private void DestroyChildren (GameObject parent) {
			if (parent == null)
				return;
			int i = 0;
			GameObject[] allChildren = new GameObject[parent.transform.childCount];
			var transformEnumerator = parent.transform.GetEnumerator ();
			while (transformEnumerator.MoveNext ()) {
				var child = (Transform)transformEnumerator.Current;
				if (i < allChildren.Length) {
					allChildren [i] = child.gameObject;
				}
				i += 1;
			}
			for (int j = 0; j < allChildren.Length; j++) {
				if (allChildren[j] != null && allChildren[j].gameObject != parent) {
					DestroyChildren (allChildren[j].gameObject);
					DestroyImmediate (allChildren[j].gameObject);
				}
			}
		}
		/// <summary>
		/// Gets the offset of the last generated preview tree in world coordinates.
		/// </summary>
		/// <returns>The preview tree world offset.</returns>
		public Vector3 GetPreviewTreeWorldOffset () {
			return gameObject.transform.position;
		}
		#endregion

		#region Validation and Loading
		/// <summary>
		/// Validates the local pipeline.
		/// </summary>
		/// <returns><c>true</c>, if pipeline is valid, <c>false</c> otherwise.</returns>
		public bool ValidatePipeline () {
			return ValidatePipeline (_localPipeline);
		}
		/// <summary>
		/// Validates a given pipeline.
		/// </summary>
		/// <returns><c>true</c>, if pipeline is valid, <c>false</c> otherwise.</returns>
		/// <param name="pipeline">Pipeline to validate.</param>
		public bool ValidatePipeline (Broccoli.Pipe.Pipeline pipeline) {
			log.Clear ();
			pipeline.Update ();
			bool result = pipeline.Validate ();
			LogPipelineState (pipeline);
			return result;
		}
		/// <summary>
		/// Determines whether this TreeFactory has a valid pipeline.
		/// </summary>
		/// <returns><c>true</c> if this instance has a valid pipeline; otherwise, <c>false</c>.</returns>
		public bool HasValidPipeline () {
			if (_localPipeline == null) {
				return false;
			}
			return _localPipeline.IsValid ();
		}
		/// <summary>
		/// Loads a pipeline to the TreeFactory instance.
		/// </summary>
		/// <param name="pipeline">Pipeline to load.</param>
		/// <param name="buildPreview">If set to <c>true</c> a preview tree is build if a valid pipeline is loaded.</param>
		public void LoadPipeline (Pipeline pipeline, bool buildPreview) {
			LoadPipeline (pipeline, "", buildPreview);
		}
		/// <summary>
		/// Loads a pipeline to the TreeFactory instance.
		/// </summary>
		/// <param name="pipeline">Pipeline to load.</param>
		/// <param name="filePath">File path to the asset the pipeline comes from.</param>
		/// <param name="buildPreview">If set to <c>true</c> a preview tree is build if a valid pipeline is loaded.</param>
		/// <param name="useLastSeed">If set to <c>true</c> use last seed to generate the tree.</param>
		public void LoadPipeline (Pipeline pipeline, string filePath, bool buildPreview, bool useLastSeed = false) {
			_componentManager.Clear ();
			_meshManager.Clear ();
			_materialManager.Clear ();
			_assetManager.Clear ();
			if (pipeline != null && _localPipeline != null) {
				DestroyImmediate (_localPipeline, true);
			}
			_localPipeline = pipeline;
			_localPipeline.Init ();
			localPipelineFilepath = filePath;
			firstReprocess = 2;
			// Set tree factory preferences
			treeFactoryPreferences = pipeline.treeFactoryPreferences.Clone ();
			if (buildPreview) {
				DestroyPreviewTree ();
				if (!useLastSeed) {
					InitPreviewTree ();
				}
				ProcessPipeline (_localPipeline, _previewTree,
					treeFactoryPreferences.previewLODIndex);
			}
		}
		/// <summary>
		/// Unloads a pipeline from this factory and destroys its elements.
		/// </summary>
		public void UnloadAndClearPipeline () {
			_localPipeline.RemoveAllElements ();
		}
		#endregion

		#region Pipeline Processing
		/// <summary>
		/// Processes the pipeline to create a preview tree or a prefab.
		/// </summary>
		/// <returns><c>true</c>, if pipeline was processed, <c>false</c> otherwise.</returns>
		/// <param name="pipeline">Pipeline to process.</param>
		/// <param name="tree">Tree object to receive the processing.</param>
		/// <param name="referenceElement">Reference element to trigger the process from.</param>
		/// <param name="useCache">If set to <c>true</c> the elements before the reference element are allowed to use cached data.</param>
		/// <param name="secondPass">If set to <c>true</c> is a preview with more details</param>
		/// <param name="forceNewTree">If set to <c>true</c> the preview tree is built anew.</param>
		public bool ProcessPipeline (Broccoli.Pipe.Pipeline pipeline, 
			BroccoTree tree,
			int lodIndex,
			PipelineElement referenceElement = null, 
			bool useCache = false, 
			bool forceNewTree = false)
		{
			//UnityEngine.Profiling.Profiler.BeginSample("ProcessPipeline");
			if (onBeforeProcessPipeline != null) {
				onBeforeProcessPipeline (pipeline, tree, lodIndex, referenceElement, useCache, forceNewTree);
			}
			bool result = false;
			lastUndoProcessed = pipeline.undoControl.undoCount;
			if ((tree == null || tree.obj == null)/* && firstReprocess > 0*/) {
				firstReprocess--;
				tree = InitPreviewTree ();
				forceNewTree = true;
				//useCache = true;
			} else {
				tree.Clear ();
			}
			if (referenceElement == null && !useCache) {
				forceNewTree = true;
			}
			if (forceNewTree) {
				_componentManager.CallClearOnComponents ();
				_meshManager.Clear ();
				_materialManager.Clear ();
				CleanTreeGameObject (tree);
				InitPreviewTree ();
			}
			bool isStructureComplete = false;
			bool isMeshComplete = false;
			bool isMappingComplete = false;

			_componentManager.BeginUsage ();
			_meshManager.BeginUsage ();
			_materialManager.BeginUsage (treeFactoryPreferences);
			if (pipeline != null) {
				if (ValidatePipeline (pipeline)) {
					// Save the current random state.
					Random.State randomState = Random.state;
					if (!useCache) {
						pipeline.GenerateSeed ();
					}
					PipelineElement pipelineElement = pipeline.root;
					TreeFactoryProcessControl processControl = new TreeFactoryProcessControl (
						referenceElement, 
						(TreeFactoryProcessControl.ProcessType)processType, 
						lodIndex);
					do {
						if (pipelineElement.isActive && pipelineElement.hasChanged) {
							ProcessPipelineElement (pipelineElement, tree, referenceElement, useCache, processControl);
						} else {
							UnprocessPipelineElement (pipelineElement, tree);
						}
						pipelineElement = pipelineElement.sinkElement;

						// Check for processing events.
						if (pipelineElement != null) {
							if (pipelineElement.positionWeight >= PipelineElement.meshGeneratorWeight && !isStructureComplete) {
								isStructureComplete = true;
								OnStructureComplete (pipeline, tree);
							}
							if (pipelineElement.positionWeight >= PipelineElement.mapperWeight && !isMeshComplete) {
								isMeshComplete = true;
								OnMeshComplete (pipeline, tree);
							}
							if (pipelineElement.positionWeight >= PipelineElement.effectWeight && !isMappingComplete) {
								isMappingComplete = true;
								OnMappingComplete (pipeline, tree);
							}
						}
					} while (pipelineElement != null);
					result = true;
					// Restore the random state.
					Random.state = randomState;
				}
			}
			_meshManager.EndUsage ();
			ProcessMesh (tree);
			_materialManager.EndUsage ();
			ProcessMaterials (tree);
			_componentManager.EndUsage ();

			// Set second call if needed.
			/*
			if (isPreviewProcess && !treeFactoryPreferences.prefabStrictLowPoly && pass == 1) {
				secondsToSecondPass = 2f;
				secondPassReferenceElement = referenceElement;
				SetEditorDeltaTime ();
			} else {
				secondPassReferenceElement = null;
			}

			lastPass = pass;
			*/

			if (onProcessPipeline != null) {
				onProcessPipeline (pipeline, tree, lodIndex, referenceElement, useCache, forceNewTree);
			}

			BroccoTreeController broccoTreeController = _previewTree.obj.GetComponent<Broccoli.Controller.BroccoTreeController> ();
			if (broccoTreeController != null) {
				broccoTreeController.shaderType = (BroccoTreeController.ShaderType)MaterialManager.leavesShaderType;
				broccoTreeController.windScale = treeFactoryPreferences.factoryScale;
				broccoTreeController.Start ();
			}
			//UnityEngine.Profiling.Profiler.EndSample();

			return result;
		}
		/// <summary>
		/// Processes the pipeline to create a preview tree.
		/// </summary>
		/// <returns><c>true</c>, if pipeline was processed, <c>false</c> otherwise.</returns>
		/// <param name="referenceElement">Reference element to trigger the process from.</param>
		/// <param name="useCache">If set to <c>true</c> the elements before the reference element are allowed to use cached data.</param>
		public bool ProcessPipelinePreview (PipelineElement referenceElement = null, bool useCache = false, bool forceRebuild = false) {
			InitPreviewTree ();
			processType = ProcessType.Preview;
			if (onBeforeProcessPipelinePreview != null) {
				onBeforeProcessPipelinePreview (_localPipeline, _previewTree, 
					treeFactoryPreferences.previewLODIndex, referenceElement, useCache, forceRebuild);
			}
			bool result = ProcessPipeline (_localPipeline, _previewTree, 
				treeFactoryPreferences.previewLODIndex, referenceElement, useCache, forceRebuild);
			if (onProcessPipelinePreview != null) {
				onProcessPipelinePreview (_localPipeline, _previewTree, 
					treeFactoryPreferences.previewLODIndex, referenceElement, useCache, forceRebuild);
			}
			return result;
		}
		/// <summary>
		/// Requests a pipeline update.
		/// </summary>
		public void RequestPipelineUpdate () {
			this.secondsToUpdatePipeline = 0.7f;
			SetEditorDeltaTime();
		}
		/// <summary>
		/// Processes the pipeline from a reference PipelineElement downstream.
		/// </summary>
		/// <returns><c>true</c>, if pipeline was processed, <c>false</c> otherwise.</returns>
		/// <param name="referenceElement">Reference pipeline element.</param>
		/// <param name="useCache">If set to <c>true</c> use cache.</param>
		public bool ProcessPipelinePreviewDownstream (PipelineElement referenceElement, bool useCache = false) {
			processType = ProcessType.Preview;
			int lodIndex = treeFactoryPreferences.previewLODIndex;
			if (onBeforeProcessPipelinePreview != null) {
				onBeforeProcessPipelinePreview (_localPipeline, _previewTree, lodIndex, referenceElement, useCache);
			}
			bool result = ProcessPipeline(_localPipeline, _previewTree, lodIndex, referenceElement, useCache);
			if (onProcessPipelinePreview != null) {
				onProcessPipelinePreview (_localPipeline, _previewTree, lodIndex, referenceElement, useCache);
			}
			return result;
		}
		/// <summary>
		/// Process the pipeline from an element upstream a reference element if it exists.
		/// </summary>
		/// <returns><c>true</c>, if pipeline preview from upstream was processed, <c>false</c> otherwise.</returns>
		/// <param name="referenceElement">Reference pipeline element.</param>
		/// <param name="classType">Class type.</param>
		/// <param name="useCache">If set to <c>true</c> use cache.</param>
		public bool ProcessPipelinePreviewFromUpstream (PipelineElement referenceElement, 
			PipelineElement.ClassType classType, 
			bool useCache = false) 
		{
			PipelineElement upstreamElement = referenceElement.GetUpstreamElement (classType);
			if (upstreamElement != null) {
				processType = ProcessType.Preview;
				int lodIndex = treeFactoryPreferences.previewLODIndex;
				if (onBeforeProcessPipelinePreview != null) {
					onBeforeProcessPipelinePreview (_localPipeline, _previewTree, lodIndex, referenceElement, useCache);
				}
				bool result = ProcessPipeline(_localPipeline, _previewTree, lodIndex, upstreamElement, useCache);
				if (onProcessPipelinePreview != null) {
					onProcessPipelinePreview (_localPipeline, _previewTree, lodIndex, referenceElement, useCache);
				}
				return result;
			}
			return false;
		}
		/// <summary>
		/// Processes a pipeline element on a valid pipeline. The element may consult data from other pipeline
		/// elements either downstream or upstream the pipeline, as well generate its own data.
		/// </summary>
		/// <returns><c>true</c>, if pipeline element was processed, <c>false</c> otherwise.</returns>
		/// <param name="pipelineElement">Pipeline element to process.</param>
		/// <param name="tree">Tree object to receive the processing.</param>
		/// <param name="referenceElement">Reference element if the processing of the pipeline was triggered by it.</param>
		/// <param name="useCache">If set to <c>true</c> use cache.</param>
		/// <param name="processControl">Cache control object.</param>
		private bool ProcessPipelineElement (PipelineElement pipelineElement, 
			BroccoTree tree, 
			PipelineElement referenceElement, 
			bool useCache, 
			TreeFactoryProcessControl processControl)
		{
			_treeFactory = this;
			TreeFactoryComponent factoryComponent = 
				_componentManager.GetFactoryComponent (pipelineElement);
			factoryComponent.pipelineElement = pipelineElement;
			factoryComponent.tree = tree;

			if (pipelineElement == referenceElement) {
				factoryComponent.Process (this, useCache, true, processControl);
				processControl.AddChangedAspects (factoryComponent.GetChangedAspects ());
			} else {
				factoryComponent.Process (this, useCache, false, processControl);
			}

			factoryComponent.pipelineElement = null;
			factoryComponent.tree = null;

			return true;
		}
		/// <summary>
		/// Processes the pipeline based on one component.
		/// </summary>
		/// <returns><c>true</c>, if pipeline component was processed, <c>false</c> otherwise.</returns>
		/// <param name="pipelineElement">Pipeline element.</param>
		public bool ProcessPipelineComponent (PipelineElement pipelineElement, int cmd) {
			return ProcessPipelineComponent (pipelineElement, cmd, _previewTree);
		}
		/// <summary>
		/// Processes the pipeline based on one component.
		/// </summary>
		/// <returns><c>true</c>, if pipeline component was processed, <c>false</c> otherwise.</returns>
		/// <param name="pipelineElement">Pipeline element.</param>
		/// <param name="tree">Tree.</param>
		private bool ProcessPipelineComponent (PipelineElement pipelineElement, int cmd, BroccoTree tree) {
			TreeFactoryComponent factoryComponent = 
				_componentManager.GetFactoryComponent (pipelineElement);
			factoryComponent.pipelineElement = pipelineElement;
			factoryComponent.tree = tree;
			factoryComponent.ProcessComponentOnly (cmd, this);
			lastUndoProcessed = pipelineElement.pipeline.undoControl.undoCount;
			return true;
		}
		/// <summary>
		/// Unprocesses a pipeline element removing owned data from the tree object mainly.
		/// </summary>
		/// <param name="pipelineElement">Pipeline element to unprocess.</param>
		/// <param name="tree">Tree object to remove the data.</param>
		private void UnprocessPipelineElement (PipelineElement pipelineElement, 
			BroccoTree tree) {
			TreeFactoryComponent factoryComponent = 
				_componentManager.GetFactoryComponent (pipelineElement);
			factoryComponent.pipelineElement = pipelineElement;
			factoryComponent.tree = tree;
			factoryComponent.Unprocess (this);
		}
		#endregion

		#region Pipeline Processing Events
		/// <summary>
		/// Raises the structure complete event.
		/// </summary>
		/// <param name="pipeline">Pipeline.</param>
		/// <param name="tree">Tree.</param>
		public void OnStructureComplete (Pipeline pipeline, BroccoTree tree) {
			tree.UpdateResolution (25);
			tree.Update ();
			tree.UpdateFollowUps ();
			tree.CalculateSprouts ();
		}
		/// <summary>
		/// Raises the mesh complete event.
		/// </summary>
		/// <param name="pipeline">Pipeline.</param>
		/// <param name="tree">Tree.</param>
		public void OnMeshComplete (Pipeline pipeline, BroccoTree tree) {
		}
		/// <summary>
		/// Raises the mapping complete event.
		/// </summary>
		/// <param name="pipeline">Pipeline.</param>
		/// <param name="tree">Tree.</param>
		public void OnMappingComplete (Pipeline pipeline, BroccoTree tree) {
		}
		#endregion

		#region Prefab Creation
		/// <summary>
		/// Creates a prefab out of a processed pipeline.
		/// </summary>
		/// <returns><c>true</c>, if the prefab was created, <c>false</c> otherwise.</returns>
		public bool CreatePrefab () {
			#if UNITY_EDITOR
			processType = ProcessType.Prefab;
			if (localPipeline != null && ValidatePipeline (localPipeline)) {
				PipelineElement pipelineElement = localPipeline.root;

				// Calculate process weight to inform the user.
				BeginPrefabProgress ();

				// Create the prefab main GameObject.
				_assetManager.onLODReady += OnAssetManagerLODReady;
				string prefabSuffix = Broccoli.Utils.FileUtils.GetNumericSuffix (GlobalSettings.prefabSavePath,
					GlobalSettings.prefabSavePrefix, "prefab");
				_assetManager.BeginWithCreatePrefab (previewTree.obj,
					GlobalSettings.prefabSavePath, GlobalSettings.prefabSavePrefix + prefabSuffix);

				// Enables apply offset to vertices on the asset manager if the conditions are met.
				if (treeFactoryPreferences.prefabRepositionEnabled) {
					if (pipelineElement is IStructureGenerator) {
						IStructureGeneratorComponent structureGenerator = 
							(IStructureGeneratorComponent) componentManager.GetFactoryComponent (pipelineElement);
						if (structureGenerator.GetAvailableRootPositions () == 1 &&
							structureGenerator.GetUniqueRootPosition () != Vector3.zero) {
							_assetManager.applyVerticesOffset = true;
							_assetManager.verticesOffset = structureGenerator.GetUniqueRootPosition ();
						}
					}
				}

				// Add the submeshes (according to their processing pass) to the asset manager.
				/*
				if (treeFactoryPreferences.prefabStrictLowPoly) {
					if (lastPass != 1) {
						ProcessPipeline (localPipeline, previewTree, null, true, 1); // TODO: process from first mesh element.
					}
					_assetManager.AddSubmeshes (_meshManager.GetSubmeshes (), 1);
				} else {
					if (treeFactoryPreferences.prefabUseLODGroups) {
						_assetManager.AddSubmeshes (_meshManager.GetSubmeshes (), lastPass);
						int nextLOD = 2;
						if (lastPass == 2) {
							nextLOD = 1;
						}
						ProcessPipeline (localPipeline, previewTree, null, true, nextLOD); // TODO: process from first mesh element.
						_assetManager.AddSubmeshes (_meshManager.GetSubmeshes (), nextLOD);
					} else {
						if (lastPass != 2) {
							ProcessPipeline (localPipeline, previewTree, null, true, 2);  // TODO: process from first mesh element.
						}
						_assetManager.AddSubmeshes (_meshManager.GetSubmeshes (), 2);
					}
				}
				*/
				// Get the LOD levels to process.
				List<int> lodIndex = new List<int> ();
				for (int i = 0; i < treeFactoryPreferences.lods.Count; i++) {
					if (treeFactoryPreferences.lods [i].includeInPrefab) {
						lodIndex.Add (i);
					}
				}
				// Process standard LOD or list of LODs.
				if (lodIndex.Count == 0) {
					ProcessPipeline (localPipeline, previewTree, -1, null, true);
					_assetManager.AddMeshToPrefab (_meshManager.GetSubmeshes (), 0, 0.7f);
				} else {
					for (int i = 0; i < lodIndex.Count; i++) {
						ProcessPipeline (localPipeline, previewTree, lodIndex[i], null, true);
						_assetManager.AddMeshToPrefab (_meshManager.GetSubmeshes (), i, treeFactoryPreferences.lods[lodIndex[i]].groupPercentage);
					}
				}

				// Traverse Pipeline and get components, call their OnProcessPrefab method.
				do {
					if (pipelineElement.isActive && _componentManager.HasFactoryComponent(pipelineElement)) {
						_componentManager.GetFactoryComponent (pipelineElement).OnProcessPrefab (this);
					}
					pipelineElement = pipelineElement.sinkElement;
				} while (pipelineElement != null);

				// Optimization, atlas creation.
				if (!treeFactoryPreferences.prefabCreateAtlas) {
					_assetManager.OptimizeOnGroups ();
				} else {
					InformPrefabProgress("Creating sprout textures atlas.", GetAtlasWeight (treeFactoryPreferences.atlasTextureSize));
					_assetManager.OptimizeForAtlas (GetAtlasSize (treeFactoryPreferences.atlasTextureSize));
					_assetManager.OptimizeOnGroups ();
				}

				// Colliders
				_assetManager.colliders = previewTree.obj.GetComponents<CapsuleCollider> ();

				// Call before commiting prefab
				if (onBeforeEndPrefabCommit != null) {
					onBeforeEndPrefabCommit (_localPipeline, _previewTree, "");
				}
				/*
				bool result = _assetManager.EndWithCommit (treeFactoryPreferences.prefabIncludeBillboard, 
					MaterialManager.leavesShaderType == MaterialManager.LeavesShaderType.TreeCreatorOrSimilar);
					*/
				bool result = _assetManager.EndWithCommit (treeFactoryPreferences.prefabIncludeBillboard, treeFactoryPreferences.prefabBillboardPercentage);
				_assetManager.onLODReady -= OnAssetManagerLODReady;
				if (result) {
					lastPrefabPath = _assetManager.prefabFullPath;
					if (onEndPrefabCommit != null) {
						onEndPrefabCommit (_localPipeline, _previewTree, lastPrefabPath);
					}
				}
				EndPrefabProgress ();

				Resources.UnloadUnusedAssets ();

				// Builds anew the preview tree (to unreference all materials used on the prefab).
				ProcessPipelinePreview (null, true, true);

				return result;
			}
			#endif
			return false;
		}
		/// <summary>
		/// Receives informs in weight on progress of the prefab creation.
		/// </summary>
		/// <param name="action">Description of the action being taken.</param>
		/// <param name="weight">Weight of the action.</param>
		public void InformPrefabProgress (string action, int weight) {
			prefabProcessAction = action;
			prefabProcessWeightCompleted += weight;
			#if UNITY_EDITOR
			// TODO: move to custom editor??
			UnityEditor.EditorUtility.DisplayProgressBar ("Creating Prefab", prefabProcessAction, prefabProcessWeightCompleted/(float)prefabProcessWeight);
			#endif
		}
		/// <summary>
		/// Begins informing on the prefab creation process.
		/// </summary>
		private void BeginPrefabProgress () {
			GetPrefabProcessWeight ();
			if (prefabProcessWeight > 0) {
				showPrefabProgressBar = true;
				prefabProcessWeightCompleted = 0;
				InformPrefabProgress ("Initializing prefab object.", 0);
			}
		}
		/// <summary>
		/// Ends informing on the prefab creation process.
		/// </summary>
		private void EndPrefabProgress () {
			//meshToMergedMesh.Clear ();
			showPrefabProgressBar = false;
			#if UNITY_EDITOR
			UnityEditor.EditorUtility.ClearProgressBar ();
			#endif
		}
		/// <summary>
		/// Gets an estimate of the weight of the prefab creation process.
		/// </summary>
		/// <returns>The prefab creation process weight.</returns>
		private int GetPrefabProcessWeight () {
			prefabProcessWeight = 0;
			if (_localPipeline != null && ValidatePipeline (_localPipeline)) {
				// Get weights from the pipeline.
				PipelineElement pipelineElement = _localPipeline.root;
				do {
					if (pipelineElement.isActive && _componentManager.HasFactoryComponent(pipelineElement)) {
						prefabProcessWeight += _componentManager.GetFactoryComponent (pipelineElement).GetProcessPrefabWeight(this);
					}
					pipelineElement = pipelineElement.sinkElement;
				} while (pipelineElement != null);
				// Get weight from this factory
				if (treeFactoryPreferences.prefabCreateAtlas) {
					prefabProcessWeight += GetAtlasWeight (treeFactoryPreferences.atlasTextureSize);
				}
			}
			return prefabProcessWeight;
		}
		/// <summary>
		/// Gets the atlas process weight.
		/// </summary>
		/// <returns>The atlas process weight.</returns>
		/// <param name="atlasTexureSize">Atlas texture size enum.</param>
		public int GetAtlasWeight (TextureSize atlasTextureSize) {
			if (atlasTextureSize == TextureSize._128px) {
				return 20;
			} else if (atlasTextureSize == TextureSize._256px) {
				return 40;
			} else if (atlasTextureSize == TextureSize._512px) {
				return 60;
			} else if (atlasTextureSize  == TextureSize._1024px) {
				return 90;
			} else {
				return 120;
			}
		}
		/// <summary>
		/// Gets the size of the atlas in pixels.
		/// </summary>
		/// <returns>The atlas size in pixels.</returns>
		/// <param name="atlasTextureSize">Atlas texture size enum.</param>
		public static int GetAtlasSize (TextureSize atlasTextureSize) {
			if (atlasTextureSize == TextureSize._128px) {
				return 128;
			} else if (atlasTextureSize == TextureSize._256px) {
				return 256;
			} else if (atlasTextureSize == TextureSize._512px) {
				return 512;
			} else if (atlasTextureSize  == TextureSize._1024px) {
				return 1024;
			} else {
				return 2048;
			}
		}
		#endregion

		#region Managers Ops
		/// <summary>
		/// Processes the materials for the tree.
		/// </summary>
		/// <param name="tree">Tree.</param>
		public void ProcessMaterials (BroccoTree tree) {
			MeshRenderer meshRenderer = GetMeshRenderer (tree);
			SetMaterials (meshRenderer, _meshManager.GetMeshesCount ());
		}
		/// <summary>
		/// Sets materials to a mesh renderer.
		/// </summary>
		/// <param name="meshRenderer">Mesh renderer.</param>
		/// <param name="neededMaterialsCount">Number of materials needed according to submeshes.</param>
		private void SetMaterials (MeshRenderer meshRenderer, int neededMaterialsCount) {
			Material[] materials = new Material[neededMaterialsCount];
			bool hasUnassignedMaterials = false;
			bool hasUnassignedMesh = false;
			for (int i = 0; i < neededMaterialsCount; i++) {
				int meshId = _meshManager.GetMergedMeshId (i);
				if ((treeFactoryPreferences.previewMode == PreviewMode.Colored || forcePreviewModeColored) && GlobalSettings.structureViewEnabled) {
					// Colored materials.
					MeshManager.MeshData meshData = _meshManager.GetMeshData (meshId);
					if (meshData != null) {
						if (meshData.type == MeshManager.MeshData.Type.Sprout) {
							// For sprout meshes.
							int groupId = _meshManager.GetMeshGroupId (meshId);
							materials [i] = _materialManager.GetColoredMaterial (
								_localPipeline.sproutGroups.GetSproutGroupColor (groupId), false, forcePreviewModeColored);
						} else {
							// For branch and custom meshes.
							materials [i] = _materialManager.GetColoredMaterial (true);
						}
						if (!_materialManager.HasMaterial (meshId)) {
							hasUnassignedMaterials = true;
						}
					} else {
						hasUnassignedMesh = true;
						materials [i] = _materialManager.GetColoredMaterial ();
					}
				} else {
					// Textured or custom materials.
					if (meshId >= 0 && _materialManager.HasMaterial (meshId) 
						&& _meshManager.HasMeshAndNotEmpty (meshId)) 
					{
						if (_materialManager.IsCustomMaterial (meshId)) {
							//hasCustomMaterials = true;
						}
						if (_materialManager.IsCustomMaterial (meshId)) {
							if (treeFactoryPreferences.overrideMaterialShaderEnabled) {
								bool isSprout = _meshManager.IsSproutMesh (meshId);
								materials [i] = _materialManager.GetOverridedMaterial (meshId, isSprout);
							} else {
								materials [i] = _materialManager.GetMaterial (meshId);
							}
						} else {
							materials [i] = _materialManager.GetMaterial (meshId, true);
						}
					} else {
						hasUnassignedMaterials = true;
						materials [i] = null;
					}
				}
			}
			/*
			if (hasCustomMaterials && (!treeFactoryPreferences.prefabCloneCustomMaterialEnabled || 
				!treeFactoryPreferences.overrideMaterialShaderEnabled)) {
				log.Enqueue (LogItem.GetInfoItem (MSG_CUSTOM_MATERIAL_NO_WINDZONE));
			}
			*/
			if (hasUnassignedMaterials) {
				log.Enqueue (LogItem.GetWarnItem (MSG_MATERIALS_UNASSIGNED));
			}
			if (hasUnassignedMesh) {
				log.Enqueue (LogItem.GetWarnItem (MSG_MESHDATA_UNASSIGNED));
			}
			meshRenderer.sharedMaterials = materials;
		}
		/// <summary>
		/// Processes the mesh for the tree.
		/// </summary>
		/// <param name="tree">Tree.</param>
		private void ProcessMesh (BroccoTree tree) {
			MeshFilter meshFilter= GetMeshFilter (tree);
			meshFilter.sharedMesh = _meshManager.MergeAll (meshFilter.transform);
		}
		/// <summary>
		/// Gets the mesh filter of the tree.
		/// </summary>
		/// <returns>The mesh filter.</returns>
		/// <param name="tree">Tree.</param>
		private MeshFilter GetMeshFilter (BroccoTree tree) {
			MeshFilter meshFilter = null;
			if (tree != null && tree.obj != null) {
				meshFilter = tree.obj.GetComponent<MeshFilter> ();
				if (meshFilter == null) {
					meshFilter = tree.obj.AddComponent<MeshFilter> ();
					MeshRenderer meshRenderer = tree.obj.AddComponent<MeshRenderer> ();
					meshRenderer.sharedMaterial = new Material (Shader.Find ("Diffuse"));
				}
			}
			return meshFilter;
		}
		/// <summary>
		/// Gets the mesh renderer of the tree.
		/// </summary>
		/// <returns>The mesh renderer.</returns>
		/// <param name="tree">Tree.</param>
		private MeshRenderer GetMeshRenderer (BroccoTree tree) {
			MeshRenderer meshRenderer = null;
			if (tree != null && tree.obj != null) {
				meshRenderer = tree.obj.GetComponent<MeshRenderer> ();
			}
			return meshRenderer;
		}
		/// <summary>
		/// Begins usage of a mesh collider component.
		/// </summary>
		public void BeginColliderUsage () {
			_meshCollider = GetComponent<MeshCollider> ();
			if (_meshCollider == null) {
				_meshCollider = gameObject.AddComponent<MeshCollider> ();
			} else {
				DestroyImmediate(_meshCollider.sharedMesh);
			}
		}
		/// <summary>
		/// Provides a mesh collider component to be used to calculate intersectipns with branches.
		/// </summary>
		/// <returns>MeshCollider component.</returns>
		public MeshCollider GetMeshCollider () {
			if (_meshCollider == null) {
				_meshCollider = GetComponent<MeshCollider> ();
			}
			return _meshCollider;
		}
		/// <summary>
		/// Ends usage of a mesh collider component.
		/// </summary>
		public void EndColliderUsage () {
			DestroyImmediate (_meshCollider);
		}
		#endregion

		#region Managers Events
		/// <summary>
		/// Called when creating a prefab a LOD GameObject is ready, right before it get added to the prefab game object.
		/// </summary>
		/// <param name="lodGO">LOD game object instance.</param>s
		void OnAssetManagerLODReady (GameObject lodGO) {
			if (onLODReady != null) onLODReady.Invoke (lodGO);
		}
		#endregion

		#region Log
		/// <summary>
		/// Logs the state of the pipeline.
		/// </summary>
		/// <param name="pipeline">Pipeline to analyze.</param>
		public void LogPipelineState (Pipeline pipeline) {
			if (pipeline.state != Pipeline.State.Valid) {
				switch (pipeline.state) {
				case Pipeline.State.Empty:
					log.Enqueue (LogItem.GetWarnItem (MSG_PIPELINE_EMPTY));
					break;
				case Pipeline.State.NoSourceElement:
					log.Enqueue (LogItem.GetErrorItem (MSG_PIPELINE_NO_SOURCE));
					break;
				case Pipeline.State.NoSinkElement:
					log.Enqueue (LogItem.GetErrorItem (MSG_PIPELINE_NO_SINK));
					break;
				case Pipeline.State.MultiElement:
					log.Enqueue (LogItem.GetErrorItem (MSG_PIPELINE_MULTI_ELEMENT));
					break;
				case Pipeline.State.MultiplePipelines:
					log.Enqueue (LogItem.GetErrorItem (MSG_PIPELINE_MULTIPLE));
					break;
				case Pipeline.State.InvalidConnection:
					log.Enqueue (LogItem.GetErrorItem (MSG_PIPELINE_INVALID_CONNECTION));
					break;
				}
			}
		}
		/// <summary>
		/// Adds an info entry to the log.
		/// </summary>
		/// <param name="msg">Message.</param>
		public void AddLogInfo (string msg) {
			log.Enqueue (LogItem.GetInfoItem (msg));
		}
		/// <summary>
		/// Adds a warning entry to the log.
		/// </summary>
		/// <param name="msg">Message.</param>
		public void AddLogWarn (string msg) {
			log.Enqueue (LogItem.GetWarnItem (msg));
		}
		/// <summary>
		/// Adds an error entry to the log.
		/// </summary>
		/// <param name="msg">Message.</param>
		public void AddLogError (string msg) {
			log.Enqueue (LogItem.GetErrorItem (msg));
		}
		#endregion
	}
}