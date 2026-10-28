using System.ComponentModel;
using Spectre.Console.Cli;

namespace StellarAdmin.Cli.Docs;

// The docs generate commands' shared options
internal class GenerateSettings : CommandSettings
{
    [CommandOption("--keep-output")]
    [Description("Keep the existing output instead of deleting it first.")]
    public bool KeepOutput { get; init; }
}
