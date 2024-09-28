using UnityEngine;
using UnityServiceLocator;

namespace DCLBattle.Battle
{
    public sealed class BattleInstantiator : MonoBehaviour
    {
        [System.Serializable]
        private struct ArmySpawnParameters
        {
            [SerializeField, Interface(typeof(IArmyModel))]
            private Object _armyModel;
            public readonly IArmyModel ArmyModel => _armyModel as IArmyModel;

            // TODO We may want to let the user chose where the armies spawn
            [SerializeField]
            private BoxCollider _armySpawnBounds;
            public readonly Bounds GetSpawnBounds() => _armySpawnBounds.bounds;
        }

        [SerializeField]
        private ArmySpawnParameters[] _armiesToSpawn;

        private IArmy[] _armies;

        public IArmy GetArmy(int index) => _armies[index];
        public int GetArmiesCount() => _armies.Length;

        void Awake()
        {
            _armies = new IArmy[_armiesToSpawn.Length];

            for (int armyIndex = 0; armyIndex < _armiesToSpawn.Length; armyIndex++)
            {
                var armySpawnParam = _armiesToSpawn[armyIndex];

                // TODO remove hard implementation
                var army = _armies[armyIndex] = new Army(armySpawnParam.ArmyModel);

                // For each type of unit in the game
                for (int unitTypeIndex = 0; unitTypeIndex < IArmyModel.UnitLength; unitTypeIndex++)
                {
                    // If the current army has this type of unit in its rank
                    UnitType unitType = (UnitType)unitTypeIndex;
                    if (armySpawnParam.ArmyModel.TryGetUnitModel(unitType, out IUnitModel unitModel))
                    {
                        // We spawn this unit based on the value given in the Launch Menu
                        int maxUnitCount = armySpawnParam.ArmyModel.GetUnitCount(unitType);
                        for (int unitIndex = 0; unitIndex < maxUnitCount; unitIndex++)
                        {
                            Vector3 position = DCLBattleUtils.GetRandomPosInBounds(armySpawnParam.GetSpawnBounds());
                            Quaternion rotation = Quaternion.identity;

                            UnitCreationParameters parameters = new(position, rotation, unitModel);
                            IUnit newUnit = unitModel.UnitFactory.CreateUnit(army, parameters);
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