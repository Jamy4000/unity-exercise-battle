using DCLBattle.LaunchMenu;
using EditorUtils;
using UnityEngine;


/// <summary>
/// ScriptableObject containing the data of an army
/// for simplicity's sake the use-case of updating the SO manually has been discarded, and
/// therefore the usage of ReadOnlyAttribute
/// </summary>
[CreateAssetMenu(menuName = "Create ArmyModel", fileName = "ArmyModel", order = 0)]
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
    [SerializeField, DrawEnumBasedArray(typeof(UnitType))]
    private Object[] _unitsModels = new Object[IArmyModel.UnitLength];
    public IUnitModel GetUnitModel(UnitType type)
    {
        return _unitsModels[(int)type] as IUnitModel;
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

        if (_unitsModels.Length != IArmyModel.UnitLength)
        {
            var unitPrefabsCopy = new Object[IArmyModel.UnitLength];
            _unitsModels.CopyTo(unitPrefabsCopy, 0);
            _unitsModels = unitPrefabsCopy;
        }

        // We could technically add a check to see if the assigned prefab is of the right unit type compared to the array index
        // But for this exercise, may be overkill
    }
}