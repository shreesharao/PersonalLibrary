using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;

namespace SSR.PL.Web
{
    static class Program
    {
        static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        private static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args).UseStartup<Startup>().Build();
        }
    }
}
