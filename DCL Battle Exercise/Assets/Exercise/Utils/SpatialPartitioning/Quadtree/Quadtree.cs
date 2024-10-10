using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utils.SpatialPartitioning
{
    public readonly struct AABB
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

    public readonly struct QuadtreeElement
    {
        public readonly int ExternalID;                               // The ID representing an element in this node (if this is a leaf node)
        public readonly Vector2 Position;

        public QuadtreeElement(int elementID = int.MinValue)
        {
            ExternalID = elementID;
            Position = Vector2.zero;
        }

        public QuadtreeElement(int elementID, Vector2 position)
        {
            ExternalID = elementID;
            Position = position;
        }

        public readonly bool HasElement => ExternalID != int.MinValue;
    }

    public sealed class QuadtreePool : GenericPoolHelper<Quadtree>
    {
        public QuadtreePool(int minPoolSize = 16, int maxPoolSize = 128, bool collectionChecks = false) :
            base(minPoolSize, maxPoolSize, collectionChecks)
        {
        }

        protected override Quadtree CreatePooledItem()
        {
            return new Quadtree(Vector2.zero, Vector2.one, this);
        }
    }

    public sealed class Quadtree : ISpatialPartitioner<Vector2>, IGenericPoolable
    {
        private AABB _boundary;                      // The AABB representing the boundary of this tree

        // using a list in case we reach max depth but still want to keep on adding elements
        private readonly List<QuadtreeElement> _elements;
        private readonly Quadtree[] _children;
        private bool HasChildren => _children[0] != null;

        public Action<IGenericPoolable> OnShouldReturnToPool { get; set; }

        private readonly QueryResultsComparer _queryResultsComparer = new();
        private QuadtreePool _pool;

        private const int MAX_CHILDREN_COUNT = 4;
        private int _maxElementsCountPerNode;
        private int _currentDepth;
        private int _maxDepth;

        public Quadtree(Vector2 center, Vector2 size, QuadtreePool pool = null, int maxDepth = 50, int maxElementPerNode = 8, int currentDepth = 0)
        {
            _maxElementsCountPerNode = maxElementPerNode;
            _maxDepth = maxDepth;
            // the root quadtree created by the client will create this quadtreePool,
            // and it will then be injected in every other quadtree when they get crated through the pool
            _pool = pool ?? new QuadtreePool();

            var halfSize = size * 0.5f;
            _boundary = new AABB(center.x - halfSize.x, center.y - halfSize.y, center.x + halfSize.x, center.y + halfSize.y);
            _elements = new List<QuadtreeElement>(maxElementPerNode);
            _children = new Quadtree[MAX_CHILDREN_COUNT];
            _currentDepth = currentDepth;
        }

        private Quadtree(AABB boundary, QuadtreePool pool, int maxDepth, int maxElementPerNode, int currentDepth)
        {
            _maxElementsCountPerNode = maxElementPerNode;
            _maxDepth = maxDepth;

            _boundary = boundary;
            _elements = new List<QuadtreeElement>(maxElementPerNode);
            _children = new Quadtree[MAX_CHILDREN_COUNT];
            _currentDepth = currentDepth;
        }

        public void Dispose()
        {
        }

        public void Insert(Vector2 position, int elementID)
        {
            int depth = 0;
            Insert_Internal(this, position, elementID, ref depth);
        }

        private static void Insert_Internal(Quadtree node, Vector2 position, int elementID, ref int depth)
        {
            // if this is the wrong quadrant
            if (!node._boundary.Contains(position))
                return;

            // If node has children, insert the point into the appropriate quadrant
            if (node.HasChildren)
            {
                InsertIntoChildren(node, position, elementID, ref depth);
                return;
            }

            int elementsCount = node._elements.Count;
            // If node is not full yet OR we have reached the maximum depth
            if (elementsCount < node._maxElementsCountPerNode || node._currentDepth == node._maxDepth)
            {
                node._elements.Add(new(elementID, position));
                return;
            }

            // We have reached max amount of elements in the node, so we divide it in 4 quadrants
            Subdivide(node);

            for (int elementIndex = 0; elementIndex < elementsCount; elementIndex++)
            {
                var element = node._elements[elementIndex];
                InsertIntoChildren(node, element.Position, element.ExternalID, ref depth);  // Move the existing point to the children
            }
            node._elements.Clear();

            InsertIntoChildren(node, position, elementID, ref depth);                   // Insert the new point into the children
        }

        private static void InsertIntoChildren(Quadtree parent, Vector2 position, int elementID, ref int depth)
        {
            depth++;
            foreach (var child in parent._children)
            {
                Insert_Internal(child, position, elementID, ref depth);
            }
        }

        // Subdivide the node into 4 child quadrants
        private static void Subdivide(Quadtree node)
        {
            float midX = (node._boundary.MinX + node._boundary.MaxX) * 0.5f;
            float midY = (node._boundary.MinY + node._boundary.MaxY) * 0.5f;

            for (int i = 0; i < MAX_CHILDREN_COUNT; i++)
            {
                node._children[i] = node._pool.RequestPoolableObject();
            }

            node._children[0].Initialize(new AABB(node._boundary.MinX, midY, midX, node._boundary.MaxY), node._maxDepth, node._maxElementsCountPerNode, node._currentDepth + 1); // NW
            node._children[1].Initialize(new AABB(midX, midY, node._boundary.MaxX, node._boundary.MaxY), node._maxDepth, node._maxElementsCountPerNode, node._currentDepth + 1); // NE
            node._children[2].Initialize(new AABB(node._boundary.MinX, node._boundary.MinY, midX, midY), node._maxDepth, node._maxElementsCountPerNode, node._currentDepth + 1); // SW
            node._children[3].Initialize(new AABB(midX, node._boundary.MinY, node._boundary.MaxX, midY), node._maxDepth, node._maxElementsCountPerNode, node._currentDepth + 1); // SE
        }

        private void Initialize(AABB aABB, int maxDepth, int maxElementsCountPerNode, int currentDepth)
        {
            _maxElementsCountPerNode = maxElementsCountPerNode;
            _maxDepth = maxDepth;
            _boundary = aABB;
            _currentDepth = currentDepth;
        }

        // Nearest neighbor search
        public QueryResult QueryClosest(Vector2 position)
        {
            int bestElementID = int.MinValue;
            float bestDistSq = float.PositiveInfinity;

            QueryClosest_Internal(this, position, ref bestElementID, ref bestDistSq);
            return new QueryResult(bestElementID, Mathf.Sqrt(bestDistSq));
        }

        private static void QueryClosest_Internal(Quadtree node, Vector2 source, ref int bestElementID, ref float bestDistSq)
        {
            if (!node._boundary.Contains(source) && 
                DistanceSquaredToAABB(source, node._boundary) > bestDistSq)
            {
                return;
            }

            if (node.HasChildren)
            {
                // Check child nodes in order of proximity
                for (int childIndex = 0; childIndex < MAX_CHILDREN_COUNT; childIndex++)
                {
                    QueryClosest_Internal(node._children[childIndex], source, ref bestElementID, ref bestDistSq);
                }
            }
            // No element when children are present (it's a leaf), check if it's closer
            else
            {
                int elementCount = node._elements.Count;
                for (int elementIndex = 0; elementIndex < elementCount; elementIndex++)
                {
                    var element = node._elements[elementIndex];
                    float distSq = Vector2.SqrMagnitude(element.Position - source);
                    if (distSq < bestDistSq)
                    {
                        bestDistSq = distSq;
                        bestElementID = element.ExternalID;
                    }
                }
            }
        }

        public int QueryWithinRange_NoAlloc(Vector2 source, float range, QueryResult[] results)
        {
            // create a list to add the elements in range
            List<QueryResult> elementsInRange = UnityEngine.Pool.ListPool<QueryResult>.Get();

            // feed the list
            int elementsCount = QueryRange_Internal(this, source, range * range, elementsInRange);
            // sort the list by elements distance
            elementsInRange.Sort(_queryResultsComparer);

            // assign elements to results
            int minElementCount = Mathf.Min(elementsCount, results.Length);
            for (int i = 0; i < minElementCount; i++)
            {
                results[i] = elementsInRange[i];
            }

            elementsInRange.Clear();
            UnityEngine.Pool.ListPool<QueryResult>.Release(elementsInRange);
            return minElementCount;
        }

        private static int QueryRange_Internal(Quadtree node, Vector2 source, float sqRange, List<QueryResult> results)
        {
            if (!node._boundary.Contains(source))
                return 0;

            int count = 0;

            if (node.HasChildren)
            {
                // Check child nodes in order of proximity
                foreach (var child in node._children)
                {
                    count += QueryRange_Internal(child, source, sqRange, results);
                }
            }
            // No element when children are present, so we check that in an else
            else
            {
                int elementCount = node._elements.Count;
                for (int elementIndex = 0; elementIndex < elementCount; elementIndex++)
                {
                    var element = node._elements[elementIndex];
                    float distSq = Vector2.SqrMagnitude(element.Position - source);
                    if (distSq < sqRange)
                    {
                        results.Add(new(element.ExternalID, Mathf.Sqrt(distSq)));
                        count++;
                    }
                }
            }


            return count;
        }

        // Remove an element from the Quadtree
        public void Remove(Vector2 position, int elementID)
        {
            Remove_Internal(this, position, elementID);
        }

        private static bool Remove_Internal(Quadtree node, Vector2 position, int elementID)
        {
            if (!node._boundary.Contains(position))
                return false;

            if (node.HasChildren)
            {
                foreach (var child in node._children)
                {
                    if (Remove_Internal(child, position, elementID))
                    {
                        // After removal, check if all children are empty and merge them back
                        if (ShouldMerge(child))
                        {
                            for (int childIndex = 0; childIndex < MAX_CHILDREN_COUNT; childIndex++)
                            {
                                child._children[childIndex] = null;
                            }
                        }
                        return true;
                    }
                }
            }
            else
            {
                int elementCount = node._elements.Count;
                for (int elementIndex = 0; elementIndex < elementCount; elementIndex++)
                {
                    var element = node._elements[elementIndex];
                    // If this is the same element
                    if (element.ExternalID == elementID)
                    {
                        node._elements.RemoveAt(elementIndex);
                        return true;
                    }
                }
                return false;
            }

            return false;
        }

        private static bool ShouldMerge(Quadtree node)
        {
            if (node.HasChildren)
            {
                // Check if all children are empty or null, if so, merge them back into a single node
                foreach (var child in node._children)
                {
                    if (child._elements.Count > 0)
                        return false;
                }
            }
            return true;
        }

        // Remove all points from the Quadtree
        public void RemoveAll()
        {
            if (HasChildren)
            {
                for (int childIndex = 0; childIndex < MAX_CHILDREN_COUNT; childIndex++)
                {
                    _children[childIndex].RemoveAll();
                    _children[childIndex] = null;
                }
            }

            int elementCount = _elements.Count;
            for (int elementIndex = 0; elementIndex < elementCount; elementIndex++)
            {
                _elements[elementIndex] = new();
            }
            _elements.Clear();
        }


        // Helper function to calculate squared distance between a point and an AABB (for pruning)
        private static float DistanceSquaredToAABB(Vector2 point, AABB boundary)
        {
            // If the point is inside the boundary, distance is zero
            float dx = Mathf.Max(boundary.MinX - point.x, 0, point.x - boundary.MaxX);
            float dy = Mathf.Max(boundary.MinY - point.y, 0, point.y - boundary.MaxY);

            return dx * dx + dy * dy;
        }

        public void Enable()
        {
        }

        public void Disable()
        {
        }

        public void Destroy()
        {
        }

#if UNITY_EDITOR
        public void OnDrawGizmos()
        {
            OnDrawGizmos(this);
        }

        private void OnDrawGizmos(Quadtree node)
        {
            // Draw the boundary of the node
            Gizmos.color = Color.green; // Set the color for the boundary
            Gizmos.DrawLine(new Vector3(node._boundary.MinX, 0, node._boundary.MinY), new Vector3(node._boundary.MaxX, 0, node._boundary.MinY));
            Gizmos.DrawLine(new Vector3(node._boundary.MaxX, 0, node._boundary.MinY), new Vector3(node._boundary.MaxX, 0, node._boundary.MaxY));
            Gizmos.DrawLine(new Vector3(node._boundary.MaxX, 0, node._boundary.MaxY), new Vector3(node._boundary.MinX, 0, node._boundary.MaxY));
            Gizmos.DrawLine(new Vector3(node._boundary.MinX, 0, node._boundary.MaxY), new Vector3(node._boundary.MinX, 0, node._boundary.MinY));

            // Recursively draw the children
            if (node.HasChildren)
            {
                for (int childIndex = 0; childIndex < MAX_CHILDREN_COUNT; childIndex++)
                {
                    OnDrawGizmos(node._children[childIndex]);
                }
            }
        }
#endif
    }
}