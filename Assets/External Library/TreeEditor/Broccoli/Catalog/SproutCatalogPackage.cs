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
	[CreateAssetMenu(fileName = "SproutCatalogPackage", menuName = "Broccoli Devel/Sprout Catalog Package", order = 1)]
	#endif
	[System.Serializable]
	public class SproutCatalogPackage : ScriptableObject {
		/// <summary>
		/// The catalog items. 
		/// </summary>
		public List<SproutCatalog.CatalogItem> catalogItems = new List<SproutCatalog.CatalogItem> ();
	}
}