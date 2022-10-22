using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

namespace Broccoli.Model
{
	[System.Serializable]
	/// <summary>
	/// Bezier Curve Model.
	/// </summary>
	public class BezierCurve : ISerializationCallbackReceiver {
		#region Vars
		/// <summary>
		/// Number of steps to process on children cubic bezier curves.
		/// </summary>
		int _cubicCurveSteps = 30;
		[SerializeField]
		/// <summary>
		/// True to close the curve using its first and last node.
		/// </summary>
		private bool _closed;
		/// <summary>
		/// Enumeration to set the nodes and handlers of the curve on a single axis.
		/// </summary>
		public enum Axis { X, Y, Z }
		/// <summary>
		/// Axis of the curve.
		/// </summary>
		[SerializeField]
		private Axis _axis;
		/// <summary>
		/// Length of the curve.
		/// </summary>
		/// <value>Length of the curve.</value>
		public float length { get; private set; }
		/// <summary>
		/// Nodes defining the curve.
		/// </summary>
		/// <typeparam name="BezierNode">List of bezier nodes.</typeparam>
		/// <returns>List of nodes in the curve.</returns>
		public List<BezierNode> nodes = new List<BezierNode> ();
		/// <summary>
		/// Cubic curves between two subsequent nodes.
		/// </summary>
		/// <typeparam name="CubicBezierCurve">Cubic curve.</typeparam>
		/// <returns>List of cubic curves making the whole curve.</returns>
		[NonSerialized]
		public List<CubicBezierCurve> bezierCurves = new List<CubicBezierCurve> ();
		/// <summary>
		/// Optimized series of points making the curve.
		/// </summary>
		/// <typeparam name="CurvePoint">Curve point.</typeparam>
		/// <returns>List of curve points.</returns>
		[NonSerialized]
		public List<CurvePoint> points = new List<CurvePoint> ();
		/// <summary>
		/// Optimized series of vector points making the curve.
		/// </summary>
		/// <typeparam name="Vector3">Point position in curve space.</typeparam>
		/// <returns>List of curve vector points.</returns>
		[NonSerialized]
		public List<Vector3> vectorPoints = new List<Vector3> ();
		/// <summary>
		/// Angle resolution used to optimize the points lists in the curve.
		/// </summary>
		public float resolutionAngle = 5f;
		/// <summary>
		/// Flag to include each node as a point in a curve when processing priority points.
		/// </summary>
		public bool alwaysIncludeNodeAsPoint = false;
		/// <summary>
		/// Perlin noise strength to use at the begining of the curve.
		/// </summary>
		public float noiseFactorAtFirstNode = 0f;
		/// <summary>
		/// Perlin noise strength to use at the end of the curve.
		/// </summary>
		public float noiseFactorAtLastNode = 0f;
		/// <summary>
		/// Perlin noise scale to use at the begining of the curve.
		/// </summary>
		public float noiseScaleAtFirstNode = 1f;
		/// <summary>
		/// Perlin noise scale to use at the end of the curve.
		/// </summary>
		public float noiseScaleAtLastNode = 1f;
		/// <summary>
		/// Length offset used to calculate noise strength.
		/// </summary>
		public float noiseLengthOffset = 0f;
		/// <summary>
		/// If true, no offset resulting from noise is applied to the first point of the curve.
		/// </summary>
		public bool spareNoiseOffsetAtFirstPoint = false;
		/// <summary>
		/// Reference forward vector used to calculate from the base of the curve.
		/// </summary>
		public Vector3 referenceForward = Vector3.up;
		/// <summary>
		/// Modes to calculate normals.
		/// </summary>
		public enum NormalMode {
			ReferenceVector,
			ReferencePoint,
			FixedNormal
		}
		/// <summary>
		/// Mode to calculate normals on points.
		/// </summary>
		public NormalMode normalMode = NormalMode.ReferenceVector; // TODO: 09/09/20
		/// <summary>
		/// Reference forward vector used to calculate from the base of the curve.
		/// </summary>
		public Vector3 referenceNormal = Vector3.forward;
		/// <summary>
		/// Reference point to use on normal mode reference point.
		/// </summary>
		public Vector3 referencePoint = Vector3.zero;
		/// <summary>
		/// Vector for fixed normals mode.
		/// </summary>
		public Vector3 fixedNormal = Vector3.forward;
		/// <summary>
		/// Relevant positions to include when processing points for this curve.
		/// </summary>
		[NonSerialized]
		public List<float> relevantPositions = new List<float> ();
		[NonSerialized]
		private bool _autoProcess = true;
		/// <summary>
		/// True to automatically process this curve after any change.
		/// </summary>s
		public bool autoProcess {
			get { return _autoProcess; }
			set {
				_autoProcess = value;
				for (int i = 0; i < bezierCurves.Count; i++) {
					bezierCurves [i].autoProcess = _autoProcess;
				}
			}
		}
		public int resolution {
			get { return _cubicCurveSteps; }
			set {
				_cubicCurveSteps = value;
				for (int i = 0; i < bezierCurves.Count; i++) {
					bezierCurves[i].stepCount = _cubicCurveSteps;
				}
			}
		}
		public Guid guid = System.Guid.Empty;
		[SerializeField]
		private string _guid = "";
		public enum SimplifyBias {
			Angle,
			Distance
		}
		public SimplifyBias simplifyBias = SimplifyBias.Angle;
		public float distanceStep = 0.05f;
		#endregion

		#region Events
		/// <summary>
        /// Event raised when one of the curve changes.
        /// </summary>
        [HideInInspector]
		[System.NonSerialized]
        public UnityEvent onChange = new UnityEvent();
		/// <summary>
		/// Called when changing the 
		/// </summary>
		/// <param name="oldLength">Curve's old length.</param>
		/// <param name="newLength">Curve's new length.</param>
		public delegate void OnLengthDelegate (float oldLength, float newLength);
		//
		public OnLengthDelegate onLengthChanged;
		#endregion

		#region Accessors
		/// <summary>
		/// Defines if the curve connects its first and last node.
		/// </summary>
		/// <value>True is the curve is closed.</value>
		public bool closed {
			get { return _closed; }
			set {
				if (_closed == value) return;
				_closed = value;
			}
		}
		/// <summary>
		/// Defines the axis to snap the curve to.
		/// </summary>
		/// <value>Curve axis.</value>
		public Axis axis {
			get { return _axis; }
			set
			{
				if (_axis == value) return;
				_axis = value;
			}
		}
		/// <summary>
		/// Number of bezier nodes defining the curve.
		/// </summary>
		/// <value>Number of bezier nodes in the curve.</value>
		public int nodeCount {
			get { return nodes.Count; }
		}
		/// <summary>
		/// Accessor to bezier nodes in the curve.
		/// </summary>
		/// <value>BezierNode by index.</value>
		public BezierNode this[int index] {
			get { return nodes[index]; }
		}
		#endregion

		#region Contructors
		/// <summary>
		/// Default constructor.
		/// </summary>
		public BezierCurve () {
			if (guid == Guid.Empty) {
				guid = System.Guid.NewGuid ();
			}
		}
		#endregion

