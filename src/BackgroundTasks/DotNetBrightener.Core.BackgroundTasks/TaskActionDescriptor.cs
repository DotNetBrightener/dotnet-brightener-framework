using System.Reflection;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Core.BackgroundTasks;

public class TaskActionDescriptor
{
    private readonly bool _isAsync;

    private readonly object[] _parameters;

    internal Task<dynamic> TaskResult { get; set; }

    public Guid Guid { get; private init; }

    public Type InvocableType { get; private init; }

    private readonly MethodInfo _action;

    public string TaskName => $"{_action.DeclaringType?.FullName}.{_action.Name}()";

    public TaskActionDescriptor(MethodInfo action, params object[] parameters)
    {
        _isAsync = action.ReturnType.GetMethod(nameof(Task.GetAwaiter)) != null;
        _action = action;
        _parameters = parameters;
        InvocableType = _action.DeclaringType;
        Guid = Guid.NewGuid();
    }

    public async Task Invoke(ILogger logger, object invokingInstance, CancellationToken cancellationToken)
    {
        logger.LogInformation("Executing task: {taskName} ({awaitable})",
                              TaskName,
                              _isAsync ? "awaitable" : "non-awaitable");

        if (!_isAsync)
        {
            _action.Invoke(invokingInstance, _parameters);

            return;
        }

        var invokeResult = Task.Run(async () =>
                                    {
                                        try
                                        {
                                            var result = await (dynamic)_action.Invoke(invokingInstance, _parameters);

                                            return result;
                                        }
                                        catch (Exception ex)
                                        {
                                            logger.LogError(ex,
                                                            "Error executing task: {taskName}. Returning result as null.",
                                                            TaskName);

                                            return null;
                                        }
                                    },
                                    cancellationToken);

        TaskResult = invokeResult;
    }
}