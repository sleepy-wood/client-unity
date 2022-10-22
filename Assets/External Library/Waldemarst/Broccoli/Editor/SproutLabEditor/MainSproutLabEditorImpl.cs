using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace Broccoli.BroccoEditor {
    [SproutLabEditorImpl (0)]
    public class MainSproutLabEditorImpl : ISproutLabEditorImpl
    {
        #region Vars
        private SproutLabEditor sproutLabEditor;
        /// <summary>
        /// Free view zoom and rotation, starting from the side of the target.
        /// </summary>
        private SproutLabEditor.CanvasSettings sideViewCanvasSettings = null;
        /// <summary>
        /// Locked view, starting from the front (top) side of the target.
        /// </summary>
        private SproutLabEditor.CanvasSettings frontViewCanvasSettings = null;
        /// <summary>
        /// Debug canvas view.
        /// </summary>
        private SproutLabEditor.CanvasSettings debugViewCanvasSettings = null;
        /// <summary>
        /// Default structure settings.
        /// </summary>
        private SproutLabEditor.StructureSettings structureSettings = null;
        #endregion

        #region Messages
        private static int IMPL_BRANCH = 0;
        private static string MSG_HEADER = "Broccoli Tree Creator v1.1.0";
        private static string MSG_PREVIEW_TITLE = "Branch Collection Descriptor";
        private static string MSG_CREATE_BRANCH = "Create a collection of branch descriptors to generate Textures and/or Meshes to be used on a Broccoli Tree Factory.";
        private static string MSG_CREATE_FROM_TEMPLATE = "Open a template from the Catalog Collection to begin working on it.";
        #endregion

        #region Init
        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public void Initialize (SproutLabEditor sproutLabEditor) {
            this.sproutLabEditor = sproutLabEditor;
            if (sideViewCanvasSettings == null) {
                sideViewCanvasSettings = new SproutLabEditor.CanvasSettings ();
                sideViewCanvasSettings.id = 1;
                sideViewCanvasSettings.viewOffset = new Vector3 (0.3f, -0.2f, -5.5f);
                sideViewCanvasSettings.viewDirection = new Vector2 (220f, -50f);
                sideViewCanvasSettings.viewTargetRotation = Quaternion.Euler (0f, 0f, 90f);
            }
            if (frontViewCanvasSettings == null) {
                frontViewCanvasSettings = new SproutLabEditor.CanvasSettings ();
                frontViewCanvasSettings.id = 2;
                frontViewCanvasSettings.freeViewEnabled = false;
            }
            if (debugViewCanvasSettings == null) {
                debugViewCanvasSettings = new SproutLabEditor.CanvasSettings ();
                debugViewCanvasSettings.resetView = false;
            }
            if (structureSettings == null) {
                structureSettings = new SproutLabEditor.StructureSettings ();
                structureSettings.id = 1;
            }
        }
        #endregion

        #region Info
        /// <summary>
        /// Get the ids for the implementations handled by this editor. 
        /// Id should be different for each implementation.
        /// </summary>
        /// <value>Ids of the implementations handled by this editor.</value>
        public int[] implIds { get { return new int[1]{IMPL_BRANCH}; } }
        /// <summary>
        /// Gets the string to show in the editor header.
        /// </summary>
        /// <returns>Header message.</returns>
        public string GetHeaderMsg () {
            return MSG_HEADER;
        }
        /// <summary>
        /// Gets the title string for the preview mesh canvas.
        /// </summary>
        /// <param name="implId">Implementation id.</param>
        /// <returns>Mesh preview title.</returns>
        public string GetPreviewTitle (int implId) {
            return MSG_PREVIEW_TITLE;
        }
        /// <summary>
        /// Gets the canvas setting configuration
        /// </summary>
        /// <param name="panel">Panel index.</param>
        /// <param name="subPanel">Subpanel index.</param>
        /// <returns>Configuration to show the canvas.</returns>
        public SproutLabEditor.CanvasSettings GetCanvasSettings (int panel, int subPanel) {
            switch (panel) {
                case SproutLabEditor.PANEL_MAPPING:
                    return frontViewCanvasSettings;
                case SproutLabEditor.PANEL_DEBUG:
                    return debugViewCanvasSettings;
                case SproutLabEditor.PANEL_STRUCTURE:
                case SproutLabEditor.PANEL_TEXTURE:
                case SproutLabEditor.PANEL_EXPORT:
                default:
                    return sideViewCanvasSettings;
            }
        }
        /// <summary>
        /// Gets the structure settings to use on an implementation.
        /// </summary>
        /// <param name="impId">Id of the implementation.</param>
        /// <returns>Structure settings.</returns>
        public SproutLabEditor.StructureSettings GetStructureSettings (int impId) {
            return structureSettings;
        }
        #endregion

        #region Draw
        /// <summary>
        /// Draw the first options on the Select Mode View.
        /// </summary>
        public void DrawSelectModeViewBeforeOptions () {
            if (GUILayout.Button ("Create a Branch Collection")) {
				sproutLabEditor.SetBranchDescriptorCollectionImpl (IMPL_BRANCH);
			}
			EditorGUILayout.HelpBox (MSG_CREATE_BRANCH, MessageType.None);
			EditorGUILayout.Space ();
        }
        /// <summary>
        /// Draw the second options on the Select Mode View.
        /// </summary>
        public void DrawSelectModeViewAfterOptions () {
            if (GUILayout.Button ("Create Project from Template")) {
				sproutLabEditor.viewMode = SproutLabEditor.ViewMode.Templates;
			}
			EditorGUILayout.HelpBox (MSG_CREATE_FROM_TEMPLATE, MessageType.None);
        }
        #endregion
    }
}