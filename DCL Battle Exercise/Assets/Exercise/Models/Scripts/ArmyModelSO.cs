using DCLBattle.Battle;
using EditorUtils;
using UnityEngine;

/// <summary>
/// ScriptableObject containing the data of an army
/// for simplicity's sake the use-case of updating the SO manually has been discarded, and
/// therefore the usage of ReadOnlyAttribute
/// </summary>
[CreateAssetMenu(menuName = "DCLBattle/Army/Create Army Model", fileName = "ArmyModel", order = 0)]
public class ArmyModelSO : ScriptableObject, IArmyModel
{
    // TODO Could control this from UI too
    [SerializeField] private string _armyName = "Army";
    public string ArmyName
    {
        get => _armyName;
        set => _armyName = value;
    }

    // TODO Could control this from UI too
    [SerializeField] private Color _armyColor;
    public Color ArmyColor
    {
        get => _armyColor;
        set => _armyColor = value;
    }

    // TODO Could control this from UI too
    // TODO Pretty bad, I'd rather create another struct or class to represent alliances instead
    // Using the SO directly here for simplicity
    [SerializeField] private ArmyModelSO[] _enemyArmies;
    public IArmyModel[] EnemyArmies
    {
        get => _enemyArmies;
    }

    [ReadOnly, SerializeField] private ArmyStrategy _strategyValue = ArmyStrategy.Basic;
    public ArmyStrategy Strategy
    {
        get => _strategyValue;
        set => _strategyValue = value;
    }

    [ReadOnly, SerializeField, DrawEnumBasedArray(typeof(UnitType))] 
    private int[] _unitsCount = new int[IArmyModel.UnitLength];
    public int GetUnitCount(UnitType unitType)
    {
        return _unitsCount[(int)unitType];
    }

    public void SetUnitCount(UnitType unitType, int unitCount)
    {
        _unitsCount[(int)unitType] = unitCount;
    }


    // This one is the only one that needs to be set in editor
    [SerializeField, Interface(typeof(IUnitModel))]
    private Object[] _unitsModels = new Object[IArmyModel.UnitLength];

    public bool TryGetUnitModel(UnitType type, out IUnitModel unitModel)
    {
        foreach (var model in _unitsModels)
        {
            unitModel = model as IUnitModel;
            if (unitModel.UnitType == type)
                return true;
        }

        unitModel = null;
        return false;
    }

    public IUnitModel GetUnitModel(UnitType type)
    {
        foreach (var modelObject in _unitsModels)
        {
            IUnitModel unitModel = (modelObject as IUnitModel);
            if (unitModel.UnitType == type)
                return unitModel;
        }
        return null;
    }

    // This makes sure our Units Prefabs are always in the right order
    private void OnValidate()
    {
        if (_unitsCount.Length != IArmyModel.UnitLength)
        {
            // kind of lame way fo doing this since this may make us lose data, but since it's set at runtime by the player, doesn't matter much
            var unitCountCopy = new int[IArmyModel.UnitLength];
            _unitsCount.CopyTo(unitCountCopy, 0);
            _unitsCount = unitCountCopy;
        }

        // We could technically add a check to see if the assigned prefab is of the right unit type compared to the array index
        // But for this exercise, may be overkill
    }
}