using System.Reflection.Metadata;
using UnityEditor;
using UnityEngine;

/*
 * SplineFollow rides a SplinePath. Each frame it moves along the path,
 * finds u, places itself on the curve, and faces the target or the tangent.
 */

public class SplineFollow : MonoBehaviour
{
    public SplinePath path;
    public Transform target;
    public float speed = 2.5f; // Positive world units per second in the completed exercise.
    public bool travelByDistance = true;
    public bool faceTarget = true;

    float _distance;
    float _u;

    void Update()
    {
        if (travelByDistance)
        {
            // TODO: Advance distance by speed over the frame and look up u for that distance.
            // Stop at TotalLength.
            _distance += speed;
            _u = path.ParameterAtDistance(_distance);
            
            if(!(_u >= path.SegmentCount)) {
                transform.SetPositionAndRotation(path.SamplePoint(_u), Quaternion.identity);  
            }
        }
        else
        {
            // TODO: Advance u in equal steps, paced so the trip takes as long as the distance
            // trip at the same speed. Stop at SegmentCount.
            // Debug.Log(path.SegmentCount/50);
            // float stepDivider = (3f*.005f);
            float step = path.SegmentCount/1000f;

            // Debug.Log(step);            

            _u += step;
            if(!(_u >= path.SegmentCount)) {
                transform.SetPositionAndRotation(path.SamplePoint(_u), Quaternion.identity);  
            }

        }

        // TODO: Place this object at the path point for u. Replay should return it to the start.

        // TODO: Look at the target if faceTarget is on, otherwise along the path tangent.
        // Use world up so the horizon stays level.
        if (faceTarget)
        {
            transform.LookAt(target.transform);
        }
        else
        {
            transform.forward = path.SampleTangent(_u);
        }
    }

    public void Restart()
    {
        _distance = 0f;
        _u = 0f;
    }
}
