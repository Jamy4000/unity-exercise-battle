using DataStructures.ViliWonka.KDTree;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

namespace DCLBattle.Battle
{
    public readonly struct TargetInfo
    {
        public readonly int ArmyID;
        public readonly int UnitID;
        public readonly Vector3 Position;

        public TargetInfo(Vector3 position, int armyID = -1, int unitID = -1)
        {
            Position = position;
            ArmyID = armyID;
            UnitID = unitID;
        }

        public TargetInfo(int armyID = -1, int unitID = -1)
        {
            ArmyID = armyID;
            UnitID = unitID;
            Position = Vector3.zero;
        }
    }

    public struct UnitData
    {
        public Vector3 Position;
        public bool IsMarkedForDeletion;

        public readonly int ArmyID;
        public readonly int UnitID;
        public readonly float AttackRange;
        public readonly float AttackCooldown;
        public readonly float MaxDistanceFromCenter;
        public readonly float MinDistanceFromOtherUnits;

        private readonly (UnitData unit, float distance)[] _unitsInRadius;

        public UnitData(Vector3 position, int armyID, int unitID, float attackRange, float attackCooldown, float maxDistanceFromCenter, float minDistanceFromOtherUnits)
        {
            Position = position;
            IsMarkedForDeletion = false;

            ArmyID = armyID;
            UnitID = unitID;
            AttackRange = attackRange;
            AttackCooldown = attackCooldown;
            MaxDistanceFromCenter = maxDistanceFromCenter;
            MinDistanceFromOtherUnits = minDistanceFromOtherUnits;

            _unitsInRadius = new (UnitData unit, float distance)[16];
        }

        public void CalculateNewData(IStrategyUpdater strategyUpdater, IArmiesHolder armiesHolder, out TargetInfo targetInfo)
        {
            // We first make sure the unit is staying around the battle
            Vector3 moveOffset = CalculateEvadeAlliesOffset(armiesHolder);

            // We then calculate the move offset for this unit using the strategy of the army
            targetInfo = strategyUpdater.UpdateStrategy(this, out Vector3 strategyMovement);

            moveOffset = Vector3.Normalize(moveOffset + strategyMovement);

            // We finally move the unit
            // TODO Unit Movement Speed + deltatime
            Position += moveOffset * 0.1f;
        }

        private readonly Vector3 CalculateEvadeAlliesOffset(IArmiesHolder armiesHolder)
        {
            // First, we check that the unit isn't too far from the center of the battle
            Vector3 center = armiesHolder.BattleCenter;
            Vector3 unitToCenter = center - Position;
            float centerDistanceSq = Vector3.SqrMagnitude(unitToCenter);

            // If unit is too far from the battle's center point
            if (centerDistanceSq > MaxDistanceFromCenter * MaxDistanceFromCenter)
            {
                // we move them to the center, regardless of them overlapping another unit or not
                float centerDistance = Mathf.Sqrt(centerDistanceSq);

                // normalizing
                unitToCenter /= centerDistance;

                return unitToCenter * (centerDistance - MaxDistanceFromCenter);
            }

            // if the unit is close enough from the battle, we make sure they are not overlapping other units
            Vector3 moveOffset = Vector3.zero;
            // for every armies on the map
            for (int armyIndex = 0; armyIndex < armiesHolder.ArmiesCount; armyIndex++)
            {
                var army = armiesHolder.GetArmy(armyIndex);

                // We check to find the units within X radius of the current unit
                int unitsInRadiusCount = army.GetUnitsInRadius_NoAlloc(Position, MinDistanceFromOtherUnits, _unitsInRadius);

                // for every unit within that radius
                for (int unitIndex = 0; unitIndex < unitsInRadiusCount; unitIndex++)
                {
                    // we move our unit away
                    UnitData otherUnit = _unitsInRadius[unitIndex].unit;
                    float distance = _unitsInRadius[unitIndex].distance;

                    Vector3 toNearest = Vector3.Normalize(otherUnit.Position - Position);
                    toNearest *= (MinDistanceFromOtherUnits - distance);
                    moveOffset -= toNearest;
                }
            }

            return moveOffset;
        }
    }

    public class Army
    {
        public readonly IArmyModel Model;
        public int RemainingUnitsCount => _units.Count;

