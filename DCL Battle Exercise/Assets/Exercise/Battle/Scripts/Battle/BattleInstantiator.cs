using UnityEngine;
using Utils;

namespace DCLBattle.Battle
{
    /// <summary>
    /// This is the entry point to create all the armies and units in the game.
    /// The armies are then updated in the BattleUpdater script, and each army is in charge of updating its own units.
    /// </summary>
    public sealed class BattleInstantiator : MonoBehaviour
    {
        /// <summary>
        /// Simple serialized struct to link an army to its spawn point
        /// </summary>
        [System.Serializable]
        private struct ArmySpawnParameters
        {
            [SerializeField]
            private Object _armyModel;
            public readonly IArmyModel ArmyModel => _armyModel as IArmyModel;

            // TODO We may want to let the user chose where the armies spawn
            [SerializeField]
            private BoxCollider _armySpawnBounds;
            public readonly Bounds GetSpawnBounds() => _armySpawnBounds.bounds;
        }

        // TODO We may want to get that from a scriptable object instead, in case we want to let player add/remove armies in the launch menu
        [SerializeField]
        private ArmySpawnParameters[] _armiesToSpawn;

        [Header("FSM Data"), SerializeField]
        private BattleStateData[] _battleStatesData;

        [SerializeField]
        private BattleStateID _defaultState = BattleStateID.OnGoing;

        private static readonly IStrategyUpdater[,] _strategyUpdaters = new IStrategyUpdater[IArmyModel.UnitLength, IStrategyUpdater.StrategyCount];

        [SerializeField, Interface(typeof(IServiceLocator))]
        private Object _serviceLocatorObject;
        private IServiceLocator _serviceLocator;

        private IArmiesHolder _armiesHolder;

        void Awake()
        {
            _serviceLocator = _serviceLocatorObject as IServiceLocator;

            // TODO Hide Implementation
            var armies = new Army[_armiesToSpawn.Length];

            // For each army that should spawn on the map
            for (int armyIndex = 0; armyIndex < _armiesToSpawn.Length; armyIndex++)
            {
                ArmySpawnParameters armySpawnParam = _armiesToSpawn[armyIndex];
                armies[armyIndex] = CreateArmy(armySpawnParam.ArmyModel, armySpawnParam.GetSpawnBounds());
            }

            // Second pass to feed the enemy armies inside each army
            for (int armyIndex = 0; armyIndex < armies.Length; armyIndex++)
            {
                int allianceID = armies[armyIndex].Model.AllianceID;

                for (int secondArmyIndex = 0; secondArmyIndex < armies.Length; secondArmyIndex++)
                {
                    // If we are checking the army against itself, skip
                    if (armyIndex == secondArmyIndex)
                        continue;

                    // If the two armies have different alliance id, we mark them as enemies
                    if (armies[secondArmyIndex].Model.AllianceID != allianceID)
                        armies[armyIndex].AddEnemyArmy(armies[secondArmyIndex]);
                }
            }

            // TODO Hide Implementation
            _armiesHolder = new BattleUpdater(armies, _serviceLocator, _battleStatesData, _defaultState);
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;

            for (int armyIndex = 0; armyIndex < _armiesHolder.ArmiesCount; armyIndex++)
            {
                var army = _armiesHolder.GetArmy(armyIndex);
                if (army.RemainingUnitsCount == 0)
                    continue;

                Gizmos.color = army.Model.ArmyColor;
                Gizmos.DrawSphere(army.Center, 2f);
            }

            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(_armiesHolder.BattleCenter, 4f);
        }

        private void OnDestroy()
        {
            _armiesHolder.Dispose();
        }

        private Army CreateArmy(IArmyModel armyModel, Bounds spawnBounds)
        {
            // TODO remove hard implementation
            Army army = new(armyModel, _serviceLocator);

            // For each type of unit in the game
            for (int unitTypeIndex = 0; unitTypeIndex < IArmyModel.UnitLength; unitTypeIndex++)
            {
                // If the current army has this type of unit in its rank
                if (armyModel.TryGetUnitModel((UnitType)unitTypeIndex, out IUnitModel unitModel))
                {
                    // We check if the strategy updaters were generated for this unit type
                    CheckForStrategyUpdaterCreation(unitModel);

                    // We create the amount of units provided in the Launch Menu
                    CreateUnitsForArmy(armyModel, army, unitModel, spawnBounds);
                }
            }

            return army;
        }

        private void CreateUnitsForArmy(IArmyModel armyModel, Army army, IUnitModel unitModel, Bounds spawnBounds)
        {
            // We spawn the amount of units of this type provided in the Launch Menu
            int maxUnitCount = armyModel.GetUnitCount(unitModel.UnitType);
            for (int unitIndex = 0; unitIndex < maxUnitCount; unitIndex++)
            {
                // We generate the parameters to create the Unit
                Vector3 position = DCLBattleUtils.GetRandomPosInBounds(spawnBounds);
                // Could randomize that or make them face a certain direction
                Quaternion rotation = Quaternion.identity;
                // Provides the reference to the strategy Updater that this unit will be using at the start of the battle, based on its army configuration
                IStrategyUpdater strategyUpdater = _strategyUpdaters[(int)unitModel.UnitType, (int)armyModel.Strategy];

                UnitCreationParameters parameters = new(position, rotation, army, unitModel, strategyUpdater, unitIndex);

                UnitBase newUnit = unitModel.InstantiateUnit(parameters);

                army.AddUnit(newUnit);
            }
        }

        private void CheckForStrategyUpdaterCreation(IUnitModel unitModel)
        {
            int unitTypeIndex = (int)unitModel.UnitType;

            // No need to recreate the strategy updaters multiple time, one object for all units of the same type using the same strategy is enough
            if (_strategyUpdaters[unitTypeIndex, 0] == null)
            {
                for (int strategyIndex = 0; strategyIndex < IStrategyUpdater.StrategyCount; strategyIndex++)
                {
                    // This will create extra stratyUpdaters that may not be necessary, though we may change design to have a dynamic strategy instead while the battle is ongoing
                    _strategyUpdaters[unitTypeIndex, strategyIndex] = unitModel.CreateStrategyUpdater((ArmyStrategy)strategyIndex);
                }
            }
        }
    }
}