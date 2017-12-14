﻿using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace RaidBattlesBot
{
  public class Program
  {
    public static void Main(string[] args) =>
      BuildWebHost(args).Run();

    public static IWebHost BuildWebHost(string[] args) =>
      WebHost
        .CreateDefaultBuilder(args)
        .UseApplicationInsights()
        .UseStartup<Startup>()
        .ConfigureServices(services => services.AddAutofac())
        .Build();
  }
}
