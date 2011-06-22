﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace VDS.RDF.Utilities.StoreManager.Tasks
{
    public enum TaskState
    {
        Unknown,
        Starting,
        NotRun,
        Running,
        RunningCancelled,
        Completed,
        CompletedWithErrors
    }

    public class TaskResult
    {
        private bool _ok = false;

        public TaskResult(bool ok)
        {
            this._ok = ok;
        }

        public bool OK
        {
            get
            {
                return this._ok;
            }
        }
    }

    public delegate void TaskCallback<T>(ITask<T> task) where T : class;

    public delegate void TaskStateChanged();

    public interface ITaskBase
    {
        TaskState State
        {
            get;
        }

        String Name
        {
            get;
        }

        String Information
        {
            get;
            set;
        }

        Exception Error
        {
            get;
        }

        TimeSpan? Elapsed
        {
            get;
        }

        bool IsCancellable
        {
            get;
        }

        void Cancel();

        event TaskStateChanged StateChanged;
    }

    public interface ITask<TResult> : ITaskBase
        where TResult : class
    {

        TResult Result
        {
            get;
        }

        void RunTask(TaskCallback<TResult> callback);

    }
}
