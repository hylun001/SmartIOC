using ServiceInterface;
using SimpleContainer;

namespace Service
{

    public class TestService : ITestService
    {
        private TestOptions _testOptions;

        [PropertyInject]
        public INiceService NiceService { get; set; }

        public TestService(TestOptions testOptions)
        {
            this._testOptions = testOptions;
        }

        public string SayHello()
        {
            return $"【{NiceService.Prefix()}】 - {_testOptions.Name}";
        }
    }
}
