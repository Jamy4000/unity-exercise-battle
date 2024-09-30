using DCLBattle.Battle;
using UnityEngine;

[CreateAssetMenu(menuName = "DCLBattle/Units/Model/Magician", fileName = "MagicianModel", order = 0)]
public sealed class MagicianModelSO : UnitModelSO
{
    [SerializeField]
    private float _maxAttackCooldown = 2f;
    public float MaxAttackCooldown => _maxAttackCooldown;

    [SerializeField]
    private float _postAttackDelay = 0.5f;
    public float PostAttackDelay => _postAttackDelay;

    [SerializeField, Interface(typeof(IProjectile))]
    private GameObject _fireBallPrefab;
    public GameObject FireBallPrefab => _fireBallPrefab;

    [Header("Fireball Pool Settings")]
    [SerializeField]
    private int _minArrowPoolSize = 64;
    public int MinFireballPoolSize => _minArrowPoolSize;

    [SerializeField]
    private int _maxArrowPoolSize = 256;
    public int MaxFireballPoolSize => _maxArrowPoolSize;
}