		#region Processing
		/// <summary>
		/// Process the curve bezier nodes to generate its curve points.
		/// </summary>
		/// <param name="resolutionAngle">Angle to filter points.</param>
		public void Process (float resolutionAngle) {
			this.resolutionAngle = resolutionAngle;
			Process ();
		}
		/// <summary>
		/// Process the curve bezier nodes to generate its curve points.
		/// </summary>
		public void Process () {
            bezierCurves.Clear();
            for (int i = 0; i < nodes.Count - (_closed?0:1); i++) {
                BezierNode n = nodes [i];
				BezierNode next;
				if (_closed && i == nodes.Count - 1) {
					next = nodes [0];
				} else {
                	next = nodes [i + 1];
				}
                CubicBezierCurve cubicBezierCurve = new CubicBezierCurve (n, next, _cubicCurveSteps, _autoProcess);
                cubicBezierCurve.onChange.AddListener (ProcessAfterCurveChanged);
                bezierCurves.Add (cubicBezierCurve);
            }
            ProcessAfterCurveChanged();
        }
		/// <summary>
		/// Process event after a change has been made to the curve.
		/// </summary>
		public void ProcessAfterCurveChanged () {
			float oldLength = length;
			length = 0;
			// Update lengh position on each node.
			for (int i = 0; i < bezierCurves.Count; i++) {
				bezierCurves[i].n1.lengthPosition = length;
				length += bezierCurves[i].length;
				if (i == bezierCurves.Count - 1 && !_closed) {
					bezierCurves[i].n2.lengthPosition = length;	
				}
			}

			// Update relative position on each node and normals
			for (int i = 0; i < bezierCurves.Count; i++) {
				bezierCurves[i].n1.relativePosition = bezierCurves[i].n1.lengthPosition/length;
				if (i == bezierCurves.Count - 1 && !_closed) {
					bezierCurves[i].n2.relativePosition = bezierCurves[i].n2.lengthPosition/length;
				}
			}
			
			// Update optimized points in curve.
			points.Clear ();

			// Recalculate normals.
			RecalculateNormals ();

			if (simplifyBias == SimplifyBias.Angle) {
				points = GetPoints (resolutionAngle);
			} else {
				points = GetPointsByStep (distanceStep);
			}

			vectorPoints.Clear ();
			AddPriorityPoints (relevantPositions);

			
			for (int i = 0; i < points.Count; i++) {
				vectorPoints.Add (points[i].position);
			}

			// Check if the length changed.
			if (!Mathf.Approximately (oldLength,length)) {
				onLengthChanged?.Invoke (oldLength, length);
			}

            onChange.Invoke();
		}
		/// <summary>
		/// Recalculates the normals on sample points or optimized points.
		/// </summary>
		public void RecalculateNormals () {
			if (normalMode == NormalMode.FixedNormal) {
				for (int i = 0; i < bezierCurves.Count; i++) {
					for (int j = 0; j < bezierCurves[i].samples.Count; j++) {
						bezierCurves[i].samples[j].normal = fixedNormal;
					}
				}
				for (int i = 0; i < points.Count; i++) {
					points[i].normal = fixedNormal;
				}
			} else {
				Vector3 _forward = referenceForward;
				Vector3 _normal = referenceNormal;
				Quaternion rotation = Quaternion.identity;
				for (int i = 0; i < bezierCurves.Count; i++) {
					for (int j = 0; j < bezierCurves[i].samples.Count; j++) {
						if (normalMode == NormalMode.ReferencePoint) {
							_normal = Vector3.ProjectOnPlane (bezierCurves[i].samples[j].position - referencePoint, _forward).normalized;
						}
						rotation = Quaternion.FromToRotation (_forward, bezierCurves[i].samples[j].forward);
						bezierCurves[i].samples[j].normal = rotation * _normal;
					}
				}
				for (int i = 0; i < points.Count; i++) {
					if (normalMode == NormalMode.ReferencePoint) {
						_normal = Vector3.ProjectOnPlane (points[i].position - referencePoint, _forward).normalized;
					}
					rotation = Quaternion.FromToRotation (_forward, points[i].forward);
					points[i].normal = rotation * _normal;
				}
			}
		}
		public void ComputeSamples () {
			for (int i = 0; i < bezierCurves.Count; i++) {
				bezierCurves[i].ComputeSamples ();
			}
		}
		public void NormalizeNormals (Vector3 previousNormal, Vector3 previousForward) {
			for (int i = 0; i < bezierCurves.Count; i++) {
				
			}
		}
		public static void NormalizeNormals (List<BezierCurve> curves) {
			Vector3 previousNormal = Vector3.forward;
			Vector3 previousForward = Vector3.up;
			CurvePoint p;
			for (int i = 0; i < curves.Count; i++) {
				curves[i].NormalizeNormals (previousNormal, previousForward);
				p = curves [i].GetPointAt (1f);
				previousNormal = p.normal;
				previousForward = p.forward;
			}
		}
		/// <summary>
		/// Set noise values on this curve and its distribution among the cubic bezier curves components.
		/// </summary>
		/// <param name="noiseFactorAtFirstNode"></param>
		/// <param name="noiseFactorAtLastNode"></param>
		/// <param name="noiseScaleAtFirstNode"></param>
		/// <param name="noiseScaleAtLastNode"></param>
		/// <param name="spareNoiseOffsetAtFirstPoint"></param>
		/// <param name="noiseLengthOffset"></param>
		public void SetNoise (
			float noiseFactorAtFirstNode, 
			float noiseFactorAtLastNode, 
			float noiseScaleAtFirstNode, 
			float noiseScaleAtLastNode, 
			bool spareNoiseOffsetAtFirstPoint,
			float noiseLengthOffset = 0f)
		{
			this.noiseFactorAtFirstNode = noiseFactorAtFirstNode;
			this.noiseFactorAtLastNode = noiseFactorAtLastNode;
			this.noiseScaleAtFirstNode = noiseScaleAtFirstNode;
			this.noiseScaleAtLastNode = noiseScaleAtLastNode;
			this.noiseLengthOffset = noiseLengthOffset;
			this.spareNoiseOffsetAtFirstPoint = spareNoiseOffsetAtFirstPoint;
			float accumLength = 0f;
			for (int i = 0; i < bezierCurves.Count; i++) {
				if (i == 0) {
					bezierCurves [i].spareNoiseOffsetAtFirstPoint = spareNoiseOffsetAtFirstPoint;
				} else {
					bezierCurves [i].spareNoiseOffsetAtFirstPoint = false;
				}
				bezierCurves [i].noiseLengthOffset = noiseLengthOffset + accumLength;
				bezierCurves [i].noiseFactorAtFirstNode = Mathf.Lerp (noiseFactorAtFirstNode, noiseFactorAtLastNode, accumLength / (float)length);
				bezierCurves [i].noiseScaleAtFirstNode = Mathf.Lerp (noiseScaleAtFirstNode, noiseScaleAtLastNode, accumLength / (float)length);
				accumLength += bezierCurves [i].length;
				bezierCurves [i].noiseFactorAtLastNode = Mathf.Lerp (noiseFactorAtFirstNode, noiseFactorAtLastNode, accumLength / (float)length);
				bezierCurves [i].noiseScaleAtLastNode = Mathf.Lerp (noiseScaleAtFirstNode, noiseScaleAtLastNode, accumLength / (float)length);
			}
		}
		/// <summary>
		/// Get a list of curve points using angle tolerance to optimize the number of points.
		/// </summary>
		/// <param name="angleTolerance">Angle tolerance to filter points.</param>
		/// <param name="positions">List of positions to .</param>
		/// <returns>List of curve points.</returns>
		public List<CurvePoint> GetPoints (float angleTolerance = 2f, List<float> relevantPositions = null) {
			CurvePoint currPoint = null, prevPoint = null;
			Vector3 currVector = Vector3.zero, prevVector = Vector3.zero;
			bool hasPrevVector = false;
			List<CurvePoint> optimizedPoints = new List<CurvePoint> ();
			float accumAngle = 0f;
			float accumLength = 0f;
			bool shouldAddPoint = false;
			bool pointFromRelevantPosition = false;
			int relevantPositionsIndex = -1;
			if (relevantPositions != null && relevantPositions.Count > 0) {
				relevantPositionsIndex = 0;
			}
			int firstIndex = 0;
			for (int i = 0; i < bezierCurves.Count; i++) {
				if (bezierCurves[i].samples == null) { 
					bezierCurves[i].samples = new List<CurvePoint>();
					bezierCurves[i].ComputeSamples ();
				}
				for (int j = firstIndex; j < bezierCurves[i].samples.Count; j++) {
					currPoint = bezierCurves[i].samples[j];
					if (prevPoint != null) {
						currVector = currPoint.position - prevPoint.position;
						if (hasPrevVector) {
							accumAngle += Vector3.Angle (prevVector, currVector);
							// If relevant positions
							if (relevantPositionsIndex >= 0 && relevantPositionsIndex < relevantPositions.Count &&
								(accumLength + prevPoint.lengthPosition) / length > relevantPositions[relevantPositionsIndex]) {
								shouldAddPoint = true;
								pointFromRelevantPosition = true;
								relevantPositionsIndex++;
							}
							if (accumAngle >= angleTolerance) {
								shouldAddPoint = true;
							}
							if (shouldAddPoint) {
								CurvePoint pointToAdd = prevPoint.Clone ();
								pointToAdd.lengthPosition = accumLength + pointToAdd.lengthPosition;
								if (pointFromRelevantPosition) {
									pointToAdd.relativePosition = relevantPositions [relevantPositionsIndex - 1];
									pointFromRelevantPosition = false;
								} else {
									pointToAdd.relativePosition = pointToAdd.lengthPosition / length;
								}
								optimizedPoints.Add (pointToAdd); // TODO adjust length and relative position.
								accumAngle = 0f;
								shouldAddPoint = false;
							}
						}
						prevVector = currVector;
						hasPrevVector = true;
						if (i == bezierCurves.Count - 1 && j == bezierCurves[i].samples.Count - 1) {
							// Add remaining relevant positions
							if (relevantPositions != null && relevantPositions.Count > 0) {
								for (int k = relevantPositionsIndex; k < relevantPositions.Count; k++) {
									CurvePoint relevantPointToAdd = GetPointAtLength (relevantPositions [k] * length);
									optimizedPoints.Add (relevantPointToAdd);
								}
							}
							// Add last point in bezier curves.
							CurvePoint pointToAdd = currPoint.Clone ();
							pointToAdd.lengthPosition = accumLength + pointToAdd.lengthPosition;
							pointToAdd.relativePosition = pointToAdd.lengthPosition / length;
							optimizedPoints.Add (pointToAdd); // TODO adjust length and relative position.
						}
					} else {
						//Add First Point
						CurvePoint pointToAdd = currPoint.Clone ();
						pointToAdd.lengthPosition = accumLength + pointToAdd.lengthPosition;
						pointToAdd.relativePosition = pointToAdd.lengthPosition / length;
						optimizedPoints.Add (pointToAdd); // TODO adjust length and relative position.
					}
					prevPoint = currPoint;
				}
				accumLength += bezierCurves[i].length;
				prevPoint = null;
				firstIndex = 1;
			}
			return optimizedPoints;
		}
		public List<CurvePoint> GetPointsByStep (float step = 0.05f) {
			List<CurvePoint> optimizedPoints = new List<CurvePoint> ();
			float accumCubicLength = 0f;
			float accumLength = 0f;
			float lengthStep = length * step;
			CurvePoint candidatePoint;
			for (int i = 0; i < bezierCurves.Count; i++) {
				if (bezierCurves[i].samples == null) { 
					bezierCurves[i].samples = new List<CurvePoint>();
					bezierCurves[i].ComputeSamples ();
				}
				for (int j = 0; j < bezierCurves[i].samples.Count; j++) {
					if ((i == 0 && j == 0) || (i == bezierCurves.Count - 1 && j == bezierCurves[i].samples.Count - 1)) {
						candidatePoint = bezierCurves[i].samples[j].Clone ();
						candidatePoint.lengthPosition = accumLength + candidatePoint.lengthPosition;
						candidatePoint.relativePosition = candidatePoint.lengthPosition / length;
						accumLength += lengthStep;
						optimizedPoints.Add (candidatePoint);
					} else {
						candidatePoint = bezierCurves[i].samples[j];
						if (candidatePoint.lengthPosition + accumCubicLength > accumLength) {
							accumLength = candidatePoint.lengthPosition + accumCubicLength + lengthStep;
							candidatePoint = candidatePoint.Clone ();
							candidatePoint.lengthPosition = accumCubicLength + candidatePoint.lengthPosition;
							candidatePoint.relativePosition = candidatePoint.lengthPosition / length;
							optimizedPoints.Add (candidatePoint);
						}
					}
				}
				accumCubicLength += bezierCurves [i].length;
			}
		return optimizedPoints;
		}
		/*
		/// <summary>
		/// Get a list of curve points using angle tolerance to optimize the number of points.
		/// </summary>
		/// <param name="angleTolerance">Angle tolerance to filter points.</param>
		/// <param name="positions">List of positions to .</param>
		/// <returns>List of curve points.</returns>
		public List<CurvePoint> GetPoints (float angleTolerance = 2f, List<float> relevantPositions = null) {
			CurvePoint currPoint = null, prevPoint = null;
			Vector3 currVector = Vector3.zero, prevVector = Vector3.zero;
			bool hasPrevVector = false;
			List<CurvePoint> optimizedPoints = new List<CurvePoint> ();
			float accumAngle = 0f;
			float accumLength = 0f;
			bool shouldAddPoint = false;
			bool pointFromRelevantPosition = false;
			int relevantPositionsIndex = -1;
			if (relevantPositions != null && relevantPositions.Count > 0) {
				relevantPositionsIndex = 0;
			}
			int lastIndex = 0;
			for (int i = 0; i < bezierCurves.Count; i++) {
				if (bezierCurves[i].samples == null) { 
					bezierCurves[i].samples = new List<CurvePoint>();
					bezierCurves[i].ComputeSamples ();
				}
				// Iterate samples from 0 to n-1. Except on the last cubic curve.
				lastIndex = bezierCurves [i].samples.Count - 1;
				if (i == bezierCurves.Count - 1) lastIndex ++;
				for (int j = 0; j < lastIndex; j++) {
					currPoint = bezierCurves[i].samples[j];
					if (prevPoint != null) {
						currVector = currPoint.position - prevPoint.position;
						if (hasPrevVector) {
							accumAngle += Vector3.Angle (prevVector, currVector);
							// If relevant positions
							if (relevantPositionsIndex >= 0 && relevantPositionsIndex < relevantPositions.Count &&
								(accumLength + prevPoint.lengthPosition) / length > relevantPositions[relevantPositionsIndex]) {
								shouldAddPoint = true;
								pointFromRelevantPosition = true;
								relevantPositionsIndex++;
							}
							if (accumAngle >= angleTolerance || (j == 0 && alwaysIncludeNodeAsPoint)) {
								shouldAddPoint = true;
							}
							if (shouldAddPoint) {
								CurvePoint pointToAdd = prevPoint.Clone ();
								pointToAdd.lengthPosition = accumLength + pointToAdd.lengthPosition;
								pointToAdd.relativePosition = pointToAdd.lengthPosition / length;
								pointToAdd.normal = (currPoint.normal + prevPoint.normal) / 2f;
								pointToAdd.forward = (currPoint.forward + prevPoint.forward) / 2f;
								optimizedPoints.Add (pointToAdd); // TODO adjust length and relative position.
								accumAngle = 0f;
								shouldAddPoint = false;
							}
						}
						prevVector = currVector;
						hasPrevVector = true;
						if (i == bezierCurves.Count - 1 && j == bezierCurves[i].samples.Count - 1) {
							// Add remaining relevant positions
							if (relevantPositions != null && relevantPositions.Count > 0) {
								for (int k = relevantPositionsIndex; k < relevantPositions.Count; k++) {
									CurvePoint relevantPointToAdd = GetPointAtLength (relevantPositions [k] * length);
									optimizedPoints.Add (relevantPointToAdd);
								}
							}
							// ADD LAST POINT IN CURVE.
							CurvePoint pointToAdd = currPoint.Clone ();
							pointToAdd.lengthPosition = length;
							pointToAdd.relativePosition = 1;
							optimizedPoints.Add (pointToAdd); // TODO adjust length and relative position.
						}
					} else {
						// ADD FIRST POINT IN CURVE.
						CurvePoint pointToAdd = currPoint.Clone ();
						pointToAdd.lengthPosition = accumLength + pointToAdd.lengthPosition;
						pointToAdd.relativePosition = pointToAdd.lengthPosition / length;
						optimizedPoints.Add (pointToAdd); // TODO adjust length and relative position.
					}
					prevPoint = currPoint;
				}
				accumLength += bezierCurves[i].length;
			}
			// IF CLOSED CURVE: AVERAGE FIRST AND LAST FORWARD, NORMAL.
			if (_closed && optimizedPoints.Count > 1) {
				Vector3 avgNormal = (optimizedPoints [0].normal + optimizedPoints [optimizedPoints.Count - 1].normal) /2f;
				Vector3 avgForward = (optimizedPoints [0].forward + optimizedPoints [optimizedPoints.Count - 1].forward) /2f;
				optimizedPoints [0].normal = avgNormal;
				optimizedPoints [optimizedPoints.Count - 1].normal = avgNormal;
				optimizedPoints [0].forward = avgForward;
				optimizedPoints [optimizedPoints.Count - 1].forward = avgForward;
			}
			// RETURN POINTS.
			return optimizedPoints;
		}
		*/
		/// <summary>
		/// Adds priority points to the curve using a list or relative positions.
		/// </summary>
		/// <param name="positionsToInclude">List of relative positions.</param>
		public void AddPriorityPoints (List<float> positionsToInclude) {
			// ADD CUSTOM POINT IF SET.
			if (positionsToInclude != null && positionsToInclude.Count > 0) {
				List<CurvePoint> pointsToInclude = new List<CurvePoint> ();
				for (int i = 0; i < positionsToInclude.Count; i++) {
					pointsToInclude.Add (GetPointAt (positionsToInclude [i]));
				}
				points.AddRange (pointsToInclude);
				points.Sort(delegate(CurvePoint a, CurvePoint b) {
					return a.relativePosition.CompareTo (b.relativePosition);
				});
			}
		}
		/// <summary>
		/// Get a list of curve points using a relative distance interval between nodes.
		/// </summary>
		/// <param name="step">Step to use between curve nodes.</param>
		/// <returns>List of curve points.</returns>
		public List<CurvePoint> GetIntervalPoints (float step = 0.05f) {
			List<CurvePoint> optimizedPoints = new List<CurvePoint> ();
			if (Mathf.Abs(step) < 0.0001f) return optimizedPoints;
			float currentPosNode = 0f;
			float nextPosNode = 0f;
			float stepPos = 0f;
			int nodeLimit = nodes.Count - 1;
			if (closed) {
				nodeLimit = nodes.Count;
			}
			for (int i = 0; i < nodeLimit; i++) {
				currentPosNode = nodes [i].relativePosition;
				if (i == nodes.Count - 1) {
					nextPosNode = 1f;
				} else {
					nextPosNode = nodes [i + 1].relativePosition;
				}
				for (stepPos = currentPosNode; stepPos < nextPosNode; stepPos += step) {
					optimizedPoints.Add ( GetPointAt (stepPos));
				}
				if (!closed && 1f - stepPos < step * 0.5f)
					optimizedPoints.Add (GetPointAt (1f));
			}
			return optimizedPoints;
		}
		/// <summary>
		/// Lerps between two bezier curves.
		/// </summary>
		/// <param name="a">A reference bezier curve.</param>
		/// <param name="b">B reference bezier curve.</param>
		/// <param name="t">Time (0 to 1).</param>
		/// <returns>Lerp curve.</returns>
		public static BezierCurve Lerp (BezierCurve a, BezierCurve b, float t) {
			if (a.nodeCount == b.nodeCount) {
				BezierCurve lerpCuve = new BezierCurve ();
				BezierNode node;
				for (int i = 0; i < a.nodeCount; i++) {
					node = new BezierNode (Vector3.Lerp (a.nodes[i].position, b.nodes[i].position, t));
					node.handleStyle = a.nodes[i].handleStyle;
					node.handle1 = Vector3.Lerp (a.nodes[i].handle1, b.nodes[i].handle1, t);
					node.handle2 = Vector3.Lerp (a.nodes[i].handle2, b.nodes[i].handle2, t);
					node.up = Vector3.Lerp (a.nodes[i].up, b.nodes[i].up, t);
					node.roll = Mathf.Lerp (a.nodes[i].roll, b.nodes[i].roll, t);
					node.scale = Vector2.Lerp (a.nodes[i].scale, b.nodes[i].scale, t);
					node.direction = Vector3.Lerp (a.nodes[i].direction, b.nodes[i].direction, t);
					lerpCuve.AddNode (node, false);
				}
				lerpCuve.Process ();
				return lerpCuve;
			}
			return a;
		}
		#endregion

