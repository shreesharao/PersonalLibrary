using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;
using System.IO;

namespace SSR.PL.Web
{
    class Program
    {
        static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        private static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder().UseStartup<Startup>().Build();
        }
    }
}
