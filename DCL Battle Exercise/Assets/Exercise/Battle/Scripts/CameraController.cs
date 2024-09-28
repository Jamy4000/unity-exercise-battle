using UnityEngine;

namespace DCLBattle.Battle
{
    [RequireComponent (typeof(Camera))]
    public sealed class CameraController : MonoBehaviour
    {
        private Transform _cameraTranform;

        private void Start()
        {
            _cameraTranform = GetComponent<Camera>().transform;
        }

        void Update()
        {
            Vector3 mainCenter = Vector3.zero;
            int armyCount = BattleInstantiator.Instance.GetArmiesCount();
            for (int armyIndex = 0; armyIndex < armyCount; armyIndex++)
            {
                mainCenter += BattleInstantiator.Instance.GetArmy(armyIndex).CalculateCenterPoint();
            }

            mainCenter /= armyCount;

            Vector3 forwardTarget = Vector3.Normalize(mainCenter - _cameraTranform.position);
            _cameraTranform.forward += (forwardTarget - _cameraTranform.forward) * 0.1f;
        }
    }
}