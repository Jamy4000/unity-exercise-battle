using UnityEngine;

namespace DCLBattle.Battle
{
    [RequireComponent (typeof(Camera))]
    public sealed class CameraController : MonoBehaviour
    {
        private IArmiesHolder _armiesHolder;
        private Transform _cameraTranform;

        private void Start()
        {
            _cameraTranform = GetComponent<Camera>().transform;
            _armiesHolder = UnityServiceLocator.ServiceLocator.Global.Get<IArmiesHolder>();
        }

        void Update()
        {
            Vector3 forwardTarget = Vector3.Normalize(_armiesHolder.BattleCenter - _cameraTranform.position);
            _cameraTranform.forward += (forwardTarget - _cameraTranform.forward) * 0.1f;
        }
    }
}