using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SRF.Industrial.Modbus.Test.Infra;

public static class TestHostBuilder
{
    public static HostApplicationBuilder Create()
    {
        var hb = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings() { });

        hb.Logging.AddConsole();

        return hb;
    }
}
