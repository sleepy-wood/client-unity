using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

using Broccoli.Base;

namespace Broccoli.Catalog
{
	[CustomEditor(typeof(SproutCatalogPackage))]
	public class SproutCatalogPackageEditor : Editor {
		#region Vars
		SproutCatalogPackage catalogPackage;
		SerializedProperty propCatalogItems;

		ReorderableList itemsList;
		#endregion

		#region Events
		public void OnEnable () {
			catalogPackage = (SproutCatalogPackage)target;
			propCatalogItems = serializedObject.FindProperty ("catalogItems");

			itemsList = new ReorderableList (serializedObject, propCatalogItems);
			itemsList.elementHeightCallback += ListItemHeightCallback;
			itemsList.drawHeaderCallback += DrawListItemHeader;
			itemsList.drawElementCallback += DrawListItemElement;
			itemsList.onAddCallback += AddListItem;
			itemsList.onRemoveCallback += RemoveListItem;
			//itemsList.onAddDropdownCallback += AddDropdownMenu;
		}
		public override void OnInspectorGUI () {
			serializedObject.Update ();
			
			itemsList.DoLayoutList ();

			//EditorUtility.SetDirty(); // TODO: ???

			serializedObject.ApplyModifiedProperties ();
		}
		#endregion

		#region Catalog Items
		private void DrawListItemHeader(Rect rect)
		{
			GUI.Label(rect, "Catalog Items");
		}
		private void DrawListItemElement (Rect rect, int index, bool isActive, bool isFocused) {
			var propCatalogItem = itemsList.serializedProperty.GetArrayElementAtIndex (index);
			SproutCatalog.CatalogItem catalogItem = catalogPackage.catalogItems [index];
				EditorGUI.DelayedTextField (new Rect (rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), propCatalogItem.FindPropertyRelative ("name"));
				rect.y += EditorGUIUtility.singleLineHeight;
				EditorGUI.DelayedTextField (new Rect (rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), propCatalogItem.FindPropertyRelative ("category"));
				rect.y += EditorGUIUtility.singleLineHeight;
			if (GUI.Button (new Rect (rect.x, rect.y, 50, EditorGUIUtility.singleLineHeight), "Path")) {
				catalogItem.path = EditorUtility.OpenFilePanel ("Select Broccoli Pipeline file", ExtensionManager.extensionPath, "asset");
				int indexOf = catalogItem.path.IndexOf (ExtensionManager.extensionPath);
				if (indexOf >= 0) {
					catalogItem.path = catalogItem.path.Substring (indexOf + ExtensionManager.extensionPath.Length);
				}
			} else {
				EditorGUI.LabelField (new Rect (rect.x + 60, rect.y, rect.width - 60, EditorGUIUtility.singleLineHeight), catalogItem.path);
				rect.y += EditorGUIUtility.singleLineHeight;
				EditorGUI.PropertyField (new Rect (rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), propCatalogItem.FindPropertyRelative ("thumb"));
			}
		}
		private void AddListItem(ReorderableList list)
		{
			SproutCatalog.CatalogItem item = new SproutCatalog.CatalogItem ();
			item.name = "default";
			catalogPackage.catalogItems.Add (item);
			EditorUtility.SetDirty (catalogPackage);
			serializedObject.ApplyModifiedProperties ();
		}
		private void RemoveListItem(ReorderableList list)
		{
			catalogPackage.catalogItems.RemoveAt (list.index);
			EditorUtility.SetDirty (catalogPackage);
			serializedObject.ApplyModifiedProperties ();
		}
		private float ListItemHeightCallback (int index) {
			return EditorGUIUtility.singleLineHeight * 4 + 10;
		}
		private void AddDropdownMenu (Rect rect, ReorderableList list) {
			/*
			var menu = new GenericMenu ();
			menu.AddItem (new GUIContent ("Add Item"), true, clickHandler, BroccoliCatalog.CatalogItem.GetItem());
			//menu.AddItem (new GUIContent ("Add Category"), true, clickHandler, BroccoliCatalog.CatalogItem.GetCategory());
			menu.ShowAsContext();
			*/
		}

		private void clickHandler(object target) {
			/*
			BroccoliCatalog.CatalogItem item = (BroccoliCatalog.CatalogItem)target;
			broccoliCatalog.items.Add (item);
			EditorUtility.SetDirty (broccoliCatalog);
			serializedObject.ApplyModifiedProperties ();
			*/
		}
		#endregion
	}
}