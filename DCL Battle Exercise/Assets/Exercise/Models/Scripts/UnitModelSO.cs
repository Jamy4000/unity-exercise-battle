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
    public string UnitName => _unitType.ToString();

    [SerializeField, Interface(typeof(IUnitFactory))]
    private Object _unitFactory;
    public IUnitFactory UnitFactory => _unitFactory as IUnitFactory;
}