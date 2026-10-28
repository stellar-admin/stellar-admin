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
            config.AddBranch(
                "docs",
                docs =>
                {
                    docs.SetDescription("Generate the website's demos and admin docs screenshots.");
                    docs.AddCommand<GenerateCommand>("generate")
                        .WithDescription(
                            "Generate the UI component demos, the admin panel screenshots or both, deleting each one's existing output first."
                        )
                        .WithExample("docs", "generate")
                        .WithExample("docs", "generate", "demos")
                        .WithExample(
                            "docs",
                            "generate",
                            "screenshots",
                            "--spec",
                            "branding-logo-icon"
                        );
                    docs.AddCommand<CheckCommand>("check")
                        .WithDescription(
                            "List the docs' screenshot placeholders no spec captures, and the screenshots no page shows."
                        );
                    docs.AddCommand<CleanCommand>("clean")
                        .WithDescription("Delete the generated demos, screenshots or both.");
                }
            );
        });

        return app.RunAsync(args);
    }
}
