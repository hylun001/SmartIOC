using SimpleContainer;

namespace ServiceInterface
{
    public interface INiceService : IDependencyService
    {
        string Prefix();
    }

}
