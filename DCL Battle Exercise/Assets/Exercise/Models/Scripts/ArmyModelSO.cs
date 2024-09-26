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
    private static readonly int _unitLength = System.Enum.GetValues(typeof(UnitType)).Length;

    // TODO Could control this from UI too
    [SerializeField] private string armyName = "Army";
    public string ArmyName
    { 
        get => armyName; 
        set => armyName = value; 
    }

    // TODO Could control this from UI too
    [SerializeField] private Color armyColor;
    public Color ArmyColor 
    { 
        get => armyColor; 
        set => armyColor = value; 
    }

    [ReadOnly, SerializeField] private ArmyStrategy strategyValue = ArmyStrategy.Basic;
    public ArmyStrategy Strategy
    {
        get => strategyValue;
        set => strategyValue = value;
    }

    [ReadOnly, SerializeField, DrawEnumBasedArray(typeof(UnitType))] 
    private int[] unitsCount = new int[_unitLength];
    public int GetUnitsCount(UnitType unitType)
    {
        return unitsCount[(int)unitType];
    }

    public void SetUnitsCount(UnitType unitType, int unitCount)
    {
        unitsCount[(int)unitType] = unitCount;
    }


    // This one is the only one that needs to be set in editor
    [SerializeField, DrawEnumBasedArray(typeof(UnitType))]
    private Object[] unitsModels = new Object[_unitLength];
    public IUnitModel GetUnitModel(UnitType type)
    {
        return unitsModels[(int)type] as IUnitModel;
    }

    // This makes sure our Units Prefabs are always in the right order
    private void OnValidate()
    {
        if (unitsCount.Length != _unitLength)
        {
            // kind of lame way fo doing this since this may make us lose data, but since it's set at runtime by the player, doesn't matter much
            var unitCountCopy = new int[_unitLength];
            unitsCount.CopyTo(unitCountCopy, 0);
            unitsCount = unitCountCopy;
        }

        if (unitsModels.Length != _unitLength)
        {
            var unitPrefabsCopy = new Object[_unitLength];
            unitsModels.CopyTo(unitPrefabsCopy, 0);
            unitsModels = unitPrefabsCopy;
        }

        // We could technically add a check to see if the assigned prefab is of the right unit type compared to the array index
        // But for this exercise, may be overkill
    }
}