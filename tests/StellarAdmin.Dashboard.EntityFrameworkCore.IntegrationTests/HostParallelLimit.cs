using TUnit.Core.Interfaces;

[assembly: ParallelLimiter<StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests.HostParallelLimit>]

namespace StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests;

public sealed class HostParallelLimit : IParallelLimit
{
    public int Limit => 4;
}
