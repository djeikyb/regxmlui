using System;
using System.ComponentModel.Design;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Ul.Core;

public static class Defaults
{
    public static Ioc Locator = Ioc.Default;

    public static IServiceProvider ConfigureDefaultServices()
    {
        return new ServiceCollection().BuildServiceProvider();
    }
}
