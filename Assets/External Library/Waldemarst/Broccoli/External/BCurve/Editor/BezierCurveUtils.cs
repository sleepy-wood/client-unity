using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using Broccoli.Model;

namespace Broccoli.Utils
{
	public class BezierCurveUtils {
		#region Drawing
		private const int RESOLUTION_TO_NUMPOINTS_FACTOR = 3;
		public static void DrawCurve (BezierCurve curve, float resolution = 5f) {
			if (curve.points.Count > 1) {
				for (int i = 0; i < curve.points.Count - 1; i++) {
					DrawCurve (curve[i], curve[i + 1], resolution);
				}
				if (curve.closed) DrawCurve (curve[curve.points.Count - 1], curve[0], resolution);
			}
		}
		/// <summary>
		/// Draws a curve between two points.
		/// </summary>
		/// <param name="p1">The bezier point at the beginning of the curve.</param>
		/// <param name="p2">The bezier point at the end of the curve.</param>
		/// <param name="resolution">The number of segments along the curve to draw.</param>
		public static void DrawCurve (BezierNode p1, BezierNode p2, float resolution = 5f) {
			var interpolated = InterpolateResolution (p1.position, p1.globalHandle2, p2.position, p2.globalHandle1, resolution);

			Vector3 lastPoint = interpolated[0];
			Vector3 currentPoint = Vector3.zero;
			//Vector3[] drawingPoints = new Vector3[2];
			for (int i = 1; i < interpolated.Length; i++)
			{
				currentPoint = interpolated[i];
				Handles.DrawLine (lastPoint, currentPoint);
				lastPoint = currentPoint;
			}
		}
		/*
		/// <summary>
		/// Draws a bezier curve with a specific color and a width.
		/// </summary>
		/// <param name="curve">Bezier curve to draw.</param>
		/// <param name="color">Line color.</param>
		/// <param name="width">Line width.</param>
		/// <param name="resolution">Resolution to get the number of points.</param>
		public static void DrawCurve (BezierCurve curve, Color color, float resolution = 5f) {
			//Vector3[] points = GetPoints (curve, resolution);
			DrawCurve (curve.vectorPoints, color);
		}
		*/
		/// <summary>
		/// Draws a bezier curve using and offset with a specific color and a width.
		/// </summary>
		/// <param name="curve">Bezier curve to draw.</param>
		/// <param name="offset">Offset to apply to drawing, not affected by scaling.</param>
		/// <param name="scale">Scale.</param>
		/// <param name="color">Line color.</param>
		/// <param name="width">Line width.</param>
		/// <param name="resolution">Resolution to get the number of points.</param>
		public static void DrawCurve (BezierCurve curve, Vector3 offset, float scale,
			Color color, float width = 1f, float resolution = 5f)
		{
			//Vector3[] points = GetPoints (curve, resolution); // TODO
			DrawCurve (curve.points, offset, scale, color, width);
		}
		/// <summary>
		/// Draws a curve using an array of points.
		/// </summary>
		/// <param name="points">Array of points.</param>
		/// <param name="color">Line color.</param>
		public static void DrawCurve (List<Vector3> points, Color color) {
			Handles.color = color;
			for (int i = 0; i < points.Count - 1; i++) {
				Handles.DrawLine (points [i], points [i+1]);
			}
		}
		/// <summary>
		/// Draws a line between two points.
		/// </summary>
		/// <param name="pointA">Point A.</param>
		/// <param name="pointB">Point B.</param>
		/// <param name="color">Line color.</param>
		/// <param name="width">Line width.</param>
		public static void DrawLine (Vector3 pointA, Vector3 pointB, Color color, float width = 1f) {
			if (width == 1f) {
				Handles.color = color;
				Handles.DrawLine (pointA, pointB);
			} else {
				//Handles.DrawAAPolyLine (EditorDrawUtils.GetInstance().GetColoredTexture (color), width, pointA, pointB);
			}
		}
		/// <summary>
		/// Draws a curve using an array of points.
		/// </summary>
		/// <param name="points">Array of points.</param>
		/// <param name="offset">Offset ot apply to the points.</param>
		/// <param name="scale">Scale.</param>
		/// <param name="color">Line color.</param>
		/// <param name="width">Line width.</param>
		public static void DrawCurve (List<CurvePoint> points, Vector3 offset, float scale, Color color, float width = 1f) {
			if (width == 1f) {
				Handles.color = color;
				for (int i = 0; i < points.Count - 1; i++) {
					Handles.DrawLine ((points [i].position * scale) + offset, (points [i+1].position * scale) + offset);
				}
			} else {
				//Handles.DrawAAPolyLine (EditorDrawUtils.GetInstance().GetColoredTexture (color), width, points); TODO
			}
		}
		/// <summary>
		/// Draws a convex polygon using handles.
		/// </summary>
		/// <param name="points">List of points in the convex polygon.</param>
		/// <param name="color">Color of the polygon.</param>
		public static void DrawConvexPolygon (Vector3[] points, Color color) {
			Handles.color = color;
			Handles.DrawAAConvexPolygon (points);
		}
		#endregion