		#region Nodes Processing
		public void AddNode (BezierNode node, bool autoProcess = true) {
			// TODO RE: set autoProcess to false by default.
			nodes.Add (node);
			node.curve = this;
			if (autoProcess)
				Process ();
		}
		public void InsertNode (int index, BezierNode node, bool autoProcess = true) {
			nodes.Insert (index, node);
			node.curve = this;
			if (autoProcess) 
				Process ();
		}
		public Broccoli.Model.BezierNode AddNodeAt (Vector3 position, bool autoProcess = true) {
			BezierNode newNode = new BezierNode (position);
			AddNode (newNode, autoProcess);
			return newNode;
		}
		public void RemoveNode (BezierNode node, bool autoProces = true)
		{
			nodes.Remove (node);
			if (autoProces)
				Process ();
		}
		public void RemoveNode (int index, bool autoProcess = true)
		{
			nodes.RemoveAt (index);
			if (autoProcess)
				Process ();
		}
		public void RemoveAllNodes (bool autoProcess = true) {
			nodes.Clear ();
			if (autoProcess)
				Process ();
		}
		#endregion

		#region Curve processing
		public void SnapNodesToAxis(List<int> indexes, Axis axis, BezierNode referenceNode) {
			if (referenceNode == null && indexes.Count > 0 && indexes[0] < nodes.Count) {
				referenceNode = nodes[indexes[0]];
			}
			if (referenceNode != null) {
				for (int i = 0; i < nodes.Count; i++) {
					if (indexes.Contains(i)) {
						switch (axis) {
							case Axis.X:
								nodes[i].position = new Vector3 (referenceNode.position.x, nodes[i].position.y, nodes[i].position.z);
								nodes[i].handle1 = new Vector3 (0, nodes[i].handle1.y, nodes[i].handle1.z);
								nodes[i].handle2 = new Vector3 (0, nodes[i].handle2.y, nodes[i].handle2.z);
								break;
							case Axis.Y:
								nodes[i].position = new Vector3 (nodes[i].position.x, referenceNode.position.y, nodes[i].position.z);
								nodes[i].handle1 = new Vector3 (nodes[i].handle1.x, 0, nodes[i].handle1.z);
								nodes[i].handle2 = new Vector3 (nodes[i].handle2.x, 0, nodes[i].handle2.z);
								break;
							case Axis.Z:
								nodes[i].position = new Vector3 (nodes[i].position.x, nodes[i].position.y, referenceNode.position.z);
								nodes[i].handle1 = new Vector3 (nodes[i].handle1.x, nodes[i].handle1.y, 0);
								nodes[i].handle2 = new Vector3 (nodes[i].handle2.x, nodes[i].handle2.y, 0);
								break;
						}
					}
				}
			}
		}
		/*
		public void MirrorAllNodesAroundAxis(Axis axis) {
			for (int i = 0; i < nodes.Length; i++) {
				switch (axis) {
					case Axis.X:
						nodes[i].localPosition = new Vector3(-nodes[i].localPosition.x, nodes[i].localPosition.y, nodes[i].localPosition.z);
						nodes[i].handle1 = new Vector3(-nodes[i].handle1.x, nodes[i].handle1.y, nodes[i].handle1.z);
						break;
					case Axis.Y:
						nodes[i].localPosition = new Vector3(nodes[i].localPosition.x, -nodes[i].localPosition.y, nodes[i].localPosition.z);
						nodes[i].handle1 = new Vector3(nodes[i].handle1.x, -nodes[i].handle1.y, nodes[i].handle1.z);
						break;
					case Axis.Z:
						nodes[i].localPosition = new Vector3(nodes[i].localPosition.x, nodes[i].localPosition.y, -nodes[i].localPosition.z);
						nodes[i].handle1 = new Vector3(nodes[i].handle1.x, nodes[i].handle1.y, -nodes[i].handle1.z);
						break;
				}
			}
		}
		*/
		/*
		/// <summary>
		/// To be called when errors start flying.
		/// </summary>
		public void CleanupNullNodes() {
			List<BezierNode> cleanNodes = new List<BezierNode>();
			foreach (var p in nodes) {
				if (p != null) cleanNodes.Add(p);
			}
			nodes = cleanNodes.ToArray();
			dirty = false;
		}
		*/
		public BezierNode First () {
			return this [0];
		}
		public BezierNode Last () {
			return this [nodes.Count - 1];
		}
		public CurvePoint GetPointAt (float t, bool getCore = false) {
			int tIndex = -1;
			return GetPointAt (t, out tIndex, getCore);
		}
		public CurvePoint GetPointAt (float t, out int tIndex, bool getCore = false) {
			tIndex = -1;
			if (nodes.Count == 0) return null;
			/*
			if (_closed) {
				if (t < 0) {
					t = 1f + t - (int)t;
				} else if (t > 1) { 
					t = t - (int)t;
				}
			} else if (!getCore) {
				if (t == 0) {
					tIndex = 0;
					return points[0];
				} else if (t == 1) {
					tIndex = nodes.Count - 1;
					return points[points.Count - 1];
				} else if (t < 0) {
					// TODO
					//Vector3 tangent = GetTangentAt (0) * -1;
					//return nodes[0].position + tangent.normalized * (-t * length);
				} else if (t > 1) {
					// TODO
					//Vector3 tangent = GetTangentAt (1);
					//return nodes[nodes.Count - 1].position + tangent.normalized * ((t - 1f) * length);
				}
			}
			*/
			if (_closed) {
				t = t % 1f;
				if (t < 0) {
					t = 1f + t;
				}
			}
			if (t == 0) {
				tIndex = 0;
				return points[0];
			} else if (t == 1) {
				tIndex = nodes.Count - 1;
				return points[points.Count - 1];
			}

			BezierNode n1 = null;
			//BezierNode n2 = null;
			float accumLength = 0f;
			if (t < 0 && !_closed) {
				tIndex = 0;
			} else if (t > 1 && !_closed) {
				tIndex = bezierCurves.Count - 1;
			} else {
				for (int i = 0; i < bezierCurves.Count; i++) {
					tIndex = i;
					if (t * length <= accumLength + bezierCurves[i].length) {
						break;
					}
					accumLength += bezierCurves[i].length;
				}
			}
			n1 = bezierCurves [tIndex].n1;
			//n2 = bezierCurves [tIndex].n2;
			/*
			if (_closed) {
			} else {
				for (int i = 0; i < nodes.Count - 1; i++) {
					tIndex = i;
					n1 = nodes [i];
					n2 = nodes [i + 1];
					if (nodes [i + 1].relativePosition > t) {
						break;
					}
				}
			}
			*/

			CurvePoint point;
			if (getCore) {
				float bezierDistance = (t * length) - n1.lengthPosition;
				point = bezierCurves[tIndex].CreateSample (bezierDistance, bezierDistance / bezierCurves [tIndex].length);
			} else {
				point = bezierCurves[tIndex].GetSampleAtDistance ((t * length) - n1.lengthPosition);
				point.lengthPosition += n1.lengthPosition;
				point.relativePosition = t;
			}
			return point;
		}
		public CurvePoint GetPointAtLength (float _length) {
			return GetPointAt (_length /length);
		}
		public Vector3 GetPositionAt (float t) {
			int index = -1;
			return GetPositionAt (t, out index);
		}
		public Vector3 GetPositionAt (float t, out int tIndex) {
			CurvePoint curvePoint = GetPointAt (t, out tIndex);
			/*
			tIndex = -1;
			if (nodes.Count == 0) return Vector3.zero;

			if (close) {
				if (t < 0) {
					t = 1f + t - (int)t;
				} else if (t > 1) { 
					t = t - (int)t;
				}
			} else {
				if (t == 0) {
					return nodes[0].position;
				} else if (t == 1) {
					return nodes[nodes.Count - 1].position;
				} else if (t < 0) {
					Vector3 tangent = GetTangentAt (0) * -1;
					return nodes[0].position + tangent.normalized * (-t * length);
				} else if (t > 1) {
					Vector3 tangent = GetTangentAt (1);
					return nodes[nodes.Count - 1].position + tangent.normalized * ((t - 1f) * length);
				}
			}

			BezierNode n1 = null;
			BezierNode n2 = null;
			for (int i = 0; i < nodes.Count - 1; i++) {
				tIndex = i;
				n1 = nodes[i];
				n2 = nodes[i+1];
				if (nodes[i + 1].relativePosition > t) {
					break;
				}
			}

			CurvePoint point = bezierCurves[tIndex].GetSampleAtDistance ((t * length) - n1.lengthPosition);
			*/
			return curvePoint.position;
		}
		public Vector3 GetTangentAtLenght (float distance) {
			return GetTangentAt (distance / length);
		}
		public Vector3 GetTangentAt (float t) {
			if (length == 0) {
				Process ();
			}
			if (t <= 0) {
				t = 0.01f;
			}
			else if (t >= 1) {
				t = 0.99f;
			}

			float totalPercent = 0;
			float curvePercent = 0;

			BezierNode p1 = null;
			BezierNode p2 = null;

			int approxResolution = 10; // added by nothke

			int index = 0;
			int maxIters = 10;

			while (p1 == null && p2 == null) {
				for (int i = 0; i < nodes.Count - 1; i++) {
					curvePercent = ApproximateLength (nodes[i], nodes[i + 1], approxResolution) / length;

					if (totalPercent + curvePercent > t) {
						p1 = nodes[i];
						p2 = nodes[i + 1];
						break;
					}

					else totalPercent += curvePercent;
				}

				index++;

				approxResolution += 10;

				if (index >= maxIters) {
					Debug.LogWarning ("Too many iterations (" + maxIters + ").");
					return Vector3.zero;
				}
			}

			if (_closed && p1 == null) {
				p1 = nodes[nodes.Count - 1];
				p2 = nodes[0];
			}

			t -= totalPercent;

			if (p1 == null) Debug.LogError ("p1 is null");
			if (p2 == null) Debug.LogError ("p2 is null");

			//return GetTangent (p1, p2, t / curvePercent);
			return CubicBezierCurve.GetTangent (t / curvePercent, p1, p2);
		}
		/*
		public Vector3 GetLocalTangent(BezierNode bp1, BezierNode bp2, float t) {
			if (bp1.handleStyle == BezierNode.HandleStyle.None &&
				bp2.handleStyle == BezierNode.HandleStyle.None) {
				return (bp2.localPosition - bp1.localPosition).normalized;
			}

			Vector3 a = bp1.localPosition;
			Vector3 b = bp1.localPosition + bp1.handle2;
			Vector3 c = bp2.localPosition + bp2.handle1;
			Vector3 d = bp2.localPosition;

			return Tangent(a, b, c, d, t);
		}
		*/

