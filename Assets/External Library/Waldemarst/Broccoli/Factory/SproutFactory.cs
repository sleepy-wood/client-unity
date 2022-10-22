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
/// Working classes that generates sprout textures.
/// </summary>
namespace Broccoli.Factory
{
	using Pipeline = Broccoli.Pipe.Pipeline;
	/// <summary>
	/// Sprout factory class. Process a sprout collection to generate textures for Broccoli trees.
	/// </summary>
	[AddComponentMenu("Broccoli/Factory/SproutFactory")]
	[ExecuteInEditMode]
	public class SproutFactory : MonoBehaviour {
		#region Vars
        /// <summary>
		/// True if this instance has been initialized.
		/// </summary>
		bool isInit = false;
		[SerializeField]
		/// <summary>
		/// Branch collection description.
		/// </summary>
		BranchDescriptorCollection _branchDescriptorCollection;
		/// <summary>
		/// Pipeline containing the definition on how to build a tree.
		/// </summary>
		[SerializeField]
		Broccoli.Pipe.Pipeline _localPipeline;
		#endregion

		#region Subfactories
		/// <summary>
		/// The sprout subfactory.
		/// </summary>
		SproutSubfactory _sproutSubFactory = null;
		#endregion

		#region Accessors
		/// <summary>
		/// Gets the loaded BranchDescriptorCollection instance.
		/// </summary>
		/// <value>The local branch description collection.</value>
		public BranchDescriptorCollection branchDescriptorCollection {
			get { return _branchDescriptorCollection; }
			set {
				_branchDescriptorCollection = value;
			}
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
		#endregion

		#region Singleton
		/// <summary>
		/// Singleton.
		/// </summary>
		static SproutFactory _sproutFactory = null;
		/// <summary>
		/// Singleton accessor.
		/// </summary>
		/// <returns>The instance.</returns>
		public static SproutFactory GetActiveInstance () {
			return _sproutFactory;
		}
		/// <summary>
		/// Set this class singleton.
		/// </summary>
		/// <param name="sproutFactory">Tree factory instance to set as singleton for the class.</param>
		public static void SetActiveInstance (SproutFactory sproutFactory) {
			_sproutFactory = sproutFactory;
		}
		#endregion

		#region Editor extension
		#if UNITY_EDITOR
		[MenuItem("GameObject/Broccoli/Sprout Lab Factory", false, 20)]
		static void CreateSproutFactoryGameObject (MenuCommand menuCommand)
		{
			// Create a custom game object
			GameObject go = new GameObject("Sprout Lab Factory");
			// Ensure it gets reparented if this was a context click (otherwise does nothing)
			GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
			go.AddComponent<Broccoli.Factory.SproutFactory> ();
			Selection.activeObject = go;
			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty (
				UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene ());
		}
		#endif
		#endregion

		#region Events
		/// <summary>
		/// Sets the instance as the accessible singleton.
		/// </summary>
		public void SetInstanceAsActive () {
			SproutFactory._sproutFactory = this;
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
                /*
				if (_localPipeline != null)
					_localPipeline.origin = this.transform.position;
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
            /*
			if (_localPipeline != null) {
				DestroyImmediate (_localPipeline);
			}
			DestroyPreviewTree ();
			DestroySproutFactory ();
			treeFactoryPreferences = null;
			log.Clear ();
            */
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

		#region Sprout SubFactory
		/// <summary>
		/// Initialize the preview tree.
		/// </summary>
		public SproutSubfactory GetSproutSubFactory () {
			if (_sproutSubFactory == null) {
				_sproutSubFactory = new SproutSubfactory ();
			}
			if (!_sproutSubFactory.HasValidTreeFactory ()) {
				Transform sproutFactoryTransform = this.transform.Find ("sproutFactory");
				if (!sproutFactoryTransform) {
					GameObject sproutFactoryObj = new GameObject ();
					sproutFactoryObj.name = "sproutFactory";
					sproutFactoryObj.transform.SetParent (this.transform);
					sproutFactoryObj.transform.localPosition = Vector3.zero;
					TreeFactory sproutTreeFactory = sproutFactoryObj.AddComponent<TreeFactory> ();
					sproutTreeFactory.buildPreviewTreeVisible = GlobalSettings.showPreviewTreeInHierarchy;
					_sproutSubFactory.Init (sproutTreeFactory);
					if (GlobalSettings.showSproutLabTreeFactoryInHierarchy) {
						sproutFactoryObj.hideFlags = HideFlags.None;
					} else {
						sproutFactoryObj.hideFlags = HideFlags.HideInHierarchy;
					}
				} else {
					TreeFactory sproutTreeFactory = sproutFactoryTransform.gameObject.GetComponent<TreeFactory> ();
					sproutTreeFactory.buildPreviewTreeVisible = GlobalSettings.showPreviewTreeInHierarchy;
					_sproutSubFactory.Init (sproutTreeFactory);
					if (GlobalSettings.showSproutLabTreeFactoryInHierarchy) {
						sproutFactoryTransform.gameObject.hideFlags = HideFlags.None;
					} else {
						sproutFactoryTransform.gameObject.hideFlags = HideFlags.HideInHierarchy;
					}
				}
			}
			return _sproutSubFactory;
		}
		/// <summary>
		/// Destroys the preview tree.
		/// </summary>
		public void DestroySproutSubFactory () {
			if (_sproutSubFactory != null) {
				_sproutSubFactory.Clear ();
				Transform sproutFactoryTransform = this.transform.Find ("sproutFactory");
				if (sproutFactoryTransform != null) {
					DestroyImmediate (sproutFactoryTransform.gameObject);
				}
				_sproutSubFactory = null;
			}
		}
		#endregion

		#region Preview Tree
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
			//log.Clear ();
			pipeline.Update ();
			bool result = pipeline.Validate ();
			//LogPipelineState (pipeline);
			return result;
		}
		/// <summary>
		/// Determines whether this SproutFactory has a valid pipeline.
		/// </summary>
		/// <returns><c>true</c> if this instance has a valid pipeline; otherwise, <c>false</c>.</returns>
		public bool HasValidPipeline () {
			if (_localPipeline == null) {
				return false;
			}
			return _localPipeline.IsValid ();
		}
		#endregion
	}
}