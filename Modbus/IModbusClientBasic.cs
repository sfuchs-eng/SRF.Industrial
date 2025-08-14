using System;
using SRF.Industrial.Modbus.Packets;
using SRF.Industrial.Packets;

namespace SRF.Industrial.Modbus;

public interface IModbusClientBasic : IDisposable
{
    bool IsConnected { get; }

    Task ConnectAsync(CancellationToken cancel);

    /// <summary>
    /// Transmits a modbus packet to a connected master and returns the awaited response.
    /// The <see cref="IModbusClient"/> implementation needs to ensure transmitted and received packet correspond to each other in a thread safe manner.
    /// </summary>
    /// <param name="tx">Modbus function command, ready to <see cref="IPacket.Encode"/></param>
    /// <param name="cancel"></param>
    /// <returns>Decoded modbus function response to <paramref name="tx"/></returns>
    /// <exception cref="ModbusException">Encoding, transmission, reception, decoding exceptions. The modbus function exception response is returned as value without throwing an <see cref="Exception"/></exception>
    Task<ModbusApplicationProtocolHeader> TransceivePacketAsync(ModbusApplicationProtocolHeader tx, CancellationToken cancel);

    Task<TPacket> TransceivePacketAsync<TPacket>(IPacket tx, CancellationToken cancel) where TPacket : IPredictableLengthPacket;
}
