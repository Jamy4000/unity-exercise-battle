using UnityEngine;

namespace Utils.SpatialPartitioning
{
    public interface IDimensionComparer<TDimension>
    {
        public int Compare(TDimension x, TDimension y, int axis);
        public float GetComponentOnAxis(TDimension value, int axis);
        public float CalculateDistanceSq(TDimension x, TDimension y);
        Vector3 ToVector3(TDimension position);
        int GetDimension();
    }

    public sealed class OneDimensionComparer : IDimensionComparer<float>
    {
        public int Compare(float a, float b, int axis)
        {
            return a.CompareTo(b);
        }

        public float GetComponentOnAxis(float value, int axis)
        {
            return value;
        }

        public float CalculateDistanceSq(float x, float y)
        {
            return Mathf.Abs(y - x);
        }

        public Vector3 ToVector3(float position)
        {
            return new Vector3(0f, position, 0f);
        }

        public int GetDimension()
        {
            return 1;
        }
    }

    public sealed class TwoDimensionComparer : IDimensionComparer<Vector2>
    {
        public int Compare(Vector2 a, Vector2 b, int axis)
        {
            return axis == 0 ? a.x.CompareTo(b.x) : a.y.CompareTo(b.y);
        }

        public float GetComponentOnAxis(Vector2 value, int axis)
        {
            return axis == 0 ? value.x : value.y;
        }

        public float CalculateDistanceSq(Vector2 x, Vector2 y)
        {
            return Vector2.SqrMagnitude(y - x);
        }

        public Vector3 ToVector3(Vector2 position)
        {
            return new Vector3(position.x, 0f, position.y);
        }

        public int GetDimension()
        {
            return 2;
        }
    }

    public sealed class ThreeDimensionComparer : IDimensionComparer<Vector3>
    {
        public int Compare(Vector3 a, Vector3 b, int axis)
        {
            if (axis == 0)
                return a.x.CompareTo(b.x);
            else if (axis == 1)
                return a.y.CompareTo(b.y);
            else
                return a.z.CompareTo(b.z);
        }

        public float GetComponentOnAxis(Vector3 value, int axis)
        {
            return axis == 0 ? value.x : axis == 1 ? value.y : value.z;
        }

        public float CalculateDistanceSq(Vector3 x, Vector3 y)
        {
            return Vector3.SqrMagnitude(y - x);
        }

        public Vector3 ToVector3(Vector3 position)
        {
            return position;
        }

        public int GetDimension()
        {
            return 3;
        }
    }
}