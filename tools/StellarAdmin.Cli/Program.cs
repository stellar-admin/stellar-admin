using Spectre.Console.Cli;
using StellarAdmin.Cli.Docs;

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
                                "Generate the UI component demos and the admin panel screenshots, deleting each one's existing output first."
                            );
                            generate.SetDefaultCommand<GenerateCommand>();
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
                            clean.SetDescription("Delete the generated demos and screenshots.");
                            clean.SetDefaultCommand<CleanCommand>();
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
        });

        return app.RunAsync(args);
    }
}
