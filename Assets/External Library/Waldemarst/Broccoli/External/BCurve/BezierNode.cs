using System;
using System.Collections;

using UnityEngine;

namespace Broccoli.Model
{
	[System.Serializable]
	public class BezierNode : ISerializationCallbackReceiver {
		#region Vars
		/// <summary>
		///     - Enumeration describing the relationship between a point's handles.
		///     - Auto : The point's handles are mirrored across the point.
		///     - Aligned : The point's handles are aligned to the same line.
		///     - Free : Each handle moves independently of the other.
		///     - None : This point has no handles (both handles are located ON the point).
		/// </summary>
		public enum HandleStyle {
			Auto,
			Aligned,
			Free,
			None,
		}
		/// <summary>
		/// Value describing the relationship between this point's handles
		/// </summary>
		[SerializeField]
		public HandleStyle handleStyle = HandleStyle.Auto;
		/// <summary>
		/// This point position (local, relative to the curve center).
		/// </summary>
		[SerializeField]
		protected Vector3 _position;
		/// <summary>
		///     - Local position of the first handle
		///     - Setting this value will cause the curve to become dirty
		///     - This handle effects the curve generated from this point and the point proceeding it in curve.points
		/// </summary>
		[SerializeField]
		protected Vector3 _handle1;
		/// <summary>
		///     - Local position of the second handle
		///     - Setting this value will cause the curve to become dirty
		///             - This handle effects the curve generated from this point and the point coming after it in curve.points
		/// </summary>
		[SerializeField]
		private Vector3 _handle2;
		/// <summary>
		/// Point is selected.
		/// </summary>
		[System.NonSerialized]
		public bool isSelected = false;
		/// <summary>
		/// Point is connected.
		/// </summary>
		[System.NonSerialized]
		public bool isConnected = false;
		/// <summary>
		/// Gives the position of a point relative to the whole length of the curve (0 to 1).
		/// </summary>
		[System.NonSerialized]
		public float relativePosition = 0f;
		/// <summary>
		/// Gives the position of a pointusing the length from the begining of the curve.
		/// </summary>
		[System.NonSerialized]
		public float lengthPosition = 0f;
		/// <summary>
		/// Node state on a curve.
		/// Value to be used as needed on an implementation.
		/// </summary>
		public int state = 0;
		public Vector3 up = Vector3.up;
		public float roll = 0;
		public Vector2 scale = Vector2.up;
		public Vector3 direction = Vector3.forward;
		public Guid guid = System.Guid.Empty;
		[SerializeField]
		private string _guid = "";
		/// <summary>
		/// Alternative Guid in reference to another entity.
		/// </summary>
		[System.NonSerialized]
		public Guid altGuid = System.Guid.Empty;
		#endregion

		#region Events
		/// <summary>
        /// Event raised when position, direction, scale or roll changes.
        /// </summary>
        [HideInInspector]
        public event EventHandler onChange;
		/// <summary>
		/// Clears the delegates for onChange;
		/// </summary>
		public void ClearOnChange () {
			onChange = null;
		}
		#endregion

		#region Accessors
		/// <summary>
		/// This point position
		/// </summary>
		/// <value>Position in world space.</value>
		public Vector3 position {
			get { return _position; }
			set {
				_position = value;
				if (onChange != null) onChange (this, EventArgs.Empty);
				//_curve.SetDirty();
			}
		}
		/// <summary>
		///     - Curve this point belongs to
		///     - Changing this value will automatically remove this point from the current curve and add it to the new one
		/// </summary>
		[System.NonSerialized]
		protected BezierCurve _curve;  // use this internally and handle adding/removing manually
		public BezierCurve curve {
			get { return _curve; }
			set {
				_curve = value;
			}
		}
		/// <summary>
		/// Checks if this node is the first one in the list of nodes of the curve it belongs to.
		/// </summary>
		/// <value><c>True</c> if the node is the first one on the parent curve.</value>
		public bool isFirstNode {
			get {
				if (_curve != null && _curve.First () == this)
					return true;
				return false;
			}
		}
		/// <summary>
		/// Checks if this node is the last one in the list of nodes of the curve it belongs to.
		/// </summary>
		/// <value><c>True</c> if the node is the last one on the parent curve.</value>
		public bool isLastNode {
			get {
				if (_curve != null && _curve.Last () == this)
					return true;
				return false;
			}
		}
		public Vector3 handle1 {
			get { return (handleStyle == HandleStyle.None?Vector3.zero:_handle1); }
			set	{
				_handle1 = value;
				if (handleStyle == HandleStyle.Auto) {
					_handle2 = -value;
				} else if (handleStyle == HandleStyle.Aligned) {
					_handle2 = -value.normalized * _handle2.magnitude;
				}
				if (onChange != null) onChange (this, EventArgs.Empty);
			}
		}
		/// <summary>
		///             - Global position of the first handle
		///             - Ultimately stored in the 'handle1' variable
		///     - Setting this value will cause the curve to become dirty
		///     - This handle effects the curve generated from this point and the point proceeding it in curve.points
		/// </summary>
		public Vector3 globalHandle1 {
			get { return handle1 + _position; }
			set { handle1 = value - _position; }
		}
		public Vector3 handle2 {
			get { return (handleStyle == HandleStyle.None?Vector3.zero:_handle2); }
			set
			{
				_handle2 = value;
				if (handleStyle == HandleStyle.Auto) {
					_handle1 = -value;
				} else if (handleStyle == HandleStyle.Aligned) {
					_handle1 = -value.normalized * _handle1.magnitude;
				}
				if (onChange != null) onChange (this, EventArgs.Empty);
			}
		}
		/// <summary>
		///             - Global position of the second handle
		///             - Ultimately stored in the 'handle2' variable
		///             - Setting this value will cause the curve to become dirty
		///             - This handle effects the curve generated from this point and the point coming after it in curve.points
		/// </summary>
		public Vector3 globalHandle2 {
			get { return handle2 + _position; }
			set { handle2 = value - position; }
		}
		#endregion

		#region Contructors
		public BezierNode (Vector3 position, HandleStyle handleStyle = HandleStyle.None) {
			if (guid == Guid.Empty) {
				guid = System.Guid.NewGuid ();
			}
			this.position = position;
			this.handleStyle = handleStyle;
		}
		#endregion

		#region Cloning and Copying
		/// <summary>
		/// Clones this instance.
		/// </summary>
		/// <returns>Clone of this instance.</returns>
		public BezierNode Clone () {
			BezierNode clone = new BezierNode (_position);
			clone.guid = guid;
			clone.altGuid = altGuid;
			clone.handleStyle = handleStyle;
			clone.handle1 = _handle1;
			clone.handle2 = _handle2;
			clone.state = state;
			return clone;
		}
		/// <summary>
		/// Copy for this instance.
		/// </summary>
		/// <returns>Copy of this instance (with a new guid).</returns>
		public BezierNode Copy () {
			BezierNode copy = new BezierNode (_position);
			copy.handleStyle = handleStyle;
			copy.handle1 = _handle1;
			copy.handle2 = _handle2;
			copy.state = state;
			return copy;
		}
		#endregion

		#region Serializable
		public void OnBeforeSerialize() {
			_guid = guid.ToString ();
		}
		public void OnAfterDeserialize() {
			if (string.IsNullOrEmpty (_guid)) {
				guid = System.Guid.NewGuid ();
				_guid = guid.ToString ();
			} else 
				guid = System.Guid.Parse (_guid);
		}
		#endregion
	}
}