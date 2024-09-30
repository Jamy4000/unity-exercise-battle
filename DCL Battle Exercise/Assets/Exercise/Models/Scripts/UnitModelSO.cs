using DCLBattle.Battle;
using UnityEngine;

/// <summary>
/// ScriptableObject containing the data of a Unit
/// </summary>
[CreateAssetMenu(menuName = "DCLBattle/Units/Create Unit Model", fileName = "UnitModel", order = 0)]
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

    public virtual IUnit InstantiateUnit(UnitCreationParameters parameters)
    {
        // TODO Pooling
        IUnit unitGameobject = Instantiate(_unitPrefab).GetComponent<IUnit>();
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

    private void OnValidate()
    {
        _unitType = _unitPrefab != null ? _unitPrefab.GetComponent<IUnit>().UnitType : UnitType.UNDEFINED;
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
    public readonly Quaternion Rotation;
    public readonly UnitType UnitType;
    public readonly Army ParentArmy;
    public readonly IStrategyUpdater StrategyUpdater;

    public UnitCreationParameters(Vector3 position, Quaternion rotation, Army parentArmy, UnitType unitType, IStrategyUpdater strategyUpdater)
    {
        Position = position;
        Rotation = rotation;
        UnitType = unitType;
        ParentArmy = parentArmy;
        StrategyUpdater = strategyUpdater;
    }
}