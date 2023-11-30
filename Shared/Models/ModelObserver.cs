﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models;

/// <summary>
/// The ModelObserver class allows global events to be hooked for entire item types
/// </summary>
public static class ModelObserver<T> where T : Item
{
    /// <summary>
    /// Run when any of this item type is updated
    /// </summary>
    public static event Func<ModelUpdateEvent<T>, Task> OnAnyUpdated;

    /// <summary>
    /// Run when any of this item type is deleted
    /// </summary>
    public static event Func<T, Task> OnAnyDeleted;

    public static async Task InvokeAnyUpdated(ModelUpdateEvent<T> eventData)
    {
        if (OnAnyUpdated != null)
            await OnAnyUpdated.Invoke(eventData);
    }

    public static async Task InvokeAnyDeleted(T deleted)
    {
        if (OnAnyDeleted != null)
            await OnAnyDeleted.Invoke(deleted);
    }
}
