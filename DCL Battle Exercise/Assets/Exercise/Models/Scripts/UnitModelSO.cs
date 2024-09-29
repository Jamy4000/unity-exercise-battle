using DCLBattle.Battle;
using UnityEngine;

/// <summary>
/// ScriptableObject containing the data of a Unit
/// </summary>
[CreateAssetMenu(menuName = "DCLBattle/Units/Create Unit Model", fileName = "UnitModel", order = 0)]
public class UnitModelSO : ScriptableObject, IUnitModel
{
    [SerializeField]
    private UnitType _unitType;
    public UnitType UnitType => _unitType;
    public string UnitName => UnitType.ToString();

    [SerializeField, Interface(typeof(IUnitFactory))]
    private Object _unitFactory;
    public IUnitFactory UnitFactory => _unitFactory as IUnitFactory;

    [SerializeField]
    private StrategySO[] _strategyCreators;

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
}

public abstract class StrategySO : ScriptableObject
{
    public abstract ArmyStrategy ArmyStrategy { get; }
    public abstract IStrategyUpdater CreateStrategyUpdater();
}