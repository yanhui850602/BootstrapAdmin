﻿// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.
// Licensed under the LGPL License, Version 3.0. See License.txt in the project root for license information.
// Website: https://admin.blazor.zone

using BootstrapAdmin.Web.Models;
using Longbow.Tasks;

namespace BootstrapAdmin.Web.Components;

/// <summary>
/// 
/// </summary>
public partial class TaskInfo
{
    /// <summary>
    /// 
    /// </summary>
    [Parameter]
    [NotNull]
    [EditorRequired]
    public TasksModel? Model { get; set; }

    private List<ConsoleMessageItem> Messages { get; } = new();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="firstRender"></param>
    /// <returns></returns>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var sche = TaskServicesManager.Get(Model.Name);
            if (sche != null)
            {
                sche.Triggers.First().PulseCallback = async t => await DispatchMessage(t);
                await DispatchMessage(sche.Triggers.First());
            }
        }
    }

    private async Task DispatchMessage(ITrigger trigger)
    {
        var message = $"Trigger({trigger.GetType().Name}) LastRuntime: {trigger.LastRuntime} Run({trigger.LastResult}) NextRuntime: {trigger.NextRuntime} Elapsed: {trigger.LastRunElapsedTime.TotalSeconds}";
        Messages.Add(new ConsoleMessageItem()
        {
            Message = message
        });
        await InvokeAsync(StateHasChanged);
    }
}
