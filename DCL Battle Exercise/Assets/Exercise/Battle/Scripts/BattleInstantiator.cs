using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class BattleInstantiator : MonoBehaviour
{
    [System.Serializable]
    private struct ArmySpawnParameters
    {
        [SerializeField, Interface(typeof(IArmyModel))]
        private UnityEngine.Object armyModel;
        public readonly IArmyModel GetModel() => armyModel as IArmyModel;
        
        [SerializeField]
        private BoxCollider armySpawnBounds;
        public readonly Bounds GetSpawnBounds() => armySpawnBounds.bounds;
    }
    
    // todo ewwww
    public static BattleInstantiator instance { get; private set; }

    [SerializeField]
    private ArmySpawnParameters[] armiesToSpawn;

    [SerializeField]
    private GameOverMenu gameOverMenu;
    
    private Army[] _armies;
    private Vector3 forwardTarget;

    public Army GetArmy(int index) => _armies[index];
    public int GetArmiesCount() => _armies.Length;

    void Awake()
    {
        if (instance != null)
            throw new System.Exception("An instance of BattleInstantiator already exists.");
        
        instance = this;
        _armies = new Army[armiesToSpawn.Length];

        for (int i = 0; i < armiesToSpawn.Length; i++)
        {
            _armies[i] = InstanceArmy(armiesToSpawn[i]);
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
            if ( army.GetUnits().Count == 0 )
            {
                gameOverMenu.gameObject.SetActive(true);
                gameOverMenu.Populate();
            }

            // Todo that's quite a big calculation for not much
            mainCenter += Utils.GetCenter(army.GetUnits());
        }

        // Todo that's quite a big calculation for not much
        mainCenter /= _armies.Length;

        Transform cameraTransform = Camera.main.transform;
        forwardTarget = Vector3.Normalize(mainCenter - cameraTransform.position);

        cameraTransform.forward += (forwardTarget - cameraTransform.forward) * 0.1f;
    }

    private Army InstanceArmy(ArmySpawnParameters parameters)
    {
        List<UnitBase> armyUnits = new();
        IArmyModel armyModel = parameters.GetModel();
        Bounds bounds = parameters.GetSpawnBounds();
        var values = Enum.GetValues(typeof(UnitType)).Cast<UnitType>();

        foreach (UnitType unitType in values)
        {
            IUnitModel unitModel = armyModel.GetUnitModel(unitType);
            if (unitModel == null)
                continue;

            int unitCount = armyModel.GetUnitsCount(unitType);
            for (int j = 0; j < unitCount; j++)
            {
                // TODO Pooling
                UnitBase unit = Instantiate(unitModel.GetUnitsPrefab());
                unit.armyModel = armyModel;
                unit.transform.position = Utils.GetRandomPosInBounds(bounds);

                unit.GetComponentInChildren<Renderer>().material.color = armyModel.ArmyColor;

                armyUnits.Add(unit);
            }
        }

        return new Army(armyModel.ArmyColor, armyUnits);
    }
}