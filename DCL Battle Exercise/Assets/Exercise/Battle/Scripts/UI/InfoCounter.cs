using DCLBattle.Battle;
using UnityEngine;
using Utils;

namespace DCLBattle
{
    /// <summary>
    /// This is a debug class that creates a bunch of Garbage, but as it's just a utility I won't fix it
    /// </summary>
    public sealed class InfoCounter : MonoBehaviour, I_Update1Hz
    {
        [SerializeField] private TMPro.TextMeshProUGUI _currentFpsCounter;
        [SerializeField] private TMPro.TextMeshProUGUI _averageFpsCounter;
        [SerializeField] private TMPro.TextMeshProUGUI _unitCounter;

        private IArmiesHolder _armiesHolder;

        private int _averageframeCount;
        private float _averageFPS;

        private void Start()
        {
            GameUpdater.Register(this);
            _armiesHolder = UnityServiceLocator.ServiceLocator.Global.Get<IArmiesHolder>();
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
    }

}