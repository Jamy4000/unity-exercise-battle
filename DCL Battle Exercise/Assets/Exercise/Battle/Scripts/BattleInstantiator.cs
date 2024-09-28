using System.Linq;
using DCLBattle.LaunchMenu;
using UnityEngine;


namespace DCLBattle.Battle
{
    public class BattleInstantiator : MonoBehaviour
    {
        [System.Serializable]
        private struct ArmySpawnParameters
        {
            [SerializeField, Interface(typeof(IArmyModel))]
            private Object _armyModel;
            public readonly IArmyModel GetModel() => _armyModel as IArmyModel;

            [SerializeField]
            private BoxCollider _armySpawnBounds;
            public readonly Bounds GetSpawnBounds() => _armySpawnBounds.bounds;
        }

        // todo ewwww
        public static BattleInstantiator Instance { get; private set; }

        [SerializeField]
        private ArmySpawnParameters[] _armiesToSpawn;

        // TODO add an event instead
        [SerializeField]
        private GameOverMenu _gameOverMenu;

        private IArmy[] _armies;

        public IArmy GetArmy(int index) => _armies[index];
        public int GetArmiesCount() => _armies.Length;

        void Awake()
        {
            if (Instance != null)
                throw new System.Exception("An instance of BattleInstantiator already exists.");

            Instance = this;
            _armies = new IArmy[_armiesToSpawn.Length];

            for (int i = 0; i < _armiesToSpawn.Length; i++)
            {
                _armies[i] = InstanceArmy(_armiesToSpawn[i]);
            }
        }

        void Update()
        {
            foreach (var army in _armies)
            {
                army.Update();
            }

            foreach (var army in _armies)
            {
                // TODO This is bad, we should just use an event and check when all armies but one are dead
                // TODO add army allies ?
                if (army.RemainingUnitsCount == 0)
                {
                    _gameOverMenu.gameObject.SetActive(true);
                    _gameOverMenu.Populate();
                }
            }
        }

        private IArmy InstanceArmy(ArmySpawnParameters parameters)
        {
            IArmyModel armyModel = parameters.GetModel();
            Bounds bounds = parameters.GetSpawnBounds();

            // TODO Hide implementation
            IArmy newArmy = new Army(armyModel);

            for (int unitIndex = 0; unitIndex < IArmyModel.UnitLength; unitIndex++)
            {
                UnitType unitType = (UnitType)unitIndex;
                IUnitModel unitModel = armyModel.GetUnitModel(unitType);
                if (unitModel == null)
                    continue;

                int unitCount = armyModel.GetUnitCount(unitType);
                for (int j = 0; j < unitCount; j++)
                {
                    // TODO Pooling
                    UnitBase unit = Instantiate(unitModel.UnitViewPrefab).GetComponent<UnitBase>();
                    // TODO Inject on Pool
                    unit.Army = newArmy;
                    unit.transform.position = DCLBattleUtils.GetRandomPosInBounds(bounds);

                    newArmy.AddUnit(unit);
                }
            }

            return newArmy;
        }
    }
}