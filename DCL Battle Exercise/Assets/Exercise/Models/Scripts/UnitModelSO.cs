using DCLBattle.Battle;
using UnityEngine;

/// <summary>
/// ScriptableObject containing the data of a Unit
/// </summary>
[CreateAssetMenu(menuName = "DCLBattle/Units/Model/Basic", fileName = "UnitModel", order = 0)]
public class UnitModelSO : ScriptableObject, IUnitModel
{
    [SerializeField, ReadOnly]
    private UnitType _unitType = UnitType.UNDEFINED;
    public UnitType UnitType => _unitType;
    public string UnitName => UnitType.ToString();

    [SerializeField]
    private GameObject _unitPrefab;

    [SerializeField]
    private StrategySO[] _strategyCreators;

    [Header("Defense Parameters")]
    [SerializeField]
    private float _baseHealth = 50f;
    public float BaseHealth => _baseHealth;

    [SerializeField]
    private float _defense = 2f;
    public float Defense => _defense;

    [Header("Attack Parameters")]
    [SerializeField]
    private float _attackRange = 2f;
    public float AttackRange => _attackRange;
    public float AttackRangeSq { get; private set; }

    [SerializeField]
    private float _attackCooldown = 1f;
    public float AttackCooldown => _attackCooldown;

    [Header("FSM Data"), SerializeField]
    private UnitStateData[] _unitStatesData;
    public UnitStateData[] UnitStatesData => _unitStatesData;

    [SerializeField]
    private UnitStateID _defaultState = UnitStateID.Fighting;
    public UnitStateID DefaultState => _defaultState;

    protected virtual void OnEnable()
    {
        AttackRangeSq = AttackRange * AttackRange;
    }

    public virtual UnitBase InstantiateUnit(UnitCreationParameters parameters)
    {
        // TODO Pooling ? In case we want to add reinforcement during battle maybe ?
        UnitBase unitGameobject = Instantiate(_unitPrefab).GetComponent<UnitBase>();
        unitGameobject.Initialize(parameters);
        return unitGameobject;
    }

    public IStrategyUpdater CreateStrategyUpdater(ArmyStrategy strategy)
    {
        foreach (var strategyCreator in _strategyCreators)
        {
            if (strategyCreator.ArmyStrategy == strategy)
                return strategyCreator.CreateStrategyUpdater();
        }

        Debug.LogWarning($"No WarriorStrategySO was provided for {UnitName}'s {strategy} Strategy.");
        return null;
    }

    protected virtual void OnValidate()
    {
        _unitType = _unitPrefab != null ? _unitPrefab.GetComponent<UnitBase>().UnitType : UnitType.UNDEFINED;
    }
}

public abstract class StrategySO : ScriptableObject
{
    public abstract ArmyStrategy ArmyStrategy { get; }
    public abstract IStrategyUpdater CreateStrategyUpdater();
}

// Data Transfer Object; this avoid modifying the signature of CreateUnit if we want to add more data later on.
// If some units have specific data that need to be injected, they can extend this class to do so
public class UnitCreationParameters
{
    public readonly Vector3 Position;
    public readonly int UnitID;
    public readonly Quaternion Rotation;
    public readonly IUnitModel UnitModel;
    public readonly Army ParentArmy;
    public readonly IStrategyUpdater StrategyUpdater;

    public UnitCreationParameters(Vector3 position, int unitID, Quaternion rotation, Army parentArmy, IUnitModel unitModel, IStrategyUpdater strategyUpdater)
    {
        Position = position;
        UnitID = unitID;
        Rotation = rotation;
        UnitModel = unitModel;
        ParentArmy = parentArmy;
        StrategyUpdater = strategyUpdater;
    }
}