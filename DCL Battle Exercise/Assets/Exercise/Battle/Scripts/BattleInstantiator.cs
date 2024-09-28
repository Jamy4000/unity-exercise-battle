using System.Collections.Generic;
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
            private UnityEngine.Object _armyModel;
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

        private Army[] _armies;
        private Vector3 _forwardTarget;

        public Army GetArmy(int index) => _armies[index];
        public int GetArmiesCount() => _armies.Length;

        void Awake()
        {
            if (Instance != null)
                throw new System.Exception("An instance of BattleInstantiator already exists.");

            Instance = this;
            _armies = new Army[_armiesToSpawn.Length];

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

            Vector3 mainCenter = Vector3.zero;
            foreach (var army in _armies)
            {
                // TODO This is bad, we should just use an event and check when all armies but one are dead
                // TODO add army allies ?
                if (army.GetUnits().Count == 0)
                {
                    _gameOverMenu.gameObject.SetActive(true);
                    _gameOverMenu.Populate();
                }

                // Todo that's quite a big calculation for not much
                mainCenter += Utils.GetCenter(army.GetUnits());
            }

            // Todo that's quite a big calculation for not much
            mainCenter /= _armies.Length;

            Transform cameraTransform = Camera.main.transform;
            _forwardTarget = Vector3.Normalize(mainCenter - cameraTransform.position);

            cameraTransform.forward += (_forwardTarget - cameraTransform.forward) * 0.1f;
        }

        private Army InstanceArmy(ArmySpawnParameters parameters)
        {
            List<UnitBase> armyUnits = new();
            IArmyModel armyModel = parameters.GetModel();
            Bounds bounds = parameters.GetSpawnBounds();

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
                    UnitBase unitView = Instantiate(unitModel.UnitViewPrefab).GetComponent<UnitBase>();
                    unitView.army = armyModel;
                    unitView.transform.position = Utils.GetRandomPosInBounds(bounds);

                    unitView.GetComponentInChildren<Renderer>().material.color = armyModel.ArmyColor;

                    armyUnits.Add(unitView);
                }
            }

            return new Army(armyModel.ArmyColor, armyModel.Strategy, armyUnits);
        }
    }
}