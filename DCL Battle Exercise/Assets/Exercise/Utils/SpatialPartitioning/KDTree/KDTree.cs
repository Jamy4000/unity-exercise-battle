using UnityEngine;

namespace Utils.SpatialPartitioning
{
    public sealed class KDTree<TElement, TDimension> : ISpatialPartitioner<TElement, TDimension>
        where TElement : ISpatialEntity<TDimension>, System.IComparable<TElement>
    {
        private sealed class KDNode
        {
            public TElement Element;
            public KDNode Left;
            public KDNode Right;
            public int Depth;

            public KDNode(TElement element, int depth)
            {
                Element = element;
                Depth = depth;
                Left = null;
                Right = null;
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

        public void Insert(TElement element)
        {
            _root = Insert_Internal(_root, element, 0);
        }

        private KDNode Insert_Internal(KDNode node, TElement element, int depth)
        {
            if (node == null)
                return new KDNode(element, depth);

            int axis = depth % _dimensions;
            if (_dimensionComparer.Compare(element.Position, node.Element.Position, axis) < 0)
                node.Left = Insert_Internal(node.Left, element, depth + 1);
            else
                node.Right = Insert_Internal(node.Right, element, depth + 1);

            return node;
        }

        public void Remove(TElement element)
        {
            _root = Remove_Internal(_root, element, 0);
        }

        private KDNode Remove_Internal(KDNode node, TElement element, int depth)
        {
            if (node == null)
                return null;

            int axis = depth % _dimensions;

            if (node.Element.CompareTo(element) == 0)
            {
                if (node.Right != null)
                {
                    var minNode = FindMin(node.Right, axis, depth + 1);
                    node.Element = minNode.Element;
                    node.Right = Remove_Internal(node.Right, minNode.Element, depth + 1);
                }
                else if (node.Left != null)
                {
                    var minNode = FindMin(node.Left, axis, depth + 1);
                    node.Element = minNode.Element;
                    node.Right = Remove_Internal(node.Left, minNode.Element, depth + 1);
                    node.Left = null;
                }
                else
                {
                    return null;
                }
            }
            else if (_dimensionComparer.Compare(element.Position, node.Element.Position, axis) < 0)
            {
                node.Left = Remove_Internal(node.Left, element, depth + 1);
            }
            else
            {
                node.Right = Remove_Internal(node.Right, element, depth + 1);
            }

            return node;
        }

        public void RemoveAll()
        {
            _root = null;
        }

        public QueryResult<TElement> QueryClosest(TDimension source)
        {
            if (_root == null)
                return new QueryResult<TElement>();

            return QueryClosest_Internal(_root, source, 0);
        }

        private QueryResult<TElement> QueryClosest_Internal(KDNode node, TDimension source, int depth)
        {
            if (node == null)
                return new QueryResult<TElement>(distance: Mathf.Infinity);

            float bestDistanceSq = node.Element.GetSqDistance(source);
            QueryResult<TElement> best = new(node.Element, Mathf.Sqrt(bestDistanceSq));
            int axis = depth % _dimensions;

            KDNode nearNode = _dimensionComparer.Compare(source, node.Element.Position, axis) < 0 ? node.Left : node.Right;
            KDNode farNode = nearNode == node.Left ? node.Right : node.Left;

            QueryResult<TElement> bestNear = QueryClosest_Internal(nearNode, source, depth + 1);
            if (bestNear.Distance < best.Distance)
                best = bestNear;

            float sourceComponent = _dimensionComparer.GetComponentOnAxis(source, axis);
            float nodeComponent = _dimensionComparer.GetComponentOnAxis(node.Element.Position, axis);
            if (Mathf.Abs(sourceComponent - nodeComponent) < best.Distance)
            {
                QueryResult<TElement> bestFar = QueryClosest_Internal(farNode, source, depth + 1);
                if (bestFar.Distance < best.Distance)
                    best = bestFar;
            }

            return best;
        }

        public int QueryWithinRange_NoAlloc(TDimension source, float range, QueryResult<TElement>[] results, int offset = 0)
        {
            return QueryWithinRange_NoAlloc_Internal(_root, source, range * range, results, 0, offset);
        }

        private int QueryWithinRange_NoAlloc_Internal(KDNode node, TDimension source, float rangeSq, QueryResult<TElement>[] results, int depth, int offset)
        {
            if (node == null)
                return offset;

            float distanceSq = node.Element.GetSqDistance(source);
            if (distanceSq <= rangeSq)
            {
                results[offset++] = new QueryResult<TElement>(node.Element, Mathf.Sqrt(distanceSq));
                if (offset >= results.Length)
                    return offset;
            }

            int axis = depth % _dimensions;
            if (_dimensionComparer.Compare(source, node.Element.Position, axis) < 0)
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
            if (b != null && _dimensionComparer.Compare(b.Element.Position, res.Element.Position, axis) < 0)
                res = b;
            if (c != null && _dimensionComparer.Compare(c.Element.Position, res.Element.Position, axis) < 0)
                res = c;

            return res;
        }
    }
}
