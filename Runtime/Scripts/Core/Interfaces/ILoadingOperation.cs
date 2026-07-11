using System.Collections;

public interface ILoadingOperation
{
    float Weight { get; }

    float Progress { get; }

    bool IsFinished { get; }

    IEnumerator Execute();
}