using System.Collections.Generic;
using UnityEngine;

namespace Utils.SpatialPartitioning
{
    public static class DimensionComparerFactory
    {
        public static IDimensionComparer<TDimension> CreateDimensionComparer<TDimension>()
        {
            if (typeof(TDimension) == typeof(float))
            {
                return (IDimensionComparer<TDimension>)(object)new OneDimensionComparer();
            }
            else if (typeof(TDimension) == typeof(Vector2))
            {
                return (IDimensionComparer<TDimension>)(object)new TwoDimensionComparer();
            }
            else if (typeof(TDimension) == typeof(Vector3))
            {
                return (IDimensionComparer<TDimension>)(object)new ThreeDimensionComparer();
            }
            else
            {
                throw new System.NotSupportedException($"Dimension comparer for type {typeof(TDimension)} is not supported.");
            }
        }
    }

    public sealed class KDTree<TDimension> : ISpatialPartitioner<TDimension>
    {
        public struct KDNode
        {
            public int ExternalID;
            public TDimension Position;

            public int LeftNodeIndex;
            public int RightNodeIndex;

            public readonly bool HasLeftChild => LeftNodeIndex != -1;
            public readonly bool HasRightChild => RightNodeIndex != -1;

            public KDNode(int elementID, TDimension position, int leftNodeIndex = -1, int rightNodeIndex = -1)
            {
                ExternalID = elementID;
                Position = position;

                LeftNodeIndex = leftNodeIndex;
                RightNodeIndex = rightNodeIndex;
            }
        }

        private readonly IDimensionComparer<TDimension> _dimensionComparer;
        private readonly List<KDNode> _nodes = new(256);

        public KDTree()
        {
            _dimensionComparer = DimensionComparerFactory.CreateDimensionComparer<TDimension>();
        }

        public void Dispose()
        {
        }

        public void Insert(TDimension position, int elementID)
        {
            if (_nodes.Count == 0)
            {
                _nodes.Add(new(elementID, position));
                return;
            }

            Insert_Internal(0, position, elementID, 0);
        }

        private int Insert_Internal(int currentIndex, TDimension position, int elementID, int depth)
        {
            if (currentIndex == -1) // Empty slot
            {
                _nodes.Add(new(elementID, position));
                return _nodes.Count - 1;
            }

            KDNode currentNode = _nodes[currentIndex];
            int axis = depth % 2; // Switch between x (0) and y (1) axis

            bool goLeft = _dimensionComparer.Compare(position, currentNode.Position, axis) < 0;

            if (goLeft)
            {
                if (!currentNode.HasLeftChild)
                {
                    currentNode.LeftNodeIndex = Insert_Internal(-1, position, elementID, depth + 1); // Insert new left child
                }
                else
                {
                    currentNode.LeftNodeIndex = Insert_Internal(currentNode.LeftNodeIndex, position, elementID, depth + 1);
                }
            }
            else
            {
                if (!currentNode.HasRightChild)
                {
                    currentNode.RightNodeIndex = Insert_Internal(-1, position, elementID, depth + 1); // Insert new right child
                }
                else
                {
                    currentNode.RightNodeIndex = Insert_Internal(currentNode.RightNodeIndex, position, elementID, depth + 1);
                }
            }

            _nodes[currentIndex] = currentNode; // Update node
            return currentIndex;
        }

        public void Remove(TDimension position, int elementID)
        {
            Remove_Internal(0, position, elementID, 0); // Start from the root, which is index 0
        }

