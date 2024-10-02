using DCLBattle.Battle;
using UnityEngine;
using Utils;

[CreateAssetMenu(menuName = "DCLBattle/Units/Model/Magician", fileName = "MagicianModel", order = 0)]
public sealed class MagicianModelSO : UnitModelSO, ISubscriber<BattleStartEvent>, ISubscriber<AllianceWonEvent>
{
    [SerializeField]
    private float _maxAttackCooldown = 2f;
    public float MaxAttackCooldown => _maxAttackCooldown;

    [SerializeField]
    private float _postAttackDelay = 0.5f;
    public float PostAttackDelay => _postAttackDelay;

    [Header("Fireball Pool Settings")]
    [SerializeField]
    private int _minFireballPoolSize = 64;

    [SerializeField]
    private int _maxFireballPoolSize = 256;

    [SerializeField, Interface(typeof(IProjectile))]
    private Object _fireBallPrefab;

    // using arrow pool for simplicity
    public ArcherArrowPool FireballPool { get; private set; }

    private void OnEnable()
    {
        MessagingSystem<BattleStartEvent>.Subscribe(this);
        MessagingSystem<AllianceWonEvent>.Subscribe(this);
    }

    private void OnDisable()
    {
        MessagingSystem<BattleStartEvent>.Unsubscribe(this);
        MessagingSystem<AllianceWonEvent>.Unsubscribe(this);
    }

    public void OnEvent(BattleStartEvent evt)
    {
        FireballPool = new ArcherArrowPool(_fireBallPrefab, _minFireballPoolSize, _maxFireballPoolSize);
    }

    public void OnEvent(AllianceWonEvent evt)
    {
        FireballPool = null;
    }
}