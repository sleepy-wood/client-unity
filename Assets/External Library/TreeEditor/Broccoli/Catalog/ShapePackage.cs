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
	[CreateAssetMenu(fileName = "ShapePackage", menuName = "Broccoli Devel/Shape Package", order = 1)]
	#endif
	[System.Serializable]
	public class ShapePackage : ScriptableObject {
		/// <summary>
		/// The catalog items.
		/// </summary>
		public List<ShapeCatalog.ShapeItem> shapeItems = new List<ShapeCatalog.ShapeItem> ();
	}
}