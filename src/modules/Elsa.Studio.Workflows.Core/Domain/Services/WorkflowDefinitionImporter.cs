using Elsa.Api.Client.Resources.WorkflowDefinitions.Contracts;
using Elsa.Api.Client.Resources.WorkflowDefinitions.Models;
using Elsa.Studio.Contracts;
using Elsa.Studio.Workflows.Domain.Contracts;
using Elsa.Studio.Workflows.Domain.Notifications;
using Refit;

namespace Elsa.Studio.Workflows.Domain.Services;

/// <summary>
/// A workflow definition service that uses a remote backend to retrieve workflow definitions.
/// </summary>
public class WorkflowDefinitionImporter(IRemoteBackendApiClientProvider remoteBackendApiClientProvider, IMediator mediator) : IWorkflowDefinitionImporter
{
    /// <inheritdoc />
    public async Task<WorkflowDefinition> ImportAsync(WorkflowDefinitionModel definitionModel, CancellationToken cancellationToken = default)
    {
        await mediator.NotifyAsync(new WorkflowDefinitionImporting(definitionModel), cancellationToken);
        var api = await GetApiAsync(cancellationToken);
        var newWorkflowDefinition = await api.ImportAsync(definitionModel, cancellationToken);
        await mediator.NotifyAsync(new WorkflowDefinitionImported(newWorkflowDefinition), cancellationToken);
        return newWorkflowDefinition;
    }
    
    /// <inheritdoc />
    public async Task<int> ImportAsync(IEnumerable<StreamPart> streamParts, CancellationToken cancellationToken = default)
    {
        var api = await GetApiAsync(cancellationToken);
        var response = await api.ImportFilesAsync(streamParts.ToList(), cancellationToken);
        return response.Count;
    }

    private async Task<IWorkflowDefinitionsApi> GetApiAsync(CancellationToken cancellationToken = default)
    {
        return await remoteBackendApiClientProvider.GetApiAsync<IWorkflowDefinitionsApi>(cancellationToken);
    }
}