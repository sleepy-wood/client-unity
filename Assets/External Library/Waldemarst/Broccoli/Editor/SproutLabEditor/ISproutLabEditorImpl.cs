using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Broccoli.BroccoEditor
{
    public interface ISproutLabEditorImpl {
        #region Initialization
        /// <summary>
        /// Initialize this instance.
        /// </summary>
        void Initialize (SproutLabEditor sproutLabEditor);
        #endregion

        #region Configuration
        /// <summary>
        /// Get the ids for the implementations handled by this editor. 
        /// Id should be different for each implementation.
        /// </summary>
        /// <value>Ids of the implementations handled by this editor.</value>
        int[] implIds { get; }
        /// <summary>
        /// Gets the string to show in the editor header.
        /// </summary>
        /// <returns>Header message.</returns>
        string GetHeaderMsg ();
        /// <summary>
        /// Gets the title string for the preview mesh canvas.
        /// </summary>
        /// <param name="implId">Implementation id.</param>
        /// <returns>Mesh preview title.</returns>
        string GetPreviewTitle (int implId);
        /// <summary>
        /// Gets the canvas setting configuration
        /// </summary>
        /// <param name="panel">Panel index.</param>
        /// <param name="subPanel">Subpanel index.</param>
        /// <returns>Configuration to show the canvas.</returns>
        SproutLabEditor.CanvasSettings GetCanvasSettings (int panel, int subPanel);
        /// <summary>
        /// Gets the structure settings to use on an implementation.
        /// </summary>
        /// <param name="impId">Id of the implementation.</param>
        /// <returns>Structure settings.</returns>
        SproutLabEditor.StructureSettings GetStructureSettings (int impId);
        #endregion

        #region Draw
        /// <summary>
        /// Draw the first options on the Select Mode View.
        /// </summary>
        void DrawSelectModeViewBeforeOptions ();
        /// <summary>
        /// Draw the second options on the Select Mode View.
        /// </summary>
        void DrawSelectModeViewAfterOptions ();
        #endregion
        //float health { get; set; } //A variable
        //void ApplyDamage(float points); //Function with one argument
    }
}