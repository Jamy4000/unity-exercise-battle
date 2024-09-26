using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static Vector3 GetRandomPosInBounds(Bounds bounds)
    {
        Vector3 pos = Vector3.zero;
        pos.x = Random.Range( bounds.min.x, bounds.max.x );
        pos.z = Random.Range( bounds.min.z, bounds.max.z );
        return pos;
    }

    public static Vector3 GetCenter<T>( List<T> objects )
        where T : Component
    {
        Vector3 result = Vector3.zero;

        foreach ( var o in objects )
        {
            result += o.transform.position;
        }

        result.x /= objects.Count;
        result.y /= objects.Count;
        result.z /= objects.Count;

        return result;
    }

    public static Vector3 GetCenter( List<GameObject> objects )
    {
        Vector3 result = Vector3.zero;

        foreach ( var o in objects )
        {
            result += o.transform.position;
        }

        result.x /= objects.Count;
        result.y /= objects.Count;
        result.z /= objects.Count;

        return result;
    }

    public static float GetNearestObject( GameObject source, List<GameObject> objects, out GameObject nearestObject )
    {
        float minDistSq = float.MaxValue;
        nearestObject = null;

        foreach ( var obj in objects )
        {
            float sqDist = Vector3.SqrMagnitude(source.transform.position - obj.transform.position);

            if ( sqDist < minDistSq )
            {
                minDistSq = sqDist;
                nearestObject = obj;
            }
        }

        return Mathf.Sqrt(minDistSq);
    }

    public static float GetNearestObject<T>(T source, List<T> objects, out T nearestObject)
        where T : Component
    {
        float minDistSq = float.MaxValue;
        nearestObject = null;

        foreach (var obj in objects)
        {
            float sqDist = Vector3.SqrMagnitude(source.transform.position - obj.transform.position);

            if (sqDist < minDistSq)
            {
                minDistSq = sqDist;
                nearestObject = obj;
            }
        }

        return Mathf.Sqrt(minDistSq);
    }
}