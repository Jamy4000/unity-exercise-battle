using DCLBattle.Battle;
using UnityEngine;
using Utils;

namespace DCLBattle
{
    public sealed class InfoCounter : MonoBehaviour, I_Update1Hz
    {
        [SerializeField] private TMPro.TextMeshProUGUI _currentFpsCounter;
        [SerializeField] private TMPro.TextMeshProUGUI _averageFpsCounter;
        [SerializeField] private TMPro.TextMeshProUGUI _unitCounter;

        private IArmiesHolder _armiesHolder;

        private void Start()
        {
            GameUpdater.Register(this);
            _armiesHolder = UnityServiceLocator.ServiceLocator.Global.Get<IArmiesHolder>();
        }

        private void Update()
        {
            float currentFPS = 1f / Time.unscaledDeltaTime;
            _currentFpsCounter.text = $"Current FPS: {currentFPS:F2}";

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
            float averageFPS = Time.frameCount / Time.time;
            _averageFpsCounter.text = $"Average FPS: {averageFPS:F2}";

        }
    }

}