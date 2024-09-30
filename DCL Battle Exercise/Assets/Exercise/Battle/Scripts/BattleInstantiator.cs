using UnityEngine;

namespace DCLBattle.Battle
{
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

        private Army[] _armies;

        public Army GetArmy(int index) => _armies[index];
        public int ArmiesCount => _armies.Length;
        public Vector3 BattleCenter { get; private set; }

        private static readonly IStrategyUpdater[,] _strategyUpdaters = new IStrategyUpdater[IArmyModel.UnitLength, IStrategyUpdater.StrategyCount];

        void Awake()
        {
            _armies = new Army[_armiesToSpawn.Length];

            // For each army that should spawn on the map
            for (int armyIndex = 0; armyIndex < _armiesToSpawn.Length; armyIndex++)
            {
                ArmySpawnParameters armySpawnParam = _armiesToSpawn[armyIndex];
                _armies[armyIndex] = CreateArmy(armySpawnParam.ArmyModel, armySpawnParam.GetSpawnBounds());
            }

            // Second pass to feed the enemy armies inside each army
            for (int armyIndex = 0; armyIndex < _armies.Length; armyIndex++)
            {
                int allianceID = _armies[armyIndex].Model.AllianceID;

                for (int secondArmyIndex = 0; secondArmyIndex < _armies.Length; secondArmyIndex++)
                {
                    // If we are checking the army against itself, skip
                    if (armyIndex == secondArmyIndex)
                        continue;

                    // If the two armies have different alliance id, we mark them as enemies
                    if (_armies[secondArmyIndex].Model.AllianceID != allianceID)
                        _armies[armyIndex].AddEnemyArmy(_armies[secondArmyIndex]);
                }
            }

            // Registering this as global, but if we want multiple BattleManagers, we could register it on the Scene level as well
            UnityServiceLocator.ServiceLocator.Global.Register(this);
        }

        void Update()
        {
            foreach (var army in _armies)
            {
                if (army.RemainingUnitsCount == 0)
                    continue;

                BattleCenter += army.Center;
                
                army.Update();
            }

            BattleCenter /= _armies.Length;
        }

        private Army CreateArmy(IArmyModel armyModel, Bounds spawnBounds)
        {
            // TODO remove hard implementation
            Army army = new Army(armyModel);

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

                UnitCreationParameters parameters = new(position, rotation, army, unitModel, strategyUpdater);

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