using System.Reflection;
using StellarAdmin.Dashboard.Resources;

namespace DashboardPlayground.Resources.FieldEditors;

// Serves one sample record and never stores a save. A save that passes validation succeeds, unless
// the form asks for every field to be rejected to show each editor's error state at once.
public sealed class FieldEditorGalleryDataSource<TRecord>
    : IResourceDataSource<TRecord>,
        IResourceCreateHandler<TRecord>,
        IResourceEditHandler<TRecord>
    where TRecord : FieldEditorGalleryRecord, IFieldEditorGalleryRecord<TRecord>
{
    public Task<ResourceOperationResult> CreateAsync(
        TRecord model,
        CancellationToken cancellationToken
    ) => Task.FromResult(Save(model));

    public Task<TRecord?> FindAsync(string id, CancellationToken cancellationToken) =>
        Task.FromResult(id == "1" ? CreateSample() : null);

    public Task<ResourceListResult<TRecord>> ListAsync(
        ResourceListRequest request,
        CancellationToken cancellationToken
    ) => Task.FromResult(new ResourceListResult<TRecord>([CreateSample()], 1));

    public Task<ResourceOperationResult> UpdateAsync(
        string id,
        TRecord model,
        CancellationToken cancellationToken
    ) => Task.FromResult(Save(model));

    private static TRecord CreateSample()
    {
        var sample = TRecord.CreateSample();
        sample.Id = 1;

        return sample;
    }

    private static ResourceOperationResult Save(TRecord model)
    {
        if (!model.RejectEveryField)
        {
            return ResourceOperationResult.Success();
        }

        return ResourceOperationResult.ValidationFailed(
            typeof(TRecord)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(property => property.DeclaringType != typeof(FieldEditorGalleryRecord))
                .Select(property => new ResourceValidationError(
                    property.Name,
                    $"Gallery error for {property.Name}."
                ))
                .Prepend(
                    new ResourceValidationError(
                        null,
                        "Reject every field is on, so the gallery rejected the save."
                    )
                )
        );
    }
}
