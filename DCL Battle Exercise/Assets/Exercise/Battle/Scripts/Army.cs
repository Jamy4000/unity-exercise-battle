using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Pool;
using Utils;
using Utils.SpatialPartitioning;
using Debug = UnityEngine.Debug;

namespace DCLBattle.Battle
{
    public class Army : IServiceConsumer, System.IDisposable
    {
        public readonly IArmyModel Model;
        public int RemainingUnitsCount => _units.Count;

        public IArmiesHolder ArmiesHolder { get; private set; }

        public Vector3 Center { get; private set; }
        public System.Action<Army> ArmyDefeatedEvent { get; set; }

        private readonly List<UnitBase> _units;

        private readonly ISpatialPartitioner<Vector2> _spatialPartitioner;
        private readonly QueryResult[] _radiusQueryResults = new QueryResult[32];

        private readonly List<Army> _enemyArmies = new();
        private readonly List<Vector2> _unitsPositions = new(256);
        private readonly List<int> _unitsIndices = new(256);

        private readonly System.Action<Army> _cachedArmyDefeatedCallback;

        public Army(IBattleModel battleModel, IArmyModel model, IServiceLocator serviceLocator)
        {
            Model = model;
            serviceLocator.AddConsumer(this);

            _spatialPartitioner = battleModel.CreateSpatialPartitioner<Vector2>();

            _cachedArmyDefeatedCallback = RemoveEnemyArmy;

            // pre-allocate the list
            int armySize = 0;
            for (int i = 0; i < IArmyModel.UnitLength; i++)
            {
                armySize += model.GetUnitCount((UnitType)i);
            }
            _units = new(armySize);
        }

#if UNITY_EDITOR
        public void OnDrawGizmos()
        {
            _spatialPartitioner.OnDrawGizmos();
        }
#endif

        public void Start()
        {
            RebuildTree();
        }

        public void Dispose()
        {
            _spatialPartitioner.Dispose();
            
#if UNITY_EDITOR
            Debug.Log($"{Model.ArmyName}: Mean Update time for Units: {(unitUpdateElapsedTime / updatesCount).ToString()}");
#endif
        }

        public void UpdateArmyData()
        {
            RebuildTree();
        }

        private float unitUpdateElapsedTime = 0f;
        private int updatesCount = 0;

        public void UpdateUnits()
        {
#if UNITY_EDITOR
            Stopwatch stopwach = Stopwatch.StartNew();
#endif
            foreach (var unit in _units)
            {
                unit.ManualUpdate();
            }
            
#if UNITY_EDITOR
            stopwach.Stop();
            unitUpdateElapsedTime += stopwach.ElapsedMilliseconds;
            updatesCount++;
#endif
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
            _unitsPositions.Clear();
            _unitsIndices.Clear();
            
            for (int unitIndex = 0; unitIndex < RemainingUnitsCount; unitIndex++)
            {
                Vector3 position = _units[unitIndex].Position;
                _unitsPositions.Add(new Vector2(position.x, position.z));
                _unitsIndices.Add(unitIndex);
                Center += position;
            }
            
            Center /= RemainingUnitsCount;
            
            _spatialPartitioner.InsertPointCloud(_unitsPositions, _unitsIndices);
        }

        public void AddUnit(UnitBase unit)
        {
            _units.Add(unit);
        }

        private UnitBase GetClosestUnit(Vector3 source, out float distance)
        {
            var queryResult = _spatialPartitioner.QueryClosest(new Vector2(source.x, source.z));

            distance = queryResult.Distance;
            return _units[queryResult.ElementID];
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
                result[resultIndex] = (_units[queryResult.ElementID], queryResult.Distance);
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
            enemy.ArmyDefeatedEvent += _cachedArmyDefeatedCallback;
        }

        private void RemoveEnemyArmy(Army enemy)
        {
            _enemyArmies.Remove(enemy);
            enemy.ArmyDefeatedEvent -= _cachedArmyDefeatedCallback;
        }

        public void ConsumeLocator(IServiceLocator locator)
        {
            ArmiesHolder = locator.GetService<IArmiesHolder>();
        }
    }
}