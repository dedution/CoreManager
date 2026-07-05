using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace core.tasks {
    public abstract class BaseTask
    {
        public TaskTypes taskType;

        // Make this function async
        public async Task Execute(Action onComplete)
        {
            await onExecute();

            // Call task completion
            if (onComplete != null)
                onComplete();

            await Task.Yield();
        }

        public async virtual Task onExecute()
        {
            await Task.Yield();
        }
    }

    public enum TaskTypes
    {
        Load,
        Unload,
        Optimizer
    }

}
