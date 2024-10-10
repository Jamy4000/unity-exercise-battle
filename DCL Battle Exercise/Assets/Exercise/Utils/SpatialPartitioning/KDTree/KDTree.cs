using UnityEngine;
using UnityEngine.UIElements;

namespace Utils.SpatialPartitioning
{
    public sealed class KDTree<TDimension> : ISpatialPartitioner<TDimension>
    {
        private sealed class KDNode
        {
            public int ElementID;
            public TDimension Position;

            public KDNode Left;
            public KDNode Right;
            public int Depth;

            public KDNode(int elementID, int depth)
            {
                ElementID = elementID;
                Depth = depth;
                Left = null;
                Right = null;
            }

            public void ResetNode()
            {
                ElementID = default;
                if (Left != null)
                {
                    Left.ResetNode();
                    Left = null;
                }
                if (Right != null)
                {
                    Right.ResetNode();
                    Right = null;
                }
            }
        }

        private KDNode _root;
        private readonly int _dimensions;
        private readonly IDimensionComparer<TDimension> _dimensionComparer;

        public KDTree(int dimensions, IDimensionComparer<TDimension> dimensionComparer)
        {
            _root = null;
            _dimensions = dimensions;
            _dimensionComparer = dimensionComparer;
        }

        public void Dispose()
        {
            if (_root != null )
                _root.ResetNode();
        }

        public void Insert(TDimension position, int elementID)
        {
            _root = Insert_Internal(_root, position, elementID, 0);
        }

        private KDNode Insert_Internal(KDNode node, TDimension position, int elementID, int depth)
        {
            if (node == null)
                return new KDNode(elementID, depth);

            int axis = depth % _dimensions;
            if (_dimensionComparer.Compare(position, node.Position, axis) < 0)
                node.Left = Insert_Internal(node.Left, position, elementID, depth + 1);
            else
                node.Right = Insert_Internal(node.Right, position, elementID, depth + 1);

            return node;
        }

        public void Remove(TDimension position, int elementID)
        {
            _root = Remove_Internal(_root, position, elementID, 0);
        }

        private KDNode Remove_Internal(KDNode node, TDimension position, int elementID, int depth)
        {
            if (node == null)
                return null;

            int axis = depth % _dimensions;

            if (node.ElementID == elementID)
            {
                if (node.Right != null)
                {
                    var minNode = FindMin(node.Right, axis, depth + 1);
                    node.ElementID = minNode.ElementID;
                    node.Position = minNode.Position;
                    node.Right = Remove_Internal(node.Right, position, minNode.ElementID, depth + 1);
                }
                else if (node.Left != null)
                {
                    var minNode = FindMin(node.Left, axis, depth + 1);
                    node.ElementID = minNode.ElementID;
                    node.Position = minNode.Position;
                    node.Right = Remove_Internal(node.Left, position, minNode.ElementID, depth + 1);
                    node.Left = null;
                }
                else
                {
                    return null;
                }
            }
            else if (_dimensionComparer.Compare(position, node.Position, axis) < 0)
            {
                node.Left = Remove_Internal(node.Left, position, elementID, depth + 1);
            }
            else
            {
                node.Right = Remove_Internal(node.Right, position, elementID, depth + 1);
            }

            return node;
        }

        public void RemoveAll()
        {
            _root = null;
        }

        public QueryResult QueryClosest(TDimension source)
        {
            return QueryClosest_Internal(_root, source, 0);
        }

        private QueryResult QueryClosest_Internal(KDNode node, TDimension source, int depth)
        {
            if (node == null)
                return new QueryResult(distance: Mathf.Infinity);

            float bestDistanceSq = _dimensionComparer.CalculateDistanceSq(node.Position, source);
            QueryResult best = new(node.ElementID, Mathf.Sqrt(bestDistanceSq));
            int axis = depth % _dimensions;

            KDNode nearNode = _dimensionComparer.Compare(source, node.Position, axis) < 0 ? node.Left : node.Right;
            KDNode farNode = nearNode == node.Left ? node.Right : node.Left;

            QueryResult bestNear = QueryClosest_Internal(nearNode, source, depth + 1);
            if (bestNear.Distance < best.Distance)
                best = bestNear;

            float sourceComponent = _dimensionComparer.GetComponentOnAxis(source, axis);
            float nodeComponent = _dimensionComparer.GetComponentOnAxis(node.Position, axis);
            if (Mathf.Abs(sourceComponent - nodeComponent) < best.Distance)
            {
                QueryResult bestFar = QueryClosest_Internal(farNode, source, depth + 1);
                if (bestFar.Distance < best.Distance)
                    best = bestFar;
            }

            return best;
        }

        public int QueryWithinRange_NoAlloc(TDimension source, float range, QueryResult[] results)
        {
            return QueryWithinRange_NoAlloc_Internal(_root, source, range * range, results, 0);
        }

        private int QueryWithinRange_NoAlloc_Internal(KDNode node, TDimension source, float rangeSq, QueryResult[] results, int depth, int offset = 0)
        {
            if (node == null)
                return offset;

            float distanceSq = _dimensionComparer.CalculateDistanceSq(node.Position, source);
            if (distanceSq <= rangeSq)
            {
                results[offset++] = new QueryResult(node.ElementID, Mathf.Sqrt(distanceSq));
                if (offset >= results.Length)
                    return offset;
            }

            int axis = depth % _dimensions;
            if (_dimensionComparer.Compare(source, node.Position, axis) < 0)
            {
                offset = QueryWithinRange_NoAlloc_Internal(node.Left, source, rangeSq, results, depth + 1, offset);
                if (offset < results.Length)
                    offset = QueryWithinRange_NoAlloc_Internal(node.Right, source, rangeSq, results, depth + 1, offset);
            }
            else
            {
                offset = QueryWithinRange_NoAlloc_Internal(node.Right, source, rangeSq, results, depth + 1, offset);
                if (offset < results.Length)
                    offset = QueryWithinRange_NoAlloc_Internal(node.Left, source, rangeSq, results, depth + 1, offset);
            }

            return offset;
        }

        private KDNode FindMin(KDNode node, int axis, int depth)
        {
            if (node == null)
                return null;

            int currentAxis = depth % _dimensions;
            if (currentAxis == axis)
            {
                if (node.Left == null)
                    return node;
                return FindMin(node.Left, axis, depth + 1);
            }

            return MinNode(node, FindMin(node.Left, axis, depth + 1), FindMin(node.Right, axis, depth + 1), axis);
        }

        private KDNode MinNode(KDNode a, KDNode b, KDNode c, int axis)
        {
            KDNode res = a;
            if (b != null && _dimensionComparer.Compare(b.Position, res.Position, axis) < 0)
                res = b;
            if (c != null && _dimensionComparer.Compare(c.Position, res.Position, axis) < 0)
                res = c;

            return res;
        }
    }
}
