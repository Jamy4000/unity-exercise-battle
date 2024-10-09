using UnityEngine;

namespace Utils.SpatialPartitioning
{
    public readonly struct QueryResult<TElement>
    {
        public readonly TElement Element;
        public readonly float Distance;

        public QueryResult(TElement element = default, float distance = Mathf.Infinity)
        {
            Element = element;
            Distance = distance;
        }
    }

    /// <summary>
    /// An interface to let us switch between 2D and 3D spatial partitioner (KD Tree, QuadTree, Octree, ...)
    /// I'm using Vector3 as a base data struct since using generics would be quite messy here.
    /// </summary>
    public interface ISpatialPartitioner<TElement, TData>
    {
        void Insert(TElement element);

        void Remove(TElement element);
        void RemoveAll();

        QueryResult<TElement> QueryClosest(TData source);
        int QueryWithinRange_NoAlloc(TData source, float range, QueryResult<TElement>[] results, int offset = 0);
    }
}