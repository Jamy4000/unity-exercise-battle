using DataStructures.ViliWonka.KDTree;
using System.Collections.Generic;
using UnityEngine;

namespace DCLBattle.Battle
{
    public sealed class Army : IArmy
    {
        public IArmyModel Model { get; }
        public int RemainingUnitsCount => _units.Count;

        private readonly List<IUnit> _units;

        private readonly KDTree _tree = new KDTree(_MAX_POINTS_PER_LEAF_NODE);
        private readonly KDQuery _query = new KDQuery();
        private readonly List<int> _queryResults = new(128);
        private readonly List<float> _queryDistances = new(128);

        private const int _MAX_POINTS_PER_LEAF_NODE = 32;
        private List<IArmy> _enemyArmies = new();

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
                pointClound[i] = _units[i].Position;
            }

            _tree.Build(pointClound, _MAX_POINTS_PER_LEAF_NODE);

            /*
            foreach (IUnit unit in _units)
            {
                unit.ManualUpdate();
            }
            */
        }

        public Vector3 CalculateCenterPoint()
        {
            return DCLBattleUtils.GetCenter(_units);
        }

        public void RemoveUnit(IUnit unit)
        {
            _units.Remove(unit);
        }

        public void AddUnit(IUnit unit)
        {
            _units.Add(unit);
        }

        public IUnit GetClosestUnit(Vector3 source, out float distance)
        {
            // TODO Check that there are still units
            _queryResults.Clear();

            // spherical query
            _query.ClosestPoint(_tree, source, _queryResults, _queryDistances);

            distance = _queryDistances[0];
            return _units[_queryResults[0]];
        }

        public int GetUnitsInRadius_NoAlloc(Vector3 source, float radius, IUnit[] result)
        {
            _queryResults.Clear();

            // spherical query
            _query.Radius(_tree, source, radius, _queryResults);

            int maxResults = Mathf.Min(result.Length, _queryResults.Count);
            for (int resultIndex = 0; resultIndex < maxResults; resultIndex++)
            {
                result[resultIndex] = _units[_queryResults[resultIndex]];
            }

            return maxResults;
        }

        public List<IArmy> GetEnemyArmies()
        {
            return _enemyArmies;
        }

        public void AddEnemyArmy(IArmy enemy)
        {
            _enemyArmies.Add(enemy);
        }
    }
}