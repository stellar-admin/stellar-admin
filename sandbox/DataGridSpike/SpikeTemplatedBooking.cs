using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace DataGridSpike;

public record SpikeTemplatedBooking(
    string Reference,
    [property: UIHint("SpikeLevel")] SpikeLevel Level,
    [property: UIHint("SpikeDate")] DateOnly? Departure,
    decimal Total,
    [property: DisplayFormat(DataFormatString = "{0:0.0} pts")] decimal Rating
);

public enum SpikeLevel
{
    Low,
    High,
}

public abstract class SpikeChannelBase
{
    public bool Enabled { get; set; }
}

[ModelMetadataType<SpikeChannelMeta>]
public class SpikeChannel : SpikeChannelBase
{
    public required string Name { get; set; }
}

public class SpikeChannelMeta
{
    [UIHint("SpikeFlag")]
    public bool Enabled { get; set; }
}
