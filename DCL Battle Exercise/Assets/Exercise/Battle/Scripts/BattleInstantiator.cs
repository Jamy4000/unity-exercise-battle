using System.Collections.Generic;
using UnityEngine;


public class BattleInstantiator : MonoBehaviour
{
    [System.Serializable]
    private struct ArmySpawnParameters
    {
        [SerializeField]
        private ArmyModelSO armyModel;
        public IArmyModel GetModel() => armyModel;
        
        [SerializeField]
        private BoxCollider armySpawnBounds;
        public Bounds GetSpawnBounds() => armySpawnBounds.bounds;
        
        [SerializeField]
        private Color armyColor;
        public Color GetArmyColor() => armyColor;
    }
    
    // todo ewwww
    public static BattleInstantiator instance { get; private set; }

    [SerializeField]
    private ArmySpawnParameters[] armiesToSpawn;

    [SerializeField]
    private Warrior warriorPrefab;

    [SerializeField]
    private Archer archerPrefab;

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
        IArmyModel model = parameters.GetModel();
        Color color = parameters.GetArmyColor();
        Bounds bounds = parameters.GetSpawnBounds();

        // TODO Should be only one for loop
        for (int i = 0; i < model.Warriors; i++)
        {
            // TODO Pooling
            GameObject go = Instantiate(warriorPrefab.gameObject);
            go.transform.position = Utils.GetRandomPosInBounds(bounds);

            go.GetComponentInChildren<UnitBase>().armyModel = model;
            go.GetComponentInChildren<Renderer>().material.color = color;

            armyUnits.Add(go.GetComponent<UnitBase>());
        }

        for (int i = 0; i < parameters.GetModel().Archers; i++)
        {
            GameObject go = Object.Instantiate(archerPrefab.gameObject);
            go.transform.position = Utils.GetRandomPosInBounds(bounds);

            go.GetComponentInChildren<UnitBase>().armyModel = model;
            go.GetComponentInChildren<Renderer>().material.color = color;

            armyUnits.Add(go.GetComponent<UnitBase>());
        }

        return new Army(color, armyUnits);
    }
}