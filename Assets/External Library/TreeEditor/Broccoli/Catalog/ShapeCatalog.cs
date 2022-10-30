using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Broccoli.Base;
using Broccoli.Utils;
using Broccoli.Builder;

namespace Broccoli.Catalog
{
	/// <summary>
	/// Broccoli shape catalog manager class.
	/// </summary>
	[System.Serializable]
	public class ShapeCatalog {
		#region ShapeItem class
		/// <summary>
		/// Catalog item.
		/// </summary>
		[System.Serializable]
		public class ShapeItem {
			/// <summary>
			/// Unique shape identifier.
			/// </summary>
			public string id = "";
			/// <summary>
			/// Display name for the shape.
			/// </summary>
			public string name = "";
			/// <summary>
			/// The thumbnail texture.
			/// </summary>
			public Texture2D thumb = null;
			public ShapeDescriptorCollection shapeCollection = null;
			/// <summary>
			/// The path to the shape collection scriptable object.
			/// </summary>
			public string path = "";
			/// <summary>
			/// The name of the category the item belongs to.
			/// </summary>
			public string category = "";
			/// <summary>
			/// The order within the category.
			/// </summary>
			public int order = 0;
		}
		#endregion

		#region Vars
		/// <summary>
		/// The total items.
		/// </summary>
		int _totalItems = 0;
		/// <summary>
		/// The total categories.
		/// </summary>
		int _totalCategories = 0;
		/// <summary>
		/// Gets the total items.
		/// </summary>
		/// <value>The total items.</value>
		public int totalItems {
			get { return _totalItems; }
		}
		/// <summary>
		/// Gets the total categories.
		/// </summary>
		/// <value>The total categories.</value>
		public int totalCategories {
			get { return _totalCategories; }
		}
		/// <summary>
		/// The GUI contents.
		/// </summary>
		[System.NonSerialized]
		public Dictionary<string, List<GUIContent>> contents = new Dictionary<string, List<GUIContent>> ();
		/// <summary>
		/// The catalog items.
		/// </summary>
		public Dictionary<string, List<ShapeItem>> items = new Dictionary<string, List<ShapeItem>>();
		/// <summary>
		/// All the shapes items on this catalog.
		/// </summary>
		/// <typeparam name="ShapeItem">Shape item instance.</typeparam>
		/// <returns>List of shape items.</returns>
		public List<ShapeItem> popUpItems = new List<ShapeItem> ();
		/// <summary>
		/// Options to populate a popup control.
		/// </summary>
		public List<string> popUpOptions = new List<string> ();
		/// <summary>
		/// The catalog asset relative path.
		/// </summary>
		private static string catalogAssetRelativePath = "Catalog";
		/// <summary>
		/// Gets the catalog asset path.
		/// </summary>
		/// <value>The catalog asset path.</value>
		private static string catalogAssetPath { get { return ExtensionManager.extensionPath + catalogAssetRelativePath; } }
		#endregion

		#region Singleton
		/// <summary>
		/// The catalog singleton.
		/// </summary>
		static ShapeCatalog _catalog = null;
		/// <summary>
		/// Gets a catalog singleton instance.
		/// </summary>
		/// <returns>The instance.</returns>
		public static ShapeCatalog GetInstance () {
			if (_catalog == null) {
				_catalog = new ShapeCatalog ();
				_catalog.LoadPackages ();
			}
			return _catalog;
		}
		#endregion

