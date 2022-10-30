using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Broccoli.Model {
	[System.Serializable]
	public class ShapeDescriptor {
		#region Vars
		public string name = "";
		public float axisLength = 1f;
		public int minResolution = 5;
		public int maxResolution = 10;
		public List<float> positions = new List<float> ();
		public List<BezierCurve> segments = new List<BezierCurve> ();
		public List<int> priorities = new List<int> ();
		public enum PositionType {
			Initial,
			Terminal,
			Middle,
			Unique
		}
		public PositionType positionType = PositionType.Middle;
		bool _hasTopCap = false;
		public bool hasTopCap {
			get { return _hasTopCap; }
		}
		bool _hasBottomCap = false;
		public bool hasBottomCap {
			get { return _hasBottomCap; }
		}
		public bool hasCap {
			get { return _hasBottomCap || _hasTopCap; }
		}
		[System.NonSerialized]
		public float maxTopCapPos = 1f;
		[System.NonSerialized]
		public float minBottomCapPos = 0f;
		#endregion

		#region Methods
		public void Process () {
			_hasTopCap = false;
			_hasBottomCap = false;
			for (int i = 0; i < positions.Count; i++) {
				if (positions [i] < 0) {
					_hasBottomCap = true;
				} else if (positions [i] > 1) {
					_hasTopCap = true;
				}
				if (positions [i] > maxTopCapPos) maxTopCapPos = positions [i];
				if (positions [i] < minBottomCapPos) minBottomCapPos = positions [i];
			}
		}
		public void Clear () {
			positions.Clear ();
			segments.Clear ();
			priorities.Clear ();
			_hasTopCap = false;
			_hasBottomCap = false;
		}
		public int AddNewSegment (float position, float radius = 0.5f, int priority = 0) {
			BezierCurve bezierCurve = GetBezierCircle (3, radius, 0f, 0f);
			return AddSegment (position, bezierCurve, priority);
		}
		/// <summary>
		/// Adds a segment to the shape at a position with a resolution priority.
		/// </summary>
		/// <param name="position">Position on the shape from 0 to 1.</param>
		/// <param name="segment">Bezier curve reresenting the segment.</param>
		/// <param name="priority">Resolution priority value, 0 is the higher priority.</param>
		/// <returns>Index of the added segment.</returns>
		public int AddSegment (float position, BezierCurve segment, int priority = 0) {
			int index = 0;
			for (int i = 0; i < positions.Count; i++) {
				if (position < positions [i]) {
					break;
				}
				index++;
			}
			segments.Insert (index, segment);
			positions.Insert (index, position);
			priorities.Insert (index, priority);
			return index;
		}
		public bool RemoveSegment (int index) {
			if (index < segments.Count) {
				segments.RemoveAt (index);
				positions.RemoveAt (index);
				priorities.RemoveAt (index);
				return true;
			}
			return false;
		}
		public bool RepositionSegment (int index, float newPosition) {
			if (index < segments.Count) {
				BezierCurve segment = segments [index];
				float position = positions [index];
				int priority = priorities [index];
				RemoveSegment (index);
				AddSegment (newPosition, segment, priority);
			}
			return false;
		}
		public BezierCurve GetSegmentAt (float position) {
			BezierCurve segment = null;
			for (int i = 0; i < positions.Count; i++) {
				if (positions[i] == position) {
					segment = segments [i];
					break;
				}
			}
			return segment;
		}
		public void NormalizePriorities () {
			priorities.Clear ();
			for (int i = 0; i < segments.Count; i++) {
				priorities.Add (0);
			}
		}
		#endregion

		#region Clone
		public ShapeDescriptor Clone () {
			ShapeDescriptor clone = new ShapeDescriptor ();
			clone.name = name;
			clone.axisLength = axisLength;
			clone.minResolution = minResolution;
			clone.maxResolution = maxResolution;
			for (int i = 0; i < positions.Count; i++) {
				clone.positions.Add (positions [i]);
			}
			for (int i = 0; i < segments.Count; i++) {
				clone.segments.Add (segments [i].Clone ());
			}
			for (int i = 0; i < priorities.Count; i++) {
				clone.priorities.Add (priorities [i]);
			}
			clone.positionType = positionType;
			return clone;
		}
		#endregion

		#region Utils
		/// <summary>
		/// Get a circle bezier curve.
		/// </summary>
		/// <param name="nodesCount">Half the number of nodes the curve will have.</param>
		/// <param name="radius">Rdius of the circle.</param>
		/// <param name="minAngleVariation">Minimum variation on the position of the nodes along the circle circumference.</param>
		/// <param name="maxAngleVariation">Maximum variation on the position of the nodes along the circle circumference.</param>
		/// <returns>Circular bezier curve.</returns>
		public static BezierCurve GetBezierCircle (int nodesCount, float radius, float minAngleVariation, float maxAngleVariation) {
			// https://stackoverflow.com/questions/1734745/how-to-create-circle-with-b%C3%A9zier-curves
			// Handle length = (4/3)*tan(pi/(2n))
			BezierCurve curve = new BezierCurve ();
			nodesCount *= 2;
			float stepAngle = Mathf.PI * 2 / (float)nodesCount;
			float nodeX = 0f;
			float nodeY = 0f;
			BezierNode lastNode = null;
			float[] angles = new float[nodesCount];
			float[] anglesDiff = new float[nodesCount];
			float angleVariance = Random.Range (minAngleVariation, maxAngleVariation);

			// Get randomized angles.
			//for (int i = 0; i <= pointyPoints; i++) {
			for (int i = 0; i <= nodesCount; i++) {
				if (i < nodesCount) {
					angles[i] = i * stepAngle + Random.Range (-angleVariance / 2f, angleVariance / 2f);
				}
				if (i > 0) {
					anglesDiff[i - 1] = (i==nodesCount?Mathf.PI * 2 + angles[0]:angles[i]) - angles[i - 1];
				}
			}
			for (int i = 0; i < nodesCount; i++) {
				nodeX = Mathf.Cos (angles[i]) * radius;
				nodeY = Mathf.Sin (angles[i]) * radius;
				BezierNode node = new BezierNode (new Vector3 (nodeX, 0, nodeY));
				if (i == 0) {
					lastNode = node;
				}
				node.handle1 = new Vector3 (nodeY, 0, -nodeX) * (4/3) * Mathf.Tan(Mathf.PI/ (float)(2 * nodesCount));
				node.handle2 = -node.handle1;
				curve.AddNode (node);
			}
			curve.AddNode (lastNode);
			return curve;
		}
		#endregion
	}
}