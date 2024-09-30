using DCLBattle.Battle;
using UnityEngine;

[CreateAssetMenu(menuName = "DCLBattle/Units/Model/Archer", fileName = "ArcherModel", order = 0)]
public sealed class ArcherModelSO : UnitModelSO
{
    [Header("Range Attack Parameters")]
    [SerializeField]
    private float _maxAttackCooldown = 2f;
    public float MaxAttackCooldown => _maxAttackCooldown;

    [SerializeField]
    private float _postAttackDelay = 0.5f;
    public float PostAttackDelay => _postAttackDelay;

    [SerializeField, Interface(typeof(IProjectile))]
    private Object _arrowPrefab;
    public Object ArrowPrefab => _arrowPrefab;

    [Header("Arrow Pool Settings")]
    [SerializeField]
    private int _minArrowPoolSize = 64;
    public int MinArrowPoolSize => _minArrowPoolSize;

    [SerializeField]
    private int _maxArrowPoolSize = 256;
    public int MaxArrowPoolSize => _maxArrowPoolSize;
}