		#endregion

		#region Draw methods
		/// <summary>
		///     - Gets the node 't' percent along a curve
		///     - Automatically calculates for the number of relevant nodes
		/// </summary>
		/// <returns>
		///     - The node 't' percent along the curve
		/// </returns>
		/// <param name='p1'>
		///     - The bezier node at the beginning of the curve
		/// </param>
		/// <param name='p2'>
		///     - The bezier node at the end of the curve
		/// </param>
		/// <param name='t'>
		///     - Value between 0 and 1 representing the percent along the curve (0 = 0%, 1 = 100%)
		/// </param>
		public static Vector3 GetNode(BezierNode p1, BezierNode p2, float t)
		{
			return GetNode(p1.position, p1.globalHandle2, p2.position, p2.globalHandle1, t);
		}

		/// <summary>
		/// All arguments are global positions. Returns global position at 't' percent along the curve.
		/// </summary>
		public static Vector3 GetNode(Vector3 p1, Vector3 p1Handle2, Vector3 p2, Vector3 p2Handle1, float t)
		{
			if (p1Handle2 != p1)
			{
				if (p2Handle1 != p2) return GetCubicCurveNode(p1, p1Handle2, p2Handle1, p2, t);
				else return GetQuadraticCurveNode(p1, p1Handle2, p2, t);
			}
			else
			{
				if (p2Handle1 != p2) return GetQuadraticCurveNode(p1, p2Handle1, p2, t);
				else return GetLinearNode(p1, p2, t);
			}
		}

