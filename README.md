# SRF.Industrial

Collection of libraries related to industrial/technical systems and related connectivity solutions.

These libraries are in an early stage, some more playground than actual use - breaking changes or discontinuations are to be expected.

## Libraries

| Name                | Description             | Development status     |
|---------------------|-------------------------|------------------------|
|SRF.Industrial.Cli |Command line tool serving as play-ground for the other libraries |Trials|
|[SRF.Industrial.Events](Events/Docs/README.md) |Manages event queues with BackgroundServices as processors. If you need a bit more than regular C# events. |Trials |
|SRF.Industrial.Events.Sample |Example application using the SRF.Industrial.Events library |Trials |
|SRF.Industrial.Modbus |Modbus TCP client library including a proxy server/client. The proxy aims to serve logging and interception needs between two other Modbus TCP devices. It aims to add multi-client capability for devices allowing only a single Modbus-TCP client by routing all communication via the proxy.| On hold|
|SRF.Industrial.Packets |Generic byte[] package Encoder/Decorder library. Foreseen for Modbus and other low level protocols. |Trials |
