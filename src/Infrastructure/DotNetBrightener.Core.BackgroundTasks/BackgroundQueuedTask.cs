﻿using System;
using System.Reflection;
using System.Threading.Tasks;

namespace DotNetBrightener.Core.BackgroundTasks;

/// <summary>
///     Represents a task that is set to run in background
/// </summary>
internal class BackgroundQueuedTask
{
    /// <summary>
    ///     The method that needs to be execute in background
    /// </summary>
    public MethodInfo Action { get; set; }

    /// <summary>
    ///     The parameters to send to the action
    /// </summary>
    public object[] Parameters { get; set; }

    public string TaskIdentifier { get; set; } = Guid.NewGuid().ToString();

    public DateTimeOffset? StartedOn { get; set; }

    internal Task<dynamic> TaskResult { get; set; }
}