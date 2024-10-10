using System.Collections.Generic;
using UnityEngine;

namespace Utils.SpatialPartitioning
{
    public class QueryResult
    {
        public readonly int ElementID;
        public readonly float Distance;

        public QueryResult(int elementID = int.MinValue, float distance = Mathf.Infinity)
        {
            ElementID = elementID;
            Distance = distance;
        }
    }

    /// <summary>
    /// An interface to let us switch between 2D and 3D spatial partitioner (KD Tree, QuadTree, Octree, ...)
    /// I'm using Vector3 as a base data struct since using generics would be quite messy here.
    /// </summary>
    public interface ISpatialPartitioner<TData> : System.IDisposable
    {
        void Insert(TData position, int elementID);

        void Remove(TData position, int elementID);
        void RemoveAll();

        QueryResult QueryClosest(TData source);
        int QueryWithinRange_NoAlloc(TData source, float range, QueryResult[] results);
    }

    public sealed class QueryResultsComparer : IComparer<QueryResult>
    {
        public int Compare(QueryResult x, QueryResult y)
        {
            return x.Distance.CompareTo(y.Distance);
        }
    }
}