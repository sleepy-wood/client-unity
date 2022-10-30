using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Broccoli.Base
{
	public static class GlobalSettings
	{
		#region Vars
		/// <summary>
		/// The gravity direction.
		/// </summary>
		public static Vector3 gravityDirection = Vector3.down;
		/// <summary>
		/// The against gravity direction.
		/// </summary>
		public static Vector3 againstGravityDirection = Vector3.up;
		/// <summary>
		/// Enables edit mode for catalog items.
		/// Changes on catalog pipeline assets can be saved.
		/// </summary>
		public static bool editCatalogEnabled = false;
		/// <summary>
		/// Show the preview tree in the factory game object hierarchy.
		/// </summary>
		public static bool showPreviewTreeInHierarchy = true;
		/// <summary>
		/// Shows the tree with special materials to display the tree structure.
		/// </summary>
		public static bool structureViewEnabled = true;
		/// <summary>
		/// When a new pipeline is requested on the factory load a template instead
		/// of an empty pipeline.
		/// </summary>
		public static bool useTemplateOnCreateNewPipeline = true;
		/// <summary>
		/// Let using the tree editor canvas when on play mode.
		/// </summary>
		public static bool useTreeEditorOnPlayMode = false;
		/// <summary>
		/// When loading a pipeline points the scene camera to the newly created preview tree.
		/// </summary>
		public static bool moveCameraToPipeline = true;
		/// <summary>
		/// Shows the GameObject with the TreeFactory component used by SproutLab.
		/// </summary>
		public static bool showSproutLabTreeFactoryInHierarchy = false;
		/// <summary>
		/// The experimental flag to merge nearby points to reduce vertex count.
		/// </summary>
		public static bool experimentalMergeCurvePointsByDistanceEnabled = false;
		/// <summary>
		/// The experimental flag to add probability to break a branch.
		/// </summary>
		public static bool experimentalBranchBreak = false;
		/// <summary>
		/// The experimental flag to weld branches 
		/// </summary>
		public static bool experimentalBranchWelding = true;
		/// <summary>
		/// Experimental flag to bake ambient occlusion in the tree mesh.
		/// </summary>
		public static bool experimentalAO = true;
		/// <summary>
		/// Experimental flag to use lab modes on the Sprout Lab Factory.
		/// </summary>
		public static bool experimentalAdvancedSproutLab = false;
		/// <summary>
		/// Path to the default pipeline to load when requesting a new pipeline.
		/// </summary>
		public static string templateOnCreateNewPipelinePath = "Editor/Resources/base_pipeline.asset";
		/// <summary>
		/// Path tothe default pipeline to use when building branches on sproutlab.
		/// </summary>
		public static string templateSproutLabPipelinePath = "Editor/Resources/sproutlab_base_pipeline.asset";
		/// <summary>
		/// The path for saving pipelines.
		/// </summary>
		public static string pipelineSavePath = "Saves/";
		/// <summary>
		/// Default path to save prefabs.A path to an existing folder must be provided.
		/// </summary>
		public static string prefabSavePath = "Assets";
		/// <summary>
		/// Default prefab filename prefix to save prefabs.
		/// </summary>
		public static string prefabSavePrefix = "BroccoTree";
		/// <summary>
		/// Default prefix for textures saved at a prefab folder.
		/// </summary>
		public static string prefabTexturesPrefix = "";
		/// <summary>
		/// Adds a controller to the created prefab.
		/// </summary>
		public static bool prefabAddController = true;
		#endregion

		#region Devel Vars
		public static bool useAutoCalculateTangents = false;
		public static bool useCrossMeshPerpendicularNormasl = false;
		public static Vector3 crossAPoint1 = Vector2.one;
		public static Vector3 crossAPoint2 = Vector2.one;
		public static Vector3 crossAPoint3 = Vector2.one;
		public static Vector3 crossAPoint4 = Vector2.one;
		public static Vector3 crossBPoint1 = Vector2.one;
		public static Vector3 crossBPoint2 = Vector2.one;
		public static Vector3 crossBPoint3 = Vector2.one;
		public static Vector3 crossBPoint4 = Vector2.one;
		public static Vector3 tangAPoint1 = Vector2.one;
		public static Vector3 tangAPoint2 = Vector2.one;
		public static Vector3 tangAPoint3 = Vector2.one;
		public static Vector3 tangAPoint4 = Vector2.one;
		public static Vector3 tangBPoint1 = Vector2.one;
		public static Vector3 tangBPoint2 = Vector2.one;
		public static Vector3 tangBPoint3 = Vector2.one;
		public static Vector3 tangBPoint4 = Vector2.one;
		#endregion

		#region Delays
		/// <summary>
		/// Delay (in seconds) to use for waiting on very low demanding processes 
		/// related to updating the preview tree.
		/// </summary>
		public static float processingDelayVeryLow = 0.025f;
		/// <summary>
		/// Delay (in seconds) to use for waiting on low demanding processes 
		/// related to updating the preview tree.
		/// </summary>
		public static float processingDelayLow = 0.05f;
		/// <summary>
		/// Delay (in seconds) to use for waiting on medium demanding processes 
		/// related to updating the preview tree.
		/// </summary>
		public static float processingDelayMedium = 0.075f;
		/// <summary>
		/// Delay (in seconds) to use for waiting on high demanding processes 
		/// related to updating the preview tree.
		/// </summary>
		public static float processingDelayHigh = 0.1f;
		/// <summary>
		/// Delay (in seconds) to use for waiting on very high demanding processes 
		/// related to updating the preview tree.
		/// </summary>
		public static float processingDelayVeryHigh = 0.125f;
		#endregion

		#region Gizmos/Handles
		/// <summary>
		/// The tree factory position gizmo size.
		/// </summary>
		public static float treeFactoryPositionGizmoSize = 0.1f;
		/// <summary>
		/// The length of the sprout gizmo.
		/// </summary>
		public static float sproutGizmoLength = 0.2f;
		/// <summary>
		/// The size of the bend point gizmo.
		/// </summary>
		public static float bendPointGizmoSize = 0.05f;
		/// <summary>
		/// Color used when drawing tree hierarchy on root branch.
		/// </summary>
		public static Color branchLineFromColor = Color.white;
		/// <summary>
		/// Color used when drawing tree hierarchy on the farest branches.
		/// </summary>
		public static Color branchLineToColor   = Color.green;
		/// <summary>
		/// Color used when drawing bend points.
		/// </summary>
		public static Color bendPointColor = Color.yellow;
		#endregion

		#region Debug
		/// <summary>
		/// Shows a button to print pipeline debugging information to the console.
		/// </summary>
		public static bool showPipelineDebugOption = false;
		/// <summary>
		/// The verbose level to print events happening on the pipeline processing.
		/// 0 = no printing, 1 = error, 2 = warning, 3 = debug, 4 = info.
		/// </summary>
		public static int verbose = 0;
		/// <summary>
		/// Verboses error messages.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="verboseLevel">Verbose level override.</param>
		public static void VerboseError (object message, int verboseLevel = -1) {
			if ((verboseLevel == -1 && verbose >= 1) || verboseLevel >= 1)
				Debug.LogError (message);
		}
		/// <summary>
		/// Verboses warning messages.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="verboseLevel">Verbose level override.</param>
		public static void VerboseWarning (object message, int verboseLevel = -1) {
			if ((verboseLevel == -1 && verbose >= 2) || verboseLevel >= 2)
				Debug.LogWarning (message);
		}
		/// <summary>
		/// Verboses debug messages.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="verboseLevel">Verbose level override.</param>
		public static void VerboseDebug (object message, int verboseLevel = -1) {
			if ((verboseLevel == -1 && verbose >= 3) || verboseLevel >= 3)
				Debug.Log (message);
		}
		/// <summary>
		/// Verboses info messages.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="verboseLevel">Verbose level override.</param>
		public static void VerboseInfo (object message, int verboseLevel = -1) {
			if ((verboseLevel == -1 && verbose >= 4) || verboseLevel >= 4)
				Debug.Log (message);
		}
		#endregion
	}
}