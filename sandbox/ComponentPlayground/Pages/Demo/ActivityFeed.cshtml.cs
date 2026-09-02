using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComponentPlayground.Pages.Demo;

/// <summary>The kind of row a job log entry renders as.</summary>
public enum JobLogEntryKind
{
    Log,
    Milestone,
    Warning,
    Done,
}

/// <summary>A single line of the simulated job run.</summary>
public record JobLogEntry(JobLogEntryKind Kind, string Text, string? Icon = null);

/// <summary>One streamed chunk: the entry to render, plus the cursor the next request polls for.</summary>
public record JobLogChunk(JobLogEntry Entry, int NextIndex, bool HasMore);

public class ActivityFeed : PageModel
{
    private static readonly JobLogEntry[] Entries =
    [
        new(JobLogEntryKind.Milestone, "Supplier sync #2471 started", "play"),
        new(JobLogEntryKind.Log, "12:04:01  INFO   Connecting to fares API (europe-west1)"),
        new(JobLogEntryKind.Log, "12:04:02  INFO   Authenticated as svc-voyager-import"),
        new(JobLogEntryKind.Log, "12:04:04  INFO   Fetching fare batch 1 of 6"),
        new(JobLogEntryKind.Log, "12:04:09  INFO   Fetching fare batch 2 of 6"),
        new(JobLogEntryKind.Log, "12:04:13  INFO   Fetching fare batch 3 of 6"),
        new(JobLogEntryKind.Milestone, "1,284 fares downloaded", "cloud-upload"),
        new(JobLogEntryKind.Log, "12:04:15  INFO   Validating fare basis codes"),
        new(JobLogEntryKind.Warning, "3 fares rejected - missing fare basis code", "triangle-alert"),
        new(JobLogEntryKind.Log, "12:04:16  WARN   NH212/LHR-HND rejected"),
        new(JobLogEntryKind.Log, "12:04:16  WARN   BA005/LHR-KIX rejected"),
        new(JobLogEntryKind.Log, "12:04:16  WARN   JL044/LHR-NRT rejected"),
        new(JobLogEntryKind.Log, "12:04:22  INFO   Writing 1,281 fares to the catalogue"),
        new(JobLogEntryKind.Log, "12:04:31  INFO   Rebuilding the Kyoto campaign index"),
        new(JobLogEntryKind.Done, "Supplier sync #2471 finished in 34s", "circle-check"),
    ];

    public void OnGet() { }

    public IActionResult OnGetJobLog(int index)
    {
        if (index < 0 || index >= Entries.Length)
        {
            return Content(string.Empty, "text/html");
        }

        return Partial(
            "_JobLogChunk",
            new JobLogChunk(Entries[index], index + 1, index + 1 < Entries.Length)
        );
    }
}
