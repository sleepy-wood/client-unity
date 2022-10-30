using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace Broccoli.Examples 
{
    [CustomEditor(typeof(ModifyPipeline))]
    public class ModifyPipelineEditor : Editor
    {
        #region Vars
        ModifyPipeline modifyPipelineController;
        private static string DESC_MSG = "This editor contains code to modify a pipeline " +
        "element properties and connections to be used when creating trees dynamically.";
        #endregion

        #region Monobehaviour Events
        public void OnEnable ()
        {
		    modifyPipelineController = (ModifyPipeline)target;
        }
        public override void OnInspectorGUI () {
		serializedObject.Update ();

        // This script description.
        EditorGUILayout.HelpBox (DESC_MSG, MessageType.None);
        EditorGUILayout.Space ();

        // PROCESSING TREE FACTORY.
        if (GUILayout.Button ("Process Pipeline Preview")) {
            modifyPipelineController.ProcessPipeline ();
        }
        EditorGUILayout.Space ();

        float factoryScale = EditorGUILayout.Slider ("Factory Scale", modifyPipelineController.factoryScale, -1f, 4f);
        if (factoryScale != modifyPipelineController.factoryScale) {
            modifyPipelineController.SetFactoryScale (factoryScale);
        }
        EditorGUILayout.Space ();

        float girthValue = EditorGUILayout.DelayedFloatField ("Branch Girth", modifyPipelineController.girthValue);
        if (girthValue != modifyPipelineController.girthValue && girthValue >= 1f && girthValue <= 3f) {
            modifyPipelineController.SetGirth (girthValue);
        }
        EditorGUILayout.Space ();

        // Replace SproutMappers.
        EditorGUILayout.LabelField ("Current SproutMapper: " + 
            ModifyPipeline.sproutMapperKeyNames [modifyPipelineController.sproutMapperSelected]);
        if (GUILayout.Button ("Switch SproutMappers")) {
            modifyPipelineController.SwitchSproutMappers ();
        }

        // Toggles the SproutMeshGenerator element.
        EditorGUILayout.LabelField ("SproutMeshGenerator is: " + 
            (modifyPipelineController.isSproutMeshGeneratorActive?"Active":"Inactive"));
        if (GUILayout.Button ((modifyPipelineController.isSproutMeshGeneratorActive?"Deactivate":"Active") + " SproutMeshGenerator")) {
            modifyPipelineController.ToggleSproutMeshGenerator ();
        }

		serializedObject.ApplyModifiedProperties ();
	}
        #endregion
    }
}
