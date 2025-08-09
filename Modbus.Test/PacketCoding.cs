using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SRF.Industrial.Modbus.Packets;
using SRF.Industrial.Modbus.Test.Infra;
using SRF.Industrial.Packets;

namespace SRF.Industrial.Modbus.Test;

public class PacketCoding
{
    private IHost? Host { get; set; }

    private ModbusClient? MBClient { get; set; }

    [SetUp]
    public void Setup()
    {
        Host = Infra.TestHostBuilder.Create().Build();
        var mbOpt = new OptionsStub<ModbusClientConfig>()
        {
            Value = new Modbus.ModbusClientConfig()
            {
                Server = "localhost"
            }
        };
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

    private static readonly string MsgMBClientIsNull = "MBClient must not be null. Test aborted.";

    private static byte[] ReadRegistersCommandPacket => [
        //MBAP
        0x12, 0x34, // Transmission Identifier
        0, 1,       // Protocol Identifier
        0, 0,       // Protocol Type
        0, 6,       // Data Length
        1,          // Locigal device Id
        //Function code
        3,          // Function code
        //Read registers
        0x7e, 0x32, // Start address
        0, 2,       // Number of registers
    ];

    private static byte[] ReadRegistersResponsePacket => [
        // MBAP
        0x12, 0x34,
        0, 1,
        0, 0,
        0, 10,
        1,
        // Function Code
        3,
        // Register Values (Read Regs Response)
        4,          // no bytes in registers
        0x01, 0x02, // register 0
        0x03, 0x04  // register 1
    ];

    [Test]
    public void ReadRegistersCommandEncoding()
    {
        if (MBClient == null)
        {
            Assert.Fail(MsgMBClientIsNull);
            return;
        }

        var rrPkt = MBClient.PacketFactory.ReadRegisters(1, 0x7e32, 0x0002);
        var expectPkt = ReadRegistersCommandPacket;

        // check expected sizes
        Assert.That(rrPkt?.Measure(), Is.EqualTo(expectPkt.Length), "MBAP Measure returns length different than expected packet total length");
        var pktFunctionCode = rrPkt.Payload as ProtocolDataUnit;
        Assert.That(pktFunctionCode?.Measure(), Is.EqualTo(5), "Function code total length mismatch");
        var pktReadRegisters = pktFunctionCode?.Payload as ReadRegisters;
        Assert.That(pktReadRegisters?.Measure(), Is.EqualTo(4), "ReadRegisters total length mismatch");

        // encode and check results
        var buf = new SRF.Industrial.Packets.PacketBuffer(20, MBClient.EndianSwap);
        rrPkt.Encode(buf.Writer);

        Assert.That(buf.Writer.BaseStream.Position, Is.EqualTo(expectPkt.Length), "Encoded packet has different length than expected, by stream position after writing.");
        for (int i = 0; i < expectPkt.Length; i++)
        {
            Assert.That(buf.Buffer[i], Is.EqualTo(expectPkt[i]), $"Encoded byte mismatch at position {i}");
        }
    }

    [Test]
    public void ReadRegistersResponseDecoding()
    {
        if (MBClient == null)
        {
            Assert.Fail(MsgMBClientIsNull);
            return;
        }

        var reqBufLen = 200;
        var buf = new SRF.Industrial.Packets.PacketBuffer(reqBufLen, MBClient.EndianSwap);
        var bufLen = buf.Buffer.Length; // actual length might be more than requested.
        ReadRegistersResponsePacket.CopyTo(buf.Buffer.AsSpan());

        var mbap = new Modbus.Packets.ModbusApplicationProtocolHeader();

        Assert.Multiple(() =>
        {
            Assert.That(reqBufLen, Is.LessThanOrEqualTo(bufLen), "to small buffer allocated.");
            Assert.That(buf.Buffer, Has.Length.EqualTo(bufLen));
            Assert.That((buf.Reader.BaseStream as MemoryStream), Has.Length.EqualTo(bufLen));
        });

        Assert.DoesNotThrow(() => mbap.Decode(buf.Reader, MBClient.PayloadObjectProviders));

        // check associated payloads
        var pktFC = mbap.Payload as ProtocolDataUnit;
        Assert.That(pktFC, Is.Not.Null, "FunctionCode payload not associated to MBAP.");
        var pktReadRegisters = pktFC.Payload as RegisterValues;
        Assert.That(pktReadRegisters, Is.Not.Null, "RegisterValues payload not associated to FunctionCode.");
        Assert.That(pktReadRegisters.Payload, Is.Null);

        // check some values
        Assert.Multiple(() =>
        {
            Assert.That(pktReadRegisters.NoBytes, Is.EqualTo(4), "Wrong number of bytes returned");
            Assert.That(pktReadRegisters.Buffer, Is.EqualTo(new byte[] { 0x01, 0x02, 0x03, 0x04 }));
            Assert.That(pktReadRegisters.Buffer, Has.Length.EqualTo(4), "Wrong number of bytes read.");
        });
    }

    private static readonly byte[] ReadRegistersExceptionResponse = [
        // MBAP
        0x12, 0x34,
        0, 1,
        0, 0,
        0, 10,
        1,
        // Function Code
        3 | 0x80,
        // Exception
        4,          // Exception code
    ];

    [Test]
    public void ExceptionResponseDecoding()
    {
        if (MBClient == null)
        {
            Assert.Fail(MsgMBClientIsNull);
            return;
        }

        var buf = new PacketBuffer(100, MBClient.EndianSwap);
        ReadRegistersExceptionResponse.CopyTo(buf.Buffer.AsSpan());

        var mbap = new ModbusApplicationProtocolHeader();
        Assert.DoesNotThrow(() => mbap.Decode(buf.Reader, MBClient.PayloadObjectProviders));

        Assert.Multiple(() =>
        {
            Assert.That(mbap.Measure(), Is.EqualTo(ReadRegistersExceptionResponse.Length), "Wrong exception packet length measured.");
            Assert.That(mbap.FunctionCode, Is.EqualTo(0x83), "Wrong exception function code.");
            Assert.That((mbap.PDU?.Payload as ExceptionResponse)?.ExceptionCode, Is.EqualTo(4), "Wrong exception code.");
        });
    }
}