		/// <summary>
		///     - Gets node 't' percent along n-order curve
		/// </summary>
		/// <returns>
		///     - The node 't' percent along the curve
		/// </returns>
		/// <param name='t'>
		///     - Value between 0 and 1 representing the percent along the curve (0 = 0%, 1 = 100%)
		/// </param>
		/// <param name='nodes'>
		///     - The nodes used to define the curve
		/// </param>
		public static Vector3 GetNode(float t, params Vector3[] nodes)
		{
			t = Mathf.Clamp01(t);

			int order = nodes.Length - 1;
			Vector3 node = Vector3.zero;
			Vector3 vectorToAdd;

			for (int i = 0; i < nodes.Length; i++)
			{
				vectorToAdd = nodes[nodes.Length - i - 1] * (BinomialCoefficient(i, order) * Mathf.Pow(t, order - i) * Mathf.Pow((1 - t), i));
				node += vectorToAdd;
			}

			return node;
		}
		/// <summary>
		///     - Gets the node 't' percent along a third-order curve
		/// </summary>
		/// <returns>
		///     - The node 't' percent along the curve
		/// </returns>
		/// <param name='p1'>
		///     - The node at the beginning of the curve
		/// </param>
		/// <param name='p2'>
		///     - The second node along the curve
		/// </param>
		/// <param name='p3'>
		///     - The third node along the curve
		/// </param>
		/// <param name='p4'>
		///     - The node at the end of the curve
		/// </param>
		/// <param name='t'>
		///     - Value between 0 and 1 representing the percent along the curve (0 = 0%, 1 = 100%)
		/// </param>
		public static Vector3 GetCubicCurveNode(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float t)
		{
			t = Mathf.Clamp01(t);

			Vector3 part1 = Mathf.Pow(1 - t, 3) * p1;
			Vector3 part2 = 3 * Mathf.Pow(1 - t, 2) * t * p2;
			Vector3 part3 = 3 * (1 - t) * Mathf.Pow(t, 2) * p3;
			Vector3 part4 = Mathf.Pow(t, 3) * p4;

			return part1 + part2 + part3 + part4;
		}

