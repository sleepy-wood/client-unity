using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

using Broccoli.Base;

namespace Broccoli.Catalog
{
	[CustomEditor(typeof(ShapePackage))]
	public class ShapePackageEditor : Editor {
		#region Vars
		ShapePackage shapePackage;
		SerializedProperty propShapeItems;

		ReorderableList itemsList;
		#endregion

		#region Events
		public void OnEnable () {
			shapePackage = (ShapePackage)target;
			propShapeItems = serializedObject.FindProperty ("shapeItems");

			itemsList = new ReorderableList (serializedObject, propShapeItems);
			itemsList.elementHeightCallback += ListItemHeightCallback;
			itemsList.drawHeaderCallback += DrawListItemHeader;
			itemsList.drawElementCallback += DrawListItemElement;
			itemsList.onAddCallback += AddListItem;
			itemsList.onRemoveCallback += RemoveListItem;
			//itemsList.onAddDropdownCallback += AddDropdownMenu;
		}
		public override void OnInspectorGUI () {
			serializedObject.Update ();

			// Catalog List
			itemsList.DoLayoutList ();
			EditorGUILayout.Space ();

			// Reload Shape Catalog
			if (GUILayout.Button ("Reload Shapes")) {
				ShapeCatalog.GetInstance ().LoadPackages ();
			}

			//EditorUtility.SetDirty(); // TODO: ???

			serializedObject.ApplyModifiedProperties ();
		}
		#endregion

		#region Shape Items
		private void DrawListItemHeader(Rect rect)
		{
			GUI.Label(rect, "Shape Items");
		}
		private void DrawListItemElement (Rect rect, int index, bool isActive, bool isFocused) {
			var propShapeItem = itemsList.serializedProperty.GetArrayElementAtIndex (index);
			ShapeCatalog.ShapeItem catalogItem = shapePackage.shapeItems [index];
				EditorGUI.DelayedTextField (new Rect (rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), propShapeItem.FindPropertyRelative ("id"));
				rect.y += EditorGUIUtility.singleLineHeight;
				EditorGUI.DelayedTextField (new Rect (rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), propShapeItem.FindPropertyRelative ("name"));
				rect.y += EditorGUIUtility.singleLineHeight;
				EditorGUI.DelayedTextField (new Rect (rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), propShapeItem.FindPropertyRelative ("category"));
				rect.y += EditorGUIUtility.singleLineHeight;
				EditorGUI.PropertyField (new Rect (rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), propShapeItem.FindPropertyRelative ("shapeCollection"));
			/*
			if (GUI.Button (new Rect (rect.x, rect.y, 50, EditorGUIUtility.singleLineHeight), "Path")) {
				catalogItem.path = EditorUtility.OpenFilePanel ("Select a ShapeDescriptorCollection Scriptable Object file", ExtensionManager.extensionPath, "asset");
				int indexOf = catalogItem.path.IndexOf (ExtensionManager.extensionPath);
				if (indexOf >= 0) {
					catalogItem.path = catalogItem.path.Substring (indexOf + ExtensionManager.extensionPath.Length);
				}
			} else {
				EditorGUI.LabelField (new Rect (rect.x + 60, rect.y, rect.width - 60, EditorGUIUtility.singleLineHeight), catalogItem.path);
				rect.y += EditorGUIUtility.singleLineHeight;
				EditorGUI.PropertyField (new Rect (rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), propShapeItem.FindPropertyRelative ("thumb"));
			}
			*/
		}
		private void AddListItem(ReorderableList list)
		{
			ShapeCatalog.ShapeItem item = new ShapeCatalog.ShapeItem ();
			item.name = "default";
			shapePackage.shapeItems.Add (item);
			EditorUtility.SetDirty (shapePackage);
			serializedObject.ApplyModifiedProperties ();
		}
		private void RemoveListItem(ReorderableList list)
		{
			shapePackage.shapeItems.RemoveAt (list.index);
			EditorUtility.SetDirty (shapePackage);
			serializedObject.ApplyModifiedProperties ();
		}
		private float ListItemHeightCallback (int index) {
			return EditorGUIUtility.singleLineHeight * 5 + 10;
		}
		private void AddDropdownMenu (Rect rect, ReorderableList list) {
			/*
			var menu = new GenericMenu ();
			menu.AddItem (new GUIContent ("Add Item"), true, clickHandler, ShapeCatalog.ShapeItem.GetItem());
			//menu.AddItem (new GUIContent ("Add Category"), true, clickHandler, ShapeCatalog.ShapeItem.GetCategory());
			menu.ShowAsContext();
			*/
		}

		private void clickHandler(object target) {
			/*
			ShapeCatalog.ShapeItem item = (ShapeCatalog.ShapeItem)target;
			broccoliCatalog.items.Add (item);
			EditorUtility.SetDirty (broccoliCatalog);
			serializedObject.ApplyModifiedProperties ();
			*/
		}
		#endregion
	}
}