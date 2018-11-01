using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// I'm sure this must already exist somewhere so can replace it with a built-in
/// implementation once I realise where it is :-)
/// </summary>
internal static class AwaitableCoRoutine
{
    class CoRoutineHelper : MonoBehaviour
    {
        internal IEnumerator RunCoRoutineWithCompletionCallback(IEnumerator coroutine, Action callback)
        {
            yield return StartCoroutine(coroutine);
            callback();
        }
    }

    internal static async Task RunCoroutineAsync(IEnumerator coRoutine)
    {
        var helper = new CoRoutineHelper();
        var completionSource = new TaskCompletionSource<bool>();

        helper.RunCoRoutineWithCompletionCallback(coRoutine,
            () =>
            {
                completionSource.SetResult(true);
            }
        );
        await completionSource.Task;
    }
}