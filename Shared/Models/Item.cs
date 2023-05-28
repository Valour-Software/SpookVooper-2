using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models;

public abstract class Item
{
    public long Id { get; set; }

    public virtual string IdRoute => $"{BaseRoute}/{Id}";

    public virtual string BaseRoute => $"api/{GetType().Name}";

    /// <summary>
    /// Ran when this item is updated
    /// </summary>
    public event Func<ModelUpdateEvent, Task> OnUpdated;

    /// <summary>
    /// Ran when this item is deleted
    /// </summary>
    public event Func<Task> OnDeleted;

    /// <summary>
    /// Custom logic on model update
    /// </summary>
    public virtual Task OnUpdate(ModelUpdateEvent eventData)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Custom logic on model deletion
    /// </summary>
    public virtual Task OnDelete()
    {
        return Task.CompletedTask;
    }

    public virtual async Task AddToCache()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Safely invokes the updated event
    /// </summary>
    public async Task InvokeUpdatedEventAsync(ModelUpdateEvent eventData)
    {
        await OnUpdate(eventData);

        if (OnUpdated != null)
            await OnUpdated.Invoke(eventData);
    }

    /// <summary>
    /// Safely invokes the deleted event
    /// </summary>
    public async Task InvokeDeletedEventAsync()
    {
        await OnDelete();

        if (OnDeleted != null)
            await OnDeleted.Invoke();
    }

    /// <summary>
    /// Attempts to create this item
    /// </summary>
    /// <typeparam name="T">The type of object being created</typeparam>
    /// <param name="item">The item to create</param>
    /// <returns>The result, with the created item (if successful)</returns>
    public static async Task<TaskResult<T>> CreateAsync<T>(T item) where T : Item
    {
        return await SVClient.PostAsyncWithResponse<T>(item.BaseRoute, item);
    }

    /// <summary>
    /// Attempts to update this item
    /// </summary>
    /// <typeparam name="T">The type of object being created</typeparam>
    /// <param name="item">The item to update</param>
    /// <returns>The result, with the updated item (if successful)</returns>
    public static async Task<TaskResult<T>> UpdateAsync<T>(T item) where T : Item
    {
        return await SVClient.PutAsyncWithResponse(item.IdRoute, item);
    }

    /// <summary>
    /// Attempts to delete this item
    /// </summary>
    /// <typeparam name="T">The type of object being deleted</typeparam>
    /// <param name="item">The item to delete</param>
    /// <returns>The result</returns>
    public static async Task<TaskResult> DeleteAsync<T>(T item) where T : Item
    {
        return await SVClient.DeleteAsync(item.IdRoute);
    }
}
