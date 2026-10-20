using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Specifies the status of a checklist item.
/// </summary>
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
        public string GetDataAttributeText()
        {
            return status switch
            {
                ChecklistStatus.Todo => "todo",
                ChecklistStatus.InProgress => "in-progress",
                ChecklistStatus.Done => "done",
                _ => string.Empty,
            };
        }

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
