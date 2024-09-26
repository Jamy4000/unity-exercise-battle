using System.Collections;
using System.Collections.Generic;
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
    public int GetUnitsCount(UnitType unitType)
    {
        return unitsCount[(int)unitType];
    }

    public void SetUnitsCount(UnitType unitType, int unitCount)
    {
        unitsCount[(int)unitType] = unitCount;
    }


    // This one is the only one that needs to be set in editor
    [SerializeField] private UnitBase[] unitsPrefabs = new UnitBase[_unitLength];
    public UnitBase GetUnitPrefab(UnitType type)
    {
        return unitsPrefabs[(int)type];
    }

    // This makes sure our Units Prefabs are always in the right order
    private void OnValidate()
    {
        var unitsPrefabComparer = new UnitPrefabComparer();
        System.Array.Sort(unitsPrefabs, unitsPrefabComparer);
    }

    private sealed class UnitPrefabComparer : IComparer<UnitBase>
    {
        public int Compare(UnitBase x, UnitBase y)
        {
            if (x == null)
                return 1;
            else if (y == null)
                return -1;
            else
                return x.UnitType.CompareTo(y.UnitType);
        }
    }
}