		/// <summary>
		///     - Gets the node 't' percent along a second-order curve
		/// </summary>
		/// <returns>
		///     - The node 't' percent along the curve
		/// </returns>
		/// <param name='p1'>
		///     - The node at the beginning of the curve
		/// </param>
		/// <param name='p2'>
		///     - The second node along the curve
		/// </param>
		/// <param name='p3'>
		///     - The node at the end of the curve
		/// </param>
		/// <param name='t'>
		///     - Value between 0 and 1 representing the percent along the curve (0 = 0%, 1 = 100%)
		/// </param>
		public static Vector3 GetQuadraticCurveNode(Vector3 p1, Vector3 p2, Vector3 p3, float t)
		{
			t = Mathf.Clamp01(t);

			Vector3 part1 = Mathf.Pow(1 - t, 2) * p1;
			Vector3 part2 = 2 * (1 - t) * t * p2;
			Vector3 part3 = Mathf.Pow(t, 2) * p3;

			return part1 + part2 + part3;
		}

		/// <summary>
		///     - Gets node 't' percent along a linear "curve" (line)
		///     - This is exactly equivalent to Vector3.Lerp
		/// </summary>
		/// <returns>
		///             - The node 't' percent along the curve
		/// </returns>
		/// <param name='p1'>
		///     - The node at the beginning of the line
		/// </param>
		/// <param name='p2'>
		///     - The node at the end of the line
		/// </param>
		/// <param name='t'>
		///     - Value between 0 and 1 representing the percent along the line (0 = 0%, 1 = 100%)
		/// </param>
		public static Vector3 GetLinearNode(Vector3 p1, Vector3 p2, float t)
		{
			return p1 + ((p2 - p1) * t);
		}

