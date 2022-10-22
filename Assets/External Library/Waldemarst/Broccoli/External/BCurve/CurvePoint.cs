using UnityEngine;

namespace Broccoli.Model {
    /// <summary>
    /// Imutable class containing all data about a point on a cubic bezier curve.
    /// </summary>
    public class CurvePoint {
        public Vector3 position;
        public Vector3 tangent;
        public Vector3 up;
        public Vector2 scale;
        public Vector3 normal;
        public Vector3 forward;
        public float roll;
        public float girth;
        public float lengthPosition;
        public float relativePosition;
        public float timePosition;
        public float perlinNoiseX = 0f;
        public float perlinNoiseY = 0f;
        public float perlinNoiseAvg = 0f;

        private Quaternion rotation = Quaternion.identity;

        /// <summary>
        /// Rotation is a look-at quaternion calculated from the tangent, roll and up vector. Mixing non zero roll and custom up vector is not advised.
        /// </summary>
        public Quaternion Rotation {
            get {
                if (rotation.Equals(Quaternion.identity)) {
                    var upVector = Vector3.Cross(tangent, Vector3.Cross(Quaternion.AngleAxis(roll, Vector3.forward) * up, tangent).normalized);
                    rotation =  Quaternion.LookRotation(tangent, upVector);
                }
                return rotation;
            }
        }

        public CurvePoint (Vector3 location,
            Vector3 tangent, 
            Vector3 forward, 
            Vector3 normal, 
            Vector3 up, 
            Vector2 scale, 
            float girth, 
            float roll, 
            float distanceInCurve,
            float positionInCurve,
            float timeInCurve) 
        {
            this.position = location;
            this.tangent = tangent;
            this.forward = forward;
            this.normal = normal;
            this.up = up;
            this.girth = girth;
            this.roll = roll;
            this.scale = scale;
            this.lengthPosition = distanceInCurve;
            this.relativePosition = positionInCurve;
            this.timePosition = timeInCurve;
        }

        /// <summary>
        /// Linearly interpolates between two curve samples.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static CurvePoint Lerp(CurvePoint a, CurvePoint b, float t) {
            CurvePoint point = new CurvePoint(
                Vector3.Lerp(a.position, b.position, t),
                Vector3.Lerp(a.tangent, b.tangent, t).normalized,
                Vector3.Lerp(a.forward, b.forward, t),
                Vector3.Lerp(a.normal, b.normal, t),
                Vector3.Lerp(a.up, b.up, t),
                Vector2.Lerp(a.scale, b.scale, t),
                Mathf.Lerp(a.girth, b.girth, t),
                Mathf.Lerp(a.roll, b.roll, t),
                Mathf.Lerp(a.lengthPosition, b.lengthPosition, t),
                Mathf.Lerp(a.relativePosition, b.relativePosition, t),
                Mathf.Lerp(a.timePosition, b.timePosition, t));
            point.perlinNoiseX = Mathf.Lerp (a.perlinNoiseX, b.perlinNoiseX, t);
            point.perlinNoiseY = Mathf.Lerp (a.perlinNoiseY, b.perlinNoiseY, t);
            return point;
        }
        #region Cloning
        public CurvePoint Clone () {
            CurvePoint clone = new CurvePoint (position, 
                tangent, forward, normal, up, scale, girth, roll, 
                lengthPosition, relativePosition, timePosition);
            clone.perlinNoiseX = perlinNoiseX;
            clone.perlinNoiseY = perlinNoiseY;
            return clone;
        }
        #endregion
    }
}
