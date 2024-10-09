using UnityEngine;

namespace Utils.SpatialPartitioning
{
    public interface IDimensionComparer<TDimension>
    {
        public int Compare(TDimension x, TDimension y, int axis);
        public float GetComponentOnAxis(TDimension value, int axis);
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
    }

    public sealed class FourDimensionComparer : IDimensionComparer<Vector4>
    {
        public int Compare(Vector4 a, Vector4 b, int axis)
        {
            if (axis == 0)
                return a.x.CompareTo(b.x);
            else if (axis == 1)
                return a.y.CompareTo(b.y);
            else if (axis == 2)
                return a.z.CompareTo(b.z);
            else
                return a.w.CompareTo(b.w);
        }

        public float GetComponentOnAxis(Vector4 value, int axis)
        {
            return axis == 0 ? value.x : axis == 1 ? value.y : axis == 3 ? value.z : value.w;
        }
    }
}