		/// <summary>
		///     - Approximates the length
		/// </summary>
		/// <returns>
		///     - The approximate length
		/// </returns>
		/// <param name='p1'>
		///     - The bezier node at the start of the curve
		/// </param>
		/// <param name='p2'>
		///     - The bezier node at the end of the curve
		/// </param>
		/// <param name='numNodes'>
		///     - The number of nodes along the curve used to create measurable segments
		/// </param>
		public static float ApproximateLength(BezierNode p1, BezierNode p2, int numNodes = 10)
		{
			return ApproximateLength(p1.position, p1.globalHandle2, p2.position, p2.globalHandle1, numNodes);
		}

		public static float ApproximateLength(Vector3 p1, Vector3 p1Handle2, Vector3 p2, Vector3 p2Handle1, int numNodes = 10)
		{
			float _res = numNodes;
			float total = 0;
			Vector3 lastPosition = p1;
			Vector3 currentPosition;

			for (int i = 0; i < numNodes + 1; i++)
			{
				currentPosition = GetNode(p1, p1Handle2, p2, p2Handle1, i / _res);
				total += (currentPosition - lastPosition).magnitude;
				lastPosition = currentPosition;
			}

			return total;
		}

		public static float ApproximateLength(BezierNode p1, BezierNode p2, float resolution = 0.5f)
		{
			int numNodes = GetNumNodes(p1, p2, resolution);
			return ApproximateLength(p1, p2, numNodes);
		}

		public static float ApproximateLength(Vector3 p1, Vector3 p1Handle2, Vector3 p2, Vector3 p2Handle1, float resolution = 0.5f)
		{
			int numNodes = GetNumNodes(p1, p1Handle2, p2, p2Handle1, resolution);
			return ApproximateLength(p1, p1Handle2, p2, p2Handle1, numNodes);
		}

		/// <summary>
		/// Returns the number of nodes required to interpolate the given bezier nodes to a given resolution.
		/// </summary>
		public static int GetNumNodes(BezierNode p1, BezierNode p2, float resolution)
		{
			return GetNumNodes(p1.position, p1.globalHandle2, p2.position, p2.globalHandle1, resolution);
		}

		public static int GetNumNodes(Vector3 p1, Vector3 p1Handle2, Vector3 p2, Vector3 p2Handle1, float resolution)
		{
			float length = ApproximateLength(p1, p1Handle2, p2, p2Handle1, 5); // TODO: check tolerance.
			int numNodes = Mathf.RoundToInt(length * resolution);
			return Math.Max(2, numNodes);
		}

		#endregion

		#region Utility Functions
		public static List<CurvePoint> MergeCurvePointsByDistance (List<CurvePoint> points, float mergingPositionDistance) {
			List<CurvePoint> newPoints = new List<CurvePoint> ();
			int referenceIndex;
			float referencePosition;
			bool shouldMerge = false;
			for (int i = 0; i < points.Count; i++) {
				referenceIndex = i;
				referencePosition = points[i].relativePosition;
				while (i != points.Count - 1 && points [i + 1].relativePosition - referencePosition < mergingPositionDistance) {
					shouldMerge = true;
					i++;
				}
				if (shouldMerge) {
					points [referenceIndex].forward = (points [referenceIndex].forward + points [i].forward) / 2f;
					points [referenceIndex].girth = (points [referenceIndex].girth + points [i].girth) / 2f;
					points [referenceIndex].lengthPosition = (points [referenceIndex].lengthPosition + points [i].lengthPosition) / 2f;
					points [referenceIndex].normal = (points [referenceIndex].normal + points [i].normal) / 2f;
					points [referenceIndex].position = (points [referenceIndex].position + points [i].position) / 2f;
					points [referenceIndex].relativePosition = (points [referenceIndex].relativePosition + points [i].relativePosition) / 2f;
					points [referenceIndex].tangent = (points [referenceIndex].tangent + points [i].tangent) / 2f;
					points [referenceIndex].up = (points [referenceIndex].up + points [i].up) / 2f;
					shouldMerge = false;
				}
				newPoints.Add (points [referenceIndex]);
			}
			newPoints [newPoints.Count - 1].relativePosition = 1;
			return newPoints;
		}
		private static int BinomialCoefficient(int i, int n)
		{
			return Factoral(n) / (Factoral(i) * Factoral(n - i));
		}
		private static int Factoral(int i)
		{
			if (i == 0) return 1;
			int total = 1;
			while (i - 1 >= 0) {
				total *= i;
				i--;
			}
			return total;
		}
		#endregion

