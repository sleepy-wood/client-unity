using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Broccoli.Model;

namespace Broccoli.Builder {
	#if BROCCOLI_DEVEL
	[CreateAssetMenu(menuName = "Broccoli Devel/Shape Descriptor Collection")]
	#endif
	public class ShapeDescriptorCollection : ScriptableObject {
		#region Vars
		public List<ShapeDescriptor> shapes = new List<ShapeDescriptor> ();
		Dictionary<int, ShapeDescriptor> _idToShapeDescriptor = new Dictionary<int, ShapeDescriptor> ();
		List<int> _initialShapeIds = new List<int> ();
		List<int> _terminalShapeIds = new List<int> ();
		List<int> _middleShapeIds = new List<int> ();
		List<int> _uniqueShapeIds = new List<int> ();
		#endregion

		#region Processing
		public void Process () {
			_idToShapeDescriptor.Clear ();
			_initialShapeIds.Clear ();
			_terminalShapeIds.Clear ();
			_middleShapeIds.Clear ();
			_uniqueShapeIds.Clear ();
			for (int i = 0; i < shapes.Count; i++) {
				_idToShapeDescriptor.Add (i, shapes[i]);
				switch (shapes[i].positionType) {
					case ShapeDescriptor.PositionType.Initial:
						_initialShapeIds.Add (i);
						break;
					case ShapeDescriptor.PositionType.Terminal:
						_terminalShapeIds.Add (i);
						break;
					case ShapeDescriptor.PositionType.Middle:
						_middleShapeIds.Add (i);
						break;
					case ShapeDescriptor.PositionType.Unique:
						_uniqueShapeIds.Add (i);
						break;
				}
			}
		}
		public ShapeDescriptor GetShape (int id) {
			ShapeDescriptor shape = null;
			if (_idToShapeDescriptor.ContainsKey (id)) {
				shape = _idToShapeDescriptor [id];
			}
			return shape;
		}
		public int GetInitialShapeId () {
			int id = -1;
			if (_initialShapeIds.Count > 0) {
				id = _initialShapeIds [Random.Range (0, _initialShapeIds.Count)];
			}
			return id;
		}
		public int GetTerminalShapeId () {
			int id = -1;
			if (_terminalShapeIds.Count > 0) {
				id = _terminalShapeIds [Random.Range (0, _terminalShapeIds.Count)];
			}
			return id;
		}
		public int GetMiddleShapeId () {
			int id = -1;
			if (_middleShapeIds.Count > 0) {
				id = _middleShapeIds [Random.Range (0, _middleShapeIds.Count)];
			}
			return id;
		}
		public int GetUniqueShapeId () {
			int id = -1;
			if (_uniqueShapeIds.Count > 0) {
				id = _uniqueShapeIds [Random.Range (0, _uniqueShapeIds.Count)];
			}
			return id;
		}
		#endregion
	}
}