using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class AwaitableMonoBehaviour : MonoBehaviour
{
    protected async Task RunCoroutineAsync(IEnumerator coRoutine)
    {
        var completionSource = new TaskCompletionSource<bool>();

        this.RunCoRoutineWithCallback(coRoutine,
            () =>
            {
                completionSource.SetResult(true);
            }
        );
        await completionSource.Task;
    }
    protected void RunCoRoutineWithCallback(IEnumerator coRoutine, Action callback)
    {
        this.StartCoroutine(RunCoRoutine(coRoutine, callback));
    }
    protected IEnumerator RunCoRoutine(IEnumerator coRoutine, Action callback)
    {
        yield return base.StartCoroutine(coRoutine);
        callback();
    }
}
