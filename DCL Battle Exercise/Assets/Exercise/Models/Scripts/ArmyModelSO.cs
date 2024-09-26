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

    [ReadOnlyAttribute, SerializeField] private ArmyStrategy strategyValue = ArmyStrategy.Basic;
    public ArmyStrategy Strategy
    {
        get => strategyValue;
        set => strategyValue = value;
    }


    [ReadOnlyAttribute, SerializeField] private int[] unitsCount = new int[_unitLength];
    public int[] UnitsCount => unitsCount;


    // This one is the only one that needs to be set in editor
    [SerializeField] private UnitBase[] unitsPrefabs = new UnitBase[_unitLength];
    public UnitBase GetUnitPrefab(UnitType type)
    {
        // We cannot be sure the prefabs will be ordered correctly in editor, so doing a for loop instead
        foreach (var prefab in unitsPrefabs)
        {
            if (prefab.UnitType == type)
                return prefab;
        }

        return null;
    }
}