using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using SRF.Industrial.Modbus.Server;

namespace SRF.Industrial.Modbus.Extensions;

public static class HostingHelper
{
    public static IHostApplicationBuilder AddModbusProxy(this IHostApplicationBuilder app, string configSection)
    {
        app.Services.AddOptions<ModbusProxyConfig>().BindConfiguration("ModbusProxy");
        app.Services.AddSingleton<IModbusConnectionFactory, ModbusTcpProxyConnectionFactory>();
        app.Services.AddSingleton<IModbusProxy, ModbusProxy>();

        return app;
    }
}
