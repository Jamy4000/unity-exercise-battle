using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Utils.SpatialPartitioning
{
    public abstract class BaseDimensionComparer<TDimension>
    {
        public abstract int Dimensions { get; }
        public const int MEDIAN_SAMPLE_COUNT = 16;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract int Compare(TDimension x, TDimension y, int axis);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract float GetComponentOnAxis(TDimension value, int axis);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract float CalculateDistanceSq(TDimension x, TDimension y);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract Vector3 ToVector3(TDimension position);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ApproximateMedianIndex(IList<TDimension> arr, int lowInclusive, int highExclusive, int axis)
        {
            // Select 5 random elements within the range and choose the median of them
            int count = Mathf.Min(MEDIAN_SAMPLE_COUNT, highExclusive - lowInclusive);
            int[] candidates = new int[count];
            for (int i = 0; i < count; i++)
            {
                // to avoid indices 0 issues in the while loop below
                candidates[i] = -1;
            }

            var random = new System.Random();
            for (int i = 0; i < count; i++)
            {
                int newRandom = random.Next(lowInclusive, highExclusive);
                while (candidates.Contains(newRandom))
                {
                    newRandom = random.Next(lowInclusive, highExclusive);
                }
                candidates[i] = newRandom;
            }

            // Sort the candidates and return the middle one as an approximate median
            System.Array.Sort(candidates, (a, b) => Compare(arr[a], arr[b], axis));
    
            return candidates[candidates.Length / 2];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CalculateMedianIndex(IList<TDimension> positions, int lowInclusive, int highExclusive, int axis)
        {
            int range = highExclusive - lowInclusive;
            int medianIndex = lowInclusive + (range / 2);
            return QuickSelect(positions, lowInclusive, highExclusive, medianIndex, axis);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int QuickSelect(IList<TDimension> arr, int lowInclusive, int highExclusive, int k, int axis)
        {
            while (true)
            {
                if (lowInclusive == highExclusive)
                    return lowInclusive;

                int pivotIndex = Partition(arr, lowInclusive, highExclusive, axis);

                if (pivotIndex == k)
                    return pivotIndex;

                if (k < pivotIndex)
                    highExclusive = pivotIndex - 1;
                else
                    lowInclusive = pivotIndex + 1;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int Partition(IList<TDimension> arr, int lowInclusive, int highExclusive, int axis)
        {
            highExclusive -= 1;
            TDimension pivot = arr[highExclusive];
            int i = lowInclusive;

            bool isSorted = true;

            for (int j = lowInclusive; j < highExclusive; j++)
            {
                if (Compare(arr[j], pivot, axis) < 0)
                {
                    if (i != j)
                    {
                        Swap(arr, i, j);
                        isSorted = false;  // A swap happened, so it's not sorted.
                    }
                    i++;
                }
                else if (j > lowInclusive && Compare(arr[j], arr[j - 1], axis) < 0)
                {
                    isSorted = false;  // Elements are out of order; not sorted.
                }

                // Early exit if the entire range is already sorted
                if (isSorted && j == highExclusive - 1)
                {
                    return (lowInclusive + highExclusive) / 2; // Return approximate midpoint
                }
            }

            Swap(arr, i, highExclusive);
            return i;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Swap(IList<TDimension> arr, int a, int b)
        {
            (arr[a], arr[b]) = (arr[b], arr[a]);
        }
    }

    public sealed class OneDimensionComparer : BaseDimensionComparer<float>
    {
        public override int Dimensions => 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int Compare(float a, float b, int axis)
        {
            return a.CompareTo(b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override float GetComponentOnAxis(float value, int axis)
        {
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override float CalculateDistanceSq(float x, float y)
        {
            return Mathf.Abs(y - x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Vector3 ToVector3(float position)
        {
            return new Vector3(0f, position, 0f);
        }
    }

    public sealed class TwoDimensionsComparer : BaseDimensionComparer<Vector2>
    {
        public override int Dimensions => 2;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int Compare(Vector2 a, Vector2 b, int axis)
        {
            return axis == 0 ? a.x.CompareTo(b.x) : a.y.CompareTo(b.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override float GetComponentOnAxis(Vector2 value, int axis)
        {
            return axis == 0 ? value.x : value.y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override float CalculateDistanceSq(Vector2 x, Vector2 y)
        {
            return Vector2.SqrMagnitude(y - x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Vector3 ToVector3(Vector2 position)
        {
            return new Vector3(position.x, 0f, position.y);
        }
    }

    public sealed class ThreeDimensionsComparer : BaseDimensionComparer<Vector3>
    {
        public override int Dimensions => 3;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int Compare(Vector3 a, Vector3 b, int axis)
        {
            return axis switch
            {
                0 => a.x.CompareTo(b.x),
                1 => a.y.CompareTo(b.y),
                _ => a.z.CompareTo(b.z)
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override  float GetComponentOnAxis(Vector3 value, int axis)
        {
            return axis switch
            {
                0 => value.x,
                1 => value.y,
                _ => value.z
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override float CalculateDistanceSq(Vector3 x, Vector3 y)
        {
            return Vector3.SqrMagnitude(y - x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Vector3 ToVector3(Vector3 position)
        {
            return position;
        }
    }
}