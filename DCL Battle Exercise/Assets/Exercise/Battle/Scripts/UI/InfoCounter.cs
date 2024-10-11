using DCLBattle.Battle;
using UnityEngine;
using Utils;

namespace DCLBattle
{
    /// <summary>
    /// This is a debug class that creates a bunch of Garbage, but as it's just a utility I won't fix it
    /// </summary>
    public sealed class InfoCounter : MonoBehaviour, IServiceConsumer, 
        ISubscriber<BattleStartEvent>, ISubscriber<AllianceWonEvent>
    {
        [SerializeField] private TMPro.TextMeshProUGUI _currentFpsCounter;
        [SerializeField] private TMPro.TextMeshProUGUI _averageFpsCounter;
        [SerializeField] private TMPro.TextMeshProUGUI _unitCounter;
        [SerializeField, Interface(typeof(IServiceLocator))]
        private Object _serviceLocatorObject;

        private IArmiesHolder _armiesHolder;

        private int _averageframeCount;
        private float _averageFPS;
        private float _averageFPSDisplayTimer = 0f;
        private float _realtimeStart;
        private readonly System.Collections.Generic.List<float> _averageFPSs = new(1064);

        private void Awake()
        {
            var serviceLocator = _serviceLocatorObject as IServiceLocator;
            serviceLocator.AddConsumer(this);
            MessagingSystem<BattleStartEvent>.Subscribe(this);
            MessagingSystem<AllianceWonEvent>.Subscribe(this);
        }

        private void Update()
        {
            float currentFPS = 1f / Time.unscaledDeltaTime;
            _currentFpsCounter.text = $"Current FPS: {currentFPS:F2}";

            _averageframeCount++;
            _averageFPS += (currentFPS - _averageFPS) / _averageframeCount;

            _averageFPSDisplayTimer -= Time.unscaledDeltaTime;
            if (_averageFPSDisplayTimer <= 0f)
            {
                _averageFpsCounter.text = $"Average FPS: {_averageFPS:F2}";
                _averageFPSs.Add(_averageFPS);
                _averageFPS = 1f / Time.unscaledDeltaTime;
                _averageFPSDisplayTimer = 1f;
            }

            int unitCount = 0;
            for (int armyIndex = 0; armyIndex < _armiesHolder.ArmiesCount; armyIndex++)
            {
                unitCount += _armiesHolder.GetArmy(armyIndex).RemainingUnitsCount;
            }
            _unitCounter.text = $"Unit Count: {unitCount}";
        }

        private void OnDestroy()
        {
            MessagingSystem<AllianceWonEvent>.Unsubscribe(this);
            MessagingSystem<BattleStartEvent>.Unsubscribe(this);
        }


        public void ConsumeLocator(IServiceLocator locator)
        {
            _armiesHolder = locator.GetService<IArmiesHolder>();
        }

        public void OnEvent(BattleStartEvent evt)
        {
            int unitCount = 0;
            for (int armyIndex = 0; armyIndex < _armiesHolder.ArmiesCount; armyIndex++)
            {
                unitCount += _armiesHolder.GetArmy(armyIndex).RemainingUnitsCount;
            }
            Debug.Log($"Battle Started with {unitCount} units.");
            _realtimeStart = Time.realtimeSinceStartup;
            MessagingSystem<BattleStartEvent>.Unsubscribe(this);
        }

        public void OnEvent(AllianceWonEvent evt)
        {
            _realtimeStart = Time.realtimeSinceStartup - _realtimeStart;
            Debug.Log($"Battle lasted for {_realtimeStart:F2} seconds.");

            float finalAverageFPS = 0f;
            foreach (var fps in _averageFPSs)
            {
                finalAverageFPS += fps;
            }
            finalAverageFPS /= _averageFPSs.Count;
            Debug.Log($"Battle has an average FPS of {finalAverageFPS:F2}.");
            MessagingSystem<AllianceWonEvent>.Unsubscribe(this);
        }
    }

}