        private int Remove_Internal(int currentIndex, TDimension position, int elementID, int depth)
        {
            if (currentIndex == -1) return -1; // Base case: node not found

            KDNode current = _nodes[currentIndex];
            int axis = depth % 2;

            if (current.ExternalID == elementID)
            {
                // If the node has only one child or no child, we return the child to replace the current node
                if (current.LeftNodeIndex == -1)
                {
                    return current.RightNodeIndex;
                }
                else if (current.RightNodeIndex == -1)
                {
                    return current.LeftNodeIndex;
                }
                else
                {
                    // Find the minimum node in the right subtree (successor)
                    int minIndex = FindMin(current.RightNodeIndex, axis, depth + 1);
                    KDNode minNode = _nodes[minIndex];

                    // Replace current node with successor
                    current.ExternalID = minNode.ExternalID;
                    current.Position = minNode.Position;

                    // Recursively remove the successor node
                    current.RightNodeIndex = Remove_Internal(current.RightNodeIndex, minNode.Position, minNode.ExternalID, depth + 1);
                }
            }
            else
            {
                bool goLeft = _dimensionComparer.Compare(position, position, axis) < 0;
                if (goLeft)
                {
                    current.LeftNodeIndex = Remove_Internal(current.LeftNodeIndex, position, elementID, depth + 1);
                }
                else
                {
                    current.RightNodeIndex = Remove_Internal(current.RightNodeIndex, position, elementID, depth + 1);
                }
            }

            _nodes[currentIndex] = current;
            return currentIndex; // Return the current index
        }

        public void RemoveAll()
        {
            _nodes.Clear(); // Clears all the nodes in the tree
        }

        public QueryResult QueryClosest(TDimension source)
        {
            return QueryClosest_Internal(0, source); // Start from the root at index 0
        }

        private QueryResult QueryClosest_Internal(int currentIndex, TDimension source)
        {
            QueryResult bestResult = new();
            if (currentIndex == -1) 
                return bestResult;

            Stack<(int nodeIndex, int depth)> stack = UnityEngine.Pool.GenericPool<Stack<(int, int)>>.Get();
            stack.Push((currentIndex, 0));

            float bestDistanceSq = Mathf.Infinity;

            while (stack.Count > 0)
            {
                var (nodeIndex, depth) = stack.Pop();
                if (nodeIndex == -1) continue;

                KDNode currentNode = _nodes[nodeIndex];
                float distanceSq = _dimensionComparer.CalculateDistanceSq(currentNode.Position, source);

                if (distanceSq < bestDistanceSq)
                {
                    bestDistanceSq = distanceSq;
                    bestResult = new QueryResult(currentNode.ExternalID, Mathf.Sqrt(distanceSq));
                }

                int axis = depth % 2;
                float sourceComponent = _dimensionComparer.GetComponentOnAxis(source, axis);
                float nodeComponent = _dimensionComparer.GetComponentOnAxis(currentNode.Position, axis);

                int nearNodeIndex;
                int farNodeIndex;
                if (sourceComponent < nodeComponent)
                {
                    nearNodeIndex = currentNode.LeftNodeIndex;
                    farNodeIndex = currentNode.RightNodeIndex;
                }
                else
                {
                    nearNodeIndex = currentNode.RightNodeIndex;
                    farNodeIndex = currentNode.LeftNodeIndex;
                }

                stack.Push((nearNodeIndex, depth + 1));

                if (Mathf.Abs(sourceComponent - nodeComponent) < bestResult.Distance)
                {
                    stack.Push((farNodeIndex, depth + 1));
                }
            }

            UnityEngine.Pool.GenericPool<Stack<(int, int)>>.Release(stack);
            return bestResult;
        }

        public int QueryWithinRange_NoAlloc(TDimension source, float range, QueryResult[] results)
        {
            return QueryWithinRange_NoAlloc_Internal(0, source, range * range, results); // Start from root at index 0
        }

