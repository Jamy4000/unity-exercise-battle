using DCLBattle.Battle;
using UnityEngine;
using Utils;

[CreateAssetMenu(menuName = "DCLBattle/Units/Model/Archer", fileName = "ArcherModel", order = 0)]
public sealed class ArcherModelSO : UnitModelSO, ISubscriber<BattleStartEvent>, ISubscriber<AllianceWonEvent>
{
    [Header("Range Attack Parameters")]
    [SerializeField]
    private float _maxAttackCooldown = 2f;
    public float MaxAttackCooldown => _maxAttackCooldown;

    [SerializeField]
    private float _postAttackDelay = 0.5f;
    public float PostAttackDelay => _postAttackDelay;

    [Header("Arrow Pool Settings")]
    [SerializeField]
    private int _minArrowPoolSize = 64;

    [SerializeField]
    private int _maxArrowPoolSize = 256;

    [SerializeField, Interface(typeof(IProjectile))]
    private Object _arrowPrefab;

    public ArcherArrowPool ArrowPool { get; private set; }

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
        ArrowPool = new ArcherArrowPool(_arrowPrefab, _minArrowPoolSize, _maxArrowPoolSize);
    }

    public void OnEvent(AllianceWonEvent evt)
    {
        ArrowPool = null;
    }
}