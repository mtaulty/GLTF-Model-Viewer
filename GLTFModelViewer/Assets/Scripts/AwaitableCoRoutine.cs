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
    internal static async Task RunCoroutineAsync(IRunCoRoutine routineRunner, IEnumerator coRoutine)
    { 
        var completionSource = new TaskCompletionSource<bool>();

        routineRunner.RunCoRoutineWithCallback(coRoutine,
            () =>
            {
                completionSource.SetResult(true);
            }
        );
        await completionSource.Task;
    }
}