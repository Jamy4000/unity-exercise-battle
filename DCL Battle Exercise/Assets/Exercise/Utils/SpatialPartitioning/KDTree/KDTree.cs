using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utils.SpatialPartitioning
{
    public sealed class KDNodePool<TDimension> : GenericPoolHelper<KDTree<TDimension>.KDNode>
    {
        public KDNodePool(int minPoolSize = 16, int maxPoolSize = 128, bool collectionChecks = false) : 
            base(minPoolSize, maxPoolSize, collectionChecks)
        {
        }

        protected override KDTree<TDimension>.KDNode CreatePooledItem()
        {
            return new KDTree<TDimension>.KDNode();
        }
    }

    public sealed class KDTree<TDimension> : ISpatialPartitioner<TDimension>
    {
        public sealed class KDNode : IGenericPoolable
        {
            public int ElementID;
            public TDimension Position;

            public KDNode Left;
            public KDNode Right;

            public Action<IGenericPoolable> OnShouldReturnToPool { get; set; }

            public KDNode()
            {
                ElementID = int.MinValue;
                Left = null;
                Right = null;
            }

            public KDNode(int elementID)
            {
                ElementID = elementID;
                Left = null;
                Right = null;
            }

            public void Destroy()
            {
            }

            public void Disable()
            {
            }

            public void Enable()
            {
            }

            public void ResetNode(KDTree<TDimension> tree)
            {
                ElementID = int.MinValue;
                if (Left != null)
                {
                    Left.ResetNode(tree);
                    Left.OnShouldReturnToPool?.Invoke(Left);
                    Left = null;
                }

                if (Right != null)
                {
                    Right.ResetNode(tree);
                    Right.OnShouldReturnToPool?.Invoke(Right);
                    Right = null;
                }
            }
        }

        private KDNode _root;
        private readonly int _dimensions;
        private readonly IDimensionComparer<TDimension> _dimensionComparer;
        private readonly KDNodePool<TDimension> _pool = new();

        public KDTree(int dimensions, IDimensionComparer<TDimension> dimensionComparer)
        {
            _root = null;
            _dimensions = dimensions;
            _dimensionComparer = dimensionComparer;
        }

        public void Dispose()
        {
            _root?.ResetNode(this);
        }

        public void Insert(TDimension position, int elementID)
        {
            _root = Insert_Internal(this, _root, position, elementID);
        }

        private static KDNode Insert_Internal(KDTree<TDimension> tree, KDNode root, TDimension position, int elementID)
        {
            KDNode currentNode = root;
            KDNode parentNode = null;
            int depth = 0;

            // Traverse the tree to find the correct position for the new node
            while (currentNode != null)
            {
                parentNode = currentNode;
                int axis = depth % tree._dimensions;

                // Compare the position with the current node's position
                if (tree._dimensionComparer.Compare(position, currentNode.Position, axis) < 0)
                    currentNode = currentNode.Left; // Move left
                else
                    currentNode = currentNode.Right; // Move right

                depth++;
            }

            // Create the new node
            KDNode newNode = tree._pool.RequestPoolableObject();
            newNode.ElementID = elementID;
            newNode.Position = position;

            // If parentNode is null, then we are inserting the root node
            if (parentNode == null)
                return newNode;

            // Determine whether the new node goes to the left or right of the parent
            int parentAxis = (depth - 1) % tree._dimensions;
            if (tree._dimensionComparer.Compare(position, parentNode.Position, parentAxis) < 0)
                parentNode.Left = newNode;
            else
                parentNode.Right = newNode;

            // Return the original root of the tree
            return root;
        }

        public void Remove(TDimension position, int elementID)
        {
            _root = Remove_Internal(this, _root, position, elementID, 0);
        }

        private static KDNode Remove_Internal(KDTree<TDimension> tree, KDNode node, TDimension position, int elementID, int depth)
        {
            KDNode current = node;
            KDNode parent = null;
            int axis;

            while (current != null)
            {
                axis = depth % tree._dimensions;

                // Found the node to remove
                if (current.ElementID == elementID)
                {
                    KDNode replacement;
                    // Node with only one child or no child
                    if (current.Left == null)
                    {
                        replacement = current.Right;
                    }
                    else if (current.Right == null)
                    {
                        replacement = current.Left;
                    }
                    else
                    {
                        // Node with two children: Get the inorder successor (smallest in the right subtree)
                        KDNode minNode = FindMinIterative(tree, current.Right, axis, depth + 1);
                        current.ElementID = minNode.ElementID;
                        current.Position = minNode.Position;

                        // Now we need to find the parent of the inorder successor to remove it
                        parent = current;
                        current = current.Right;
                        depth++; // Move down to the right subtree to find the successor

                        // Find the successor
                        while (current != null && current.ElementID != minNode.ElementID)
                        {
                            parent = current;
                            if (tree._dimensionComparer.Compare(minNode.Position, current.Position, depth % tree._dimensions) < 0)
                            {
                                current = current.Left;
                                depth++;
                            }
                            else
                            {
                                current = current.Right;
                                depth++;
                            }
                        }

                        // Set replacement to the right child of the successor
                        if (parent.Left == current) // If the successor is a left child
                            parent.Left = current.Right;
                        else // If the successor is a right child
                            parent.Right = current.Right;

                        return node; // Return the unchanged node pointer after removal
                    }

                    // If the node is the root, return the replacement
                    if (parent == null)
                    {
                        return replacement;
                    }

                    // Link the parent to the replacement
                    if (parent.Left == current)
                        parent.Left = replacement;
                    else
                        parent.Right = replacement;

                    return node; // Return the unchanged node pointer after removal
                }
                // Traverse the tree to find the node to remove
                else if (tree._dimensionComparer.Compare(position, current.Position, axis) < 0)
                {
                    parent = current;
                    current = current.Left;
                    depth++;
                }
                else if (tree._dimensionComparer.Compare(position, current.Position, axis) > 0)
                {
                    parent = current;
                    current = current.Right;
                    depth++;
                }
            }

            return node; // Return the unchanged node pointer if not found
        }

        public void RemoveAll()
        {
            // Reset the root node which will clear all nodes in the KDTree
            if (_root != null)
            {
                _root.ResetNode(this);
                _root.OnShouldReturnToPool?.Invoke(_root);
                _root = null; // Set root to null after resetting to allow garbage collection
            }
        }

        public QueryResult QueryClosest(TDimension source)
        {
            return QueryClosest_Internal(this, _root, source);
        }

        private static QueryResult QueryClosest_Internal(KDTree<TDimension> tree, KDNode node, TDimension source)
        {
            QueryResult bestResult = new(distance: Mathf.Infinity);
            if (node == null)
                return bestResult;

            float bestDistanceSq = Mathf.Infinity;

            Stack<(KDNode node, int depth)> stack = UnityEngine.Pool.GenericPool<Stack<(KDNode, int)>>.Get();
            stack.Push((node, 0));

            while (stack.Count > 0)
            {
                var (currentNode, depth) = stack.Pop();
                if (currentNode == null)
                    continue;

                float distanceSq = tree._dimensionComparer.CalculateDistanceSq(currentNode.Position, source);

                // Update the best result if the current node is closer
                if (distanceSq < bestDistanceSq)
                {
                    bestDistanceSq = distanceSq;
                    bestResult = new QueryResult(currentNode.ElementID, Mathf.Sqrt(distanceSq));
                }

                int axis = depth % tree._dimensions;
                float sourceComponent = tree._dimensionComparer.GetComponentOnAxis(source, axis);
                float nodeComponent = tree._dimensionComparer.GetComponentOnAxis(currentNode.Position, axis);

                // Determine which side to explore first (nearNode/farNode)
                KDNode nearNode = sourceComponent < nodeComponent ? currentNode.Left : currentNode.Right;
                KDNode farNode = nearNode == currentNode.Left ? currentNode.Right : currentNode.Left;

                // Push the near node first, as it is more likely to have a closer point
                stack.Push((nearNode, depth + 1));

                // Check if the far side is worth exploring by comparing distances along the axis
                if (Mathf.Abs(sourceComponent - nodeComponent) < bestResult.Distance)
                {
                    stack.Push((farNode, depth + 1));
                }
            }


            UnityEngine.Pool.GenericPool<Stack<(KDNode, int)>>.Release(stack);
            return bestResult;
        }

        public int QueryWithinRange_NoAlloc(TDimension source, float range, QueryResult[] results)
        {
            return QueryWithinRange_NoAlloc_Internal(this, _root, source, range * range, results);
        }

        private static int QueryWithinRange_NoAlloc_Internal(KDTree<TDimension> tree, KDNode node, TDimension source, float rangeSq, QueryResult[] results)
        {
            if (node == null)
                return 0;

            int offset = 0;
            Stack<(KDNode node, int depth)> stack = UnityEngine.Pool.GenericPool<Stack<(KDNode, int)>>.Get();
            stack.Push((node, 0));

            while (stack.Count > 0)
            {
                var (currentNode, depth) = stack.Pop();
                if (currentNode == null)
                    continue;

                // Calculate squared distance between current node and source
                float distanceSq = tree._dimensionComparer.CalculateDistanceSq(currentNode.Position, source);

                // Check if the current node is within the specified range
                if (distanceSq <= rangeSq)
                {
                    results[offset++] = new QueryResult(currentNode.ElementID, Mathf.Sqrt(distanceSq));
                    if (offset >= results.Length)
                        return offset;
                }

                // Determine the axis for comparison
                int axis = depth % tree._dimensions;
                float sourceComponent = tree._dimensionComparer.GetComponentOnAxis(source, axis);
                float nodeComponent = tree._dimensionComparer.GetComponentOnAxis(currentNode.Position, axis);

                // Decide which side to visit first (left or right)
                KDNode nearNode = sourceComponent < nodeComponent ? currentNode.Left : currentNode.Right;
                KDNode farNode = nearNode == currentNode.Left ? currentNode.Right : currentNode.Left;

                // Add near node to the stack first
                stack.Push((nearNode, depth + 1));

                // Add far node to the stack if it's within the range on the current axis
                if (Mathf.Abs(sourceComponent - nodeComponent) <= Mathf.Sqrt(rangeSq))
                {
                    stack.Push((farNode, depth + 1));
                }
            }

            UnityEngine.Pool.GenericPool<Stack<(KDNode, int)>>.Release(stack);
            return offset;
        }

        private static KDNode FindMinIterative(KDTree<TDimension> tree, KDNode node, int axis, int depth)
        {
            KDNode current = node;
            KDNode best = current;

            while (current != null)
            {
                int currentAxis = depth % tree._dimensions;

                if (currentAxis == axis)
                {
                    if (current.Left == null)
                        return current;
                    current = current.Left;
                }
                else
                {
                    best = current;
                    if (current.Left != null && tree._dimensionComparer.Compare(current.Left.Position, best.Position, axis) < 0)
                        best = current.Left;
                    if (current.Right != null && tree._dimensionComparer.Compare(current.Right.Position, best.Position, axis) < 0)
                        best = current.Right;

                    break;
                }

                depth++;
            }

            return best;
        }

#if UNITY_EDITOR
        public void OnDrawGizmos()
        {
            if (_root != null)
            {
                if (_root.Position is Vector2 vector2)
                    DrawNodeGizmos(this, _root, new(vector2.x, 1f, vector2.y), 1f, 0); // Starting point and scale
                else if (_root.Position is Vector3 vector3)
                    DrawNodeGizmos(this, _root, vector3, 1f, 0); // Starting point and scale
            }
        }

        private void DrawNodeGizmos(KDTree<TDimension> tree, KDNode node, Vector3 position, float scale, int depth)
        {
            if (node == null)
                return;

            // Draw the node's position
            Gizmos.color = Color.Lerp(Color.red, Color.green, (depth % _dimensions) / (float)_dimensions);
            Gizmos.DrawSphere(position, scale * 0.1f); // Draw a small sphere at the node's position

            // Draw lines to children
            if (node.Left != null)
            {
                Gizmos.color = Color.blue; // Color for left child connection
                // ugly casting, but that's just for debug purposes
                if (node.Left.Position is Vector2 leftNodePos2)
                {
                    Vector3 leftNodePosition = new(leftNodePos2.x, position.y, leftNodePos2.y);
                    Gizmos.DrawLine(position, leftNodePosition);
                    DrawNodeGizmos(tree, node.Left, leftNodePosition, scale * 0.9f, depth + 1); // Draw left subtree
                }
                else if (node.Left.Position is Vector3 leftNodePosition)
                {
                    Gizmos.DrawLine(position, leftNodePosition);
                    DrawNodeGizmos(tree, node.Left, leftNodePosition, scale * 0.9f, depth + 1); // Draw left subtree
                }
            }

            if (node.Right != null)
            {
                Gizmos.color = Color.magenta; // Color for right child connection
                // ugly casting, but that's just for debug purposes
                if (node.Right.Position is Vector2 rightNodePos2)
                {
                    Vector3 rightNodePosition = new(rightNodePos2.x, position.y, rightNodePos2.y);
                    Gizmos.DrawLine(position, rightNodePosition);
                    DrawNodeGizmos(tree, node.Right, rightNodePosition, scale * 0.9f, depth + 1); // Draw right subtree
                }
                else if (node.Right.Position is Vector3 rightNodePosition)
                {
                    Gizmos.DrawLine(position, rightNodePosition);
                    DrawNodeGizmos(tree, node.Right, rightNodePosition, scale * 0.9f, depth + 1); // Draw right subtree
                }
            }
        }
#endif
    }
}