        private int QueryWithinRange_NoAlloc_Internal(int currentIndex, TDimension source, float rangeSq, QueryResult[] results)
        {
            if (currentIndex == -1) 
                return 0;

            Stack<(int nodeIndex, int depth)> stack = UnityEngine.Pool.GenericPool<Stack<(int, int)>>.Get();
            List<QueryResult> queryResults = UnityEngine.Pool.ListPool<QueryResult>.Get();
            queryResults.Capacity = 64;

            stack.Push((currentIndex, 0));

            while (stack.Count > 0)
            {
                var (nodeIndex, depth) = stack.Pop();
                if (nodeIndex == -1) 
                    continue;

                KDNode currentNode = _nodes[nodeIndex];
                float distanceSq = _dimensionComparer.CalculateDistanceSq(currentNode.Position, source);

                if (distanceSq <= rangeSq)
                {
                    queryResults.Add(new QueryResult(currentNode.ExternalID, Mathf.Sqrt(distanceSq)));
                }

                int axis = depth % 2;
                float sourceComponent = _dimensionComparer.GetComponentOnAxis(source, axis);
                float nodeComponent = _dimensionComparer.GetComponentOnAxis(currentNode.Position, axis);

                int nearNodeIndex = sourceComponent < nodeComponent ? currentNode.LeftNodeIndex : currentNode.RightNodeIndex;
                int farNodeIndex = nearNodeIndex == currentNode.LeftNodeIndex ? currentNode.RightNodeIndex : currentNode.LeftNodeIndex;

                stack.Push((nearNodeIndex, depth + 1));

                float axisDistance = Mathf.Abs(sourceComponent - nodeComponent);
                if (axisDistance * axisDistance <= rangeSq)
                {
                    stack.Push((farNodeIndex, depth + 1));
                }
            }

            queryResults.Sort((a, b) => { return (int)(a.Distance - b.Distance); });
            int maxResults = Mathf.Min(results.Length, queryResults.Count);
            for (int i = 0; i < maxResults; i++)
            {
                results[i] = queryResults[i];
            }

            UnityEngine.Pool.ListPool<QueryResult>.Release(queryResults);
            UnityEngine.Pool.GenericPool<Stack<(int, int)>>.Release(stack);
            return maxResults;
        }

        private int FindMin(int currentIndex, int axis, int depth)
        {
            int minIndex = currentIndex;

            while (currentIndex != -1)
            {
                KDNode currentNode = _nodes[currentIndex];
                int currentAxis = depth % 2;

                if (currentAxis == axis)
                {
                    if (currentNode.LeftNodeIndex == -1) 
                        return currentIndex;

                    currentIndex = currentNode.LeftNodeIndex;
                }
                else
                {
                    float currentComponent = _dimensionComparer.GetComponentOnAxis(currentNode.Position, axis);
                    float minComponent = _dimensionComparer.GetComponentOnAxis(_nodes[minIndex].Position, axis);
                    if (currentComponent < minComponent)
                    {
                        minIndex = currentIndex;
                        minComponent = currentComponent;
                    }

                    float leftComponent = _dimensionComparer.GetComponentOnAxis(_nodes[currentNode.LeftNodeIndex].Position, axis);
                    if (currentNode.LeftNodeIndex != -1 && leftComponent < minComponent)
                    {
                        minIndex = currentNode.LeftNodeIndex;
                    }

                    float rightComponent = _dimensionComparer.GetComponentOnAxis(_nodes[currentNode.RightNodeIndex].Position, axis);
                    if (currentNode.RightNodeIndex != -1 && rightComponent < minComponent)
                    {
                        minIndex = currentNode.RightNodeIndex;
                    }

                    break;
                }

                depth++;
            }

            return minIndex;
        }

#if UNITY_EDITOR
        public void OnDrawGizmos()
        {
            var dimensionComparer = DimensionComparerFactory.CreateDimensionComparer<TDimension>();
            Gizmos.color = Color.yellow; // Set color for the node spheres
            foreach (var node in _nodes)
            {
                // Draw a sphere at the position of the node
                Gizmos.DrawSphere(dimensionComparer.ToVector3(node.Position), 0.1f);
            }

            // Optionally, draw lines between parents and children
            Gizmos.color = Color.red; // Set color for the lines
            for (int i = 0; i < _nodes.Count; i++)
            {
                KDNode currentNode = _nodes[i];
                Vector3 parentPosition = dimensionComparer.ToVector3(currentNode.Position);

                if (currentNode.HasLeftChild)
                {
                    Vector3 leftChildPosition = dimensionComparer.ToVector3(_nodes[currentNode.LeftNodeIndex].Position);
                    Gizmos.DrawLine(parentPosition, leftChildPosition);
                }

                if (currentNode.HasRightChild)
                {
                    Vector3 rightChildPosition = dimensionComparer.ToVector3(_nodes[currentNode.RightNodeIndex].Position);
                    Gizmos.DrawLine(parentPosition, rightChildPosition);
                }
            }
        }
#endif
    }
}