        public IArmiesHolder ArmiesHolder { get; private set; }

        public Vector3 Center { get; private set; }
        public System.Action<Army> ArmyDefeatedEvent { get; set; }

        private readonly List<UnitBase> _units;
        private readonly List<UnitData> _unitsData;
        private readonly List<TargetInfo> _targets;

        private readonly KDTree _tree = new(_MAX_POINTS_PER_LEAF_NODE);

        // High value = fast build, slow search; Low value = slow build, fast search
        private const int _MAX_POINTS_PER_LEAF_NODE = 2;
        private readonly List<Army> _enemyArmies = new();

        public readonly int ArmyID;

        public Army(IArmyModel model, int armyID)
        {
            Model = model;
            ArmyID = armyID;

            // pre-allocate the list
            int armySize = 0;
            for (int i = 0; i < IArmyModel.UnitLength; i++)
            {
                armySize += model.GetUnitCount((UnitType)i);
            }
            _units = new(armySize);
            _unitsData = new(armySize);
            _targets = new(armySize);
        }

        public void Start()
        {
            ArmiesHolder = UnityServiceLocator.ServiceLocator.Global.Get<IArmiesHolder>();
            ArmiesHolder.ArmyDefeatedEvent += RemoveEnemyArmy;
            RebuildTree();
        }

        public void UpdateArmyData()
        {
            RebuildTree();
        }

        private ParallelLoopResult _loopResult;
        public bool IsDoneUpdatingUnits => _loopResult.IsCompleted;

        public void CalculateNewData()
        {
            _loopResult = Parallel.For(0, RemainingUnitsCount, (int i) =>
            {
                _unitsData[i].CalculateNewData(_units[i].StrategyUpdater, ArmiesHolder, out TargetInfo target);
                _targets[i] = target;
            });
        }

        public void ApplyCalculatedData()
        {
            for (int i = 0; i < RemainingUnitsCount; i++)
            {
                _units[i].ApplyCalculatedData(_unitsData[i], _targets[i], ArmiesHolder);
            }
        }

        public void LateUpdate()
        {
            for (int i = _unitsData.Count - 1; i > -1; i--)
            {
                if (_unitsData[i].IsMarkedForDeletion)
                {
                    _units.RemoveAt(i);
                    _unitsData.RemoveAt(i);
                    _targets.RemoveAt(i);
                }
            }
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
            // todo hard coded
            _unitsData.Add(new UnitData(unit.Position, unit.Army.ArmyID, unit.UnitID, unit.AttackRange, unit.AttackCooldown, 80f, 2f));
            _targets.Add(new());
        }

        /*
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
        */

        public UnitData GetClosestUnit(Vector3 source, out float distance)
        {
            if (RemainingUnitsCount == 0)
            {
                distance = Mathf.Infinity;
                return default;
            }

            // Cannot pre-allocate since we are querying in threads
            var queryResults = new List<int>(1);
            var queryDistances = new List<float>(1);
            var query = new KDQuery();

            // spherical query
            query.ClosestPoint(_tree, source, queryResults, queryDistances);

            distance = queryDistances[0];
            return _unitsData[queryResults[0]];
        }

        public int GetUnitsInRadius_NoAlloc(Vector3 source, float radius, (UnitData unit, float distance)[] result)
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
                result[resultIndex] = (_unitsData[queryResults[resultIndex]], queryDistances[resultIndex]);
            }

            return maxResults;
        }

        public TargetInfo GetClosestEnemy(Vector3 position, out float closestDistance)
        {
            UnitData closestEnemy = _enemyArmies[0].GetClosestUnit(position, out float enemyDistance);
            closestDistance = enemyDistance;

            for (int armyIndex = 1; armyIndex < _enemyArmies.Count; armyIndex++)
            {
                UnitData enemyUnit = _enemyArmies[armyIndex].GetClosestUnit(position, out enemyDistance);

                if (enemyDistance < closestDistance)
                {
                    closestEnemy = enemyUnit;
                    closestDistance = enemyDistance;
                }
            }

            return new TargetInfo(closestEnemy.Position, closestEnemy.ArmyID, closestEnemy.UnitID);
        }

        /*
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
        */

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

        public UnitBase GetUnit(int unitID)
        {
            return _units[unitID];
        }
    }
}