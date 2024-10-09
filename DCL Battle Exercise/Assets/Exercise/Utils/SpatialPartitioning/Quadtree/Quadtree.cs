using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Utils.SpatialPartitioning
{
    public interface ISpatialEntity<TData>
    {
        TData Position { get; }
        float GetSqDistance(TData otherPoint);
    }

    public sealed class Quadtree<TElement> : ISpatialPartitioner<TElement, Vector2> 
        where TElement : ISpatialEntity<Vector2>, System.IComparable<TElement>
    {
        private const int MAX_POINTS = 4;
        private readonly TElement[] _elements = new TElement[MAX_POINTS];
        private int _elementCount = 0;

        // Child nodes
        private Quadtree<TElement> _northEast;
        private Quadtree<TElement> _northWest;
        private Quadtree<TElement> _southEast;
        private Quadtree<TElement> _southWest;

        // Center and size of this node
        private Vector2 _center;
        private Vector2 _halfSize;

        private bool _isDivided = false;

        public Quadtree()
        {
            _center = Vector2.zero;
            _halfSize = Vector2.one;
        }

        public Quadtree(Vector2 center, Vector2 halfSize)
        {
            _center = center;
            if (halfSize == Vector2.zero)
                throw new System.Exception("Half size of the Quadtree should be above 0!");
            _halfSize = halfSize;
        }

        public void OnDrawGizmos()
        {
            // Set the Gizmos color to differentiate between root and subdivided nodes
            Gizmos.color = _isDivided ? Color.yellow : Color.green;

            // Draw the boundaries of this Quadtree node
            Gizmos.DrawWireCube(new(_center.x, 0f, _center.y), new(_halfSize.x * 2, 0.5f, _halfSize.y * 2f));

            // If the node is subdivided, recursively draw the child quadrants
            if (_isDivided)
            {
                _northEast.OnDrawGizmos();
                _northWest.OnDrawGizmos();
                _southEast.OnDrawGizmos();
                _southWest.OnDrawGizmos();
            }

            // Optionally, draw the points inside this node for visual debugging
            Gizmos.color = Color.red;
            for (int i = 0; i < _elementCount; i++)
            {
                Vector3 pos = new(_elements[i].Position.x, 0f, _elements[i].Position.y);
                Gizmos.DrawSphere(pos, 0.1f); // Adjust the size of the spheres as needed
            }
        }

        public void Insert(TElement element)
        {
            Vector2 point = element.Position;

            // If the point is outside the bounds, expand the quadtree
            if (!ContainsPoint(point))
            {
                throw new ArgumentException("point is outside quadtree bounds");
            }

            // Subdivide if needed and insert the point into the appropriate child node
            if (_elementCount == MAX_POINTS)
            {
                Subdivide();
            }

            Quadtree<TElement> container = GetContainerForPoint(point);
            if (container == null)
            {
                // TODO this may provoke OutOfBounds
                _elements[_elementCount++] = element;
            }
            else
            {
                container.Insert(element);
            }
        }

        public void Remove(TElement element)
        {
            var container = GetContainerForPoint(element.Position);

            if (container == null)
            {
                // Remove the point from the current node
                for (int i = 0; i < _elementCount; i++)
                {
                    // If both elements are the same
                    if (_elements[i].CompareTo(element) == 0)
                    {
                        _elements[i] = _elements[--_elementCount]; // Swap with last and decrease count
                        return;
                    }
                }

                throw new System.Exception("Couldn't remove element from quadtree");
            }
            else
            {
                container.Remove(element);
            }
        }

        public void RemoveAll()
        {
            _elementCount = 0;

            if (_isDivided)
            {
                _northEast.RemoveAll();
                _northWest.RemoveAll();
                _southEast.RemoveAll();
                _southWest.RemoveAll();
            }
        }

        public QueryResult<TElement> QueryClosest(Vector2 source)
        {
            if (_isDivided)
            {
                List<QueryResult<TElement>> results = new(4);

                if (_northWest.ContainsPoint(source))
                    results.Add(_northWest.QueryClosest(source));

                if (_northEast.ContainsPoint(source))
                    results.Add(_northEast.QueryClosest(source));

                if (_southWest.ContainsPoint(source))
                    results.Add(_southWest.QueryClosest(source));

                if (_southEast.ContainsPoint(source))
                    results.Add(_southEast.QueryClosest(source));

                QueryResult<TElement> finalResult = results[0];
                for (int resultIndex = 0; resultIndex < results.Count; resultIndex++)
                {
                    if (finalResult.Distance > results[resultIndex].Distance)
                        finalResult = results[resultIndex];
                }

                return finalResult;
            }
            else
            {
                float minDistanceSq = Mathf.Infinity;
                QueryResult<TElement> result = new();

                for (int elementIndex = 0; elementIndex < _elementCount; elementIndex++)
                {
                    var element = _elements[elementIndex];
                    float sqDistance = element.GetSqDistance(source);
                    if (sqDistance < minDistanceSq)
                    {
                        result = new QueryResult<TElement>(element, Mathf.Sqrt(sqDistance));
                        minDistanceSq = sqDistance;
                    }
                }

                return result;
            }
        }

        private Quadtree<TElement> GetContainerForPoint(Vector2 point)
        {
            if (!_isDivided)
                return null;

            if (_northWest.ContainsPoint(point))
                return _northWest;

            if (_northEast.ContainsPoint(point))
                return _northEast;

            if (_southWest.ContainsPoint(point))
                return _southWest;

            if (_southEast.ContainsPoint(point))
                return _southEast;

            return null;
        }

        public int QueryWithinRange_NoAlloc(Vector2 source, float range, QueryResult<TElement>[] results, int offset = 0)
        {
            // Check if the results array is empty.
            // Important for the slicing (results[count..]) done below when checking the children quadrants
            if (results.Length == 0)
                return 0;

            // Check if the current node intersects with the query range
            if (!IsWithinRange(source, range))
                return 0;

            float sqRange = range * range;
            int count = 0;

            // Check points in the current node
            for (int i = 0; i < _elementCount; i++)
            {
                float sqDist = _elements[i].GetSqDistance(source);
                if (sqDist <= sqRange)
                {
                    results[offset + count++] = new QueryResult<TElement>(_elements[i], Mathf.Sqrt(sqDist));

                    if (offset + count == results.Length)
                        return count; // Stop if results are full
                }
            }

            // Check children quadrants
            if (_isDivided)
            {
                count += _northEast.QueryWithinRange_NoAlloc(source, range, results, offset + count);
                count += _northWest.QueryWithinRange_NoAlloc(source, range, results, offset + count);
                count += _southEast.QueryWithinRange_NoAlloc(source, range, results, offset + count);
                count += _southWest.QueryWithinRange_NoAlloc(source, range, results, offset + count);
            }

            return count; // Return the total number of results found
        }

        private bool IsWithinRange(Vector2 point, float range)
        {
            return point.x >= (_center.x - _halfSize.x - range) &&
                   point.x <= (_center.x + _halfSize.x + range) &&
                   point.y >= (_center.y - _halfSize.y - range) &&
                   point.y <= (_center.y + _halfSize.y + range);
        }

        // Subdivide the current node into 4 quadrants
        private void Subdivide()
        {
            // no need to subdivide if we already did it
            if (_isDivided)
                return;

            Vector2 childHalfSize = _halfSize * 0.5f;

            _northWest = new Quadtree<TElement>(new Vector2(_center.x - childHalfSize.x, _center.y - childHalfSize.y), childHalfSize);
            _northEast = new Quadtree<TElement>(new Vector2(_center.x + childHalfSize.x, _center.y - childHalfSize.y), childHalfSize);
            _southWest = new Quadtree<TElement>(new Vector2(_center.x - childHalfSize.x, _center.y + childHalfSize.y), childHalfSize);
            _southEast = new Quadtree<TElement>(new Vector2(_center.x + childHalfSize.x, _center.y + childHalfSize.y), childHalfSize);

            foreach (var element in _elements)
            {
                Quadtree<TElement> containingChild = GetContainerForPoint(element.Position);
                // An element is only moved if it completely fits into a child quadrant.
                if (containingChild != null)
                {
                    Remove(element);
                    containingChild.Insert(element);
                }
            }

            _isDivided = true;
        }

        // Checks if a point is within the bounds of this quadtree node
        private bool ContainsPoint(Vector2 point)
        {
            return point.x >= _center.x - _halfSize.x && point.x < _center.x + _halfSize.x &&
                   point.y >= _center.y - _halfSize.y && point.y < _center.y + _halfSize.y;
        }
    }
}