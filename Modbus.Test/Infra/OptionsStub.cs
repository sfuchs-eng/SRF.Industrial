using System;
using Microsoft.Extensions.Options;

namespace SRF.Industrial.Modbus.Test.Infra;

public class OptionsStub<TConfig> : IOptions<TConfig> where TConfig : class
{
    public required TConfig Value { get; set; }
}
