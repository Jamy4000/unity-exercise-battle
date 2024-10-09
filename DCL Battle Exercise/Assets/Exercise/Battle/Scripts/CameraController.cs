using UnityEngine;
using Utils;

namespace DCLBattle.Battle
{
    [RequireComponent (typeof(Camera))]
    public sealed class CameraController : MonoBehaviour, IServiceConsumer
    {
        private IArmiesHolder _armiesHolder;
        private Transform _cameraTranform;

        [SerializeField, Interface(typeof(IServiceLocator))]
        private Object _serviceLocatorObject;

        private void Awake()
        {
            var serviceLocator = _serviceLocatorObject as IServiceLocator;
            serviceLocator.AddConsumer(this);
        }

        private void Start()
        {
            _cameraTranform = GetComponent<Camera>().transform;
        }

        void Update()
        {
            Vector3 forwardTarget = Vector3.Normalize(_armiesHolder.BattleCenter - _cameraTranform.position);
            _cameraTranform.forward += (forwardTarget - _cameraTranform.forward) * 0.1f;
        }

        public void ConsumeLocator(IServiceLocator locator)
        {
            _armiesHolder = locator.GetService<IArmiesHolder>();
        }
    }
}