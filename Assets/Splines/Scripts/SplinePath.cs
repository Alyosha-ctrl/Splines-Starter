using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Xml.Schema;
using UnityEditor;
using UnityEngine;

/*
 * SplinePath joins cubic Bezier segments end to end. 3n + 1 points make n
 * segments: each segment's last point is the next segment's first point.
 * u = segment index + local t, so u runs from 0 to SegmentCount.
 *
 * The distance table approximates arc length with short chords at equal steps in u.
 * It is built once, in Awake. SamplePoint keeps reading the Transforms, so moving
 * a point updates the curve before the table is rebuilt.
 */

public class SplinePath : MonoBehaviour
{
    public Transform[] points;
    
    [Min(1)]
    public int samplesPerSegment = 64;
    
    [Serializable]
    struct DistanceRow
    {
        public float u;
        public float distance;
    }

    [SerializeField] List<DistanceRow> _distanceTable;

    public int SegmentCount => (points.Length - 1) / 3;
    
    // TODO: Return the total path length, which is the distance on the table's last row.
    public float TotalLength => _distanceTable.Last().distance;

    void Awake() => BuildDistanceTable();

    public Vector3 SamplePoint(float u)
    {
        // TODO: Return the world-space point on the spline at u.
        // u can reach SegmentCount, the very end of the path.
        int segment= Math.Min((int)u, SegmentCount-1);
        int pointStart = segment * 3;
        return CubicBezierMath.SamplePoint(points[pointStart].position, points[pointStart+1].position, points[pointStart+2].position, points[pointStart+3].position, u-segment);
    }

    public Vector3 SampleTangent(float u)
    {
        // TODO: Return the tangent at u, using the same segment rules as SamplePoint.
        int segment= Math.Min((int)u, SegmentCount-1);
        int pointStart = segment * 3;
        return CubicBezierMath.SampleTangent(points[pointStart].position, points[pointStart+1].position, points[pointStart+2].position, points[pointStart+3].position, u-segment);
    }

    // Walk the path once at equal steps in u and add up the chords.
    public void BuildDistanceTable()
    {
        // TODO: Fill the table with accumulated world distance at equal steps in u.
        // Start at distance 0 and include every segment boundary through the final endpoint.
        float divisor = 1f/ (samplesPerSegment);

        _distanceTable = new List<DistanceRow>();

        _distanceTable.Clear();


        Vector3 lastPoint = points[0].position;
        float cumulativeDistance = 0f;
        float u = 0f;

        DistanceRow start = new DistanceRow();
        start.distance = cumulativeDistance;
        start.u = u;
        _distanceTable.Add(start);

        //Make a distance row for each point
        for (int i = 0; i < samplesPerSegment * SegmentCount; i++)
        {
            u += divisor;
            Vector3 newpoint = SamplePoint(u);
            float distance = (newpoint - lastPoint).magnitude;
            cumulativeDistance += distance;
            DistanceRow distanceRow = new DistanceRow();
            distanceRow.distance = cumulativeDistance;
            distanceRow.u = u;
            _distanceTable.Add(distanceRow);
            lastPoint = newpoint;
        }
        
    }

    // Find the two rows around the distance, then interpolate u between them.
    public float ParameterAtDistance(float distance)
    {
        // TODO: Return the u at a distance along the path. Interpolate u (not position)
        // between the two rows around it.
        //Checks if you're at the start or the end to avoid issues
        if(distance == 0f)
        {
            return 0f;
        }
        else if(distance == TotalLength)
        {
            return SegmentCount;
        }
        int low = 0;
        int high = _distanceTable.Count-2;
        int mid = (low+high)/2;

        int startIndex = -1;
        int endIndex = 0;
        
        bool checker = true;
        int counter = 0;

        while (checker)
        {
            float pointStart = _distanceTable[mid].distance;
            float pointEnd = _distanceTable[mid+1].distance;
            //If it matches
            if(distance > pointStart && distance < pointEnd)
            {
                startIndex = mid;
                endIndex = mid+1;
                checker = false;
            }
            //If it's bigger move up the floor
            else if (distance > pointEnd)
            {
                low = mid;
            }
            //If it's smaller move down the ceiling
            else if (distance < pointStart)
            {
                high = mid;
            }
            counter += 1;
            mid = (low+high)/2;
            //Exit condition for no infinite loops if it gets really out of wack
            if(mid < 0 || mid > _distanceTable.Count-2) checker = false;
            if(counter > _distanceTable.Count-1) checker = false;
        }
        
        //The fraction along the u
        float fraction = (distance - _distanceTable[startIndex].distance)/(_distanceTable[endIndex].distance - _distanceTable[startIndex].distance);
        //How far you move forward on the u each time
        float step = _distanceTable[endIndex].u - _distanceTable[startIndex].u;

        //Do the math on slide 9 for getting the inbetween thing
        return _distanceTable[startIndex].u + fraction*step;
    }

    void OnDrawGizmos()
    {
        // TODO: Draw the control points, control polygon, and the whole spline with CurveGizmos.
        // CurveGizmos calls your sampling function with a value from 0 to 1, but SamplePoint
        // expects u from 0 to SegmentCount. Scale the value so 0 to 1 covers the whole
        // path, not just the first segment, and ask for enough samples for every segment.
        CurveGizmos.Draw(samplesPerSegment*SegmentCount, t => SamplePoint(t*SegmentCount), points);
    }
}