		#region Cloning and Copying
		public BezierCurve Clone () {
			BezierCurve clone = new BezierCurve ();
			clone.guid = guid;
			clone.closed = _closed;
			clone.axis = _axis;
			for (int i = 0; i < nodes.Count; i++) {
				clone.AddNode (nodes[i].Clone (), false);
			}
			clone.noiseFactorAtFirstNode = noiseFactorAtFirstNode;
			clone.noiseFactorAtLastNode = noiseFactorAtLastNode;
			clone.noiseScaleAtFirstNode = noiseScaleAtFirstNode;
			clone.noiseScaleAtLastNode = noiseScaleAtLastNode;
			clone.noiseLengthOffset = noiseLengthOffset;
			clone.spareNoiseOffsetAtFirstPoint = spareNoiseOffsetAtFirstPoint;
			clone.Process ();
			return clone;
		}
		public BezierCurve Copy () {
			BezierCurve copy = new BezierCurve ();
			copy.closed = _closed;
			copy.axis = _axis;
			for (int i = 0; i < nodes.Count; i++) {
				copy.AddNode (nodes[i].Copy (), false);
			}
			copy.noiseFactorAtFirstNode = noiseFactorAtFirstNode;
			copy.noiseFactorAtLastNode = noiseFactorAtLastNode;
			copy.noiseScaleAtFirstNode = noiseScaleAtFirstNode;
			copy.noiseScaleAtLastNode = noiseScaleAtLastNode;
			copy.noiseLengthOffset = noiseLengthOffset;
			copy.spareNoiseOffsetAtFirstPoint = spareNoiseOffsetAtFirstPoint;
			copy.Process ();
			return copy;
		}
		#endregion
		public Vector3 GetPositionAtLength (float _length) {
			return GetPositionAt (_length / length);
		}
		public Vector3 FindIntersectionXZ (Vector3 p1, Vector3 p2, out bool intersect, int resolution = 2) {
			Vector3 intersection = Vector3.zero;
			intersect = false;
			if (resolution < 2) {
				resolution = 2;
			}
			float segmentLength = 1f / (float)resolution;
			for (int i = 0; i < resolution; i++) {
				FindIntersection (p1, p2, GetPositionAt (i * segmentLength), GetPositionAt ((i + 1) * segmentLength), out intersect, out intersection);
				if (intersect) break;
			}
			return intersection;
		}
		// Find the point of intersection between
		// the lines p1 --> p2 and p3 --> p4.
		private bool FindIntersection (
			Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4,
			out bool segmentsIntersect,
			out Vector3 intersection)
		{
			// Get the segments' parameters.
			float dx12 = p2.x - p1.x;
			float dy12 = p2.z - p1.z;
			float dx34 = p4.x - p3.x;
			float dy34 = p4.z - p3.z;
			bool linesIntersect = false;
			//Vector3 close_p1, close_p2;

			// Solve for t1 and t2
			float denominator = (dy12 * dx34 - dx12 * dy34);

			float t1 =
				((p1.x - p3.x) * dy34 + (p3.z - p1.z) * dx34)
					/ denominator;
			if (float.IsInfinity(t1))
			{
				// The lines are parallel (or close enough to it).
				linesIntersect = false;
				segmentsIntersect = false;
				intersection = new Vector3(float.NaN, float.NaN);
				//close_p1 = new Vector3(float.NaN, float.NaN);
				//close_p2 = new Vector3(float.NaN, float.NaN);
				return linesIntersect;
			}
			linesIntersect = true;

			float t2 =
				((p3.x - p1.x) * dy12 + (p1.z - p3.z) * dx12)
					/ -denominator;

			// Find the point of intersection.
			intersection = new Vector3(p1.x + dx12 * t1, p1.z + dy12 * t1);

			// The segments intersect if t1 and t2 are between 0 and 1.
			segmentsIntersect =
				((t1 >= 0) && (t1 <= 1) &&
				(t2 >= 0) && (t2 <= 1));

			// Find the closest points on the segments.
			if (t1 < 0)
			{
				t1 = 0;
			}
			else if (t1 > 1)
			{
				t1 = 1;
			}

			if (t2 < 0)
			{
				t2 = 0;
			}
			else if (t2 > 1)
			{
				t2 = 1;
			}

			//close_p1 = new Vector3(p1.x + dx12 * t1, p1.z + dy12 * t1);
			//close_p2 = new Vector3(p3.x + dx34 * t2, p3.z + dy34 * t2);
			return linesIntersect;
		}
		/*
		// Find the point of intersection between
		// the lines p1 --> p2 and p3 --> p4.
		private void FindIntersection(
			PointF p1, PointF p2, PointF p3, PointF p4,
			out bool lines_intersect, out bool segments_intersect,
			out PointF intersection,
			out PointF close_p1, out PointF close_p2)
		{
			// Get the segments' parameters.
			float dx12 = p2.X - p1.X;
			float dy12 = p2.Y - p1.Y;
			float dx34 = p4.X - p3.X;
			float dy34 = p4.Y - p3.Y;

			// Solve for t1 and t2
			float denominator = (dy12 * dx34 - dx12 * dy34);

			float t1 =
				((p1.X - p3.X) * dy34 + (p3.Y - p1.Y) * dx34)
					/ denominator;
			if (float.IsInfinity(t1))
			{
				// The lines are parallel (or close enough to it).
				lines_intersect = false;
				segments_intersect = false;
				intersection = new PointF(float.NaN, float.NaN);
				close_p1 = new PointF(float.NaN, float.NaN);
				close_p2 = new PointF(float.NaN, float.NaN);
				return;
			}
			lines_intersect = true;

			float t2 =
				((p3.X - p1.X) * dy12 + (p1.Y - p3.Y) * dx12)
					/ -denominator;

			// Find the point of intersection.
			intersection = new PointF(p1.X + dx12 * t1, p1.Y + dy12 * t1);

			// The segments intersect if t1 and t2 are between 0 and 1.
			segments_intersect =
				((t1 >= 0) && (t1 <= 1) &&
				(t2 >= 0) && (t2 <= 1));

			// Find the closest points on the segments.
			if (t1 < 0)
			{
				t1 = 0;
			}
			else if (t1 > 1)
			{
				t1 = 1;
			}

			if (t2 < 0)
			{
				t2 = 0;
			}
			else if (t2 > 1)
			{
				t2 = 1;
			}

			close_p1 = new PointF(p1.X + dx12 * t1, p1.Y + dy12 * t1);
			close_p2 = new PointF(p3.X + dx34 * t2, p3.Y + dy34 * t2);
		}
		*/
		/*
		public Vector3 GetNodeAtLength (float distance) {
			if (close) {
				if(distance < 0) while(distance < 0) { distance += _length; }
				else if(distance > _length) while(distance > _length) { distance -= _length; }
			} else {
				if(distance <= 0) return nodes[0].position;
				else if(distance >= _length) return nodes[nodes.Length - 1].position;
			}
			float totalLength = 0;
			float curveLength = 0;
			BezierNode firstNode = null;
			BezierNode secondNode = null;
			for(int i = 0; i < nodes.Length - 1; i++) {
				curveLength = ApproximateLength(nodes[i], nodes[i + 1], resolution);
				if(totalLength + curveLength >= distance) {
					firstNode = nodes[i];
					secondNode = nodes[i+1];
					break;
				}
				else totalLength += curveLength;
			}
			if(firstNode == null) {
				firstNode = nodes[nodes.Length - 1];
				secondNode = nodes[0];
				curveLength = ApproximateLength(firstNode, secondNode, resolution);
			}
			distance -= totalLength;
			return GetNode (firstNode, secondNode, distance / curveLength);
		}
		*/
		public Vector3 FindNearestPointTo (Vector3 worldPos, float accuracy = 100f ) {
			float normalizedT;
			return FindNearestPointTo( worldPos, out normalizedT, accuracy );
		}
		public Vector3 FindNearestPointTo (Vector3 worldPos, out float normalizedT, float lowerLimit = 0f, float upperLimit = 1f, float accuracy = 100f)	{
			if (nodes.Count <= 1) {
				normalizedT = 1f;
				return Vector3.zero;
			}
			Vector3 result = Vector3.zero;
			normalizedT = -1f;

			float step = AccuracyToStepSize( accuracy );

			float minDistance = Mathf.Infinity;
			for( float i = lowerLimit; i < upperLimit; i += step )
			{
				Vector3 thisNode = GetPositionAt ( i );
				float thisDistance = ( worldPos - thisNode ).sqrMagnitude;
				if( thisDistance < minDistance )
				{
					minDistance = thisDistance;
					result = thisNode;
					normalizedT = i;
				}
			}

			return result;
		}
		public CurvePoint FindNearestPointToY (float yPos, float accuracy = 100f )	{
			float normalizedT = 0f;
			if (nodes.Count <= 1) {
				normalizedT = 1f;
				return points [0];
			}
			CurvePoint result = null;
			normalizedT = -1f;

			float step = AccuracyToStepSize( accuracy );

			float minDistance = Mathf.Infinity;
			for( float i = 0f; i < 1f; i += step )
			{
				Vector3 thisNode = GetPositionAt ( i );
				float thisDistance = Mathf.Abs (yPos - thisNode.y );
				if( thisDistance < minDistance )
				{
					minDistance = thisDistance;
					normalizedT = i;
				}
			}
			result = GetPointAt (normalizedT);

			return result;
		}
		public Bounds GetBounds () {
			Bounds bounds = new Bounds ();
			if (nodes.Count > 0) {
				bounds.center = nodes[0].position;
				if (nodes.Count > 1) {
					float resolution = 0.1f;
					for (float i = resolution; i < 1f; i += resolution) {
						Vector3 node = GetPositionAt (i);
						bounds.Encapsulate (node);
					}
					bounds.Encapsulate (nodes [nodes.Count - 1].position);
				}
			}
			return bounds;
		}
		private float AccuracyToStepSize( float accuracy )
		{
			if( accuracy <= 0f )
				return 0.2f;

			return Mathf.Clamp( 1f / accuracy, 0.001f, 0.2f );
		}
		
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
			for (int i = 0; i < nodes.Count; i++) {
				nodes [i].curve = this;
			}
		}
		#endregion
	}
}