		#region Catalog Operations
		/// <summary>
		/// Loads the packages available for the catalog.
		/// </summary>
		public void LoadPackages () {
			Clear ();
			#if UNITY_EDITOR
			string[] catalogPath = {catalogAssetPath};
			string[] packagesPath = UnityEditor.AssetDatabase.FindAssets ("t:shapePackage", catalogPath);
			for (int i = 0; i < packagesPath.Length; i++) {
				ShapePackage package = UnityEditor.AssetDatabase.LoadAssetAtPath<ShapePackage> (
					UnityEditor.AssetDatabase.GUIDToAssetPath (packagesPath [i]));
				if (package) {
					for (int j = 0; j < package.shapeItems.Count; j++) {
						AddShapeItem (package.shapeItems[j]);
					}
				}
			}
			#endif
			PrepareGUIContents ();
		}
		/// <summary>
		/// Adds a category to the catalog.
		/// </summary>
		/// <param name="name">Name of the category.</param>
		/// <param name="order">Order.</param>
		void AddCatalogCategory (string name, int order = 0) {
		}
		/// <summary>
		/// Adds an item to the catalog.
		/// </summary>
		/// <param name="shapeItem">Catalog item.</param>
		void AddShapeItem (ShapeItem shapeItem) {
			if (shapeItem != null) {
				if (shapeItem.category == null) {
					shapeItem.category = "";
				}
				AddCatalogCategory (shapeItem.category);
				if (!items.ContainsKey (shapeItem.category)) {
					items [shapeItem.category] = new List<ShapeItem> ();
				}
				items [shapeItem.category].Add (shapeItem);
				popUpOptions.Add (shapeItem.name);
				popUpItems.Add (shapeItem);
			}
		}
		/// <summary>
		/// Clear this instance.
		/// </summary>
		public void Clear () {
			_totalCategories = 0;
			_totalItems = 0;
			var enumerator = items.GetEnumerator ();
			while (enumerator.MoveNext ()) {
				enumerator.Current.Value.Clear ();
			}
			items.Clear ();
			popUpOptions.Clear ();
			popUpItems.Clear ();
			ClearContent ();
		}
		/// <summary>
		/// Clears the GUI content.
		/// </summary>
		private void ClearContent () {
			var enumerator = contents.GetEnumerator ();
			while (enumerator.MoveNext ()) {
				enumerator.Current.Value.Clear ();
			}
			contents.Clear ();
		}
		/// <summary>
		/// Prepares the GUI contents.
		/// </summary>
		public void PrepareGUIContents () {
			ClearContent ();
			_totalItems = 0;
			_totalCategories = 0;
			var enumerator = items.GetEnumerator ();
			while (enumerator.MoveNext ()) {
				var itemsPair = enumerator.Current;
				if (!contents.ContainsKey (itemsPair.Key)) {
					contents [itemsPair.Key] = new List<GUIContent> ();
					_totalCategories++;
				}
				for (int i = 0; i < itemsPair.Value.Count; i++) {
					if (itemsPair.Value[i].thumb != null) {
						contents [itemsPair.Key].Add (new GUIContent (itemsPair.Value[i].name, itemsPair.Value[i].thumb));
					} else {
						contents [itemsPair.Key].Add (new GUIContent (itemsPair.Value[i].name, GUITextureManager.GetLogoBox ()));
					}
					_totalItems++;
				}
			}
		}
		/// <summary>
		/// Gets the GUI contents.
		/// </summary>
		/// <returns>The GUI contents.</returns>
		public Dictionary<string, List<GUIContent>> GetGUIContents () {
			return contents;
		}
		/// <summary>
		/// Gets a catalog item at a given index.
		/// </summary>
		/// <returns>The item at index.</returns>
		/// <param name="categoryName">Category name.</param>
		/// <param name="index">Index.</param>
		public ShapeItem GetItemAtIndex (string categoryName, int index) {
			ShapeItem item = null;
			if (items.ContainsKey(categoryName) && index >= 0 && index < items[categoryName].Count) {
				item = items[categoryName][index];
			}
			return item;
		}
		/// <summary>
		/// Get the shape options to populate a popup.
		/// </summary>
		/// <returns>Array of strings representing the options.</returns>
		public string[] GetShapeOptions () {
			return popUpOptions.ToArray ();
		}
		public ShapeItem GetShapeItem (string shapeId) {
			ShapeItem shapeItem = null;
			int index = GetShapeIndex (shapeId);
			if (index != -1) {
				shapeItem = popUpItems [index];
			}
			return shapeItem;
		}
		public ShapeItem GetShapeItem (int index) {
			ShapeItem shapeItem = null;
			if (index != -1) {
				shapeItem = popUpItems [index];
			}
			return shapeItem;
		}
		/// <summary>
		/// Get the indes of a shape in the array of options.
		/// </summary>
		/// <param name="shapeId">Name of the shape to look for.</param>
		/// <returns>Index of the shape in the array, -1 if not found.</returns>
		public int GetShapeIndex (string shapeId) {
			for (int i = 0; i < popUpItems.Count; i++) {
				if (popUpItems [i].id.Equals (shapeId)) {
					return i;
				}
			}
			return -1;
		}
		#endregion
	}
}