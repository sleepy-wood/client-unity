using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Broccoli.Base;
using Broccoli.Utils;

namespace Broccoli.Catalog
{
	/// <summary>
	/// Catalog package.
	/// </summary>
	#if BROCCOLI_DEVEL
	[CreateAssetMenu(fileName = "CatalogPackage", menuName = "Broccoli Devel/Catalog Package", order = 1)]
	#endif
	[System.Serializable]
	public class CatalogPackage : ScriptableObject {
		/// <summary>
		/// The catalog items.
		/// </summary>
		public List<BroccoliCatalog.CatalogItem> catalogItems = new List<BroccoliCatalog.CatalogItem> ();
		/// <summary>
		/// The catalog categories.
		/// </summary>
		public List<BroccoliCatalog.CatalogCategory> catalogCategories = new List<BroccoliCatalog.CatalogCategory> ();
	}
}