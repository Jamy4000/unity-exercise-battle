using DataStructures.ViliWonka.KDTree;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCLBattle.Battle
{
    public class Army
    {
        public readonly IArmyModel Model;
        public int RemainingUnitsCount => _units.Count;

        public Vector3 Center { get; private set; }

        private readonly List<UnitBase> _units;

        private readonly KDTree _tree = new(_MAX_POINTS_PER_LEAF_NODE);
        private readonly KDQuery _query = new();
        private readonly List<int> _queryResults = new(128);
        private readonly List<float> _queryDistances = new(128);

        // High value = fast build, slow search; Low value = slow build, fast search
        private const int _MAX_POINTS_PER_LEAF_NODE = 2;
        private readonly List<Army> _enemyArmies = new();

        public Army(IArmyModel model)
        {
            Model = model;

            // pre-allocate the list
            int armySize = 0;
            for (int i = 0; i < IArmyModel.UnitLength; i++)
            {
                armySize += model.GetUnitCount((UnitType)i);
            }
            _units = new(armySize);
        }

        public void Update()
        {
            Vector3[] pointClound = new Vector3[RemainingUnitsCount];
            for (int i = 0; i < RemainingUnitsCount; i++)
            {
                Vector3 position = _units[i].Position;
                pointClound[i] = position;
                Center += position;
            }

            Center /= RemainingUnitsCount;
            _tree.Build(pointClound, _MAX_POINTS_PER_LEAF_NODE);
        }

        public void AddUnit(UnitBase unit)
        {
            _units.Add(unit);
        }

        public void RemoveUnit(UnitBase unit)
        {
            _units.Remove(unit);
        }

        public UnitBase GetClosestUnit(Vector3 source, out float distance)
        {
            // TODO this check shouldn't be necessary, search should not be done if an army is empty
            if (_units.Count == 0)
            {
                distance = Mathf.Infinity;
                return null;
            }

            _queryResults.Clear();

            // spherical query
            _query.ClosestPoint(_tree, source, _queryResults, _queryDistances);

            distance = _queryDistances[0];
            return _units[_queryResults[0]];
        }

        public int GetUnitsInRadius_NoAlloc(Vector3 source, float radius, (UnitBase unit, float distance)[] result)
        {
            // TODO this check shouldn't be necessary, search should not be done if an army is empty
            if (_units.Count == 0)
            {
                return 0;
            }

            _queryResults.Clear();

            // spherical query
            _query.Radius(_tree, source, radius, _queryResults, _queryDistances);

            int maxResults = Mathf.Min(result.Length, _queryResults.Count);
            for (int resultIndex = 0; resultIndex < maxResults; resultIndex++)
            {
                result[resultIndex] = (_units[_queryResults[resultIndex]], _queryDistances[resultIndex]);
            }

            return maxResults;
        }

        public UnitBase GetClosestEnemy(Vector3 position, out float closestDistance)
        {
            closestDistance = Mathf.Infinity;
            UnitBase closestEnemy = null;
            for (int armyIndex = 0; armyIndex < _enemyArmies.Count; armyIndex++)
            {
                UnitBase enemyUnit = _enemyArmies[armyIndex].GetClosestUnit(position, out float enemyDistance);
                // no unit found
                if (enemyUnit == null)
                    continue;

                if (closestEnemy == null || enemyDistance < closestDistance)
                {
                    closestEnemy = enemyUnit;
                    closestDistance = enemyDistance;
                }
            }

            return closestEnemy;
        }

        public List<Army> GetEnemyArmies()
        {
            return _enemyArmies;
        }

        public void AddEnemyArmy(Army enemy)
        {
            _enemyArmies.Add(enemy);
        }

        public void RemoveEnemyArmy(Army enemy)
        {
            _enemyArmies.Remove(enemy);
        }
    }
}