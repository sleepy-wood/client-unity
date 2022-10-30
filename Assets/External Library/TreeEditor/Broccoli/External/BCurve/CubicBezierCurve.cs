using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Broccoli.Model {
    [Serializable]
    public class CubicBezierCurve {
        #region Vars
        /// <summary>
        /// Keeps the number of steps to subdivide this curve.
        /// </summary>
        private int _stepCount = 30;
        /// <summary>
        /// Keeps the resolution of every step on this curve.
        /// </summary>
        private float _step = 1.0f / 30;
        /// <summary>
        /// Number of steps to subdivide this curve when processing its points.
        /// </summary>
        public int stepCount {
            get { return _stepCount; }
            set {
                if (value <= 0) _stepCount = 30;
                else _stepCount = value;
                _step = 1.0f / _stepCount;
            }
        }
        /// <summary>
        /// Resolution or relative distance between the points when processing this curve.
        /// </summary>
        public float step {
            get {
                if (_step < 0) _step = 1.0f / _stepCount;
                return _step;
            }
        }
        /*
        private const int STEP_COUNT = 30;
        private const float T_STEP = 1.0f / STEP_COUNT;
        */
        /// <summary>
        /// Number of samples to get from the curve when processing it.
        /// </summary>
        public int sampleStepCount = 25;
        /// <summary>
        /// Resolution size of every sample of the curve.
        /// </summary>
        public float sampleStep {
            get {
                return 1.0f/ (sampleStepCount>0?sampleStepCount:25);
            }
        }
        public List<CurvePoint> samples = new List<CurvePoint> ();
        public BezierNode n1, n2;
        /// <summary>
		/// Perlin noise strength to use at the begining of the curve.
		/// </summary>
		public float noiseFactorAtFirstNode = 0f;
		/// <summary>
		/// Perlin noise strength to use at the end of the curve.
		/// </summary>
		public float noiseFactorAtLastNode = 0f;
        /// <summary>
        /// Check if noise has to be applied to this cubic bezier curve.
        /// </summary>
        /// <value></value>
        public bool hasNoise {
            get { return noiseFactorAtFirstNode != 0 || noiseFactorAtLastNode != 0; }
        }
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
        /// Length of the curve in world unit.
        /// </summary>
        public float length { get; private set; }
        /// <summary>
        /// True to automatically process changes made to this cubic curve.
        /// </summary>
        [NonSerialized]
        public bool autoProcess = true;
        #endregion

        #region Events
        /// <summary>
        /// This event is raised when of of the control points has moved.
        /// </summary>
        [NonSerialized]
        public UnityEvent onChange = new UnityEvent();
        #endregion

        #region Constructor
        /// <summary>
        /// Build a new cubic Bézier curve between two given spline node.
        /// </summary>
        /// <param name="n1"></param>
        /// <param name="n2"></param>
        public CubicBezierCurve(BezierNode n1, BezierNode n2, int resolutionSteps = 30, bool autoProcess = true) {
            this.stepCount = resolutionSteps;
            this.n1 = n1;
            this.n2 = n2;
            n1.ClearOnChange ();
            n1.onChange += ComputeSamplesRequest;
            n2.ClearOnChange ();
            n2.onChange += ComputeSamplesRequest;
            this.autoProcess = autoProcess;
            if (autoProcess)
                ComputeSamples();
        }
        #endregion

        #region Processing
        public void ComputeSamplesRequest (object sender, EventArgs e) {
            if (autoProcess) ComputeSamples ();
        }
        public void ComputeSamples() {
            samples.Clear();
            length = 0;
            Vector3 previousPosition = GetLocation(0);
            CurvePoint _point = null;
            for (float t = 0; t < 1; t += _step) {
                Vector3 position = GetLocation(t);
                length += Vector3.Distance(previousPosition, position);
                previousPosition = position;
                _point = CreateSample(length, t);
                samples.Add(_point);
            }
            length += Vector3.Distance(previousPosition, GetLocation(1));
            _point = CreateSample(length, 1);
            samples.Add(_point);

            // Normalize position in curve
            for (int i = 0; i < samples.Count; i++) {
                samples [i].relativePosition = samples [i].lengthPosition / length;
            }

            if (hasNoise) {
                for (int i = (spareNoiseOffsetAtFirstPoint?1:0); i < samples.Count; i++) {
                    ApplyNoise (samples [i]);
                }
            }

            onChange?.Invoke();
        }
        public CurvePoint CreateSample (float distance, float time) {
            Vector3 tangent;
            if (time == 0) {
                tangent = GetTangent (0.01f);
            } else {
                tangent = GetTangent (time);
            }
            Vector3 forward = GetForward (tangent);
            CurvePoint _curvePoint = new CurvePoint (
                GetLocation (time),
                tangent,
                forward,
                //GetNormal (forward, tangent, (previousPoint!=null?previousPoint.normal:Vector3.zero)),
                GetNormalFromPlane (n1.position, n2.position, time),
                GetUp (time),
                GetScale (time),
                GetGirth (time),
                GetRoll (time),
                distance,
                time,
                time);
            //Debug.Log ("normal: " + _curvePoint.normal.ToString("F5"));
            return _curvePoint;
        }
        private void ApplyNoise (CurvePoint curvePoint) {
            // Apply offset.
            Vector3 offset = new Vector3 (
                Mathf.PerlinNoise ((curvePoint.lengthPosition + noiseLengthOffset + 0.5f) * Mathf.Lerp (noiseScaleAtFirstNode, noiseScaleAtLastNode, curvePoint.relativePosition), 0) - 0.5f,
                0f,
                Mathf.PerlinNoise (0, (curvePoint.lengthPosition + noiseLengthOffset) * Mathf.Lerp (noiseScaleAtFirstNode, noiseScaleAtLastNode, curvePoint.relativePosition)) - 0.5f
            );
            offset *= Mathf.Lerp (noiseFactorAtFirstNode, noiseFactorAtLastNode, curvePoint.relativePosition);
            offset = Quaternion.FromToRotation (Vector3.up, curvePoint.forward) * offset;
            curvePoint.position += offset;
        }
        /// <summary>
        /// Convinent method to get the third control point of the curve, as the direction of the end spline node indicates the starting tangent of the next curve.
        /// </summary>
        /// <returns></returns>
        public Vector3 GetInverseDirection () {
            return GetInverseDirection (n2);
        }
        public static Vector3 GetInverseDirection (BezierNode node) {
            return (2 * node.position) - (node.position - node.handle1);
        }
        /// <summary>
        /// Returns point on curve at given time. Time must be between 0 and 1.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private Vector3 GetLocation (float t) {
            float omt = 1f - t;
            float omt2 = omt * omt;
            float t2 = t * t;
            return
                n1.position * (omt2 * omt) +
                (n1.position + n1.handle2) * (3f * omt2 * t) +
                GetInverseDirection () * (3f * omt * t2) +
                n2.position * (t2 * t);
        }
        /// <summary>
        /// Returns tangent of curve at given time. Time must be between 0 and 1.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private Vector3 GetTangent (float t) {
            return GetTangent (t, n1, n2);
        }
        public static Vector3 GetTangent (float t, BezierNode n1, BezierNode n2) {
            float omt = 1f - t;
            float omt2 = omt * omt;
            float t2 = t * t;
            Vector3 tangent =
                n1.position * (-omt2) +
                (n1.position + n1.handle2) * (3 * omt2 - 2 * omt) +
                GetInverseDirection (n2) * (-3 * t2 + 2 * t) +
                n2.position * (t2);
            return tangent;
        }
        private Vector3 GetForward (Vector3 tangent) {
            if (tangent != Vector3.zero) {
                return Quaternion.LookRotation (tangent) * Vector3.forward;
            }
            return (n2.position - n1.position).normalized;
        }
        private Vector3 GetNormalFromPlane (Vector3 nodeA, Vector3 nodeB, float time) {
            if (time == 0) {
                time = 0.01f;
            } else if (time == 1) {
                time = 0.99f;
            }
            Vector3 point = GetLocation (time);
            Vector3 _normal = Vector3.Cross ((nodeA - point), (nodeB - point));
            if (_normal == Vector3.zero) {
                _normal = Quaternion.LookRotation (nodeB - nodeA) * Vector3.up; // TODO: see if this is slowing down the process.
            }
            return _normal.normalized;
        }
        private Vector3 GetNormal (Vector3 forward, Vector3 tangent, Vector3 previousNormal) {
            Vector3 normal = Quaternion.LookRotation (forward, tangent) * Vector3.up;
            float angle = Vector3.Angle(normal, previousNormal);
            if (angle > 45) {
                normal *= -1;
            }
            return normal;
            /*
            if (forward != Vector3.zero) {
                return Quaternion.LookRotation (forward, Vector3.up) * Vector3.up;
            }
            return Quaternion.LookRotation (Vector3.up) * (n2.position - n1.position).normalized;
            */
        }
        private Vector3 GetUp (float t) {
            return Vector3.Lerp (n1.up, n2.up, t);
        }
        private Vector2 GetScale (float t) {
            return Vector2.Lerp (n1.scale, n2.scale, t);
        }
        private float GetGirth (float t) {
            //return Vector2.Lerp (n1.scale, n2.scale, t);
            return 1f;
        }
        private float GetRoll (float t) {
            return Mathf.Lerp (n1.roll, n2.roll, t);
        }
        /// <summary>
        /// Returns an interpolated sample of the curve, containing all curve data at this time.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public CurvePoint GetSample (float time) {
            AssertTimeInBounds (time);
            CurvePoint previous = samples[0];
            CurvePoint next = null;
            foreach (CurvePoint cp in samples) {
                if (cp.relativePosition >= time) {
                    next = cp;
                    break;
                }
                previous = cp;
            }
            if (next == null) {
                throw new Exception ("Can't find curve samples.");
            }
            float t = next == previous ? 0 : (time - previous.relativePosition) / (next.relativePosition - previous.relativePosition);

            return CurvePoint.Lerp (previous, next, t);
        }

        /// <summary>
        /// Returns an interpolated sample of the curve, containing all curve data at this distance.
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public CurvePoint GetSampleAtDistance(float d) {
            if (d < 0) {
                CurvePoint negativePoint = samples[0].Clone ();
                negativePoint.position = negativePoint.position + (negativePoint.forward.normalized * d);
                negativePoint.lengthPosition = d;
                negativePoint.relativePosition = d / length;
                return negativePoint;
            }
            if (d > length) {
                CurvePoint positivePoint = samples[samples.Count - 1].Clone ();
                positivePoint.position = positivePoint.position + (positivePoint.forward.normalized * (d - length));
                positivePoint.lengthPosition = d;
                positivePoint.relativePosition = d / length;
                return positivePoint;
            }

            CurvePoint previous = samples[0];
            CurvePoint next = null;
            foreach (CurvePoint cp in samples) {
                if (cp.lengthPosition >= d) {
                    next = cp;
                    break;
                }
                previous = cp;
            }
            if (next == null) {
                throw new Exception("Can't find curve samples.");
            }
            float t = next == previous ? 0 : (d - previous.lengthPosition) / (next.lengthPosition - previous.lengthPosition);

            return CurvePoint.Lerp(previous, next, t);
        }
        private static void AssertTimeInBounds(float time) {
            if (time < 0 || time > 1) throw new ArgumentException("Time must be between 0 and 1 (was " + time + ").");
        }
        #endregion

        #region Node Methods
        /// <summary>
        /// Change the start node of the curve.
        /// </summary>
        /// <param name="n1"></param>
        public void ConnectStart(BezierNode n1) {
            this.n1.onChange -= ComputeSamplesRequest;
            this.n1 = n1;
            n1.onChange += ComputeSamplesRequest;
            if (autoProcess)
                ComputeSamples();
        }

        /// <summary>
        /// Change the end node of the curve.
        /// </summary>
        /// <param name="n2"></param>
        public void ConnectEnd(BezierNode n2) {
            this.n2.onChange -= ComputeSamplesRequest;
            this.n2 = n2;
            n2.onChange += ComputeSamplesRequest;
            if (autoProcess)
                ComputeSamples ();
        }
        #endregion
    }
}
