using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace Utils.SpatialPartitioning
{
    public interface ISpatialEntity<TData>
    {
        TData Position { get; }
        float GetSqDistance(TData otherVector2);
    }

    public class AABB
    {
        public readonly float MinX, MinY, MaxX, MaxY;

        public AABB(float MinX, float MinY, float MaxX, float MaxY)
        {
            this.MinX = MinX;
            this.MinY = MinY;
            this.MaxX = MaxX;
            this.MaxY = MaxY;
        }

        public bool Contains(Vector2 point)
        {
            return point.x >= MinX && point.x <= MaxX && point.y >= MinY && point.y <= MaxY;
        }

        // Checks if the bounding box intersects with another bounding box
        public bool Intersects(AABB other)
        {
            return !(other.MinX > MaxX || other.MaxX < MinX || other.MinY > MaxY || other.MaxY < MinY);
        }
    }

    public class QuadtreeNode<TElement>
        where TElement : class, ISpatialEntity<Vector2>, IComparable<TElement>
    {
        public TElement Element;                              // An element in this node (if this is a leaf node)

        public readonly AABB Boundary;                      // The AABB representing the boundary of this node
        public QuadtreeNode<TElement>[] Children { get; private set; }  // The four child quadrants (NW, NE, SW, SE)

        public QuadtreeNode(AABB boundary, int maxChildrenCount)
        {
            this.Boundary = boundary;
            this.Children = new QuadtreeNode<TElement>[maxChildrenCount];
        }

        public void ResetChildren()
        {
            for (int i = 0; i < Children.Length; i++)
            {
                if (Children[i].HasChildren)
                    Children[i].ResetChildren();

                Children[i].Element = null;
                Children[i] = null;
            }
        }

        public bool HasElement => Element != null;
        public bool HasChildren => Children[0] != null;
    }

    public sealed class Quadtree<TElement> : ISpatialPartitioner<TElement, Vector2>
        where TElement : class, ISpatialEntity<Vector2>, IComparable<TElement>
    {
        private QuadtreeNode<TElement> _root;
        private readonly QueryResultsComparer<TElement> _queryResultsComparer = new();

        private const int MAX_CHILDREN_COUNT = 4;

        public Quadtree(Vector2 center, Vector2 size)
        {
            var halfSize = size * 0.5f;
            var aabb = new AABB(center.x - halfSize.x, center.y - halfSize.y, center.x + halfSize.x, center.y + halfSize.y);
            _root = new QuadtreeNode<TElement>(aabb, MAX_CHILDREN_COUNT);
        }

        public void Dispose()
        {
            _root.ResetChildren();
        }

        public void Insert(TElement element)
        {
            Insert_Internal(_root, element);
        }

        private void Insert_Internal(QuadtreeNode<TElement> node, TElement element)
        {
            if (!node.Boundary.Contains(element.Position))
                return;

            // If node has children, insert the point into the appropriate quadrant
            if (node.HasChildren)
            {
                InsertIntoChildren(node, element);
                return;
            }

            // If node is not a leaf yet
            if (!node.HasElement)
            {
                node.Element = element;
                return;
            }

            // If node is a leaf but already has a point, subdivide and redistribute
            Subdivide(node);

            InsertIntoChildren(node, node.Element);  // Move the existing point to the children
            node.Element = null;                    // This node no longer holds the point itself

            InsertIntoChildren(node, element);       // Insert the new point into the children
        }

        private void InsertIntoChildren(QuadtreeNode<TElement> node, TElement element)
        {
            foreach (var child in node.Children)
            {
                Insert_Internal(child, element);
            }
        }

        // Subdivide the node into 4 child quadrants
        private void Subdivide(QuadtreeNode<TElement> node)
        {
            float midX = (node.Boundary.MinX + node.Boundary.MaxX) / 2;
            float midY = (node.Boundary.MinY + node.Boundary.MaxY) / 2;

            node.Children[0] = new QuadtreeNode<TElement>(new AABB(node.Boundary.MinX, midY, midX, node.Boundary.MaxY), MAX_CHILDREN_COUNT); // NW
            node.Children[1] = new QuadtreeNode<TElement>(new AABB(midX, midY, node.Boundary.MaxX, node.Boundary.MaxY), MAX_CHILDREN_COUNT); // NE
            node.Children[2] = new QuadtreeNode<TElement>(new AABB(node.Boundary.MinX, node.Boundary.MinY, midX, midY), MAX_CHILDREN_COUNT); // SW
            node.Children[3] = new QuadtreeNode<TElement>(new AABB(midX, node.Boundary.MinY, node.Boundary.MaxX, midY), MAX_CHILDREN_COUNT); // SE
        }

        // Nearest neighbor search
        public QueryResult<TElement> QueryClosest(Vector2 position)
        {
            TElement bestElement = null;
            float bestDistSq = float.PositiveInfinity;

            QueryClosest_Internal(_root, position, ref bestElement, ref bestDistSq);
            return new QueryResult<TElement>(bestElement, Mathf.Sqrt(bestDistSq));
        }

        private void QueryClosest_Internal(QuadtreeNode<TElement> node, Vector2 source, ref TElement bestElement, ref float bestDistSq)
        {
            if (!node.Boundary.Contains(source) && 
                DistanceSquaredToAABB(source, node.Boundary) > bestDistSq)
                return;

            if (node.HasChildren)
            {
                // Check child nodes in order of proximity
                foreach (var child in node.Children)
                {
                    QueryClosest_Internal(child, source, ref bestElement, ref bestDistSq);
                }
            }
            // No element when children are present (it's a leaf), check if it's closer
            else if (node.HasElement)
            {
                float distSq = node.Element.GetSqDistance(source);
                if (distSq < bestDistSq)
                {
                    bestDistSq = distSq;
                    bestElement = node.Element;
                }
            }
        }

        public int QueryWithinRange_NoAlloc(Vector2 source, float range, QueryResult<TElement>[] results)
        {
            // create a list to add the elements in range
            List<QueryResult<TElement>> elementsInRange = GenericPool<List<QueryResult<TElement>>>.Get();

            // feed the list
            int elementsCount = QueryRange_Internal(_root, source, range * range, elementsInRange);
            // sort the list by elements distance
            elementsInRange.Sort(_queryResultsComparer);

            // assign elements to results
            int minElementCount = Mathf.Min(elementsCount, results.Length);
            for (int i = 0; i < minElementCount; i++)
            {
                results[i] = elementsInRange[i];
            }

            elementsInRange.Clear();
            GenericPool<List<QueryResult<TElement>>>.Release(elementsInRange);
            return minElementCount;
        }

        private int QueryRange_Internal(QuadtreeNode<TElement> node, Vector2 source, float sqRange, List<QueryResult<TElement>> results)
        {
            if (!node.Boundary.Contains(source))
                return 0;

            int count = 0;

            if (node.HasChildren)
            {
                foreach (var child in node.Children)
                {
                    count += QueryRange_Internal(child, source, sqRange, results);
                }
            }
            // No element when children are present, so we check that in an else
            else if (node.HasElement)
            {
                float sqDistance = node.Element.GetSqDistance(source);
                if (sqDistance <= sqRange)
                {
                    results.Add(new(node.Element, Mathf.Sqrt(sqDistance)));
                    count++;
                }
            }


            return count;
        }

        // Remove an element from the Quadtree
        public void Remove(TElement element)
        {
            Remove_Internal(_root, element);
        }

        private bool Remove_Internal(QuadtreeNode<TElement> node, TElement element)
        {
            if (!node.Boundary.Contains(element.Position))
                return false;

            if (node.HasChildren)
            {
                for (int i = 0; i < MAX_CHILDREN_COUNT; i++)
                {
                    if (Remove_Internal(node.Children[i], element))
                    {
                        // After removal, check if all children are empty and merge them back
                        if (ShouldMerge(node))
                        {
                            node.ResetChildren();
                        }
                        return true;
                    }
                }
            }
            // No element when children are present, so we check that in an else
            else if (node.HasElement)
            {
                // If this is the same element
                if (node.Element.CompareTo(element) == 0)
                {
                    node.Element = null;
                    return true;
                }
                return false;
            }

            return false;
        }

        private bool ShouldMerge(QuadtreeNode<TElement> node)
        {
            if (!node.HasChildren)
                return true;

            // Check if all children are empty or null, if so, merge them back into a single node
            foreach (var child in node.Children)
            {
                if (child.HasElement)
                    return false;
            }
            return true;
        }

        // Remove all points from the Quadtree
        public void RemoveAll()
        {
            RemoveAll_Internal(_root);
        }

        private void RemoveAll_Internal(QuadtreeNode<TElement> node)
        {
            if (node.HasChildren)
            {
                node.ResetChildren();
            }

            node.Element = null;
        }

        // Helper function to calculate squared distance between a point and an AABB (for pruning)
        private static float DistanceSquaredToAABB(Vector2 point, AABB boundary)
        {
            // If the point is inside the boundary, distance is zero
            float dx = Mathf.Max(boundary.MinX - point.x, 0, point.x - boundary.MaxX);
            float dy = Mathf.Max(boundary.MinY - point.y, 0, point.y - boundary.MaxY);

            return dx * dx + dy * dy;
        }

#if UNITY_EDITOR
        public void OnDrawGizmos()
        {
            OnDrawGizmos(_root);
        }

        private void OnDrawGizmos(QuadtreeNode<TElement> node)
        {
            if (node == null)
                return;

            // Draw the boundary of the node
            Gizmos.color = Color.green; // Set the color for the boundary
            Gizmos.DrawLine(new Vector3(node.Boundary.MinX, 0, node.Boundary.MinY), new Vector3(node.Boundary.MaxX, 0, node.Boundary.MinY));
            Gizmos.DrawLine(new Vector3(node.Boundary.MaxX, 0, node.Boundary.MinY), new Vector3(node.Boundary.MaxX, 0, node.Boundary.MaxY));
            Gizmos.DrawLine(new Vector3(node.Boundary.MaxX, 0, node.Boundary.MaxY), new Vector3(node.Boundary.MinX, 0, node.Boundary.MaxY));
            Gizmos.DrawLine(new Vector3(node.Boundary.MinX, 0, node.Boundary.MaxY), new Vector3(node.Boundary.MinX, 0, node.Boundary.MinY));

            // Recursively draw the children
            if (node.HasChildren)
            {
                foreach (var child in node.Children)
                {
                    OnDrawGizmos(child);
                }
            }
        }
#endif
    }
}