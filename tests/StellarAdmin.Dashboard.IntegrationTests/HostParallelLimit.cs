using TUnit.Core.Interfaces;

[assembly: ParallelLimiter<StellarAdmin.Dashboard.IntegrationTests.HostParallelLimit>]

namespace StellarAdmin.Dashboard.IntegrationTests;

public sealed class HostParallelLimit : IParallelLimit
{
    public int Limit => 4;
}
