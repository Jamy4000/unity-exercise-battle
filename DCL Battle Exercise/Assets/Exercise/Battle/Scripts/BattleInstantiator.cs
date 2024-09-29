using UnityEngine;
using UnityServiceLocator;

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

        private IArmy[] _armies;

        public IArmy GetArmy(int index) => _armies[index];
        public int ArmiesCount => _armies.Length;


        private static readonly IStrategyUpdater[,] _strategyUpdaters = new IStrategyUpdater[IArmyModel.UnitLength, IStrategyUpdater.StrategyCount];



        void Awake()
        {
            _armies = new IArmy[_armiesToSpawn.Length];


            // For each army that should spawn on the map
            for (int armyIndex = 0; armyIndex < _armiesToSpawn.Length; armyIndex++)
            {
                ArmySpawnParameters armySpawnParam = _armiesToSpawn[armyIndex];

                // TODO remove hard implementation
                IArmy army = new Army(armySpawnParam.ArmyModel);
                _armies[armyIndex] = army;

                // For each type of unit in the game
                for (int unitTypeIndex = 0; unitTypeIndex < IArmyModel.UnitLength; unitTypeIndex++)
                {
                    // If the current army has this type of unit in its rank
                    UnitType unitType = (UnitType)unitTypeIndex;
                    if (armySpawnParam.ArmyModel.TryGetUnitModel(unitType, out IUnitModel model))
                    {
                        // No need to recreate the strategy updaters multiple tile, one object for all units of the same type using the same strategy is enough
                        if (_strategyUpdaters[unitTypeIndex, 0] == null)
                        {
                            for (int strategyIndex = 0; strategyIndex < IStrategyUpdater.StrategyCount; strategyIndex++)
                            {
                                _strategyUpdaters[unitTypeIndex, strategyIndex] = model.CreateStrategyUpdater((ArmyStrategy)strategyIndex);
                            }
                        }

                        // We spawn the amount of units of this type provided in the Launch Menu
                        int maxUnitCount = armySpawnParam.ArmyModel.GetUnitCount(unitType);
                        for (int unitIndex = 0; unitIndex < maxUnitCount; unitIndex++)
                        {
                            // We generate the parameters to create the Unit
                            Vector3 position = DCLBattleUtils.GetRandomPosInBounds(armySpawnParam.GetSpawnBounds());
                            // Could randomize that or make them face a certain direction
                            Quaternion rotation = Quaternion.identity;
                            IStrategyUpdater strategyUpdater = _strategyUpdaters[(int)model.UnitType, (int)armySpawnParam.ArmyModel.Strategy];

                            UnitCreationParameters parameters = new(position, rotation, army, model.UnitType, strategyUpdater);

                            // Todo do we really need a model ?
                            IUnit newUnit = model.InstantiateUnit(parameters);
                            
                            army.AddUnit(newUnit);
                        }
                    }
                }
            }

            // Registering this as global, but if we want multiple BattleManagers, we could register it on the Scene level as well
            ServiceLocator.Global.Register(this);
        }

        void Update()
        {
            // TODO introduce GameUpdater
            foreach (var army in _armies)
            {
                army.Update();
            }
        }
    }
}