using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SRF.Industrial.Modbus.Test.Infra;

namespace SRF.Industrial.Modbus.Test;

public class Encoding
{
    private IHost? Host { get; set; }

    private ModbusClient? MBClient { get; set; }

    [SetUp]
    public void Setup()
    {
        Host = Infra.TestHostBuilder.Create().Build();
        var mbOpt = new OptionsStub<ModbusClientConfig>() { Value = new Modbus.ModbusClientConfig()
        {
            Server = "localhost"
        } };
        MBClient = new Modbus.ModbusClient(
            mbOpt,
            new ModbusPacketFactory(),
            Host.Services.GetRequiredService<ILogger<ModbusClient>>()
        );
    }

    [TearDown]
    public void TearDown()
    {
        Host?.Dispose();
        MBClient?.Dispose();
    }

    [Test]
    public void SetupValidation()
    {
        Assert.Multiple(() =>
        {
            Assert.That(MBClient, Is.Not.Null);
            Assert.That(Host, Is.Not.Null);
        });
    }

    [Test]
    public void Test1()
    {
        Assert.Fail();
    }
}
