using Service;
using ServiceInterface;
using SimpleContainer;
using System;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            SmartContainer.Instance.ScanAssemblies(typeof(INiceService).Assembly, typeof(TestService).Assembly);
            SmartContainer.Instance.Register(new TestOptions() { Id = 1, Name = "Today is a nice day!" });
            ITestService testService = SmartContainer.Instance.Resolve<ITestService>();
            string res = testService.SayHello();
            Console.WriteLine(res);

            Console.ReadLine();
        }
    }
}
