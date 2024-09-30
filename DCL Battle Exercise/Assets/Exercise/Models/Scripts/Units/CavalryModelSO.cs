using DCLBattle.Battle;
using UnityEngine;

[CreateAssetMenu(menuName = "DCLBattle/Units/Model/Cavalry", fileName = "CavalryModel", order = 0)]
public sealed class CavalryModelSO : UnitModelSO
{
    [SerializeField]
    private float _damage = 10f;
    public float Damage { get; private set; }
}