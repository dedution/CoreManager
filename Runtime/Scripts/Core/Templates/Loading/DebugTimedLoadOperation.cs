using System.Collections;
using UnityEngine;

public class DebugTimedLoadOperation : ILoadingOperation
{
    public float Weight => 1.0f;

    public float Progress { get; private set; }

    public bool IsFinished { get; private set; }

    public float DebugTime = 1.0f;

    public DebugTimedLoadOperation(float time)
    {
        DebugTime = time;
    }

    public IEnumerator Execute()
    {
        Progress = 0.0f;
        IsFinished = false;

        yield return new WaitForSecondsRealtime(DebugTime);

        // float elapsed = 0f;

        // while (elapsed < DebugTime)
        // {
        //     elapsed += Time.deltaTime;
        //     Progress = Mathf.Clamp01(elapsed / DebugTime);

        //     yield return null;
        // }

        Progress = 1.0f;
        IsFinished = true;
    }
}