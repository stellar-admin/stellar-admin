using Spectre.Console.Cli;
using StellarAdmin.Cli.Docs;
using StellarAdmin.Cli.Scenarios;

namespace StellarAdmin.Cli;

public static class Program
{
    public static Task<int> Main(string[] args)
    {
        var app = new CommandApp();
        app.Configure(config =>
        {
            config.SetApplicationName("stellar");
            config.UseStrictParsing();
            config.AddBranch(
                "docs",
                docs =>
                {
                    docs.SetDescription("Generate the website's demos and admin docs screenshots.");
                    docs.AddBranch<GenerateSettings>(
                        "generate",
                        generate =>
                        {
                            generate.SetDescription(
                                "Generate the UI component demos or the admin panel screenshots, deleting the existing output first."
                            );
                            generate
                                .AddCommand<GenerateAllCommand>("all")
                                .WithDescription("Generate the demos, then the screenshots.");
                            generate
                                .AddCommand<GenerateDemosCommand>("demos")
                                .WithDescription("Generate the UI component demos.");
                            generate
                                .AddCommand<GenerateScreenshotsCommand>("screenshots")
                                .WithDescription("Capture the admin panel screenshots.")
                                .WithExample(
                                    "docs",
                                    "generate",
                                    "screenshots",
                                    "--spec",
                                    "branding-logo-icon"
                                );
                        }
                    );
                    docs.AddCommand<CheckCommand>("check")
                        .WithDescription(
                            "List the docs' screenshot placeholders no spec captures, and the screenshots no page shows."
                        );
                    docs.AddBranch(
                        "clean",
                        clean =>
                        {
                            clean.SetDescription("Delete the generated demos or screenshots.");
                            clean
                                .AddCommand<CleanAllCommand>("all")
                                .WithDescription("Delete the generated demos and screenshots.");
                            clean
                                .AddCommand<CleanDemosCommand>("demos")
                                .WithDescription("Delete the generated demos.");
                            clean
                                .AddCommand<CleanScreenshotsCommand>("screenshots")
                                .WithDescription(
                                    "Delete the generated screenshots and their manifest."
                                );
                        }
                    );
                }
            );
            config.AddBranch(
                "scenarios",
                scenarios =>
                {
                    scenarios.SetDescription(
                        "Host the test and docs scenarios, and run specs against them."
                    );
                    scenarios
                        .AddCommand<ListCommand>("list")
                        .WithDescription("List every scenario.");
                    scenarios
                        .AddCommand<StartCommand>("start")
                        .WithDescription("Host a scenario until Ctrl+C.")
                        .WithExample(
                            "scenarios",
                            "start",
                            "default",
                            "--urls",
                            "http://127.0.0.1:5207"
                        );
                    scenarios
                        .AddCommand<RunCommand>("run")
                        .WithDescription("Run spec files, or every spec in folders.")
                        .WithExample(
                            "scenarios",
                            "run",
                            "tests/StellarAdmin.ScenarioTesting.Tests/Specs",
                            "--output",
                            "artifacts/screenshots"
                        );
                }
            );
        });

        return app.RunAsync(args);
    }
}
