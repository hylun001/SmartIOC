using SimpleContainer;

namespace ServiceInterface
{
    public interface ITestService : IDependencyService
    {
        string SayHello();
    }
}
