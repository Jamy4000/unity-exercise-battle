using UnityEngine;

namespace DCLBattle.Battle
{
    [RequireComponent (typeof(Camera))]
    public sealed class CameraController : MonoBehaviour
    {
        private BattleInstantiator _battleInstantiator;
        private Transform _cameraTranform;

        private void Start()
        {
            _cameraTranform = GetComponent<Camera>().transform;
            _battleInstantiator = UnityServiceLocator.ServiceLocator.ForSceneOf(this).Get<BattleInstantiator>();
        }

        void Update()
        {
            Vector3 forwardTarget = Vector3.Normalize(_battleInstantiator.BattleCenter - _cameraTranform.position);
            _cameraTranform.forward += (forwardTarget - _cameraTranform.forward) * 0.1f;
        }
    }
}