using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Specifies the status of a checklist item.
/// </summary>
[GenerateDataAttributeText]
public enum ChecklistStatus
{
    /// <summary>
    ///     The item has not been started.
    /// </summary>
    Todo,

    /// <summary>
    ///     The item has been started but is not finished.
    /// </summary>
    InProgress,

    /// <summary>
    ///     The item is finished.
    /// </summary>
    Done,
}

internal static class ChecklistStatusExtensions
{
    extension(ChecklistStatus status)
    {
        public SemanticIconRole GetIconRole()
        {
            return status switch
            {
                ChecklistStatus.InProgress => SemanticIconRole.ChecklistInProgress,
                ChecklistStatus.Done => SemanticIconRole.ChecklistDone,
                _ => SemanticIconRole.ChecklistTodo,
            };
        }

        public string GetLabel()
        {
            return status switch
            {
                ChecklistStatus.InProgress => "In progress",
                ChecklistStatus.Done => "Done",
                _ => "To do",
            };
        }
    }
}
