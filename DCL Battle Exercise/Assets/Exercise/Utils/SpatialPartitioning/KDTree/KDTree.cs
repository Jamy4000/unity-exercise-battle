using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Utils.SpatialPartitioning
{
    public static class DimensionComparerFactory
    {
        public static BaseDimensionComparer<TDimension> CreateDimensionComparer<TDimension>()
        {
            if (typeof(TDimension) == typeof(float))
            {
                return (BaseDimensionComparer<TDimension>)(object)new OneDimensionComparer();
            }
            
            if (typeof(TDimension) == typeof(Vector2))
            {
                return (BaseDimensionComparer<TDimension>)(object)new TwoDimensionsComparer();
            }
            
            if (typeof(TDimension) == typeof(Vector3))
            {
                return (BaseDimensionComparer<TDimension>)(object)new ThreeDimensionsComparer();
            }
            
            throw new System.NotSupportedException($"Dimension comparer for type {typeof(TDimension)} is not supported.");
        }
    }

    public sealed class KDTree<TDimension> : ISpatialPartitioner<TDimension>
    {
        private struct KDNode
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

        private readonly struct NodeInsertionWrapper
        {
            public readonly int Start;
            public readonly int End;
            public readonly int Depth;
            public readonly int ParentIndex;
            public readonly bool IsLeftChild;

            public NodeInsertionWrapper(int start, int end, int depth, int parentIndex, bool isLeftChild)
            {
                Start = start;
                End = end;
                Depth = depth;
                ParentIndex = parentIndex;
                IsLeftChild = isLeftChild;
            }
        }

        private readonly BaseDimensionComparer<TDimension> _dimensionComparer;
        private readonly List<KDNode> _nodes = new(256);

        private readonly Stack<NodeInsertionWrapper> _treeBuildingStack = new();
        private readonly Stack<(int nodeIndex, int depth)> _treeSearchStack = new();

        public KDTree()
        {
            _dimensionComparer = DimensionComparerFactory.CreateDimensionComparer<TDimension>();
        }

        public void Dispose()
        {
        }
        
        public void InsertPointCloud(IList<TDimension> positions, IList<int> elementIDs, bool rebuildTree = true)
        {
            if (rebuildTree) 
                RemoveAll();
            
            int count = positions.Count;
            if (count == 0) 
                return;

            // Iterative tree building using a stack
            _treeBuildingStack.Push(new NodeInsertionWrapper(0, count, 0, -1, false));

            while (_treeBuildingStack.Count > 0)
            {
                var nodeInsertionWrapper = _treeBuildingStack.Pop();
                if (nodeInsertionWrapper.Start >= nodeInsertionWrapper.End) 
                    continue;

                // Select the axis based on depth
                int axis = nodeInsertionWrapper.Depth % _dimensionComparer.Dimensions;

                // Find the median
                int medianIndex = _dimensionComparer.ApproximateMedianIndex(positions, nodeInsertionWrapper.Start, nodeInsertionWrapper.End, axis);

                // Create the current node
                int currentNodeIndex = _nodes.Count; // Get the index before adding the node
                _nodes.Add(new KDNode(elementIDs[medianIndex], positions[medianIndex]));

                if (nodeInsertionWrapper.ParentIndex > -1)
                {
                    // TODO it kind of sucks we need to make a copy here
                    KDNode parentNodeCopy = _nodes[nodeInsertionWrapper.ParentIndex];
                    
                    if (nodeInsertionWrapper.IsLeftChild)
                        parentNodeCopy.LeftNodeIndex = currentNodeIndex;
                    else
                        parentNodeCopy.RightNodeIndex = currentNodeIndex;
                    
                    _nodes[nodeInsertionWrapper.ParentIndex] = parentNodeCopy;
                }

                // Push left and right child ranges onto the stack
                _treeBuildingStack.Push(new NodeInsertionWrapper(nodeInsertionWrapper.Start, medianIndex, nodeInsertionWrapper.Depth + 1, currentNodeIndex, true));
                _treeBuildingStack.Push(new NodeInsertionWrapper(medianIndex + 1, nodeInsertionWrapper.End, nodeInsertionWrapper.Depth + 1, currentNodeIndex, false));
            }
            
            _treeBuildingStack.Clear();
        }

        public void Insert(TDimension position, int elementID)
        {
            if (_nodes.Count == 0)
            {
                _nodes.Add(new KDNode(elementID, position));
                return;
            }

            Insert_Internal(0, position, elementID, 0);
        }
        
        private int Insert_Internal(int currentIndex, TDimension position, int elementID, int depth)
        {
            if (currentIndex == -1) // Empty slot
            {
                _nodes.Add(new KDNode(elementID, position));
                return _nodes.Count - 1;
            }

            // TODO that's a mistake, we are setting the copy of a node, not the node in the array
            KDNode currentNode = _nodes[currentIndex];
            int axis = depth % 2; // Switch between x (0) and y (1) axis

            bool goLeft = _dimensionComparer.Compare(position, currentNode.Position, axis) < 0;

            if (goLeft)
            {
                // Insert new left child
                currentNode.LeftNodeIndex = currentNode.HasLeftChild ?
                    Insert_Internal(currentNode.LeftNodeIndex, position, elementID, depth + 1) :
                    Insert_Internal(-1, position, elementID, depth + 1) ;
            }
            else
            {
                // Insert new right child
                currentNode.RightNodeIndex = currentNode.HasRightChild ? 
                    Insert_Internal(currentNode.RightNodeIndex, position, elementID, depth + 1) : 
                    Insert_Internal(-1, position, elementID, depth + 1);
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
            if (currentIndex == -1) 
                return -1; // Base case: node not found

            KDNode current = _nodes[currentIndex];
            int axis = depth % 2;

            if (current.ExternalID == elementID)
            {
                // If the node has only one child or no child, we return the child to replace the current node
                if (current.LeftNodeIndex == -1)
                {
                    return current.RightNodeIndex;
                }
                
                if (current.RightNodeIndex == -1)
                {
                    return current.LeftNodeIndex;
                }
                
                // Find the minimum node in the right subtree (successor)
                int minIndex = FindMin(current.RightNodeIndex, axis, depth + 1);
                KDNode minNode = _nodes[minIndex];

                // Replace current node with successor
                current.ExternalID = minNode.ExternalID;
                current.Position = minNode.Position;

                // Recursively remove the successor node
                current.RightNodeIndex = Remove_Internal(current.RightNodeIndex, minNode.Position, minNode.ExternalID, depth + 1);
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

            _treeSearchStack.Push((currentIndex, 0));

            float bestDistanceSq = Mathf.Infinity;

            while (_treeSearchStack.Count > 0)
            {
                var (nodeIndex, depth) = _treeSearchStack.Pop();
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

                _treeSearchStack.Push((nearNodeIndex, depth + 1));

                if (Mathf.Abs(sourceComponent - nodeComponent) < bestResult.Distance)
                {
                    _treeSearchStack.Push((farNodeIndex, depth + 1));
                }
            }

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

            // TODO this is not thread safe
            Stack<(int nodeIndex, int depth)> stack = GenericPool<Stack<(int, int)>>.Get();
            List<QueryResult> queryResults = ListPool<QueryResult>.Get();
            // TODO Hard coded value
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

            ListPool<QueryResult>.Release(queryResults);
            GenericPool<Stack<(int, int)>>.Release(stack);
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
