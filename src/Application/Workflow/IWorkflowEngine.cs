using ERP_Government.Domain.Workflow.Entities;

namespace ERP_Government.Application.Workflow;

public interface IWorkflowEngine
{
    Task<WorkflowInstance> StartInstanceAsync(string entityName, int entityId, CancellationToken cancellationToken = default);
    Task<WorkflowInstance> AdvanceInstanceAsync(int instanceId, int stepId, string decision, string? reason, CancellationToken cancellationToken = default);
    Task<WorkflowInstance> CancelInstanceAsync(int instanceId, CancellationToken cancellationToken = default);
    Task<WorkflowInstance?> GetInstanceAsync(int instanceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkflowInstance>> GetInstancesAsync(string? entityName, int? entityId, string? status, CancellationToken cancellationToken = default);
}