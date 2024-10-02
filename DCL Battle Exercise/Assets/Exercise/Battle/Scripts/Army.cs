using DataStructures.ViliWonka.KDTree;
using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace DCLBattle.Battle
{
    public class Army
    {
        public readonly IArmyModel Model;
        public int RemainingUnitsCount => _units.Count;

        public IArmiesHolder ArmiesHolder { get; private set; }

        public Vector3 Center { get; private set; }
        public System.Action<Army> ArmyDefeatedEvent { get; set; }

        private readonly List<UnitBase> _units;

        private readonly KDTree _tree = new(_MAX_POINTS_PER_LEAF_NODE);

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

        public void Start()
        {
            ArmiesHolder = UnityServiceLocator.ServiceLocator.Global.Get<IArmiesHolder>();
            RebuildTree();
        }

        public void UpdateArmyData()
        {
            RebuildTree();
        }

        public void CalculateNewData()
        {
            foreach (var unit in _units)
            {
                unit.CalculateNewData();
            }
        }

        public void ApplyCalculatedData()
        {
            foreach (var unit in _units)
            {
                unit.ApplyCalculatedData();
            }
        }

        public void LateUpdate()
        {
            _units.RemoveAll(unit => unit.IsMarkedForDeletion);
        }

        private void RebuildTree()
        {
            Center = Vector3.zero;
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

        public UnitBase GetClosestUnit(Vector3 source, out float distance)
        {
            if (RemainingUnitsCount == 0)
            {
                distance = Mathf.Infinity;
                return null;
            }

            // Cannot pre-allocate since we are querying in threads
            var queryResults = new List<int>(1);
            var queryDistances = new List<float>(1);
            var query = new KDQuery();

            // spherical query
            query.ClosestPoint(_tree, source, queryResults, queryDistances);

            distance = queryDistances[0];
            return _units[queryResults[0]];
        }

        public int GetUnitsInRadius_NoAlloc(Vector3 source, float radius, (UnitBase unit, float distance)[] result)
        {
            if (RemainingUnitsCount == 0)
            {
                return 0;
            }

            // Cannot pre-allocate since we are querying in threads
            var queryResults = new List<int>(result.Length);
            var queryDistances = new List<float>(result.Length);
            var query = new KDQuery();

            // spherical query
            query.Radius(_tree, source, radius, queryResults, queryDistances);

            int maxResults = Mathf.Min(result.Length, queryResults.Count);
            for (int resultIndex = 0; resultIndex < maxResults; resultIndex++)
            {
                result[resultIndex] = (_units[queryResults[resultIndex]], queryDistances[resultIndex]);
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