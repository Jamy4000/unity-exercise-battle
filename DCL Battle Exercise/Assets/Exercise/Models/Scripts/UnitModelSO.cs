using System;
using UnityEngine;

/// <summary>
/// ScriptableObject containing the data of a Unit
/// </summary>
[CreateAssetMenu(menuName = "Create UnitModel", fileName = "UnitModel", order = 0)]
public class UnitModelSO : ScriptableObject, IUnitModel
{
    private int _unitCount = 80;

    [SerializeField]
    private UnitType _unitType;

    [SerializeField]
    private UnitBase _unitPrefab;

    public Action<IUnitModel> OnUnitsChanged { get; set; }

    public int GetUnitsCount()
    {
        return _unitCount;
    }

    public void SetUnitsCount(int newUnitCount)
    {
        _unitCount = newUnitCount;
        OnUnitsChanged?.Invoke(this);
    }

    public string GetUnitsName()
    {
        return _unitType.ToString();
    }

    public UnitType GetUnitsType()
    {
        return _unitType;
    }

    public UnitBase GetUnitsPrefab()
    {
        return _unitPrefab;
    }
}