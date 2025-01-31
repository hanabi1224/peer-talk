﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PeerTalk
{
    /// <summary>
    ///   Some helpers for tasks.
    /// </summary>
    public class TaskHelper
    {
        /// <summary>
        ///   Gets the first result from a set of tasks.
        /// </summary>
        /// <typeparam name="T">
        ///   The result type of the <paramref name="tasks"/>.
        /// </typeparam>
        /// <param name="tasks">
        ///   The tasks to perform.
        /// </param>
        /// <param name="cancel">
        ///   Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException"/> is raised.
        /// </param>
        /// <returns>
        ///   A task that represents the asynchronous operation. The task's result is
        ///   a <typeparamref name="T"/>>.
        /// </returns>
        /// <remarks>
        ///   Returns the result of the first task that is not
        ///   faulted or canceled.
        /// </remarks>
        public static async Task<T> WhenAnyResult<T>(
            IEnumerable<Task<T>> tasks,
            CancellationToken cancel)
        {
            var exceptions = new List<Exception>();
            var running = tasks.ToList();
            while (running.Count > 0)
            {
                cancel.ThrowIfCancellationRequested();
                var winner = await Task.WhenAny(running).ConfigureAwait(false);
                if (!winner.IsCanceled && !winner.IsFaulted)
                {
                    return winner.Result;
                }
                if (winner.IsFaulted)
                {
                    if (winner.Exception is AggregateException ae)
                    {
                        exceptions.AddRange(ae.InnerExceptions);
                    }
                    else
                    {
                        exceptions.Add(winner.Exception);
                    }
                }
                running.Remove(winner);
            }
            cancel.ThrowIfCancellationRequested();
            throw new AggregateException("No task(s) returned a result.", exceptions);
        }
    }
}
