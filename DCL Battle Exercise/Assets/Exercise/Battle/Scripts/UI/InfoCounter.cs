using DCLBattle.Battle;
using UnityEngine;
using Utils;

namespace DCLBattle
{
    /// <summary>
    /// This is a debug class that creates a bunch of Garbage, but as it's just a utility I won't fix it
    /// </summary>
    public sealed class InfoCounter : MonoBehaviour, I_Update1Hz, IServiceConsumer
    {
        [SerializeField] private TMPro.TextMeshProUGUI _currentFpsCounter;
        [SerializeField] private TMPro.TextMeshProUGUI _averageFpsCounter;
        [SerializeField] private TMPro.TextMeshProUGUI _unitCounter;
        [SerializeField, Interface(typeof(IServiceLocator))]
        private Object _serviceLocatorObject;

        private IArmiesHolder _armiesHolder;

        private int _averageframeCount;
        private float _averageFPS;

        private void Awake()
        {
            var serviceLocator = _serviceLocatorObject as IServiceLocator;
            serviceLocator.AddConsumer(this);
        }


        private void Start()
        {
            GameUpdater.Register(this);
        }

        private void Update()
        {
            float currentFPS = 1f / Time.unscaledDeltaTime;
            _currentFpsCounter.text = $"Current FPS: {currentFPS:F2}";

            _averageframeCount++;
            _averageFPS += (currentFPS - _averageFPS) / _averageframeCount;

            int unitCount = 0;
            for (int armyIndex = 0; armyIndex < _armiesHolder.ArmiesCount; armyIndex++)
            {
                unitCount += _armiesHolder.GetArmy(armyIndex).RemainingUnitsCount;
            }
            _unitCounter.text = $"Unit Count: {unitCount}";
        }

        private void OnDestroy()
        {
            GameUpdater.Unregister(this);
        }

        public void ManualUpdate1Hz()
        {
            _averageFpsCounter.text = $"Average FPS: {_averageFPS:F2}";

        }

        public void ConsumeLocator(IServiceLocator locator)
        {
            _armiesHolder = locator.GetService<IArmiesHolder>();
        }
    }

}