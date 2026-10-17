namespace StellarAdmin.Resources;

/// <summary>
///     Lists, loads, creates, updates and deletes a resource.
/// </summary>
/// <typeparam name="TResource">The type of the resource.</typeparam>
public interface IResourceCrudDataSource<TResource>
    : IResourceDataSource<TResource>,
        IResourceCreateHandler<TResource>,
        IResourceEditHandler<TResource>,
        IResourceDeleteHandler<TResource> { }
