/**
 * @file   ConvexHull.cs
 * @author Benjamin Williams <bwilliams@lincoln.ac.uk>
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static partial class ConvexHull
{   
    /// <summary>
    /// Computes the convex hull of a list of points.
    /// </summary>
    /// <param name="points">Points.</param>
    /// <param name="loop">If set to <c>true</c> loop.</param>
    public static List<Vector3> compute(List<Vector3> points, bool loop = false)
    {
        //The points which will be returned
        var returnPoints = new List<Vector3>();

        //Find left-most point on x axis
        var currentPoint = lowestXCoord(points);

        //The endpoint to compare against
        Vector3 endpoint = Vector3.zero;

        while (true)
        {
            //Add current point
            returnPoints.Add(currentPoint);

            //Set endpoint back to the first point in the list of points
            endpoint = points[0];

            for(var j = 1; j < points.Count; j++)
            {
                //Run through points -- if the turn from this point to the other is greater, set endpoint to this
                if ((endpoint == currentPoint) || (ccw(currentPoint, endpoint, points[j]) < 0))
                    endpoint = points[j];
            }

            //Set current point
            currentPoint = endpoint;

            //Break condition -- if we've looped back around then we've made a convex hull!
            if (endpoint == returnPoints[0])
                break;
        }

        //If we want to loop, include the first vertex again
        if (loop)
            returnPoints.Add(returnPoints[0]);

        //And finally, return the points
        return returnPoints;
    }

    /// <summary>
    /// Finds the lowest x value.
    /// </summary>
    /// <returns>The X coordinate.</returns>
    /// <param name="array">Array.</param>
    private static Vector3 lowestXCoord(List<Vector3> array)
    {
        return array.Where(p => p.x == (array.Min(y => y.x))).First();
    }


    /// <summary>
    /// Swap two elements in an array, given two indices
    /// </summary>
    /// <param name="array">Array.</param>
    /// <param name="idxA">Index a.</param>
    /// <param name="idxB">Index b.</param>
    private static void swap(ref Vector3[] array, int idxA, int idxB)
    {
        //temp = a
        var temp = array[idxA];

        //a overwritten with b
        array[idxA] = array[idxB];

        //b overwritten with temp
        array[idxB] = temp;
    }


    /// <summary>
    /// Determines if three points are in a counter-clockwise turn. Returns:
    /// n < 0: If clockwise
    /// n > 0: If counter-clockwise
    /// n = 0: If collinear
    /// </summary>
    /// <param name="p1">P1.</param>
    /// <param name="p2">P2.</param>
    /// <param name="p3">P3.</param>
    public static float ccw(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        // Determinant
        return Mathf.Sign((p2.x - p1.x) * (p3.z - p1.z) - (p3.x - p1.x) * (p2.z - p1.z));
    }
}