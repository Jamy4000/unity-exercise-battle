namespace Utils
{
    public interface IServiceLocator
    {
        void AddService<T>(T service) where T : IService;
        void RemoveService<T>() where T : IService;
        bool TryGetService<T>(out T service) where T : class, IService;
        T GetService<T>() where T : class, IService;
    }

    public interface IService
    {
        void Initialize(IServiceLocator serviceLocator);
    }

    public interface IServiceConsumer
    {
        void ConsumeLocator(IServiceLocator locator);
    }
}
