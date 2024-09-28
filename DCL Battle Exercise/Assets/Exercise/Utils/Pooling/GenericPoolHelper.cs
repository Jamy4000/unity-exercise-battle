using UnityEngine.Pool;

namespace PRISMUtils
{
    public abstract class GenericPoolHelper<TPoolable> where TPoolable : class, IGenericPoolable
    {
        private readonly ObjectPool<TPoolable> _objectPool;

        public delegate void OnObjectPooledStatusChangedDelegate(TPoolable poolable);
        public event OnObjectPooledStatusChangedDelegate OnObjectWasPooledEvent;
        public event OnObjectPooledStatusChangedDelegate OnObjectWasReturnedEvent;

        protected GenericPoolHelper(int minPoolSize, int maxPoolSize, bool collectionChecks)
        {
            _objectPool = new ObjectPool<TPoolable>(CreatePooledItem, OnTakeFromPool,
                OnReturnedToPool, OnDestroyPoolObject, collectionChecks, minPoolSize, maxPoolSize);
        }

        public TPoolable RequestPoolableObject()
        {
            TPoolable poolable = _objectPool.Get();
            poolable.OnShouldReturnToPool += ReleasePoolable;
            OnObjectWasPooledEvent?.Invoke(poolable);
            return poolable;
        }

        public void AddPreplacedPoolable(TPoolable poolable)
        {
            poolable.OnShouldReturnToPool += ReleasePoolable;
        }

        protected void ReleasePoolable(IGenericPoolable poolableToRelease)
        {
            TPoolable releasedPoolable = poolableToRelease as TPoolable;
            releasedPoolable.OnShouldReturnToPool -= ReleasePoolable;
            OnObjectWasReturnedEvent?.Invoke(releasedPoolable);
            _objectPool.Release(releasedPoolable);
        }

        /// <summary>
        /// Called when the objects are being created inside the Pool
        /// </summary>
        protected abstract TPoolable CreatePooledItem();

        /// <summary>
        /// Called when an item is taken from the pool using Get
        /// </summary>
        protected virtual void OnTakeFromPool(TPoolable takenObject)
        {
            takenObject.Enable();
        }

        /// <summary>
        /// Called when an item is returned to the pool using Release
        /// </summary>
        /// <param name="system"></param>
        protected virtual void OnReturnedToPool(TPoolable releasedObject)
        {
            releasedObject.Disable();
        }

        /// <summary>
        /// If the pool capacity is reached then any items returned will be destroyed.
        /// We can control what the destroy behavior does, here we destroy the GameObject.
        /// </summary>
        protected virtual void OnDestroyPoolObject(TPoolable destroyedObject)
        {
            destroyedObject.Destroy();
        }
    }

    public interface IGenericPoolable
    {
        System.Action<IGenericPoolable> OnShouldReturnToPool { get; set; }

        void Enable();

        void Disable();

        void Destroy();
    }
}