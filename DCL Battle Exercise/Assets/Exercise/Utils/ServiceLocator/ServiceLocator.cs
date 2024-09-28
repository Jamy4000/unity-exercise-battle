using System.Collections.Generic;

namespace Utils
{
    public sealed class ServiceLocator : IServiceLocator, System.IDisposable
    {
        private readonly Dictionary<System.Type, IService> _services = new(64);

        public void AddService<T>(T service) where T : IService
        {
            _services.Add(typeof(T), service);
        }

        public void RemoveService<T>() where T : IService
        {
            _services.Remove(typeof(T));
        }

        public bool TryGetService<T>(out T serviceImpl) where T : class, IService
        {
            if (_services.TryGetValue(typeof(T), out IService service))
            {
                serviceImpl = service as T;
                return true;
            }
            else
            {
                serviceImpl = null;
                return false;
            }
        }


        public T GetService<T>() where T : class, IService
        {
            return _services[typeof(T)] as T;
        }

        public void Dispose()
        {
            _services.Clear();
        }
    }
}