using DCLBattle.LaunchMenu;
using UnityEngine;

/// <summary>
/// ScriptableObject containing the data of a Unit
/// </summary>
[CreateAssetMenu(menuName = "Create UnitModel", fileName = "UnitModel", order = 0)]
public class UnitModelSO : ScriptableObject, IUnitModel
{
    [SerializeField, ReadOnly]
    private UnitType _unitType;
    public UnitType UnitType => _unitType;
    public string UnitName => _unitType.ToString();

    // Only data set in editor
    [SerializeField]
    private GameObject _unitViewPrefab;
    public GameObject UnitViewPrefab => _unitViewPrefab;
}