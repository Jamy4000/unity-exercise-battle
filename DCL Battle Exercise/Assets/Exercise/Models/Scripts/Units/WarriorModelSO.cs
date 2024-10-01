using UnityEngine;

[CreateAssetMenu(menuName = "DCLBattle/Units/Model/Warrior", fileName = "WarriorModel", order = 0)]
public sealed class WarriorModelSO : UnitModelSO
{
    [SerializeField]
    private float _damage = 10f;
    public float Damage => _damage;
}