		#region Traversing
		
		public static Vector3[] GetPoints (BezierNode p1, BezierNode p2, float resolution = 5f) {
			Vector3[] interpolated = InterpolateResolution (p1.position, p1.globalHandle2, p2.position, p2.globalHandle1, resolution);
			return interpolated;
		}
		/// <summary>
		/// All arguments are global positions. Returns 'numPoints + 1' interpolated points from 'p1' until 'p2', inclusive.
		/// </summary>
		public static Vector3[] Interpolate(Vector3 p1, Vector3 p1Handle2, Vector3 p2, Vector3 p2Handle1, int numPoints)
		{
			var points = new Vector3[numPoints + 1];
			points[0] = p1;
			points[points.Length - 1] = p2;
			float _res = numPoints;
			for (int i = 1; i < points.Length - 1; i++) {
				points[i] = GetPoint(p1, p1Handle2, p2, p2Handle1, i / _res);
			}
			return points;
		}
		public static Vector3[] InterpolateResolution (Vector3 p1, Vector3 p1Handle2, Vector3 p2, Vector3 p2Handle1, float resolution)
		{
			int numPoints = GetNumPoints (p1, p1Handle2, p2, p2Handle1, resolution);
			return Interpolate (p1, p1Handle2, p2, p2Handle1, numPoints);
		}
		/// <summary>
		///     - Gets the point 't' percent along a curve
		///     - Automatically calculates for the number of relevant points
		/// </summary>
		/// <returns>
		///     - The point 't' percent along the curve
		/// </returns>
		/// <param name='p1'>
		///     - The bezier point at the beginning of the curve
		/// </param>
		/// <param name='p2'>
		///     - The bezier point at the end of the curve
		/// </param>
		/// <param name='t'>
		///     - Value between 0 and 1 representing the percent along the curve (0 = 0%, 1 = 100%)
		/// </param>
		public static Vector3 GetPoint (BezierNode p1, BezierNode p2, float t)
		{
			return GetPoint(p1.position, p1.globalHandle2, p2.position, p2.globalHandle1, t);
		}
		/// <summary>
		/// All arguments are global positions. Returns global position at 't' percent along the curve.
		/// </summary>
		public static Vector3 GetPoint(Vector3 p1, Vector3 p1Handle2, Vector3 p2, Vector3 p2Handle1, float t)
		{
			if (p1Handle2 != p1)
			{
				if (p2Handle1 != p2) return GetCubicCurvePoint(p1, p1Handle2, p2Handle1, p2, t);
				else return GetQuadraticCurvePoint(p1, p1Handle2, p2, t);
			}
			else
			{
				if (p2Handle1 != p2) return GetQuadraticCurvePoint(p1, p2Handle1, p2, t);
				else return GetLinearPoint(p1, p2, t);
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
		public static Vector3 GetPoint(float t, params Vector3[] nodes)
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
		public static Vector3 GetPointLocal(BezierNode n1, BezierNode n2, float t)
		{
			//Vector3 n1H1 = n1.position + n1.handle1;
			Vector3 n1H2 = n1.position + n1.handle2;
			Vector3 n2H1 = n2.position + n2.handle1;
			//Vector3 n2H2 = n2.position + n2.handle2;

			if (n1.handle2 != Vector3.zero) {
				if (n2.handle1 != Vector3.zero) return GetCubicCurvePoint(n1.position, n1H2, n2H1, n2.position, t);
				else return GetQuadraticCurvePoint(n1.position, n1H2, n2.position, t);
			} else {
				if (n2.handle1 != Vector3.zero) return GetQuadraticCurvePoint(n1.position, n2H1, n2.position, t);
				else return GetLinearPoint(n1.position, n2.position, t);
			}
		}

		/// <summary>
		///     - Gets the point 't' percent along a third-order curve
		/// </summary>
		/// <returns>
		///     - The point 't' percent along the curve
		/// </returns>
		/// <param name='p1'>
		///     - The point at the beginning of the curve
		/// </param>
		/// <param name='p2'>
		///     - The second point along the curve
		/// </param>
		/// <param name='p3'>
		///     - The third point along the curve
		/// </param>
		/// <param name='p4'>
		///     - The point at the end of the curve
		/// </param>
		/// <param name='t'>
		///     - Value between 0 and 1 representing the percent along the curve (0 = 0%, 1 = 100%)
		/// </param>
		public static Vector3 GetCubicCurvePoint (Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float t)
		{
			t = Mathf.Clamp01(t);

			Vector3 part1 = Mathf.Pow(1 - t, 3) * p1;
			Vector3 part2 = 3 * Mathf.Pow(1 - t, 2) * t * p2;
			Vector3 part3 = 3 * (1 - t) * Mathf.Pow(t, 2) * p3;
			Vector3 part4 = Mathf.Pow(t, 3) * p4;

			return part1 + part2 + part3 + part4;
		}

		/// <summary>
		///     - Gets the point 't' percent along a second-order curve
		/// </summary>
		/// <returns>
		///     - The point 't' percent along the curve
		/// </returns>
		/// <param name='p1'>
		///     - The point at the beginning of the curve
		/// </param>
		/// <param name='p2'>
		///     - The second point along the curve
		/// </param>
		/// <param name='p3'>
		///     - The point at the end of the curve
		/// </param>
		/// <param name='t'>
		///     - Value between 0 and 1 representing the percent along the curve (0 = 0%, 1 = 100%)
		/// </param>
		public static Vector3 GetQuadraticCurvePoint (Vector3 p1, Vector3 p2, Vector3 p3, float t)
		{
			t = Mathf.Clamp01(t);

			Vector3 part1 = Mathf.Pow(1 - t, 2) * p1;
			Vector3 part2 = 2 * (1 - t) * t * p2;
			Vector3 part3 = Mathf.Pow(t, 2) * p3;

			return part1 + part2 + part3;
		}

		/// <summary>
		///     - Gets point 't' percent along a linear "curve" (line)
		///     - This is exactly equivalent to Vector3.Lerp
		/// </summary>
		/// <returns>
		///             - The point 't' percent along the curve
		/// </returns>
		/// <param name='p1'>
		///     - The point at the beginning of the line
		/// </param>
		/// <param name='p2'>
		///     - The point at the end of the line
		/// </param>
		/// <param name='t'>
		///     - Value between 0 and 1 representing the percent along the line (0 = 0%, 1 = 100%)
		/// </param>
		public static Vector3 GetLinearPoint (Vector3 p1, Vector3 p2, float t)
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
		///     - The bezier point at the start of the curve
		/// </param>
		/// <param name='p2'>
		///     - The bezier point at the end of the curve
		/// </param>
		/// <param name='numPoints'>
		///     - The number of points along the curve used to create measurable segments
		/// </param>
		public static float ApproximateLength (BezierNode p1, BezierNode p2, int numPoints = 10)
		{
			return ApproximateLength(p1.position, p1.globalHandle2, p2.position, p2.globalHandle1, numPoints);
		}

		public static float ApproximateLength(Vector3 p1, Vector3 p1Handle2, Vector3 p2, Vector3 p2Handle1, int numPoints = 10)
		{
			float _res = numPoints;
			float total = 0;
			Vector3 lastPosition = p1;
			Vector3 currentPosition;

			for (int i = 0; i < numPoints + 1; i++)
			{
				currentPosition = GetPoint(p1, p1Handle2, p2, p2Handle1, i / _res);
				total += (currentPosition - lastPosition).magnitude;
				lastPosition = currentPosition;
			}

			return total;
		}

		public static float ApproximateLength(BezierNode p1, BezierNode p2, float resolution = 0.5f)
		{
			int numPoints = GetNumPoints(p1, p2, resolution);
			return ApproximateLength(p1, p2, numPoints);
		}

		public static float ApproximateLength(Vector3 p1, Vector3 p1Handle2, Vector3 p2, Vector3 p2Handle1, float resolution = 0.5f)
		{
			int numPoints = GetNumPoints(p1, p1Handle2, p2, p2Handle1, resolution);
			return ApproximateLength(p1, p1Handle2, p2, p2Handle1, numPoints);
		}

		/// <summary>
		/// Returns the number of points required to interpolate the given bezier points to a given resolution.
		/// </summary>
		public static int GetNumPoints(BezierNode p1, BezierNode p2, float resolution)
		{
			return GetNumPoints(p1.position, p1.globalHandle2, p2.position, p2.globalHandle1, resolution);
		}

		public static int GetNumPoints(Vector3 p1, Vector3 p1Handle2, Vector3 p2, Vector3 p2Handle1, float resolution)
		{
			float length = ApproximateLength(p1, p1Handle2, p2, p2Handle1, RESOLUTION_TO_NUMPOINTS_FACTOR);
			int numPoints = Mathf.RoundToInt(length * resolution);
			return Math.Max(2, numPoints);
		}

		private static int BinomialCoefficient(int i, int n) {
			return Factoral(n) / (Factoral(i) * Factoral(n - i));
		}

		private static int Factoral(int i) {
			if (i == 0) return 1;

			int total = 1;

			while (i - 1 >= 0)
			{
				total *= i;
				i--;
			}

			return total;
		}
		#endregion
	}
}
