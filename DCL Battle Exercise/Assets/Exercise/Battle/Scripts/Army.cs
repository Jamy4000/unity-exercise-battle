using System.Collections.Generic;
using UnityEngine;
using Utils;
using Utils.SpatialPartitioning;

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

        private readonly ISpatialPartitioner<UnitBase, Vector2> _spatialPartitioner;
        private readonly QueryResult<UnitBase>[] _radiusQueryResults = new QueryResult<UnitBase>[32];

        private readonly List<Army> _enemyArmies = new();

        public Army(IArmyModel model)
        {
            Model = model;
            //_spatialPartitioner = new Quadtree<UnitBase>(Vector2.zero, Vector2.one * 100f);
            _spatialPartitioner = new KDTree<UnitBase, Vector2>(2, new TwoDimensionComparer());

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

        public void UpdateUnits()
        {
            foreach (var unit in _units)
            {
                unit.ManualUpdate();
            }
        }

        public void LateUpdate()
        {
            _units.RemoveAll(unit => unit.IsMarkedForDeletion);

            if (RemainingUnitsCount == 0)
            {
                ArmyDefeatedEvent?.Invoke(this);
            }
        }

        private void RebuildTree()
        {
            Center = Vector3.zero;
            _spatialPartitioner.RemoveAll();

            for (int i = 0; i < RemainingUnitsCount; i++)
            {
                _spatialPartitioner.Insert(_units[i]);
                Center += _units[i].Position;
            }

            Center /= RemainingUnitsCount;
        }

        public void AddUnit(UnitBase unit)
        {
            _units.Add(unit);
        }

        public UnitBase GetClosestUnit(Vector3 source, out float distance)
        {
            var queryResult = _spatialPartitioner.QueryClosest(new(source.x, source.z));

            distance = queryResult.Distance;
            return queryResult.Element;
        }

        public int GetUnitsInRadius_NoAlloc(Vector3 source, float radius, (UnitBase unit, float distance)[] result)
        {
            if (RemainingUnitsCount == 0)
            {
                return 0;
            }

            Vector2 source2D = new(source.x, source.z);
            int resultCount = _spatialPartitioner.QueryWithinRange_NoAlloc(source2D, radius, _radiusQueryResults);

            int maxResults = Mathf.Min(result.Length, resultCount);
            for (int resultIndex = 0; resultIndex < maxResults; resultIndex++)
            {
                var queryResult = _radiusQueryResults[resultIndex];
                result[resultIndex] = (queryResult.Element, queryResult.Distance);
            }

            return maxResults;
        }

        public UnitBase GetClosestEnemy(Vector3 position, out float closestDistance)
        {
            UnitBase closestEnemy = _enemyArmies[0].GetClosestUnit(position, out closestDistance);

            for (int armyIndex = 1; armyIndex < _enemyArmies.Count; armyIndex++)
            {
                UnitBase enemyUnit = _enemyArmies[armyIndex].GetClosestUnit(position, out float enemyDistance);

                if (enemyDistance < closestDistance)
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
            enemy.ArmyDefeatedEvent += RemoveEnemyArmy;
        }

        private void RemoveEnemyArmy(Army enemy)
        {
            _enemyArmies.Remove(enemy);
        }
    }
}