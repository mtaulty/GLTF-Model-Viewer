using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// The main challenge I'm trying to "deal with" (rather than solve) here is that I
/// pick up code from UnityGLTF which works with CoRoutines. The challenge is that
/// code can throw and because of the CoRoutine nature, the exceptions simply
/// get swallowed which means I don't know whether a load of a GLTF model has worked
/// or not. I wrote this code to try and have CoRoutines with exceptions & then I
/// tried to marry that up with a bit of Tasks so that I can just try { await } catch { }
/// I can't say this works for all CoRoutines out there, it just seems to be "ok" for
/// what I need right now.
/// </summary>
public class ExtendedMonoBehaviour : MonoBehaviour
{
    protected async Task RunCoroutineAsync(IEnumerator coRoutine)
    {
        var completionSource = new TaskCompletionSource<bool>();

        this.StartCoRoutineWithExceptions(coRoutine,
            (exception) =>
            {
                if (exception == null)
                {
                    completionSource.SetResult(true);
                }
                else
                {
                    completionSource.SetException(exception);
                }
            }
        );
        await completionSource.Task;
    }
    protected void StartCoRoutineWithExceptions(IEnumerator coRoutine, Action<Exception> callback)
    {
        var exceptions = new List<Exception>();

        base.StartCoroutine(
            this.RunCoRoutineWithExceptions(coRoutine, callback));
    }
    protected IEnumerator RunCoRoutineWithExceptions(IEnumerator coRoutine,
        Action<Exception> callback)
    {
        var exceptions = new List<Exception>();

        yield return this.RecurseCoRoutineWithExceptions(coRoutine, exceptions);

        callback?.Invoke(exceptions.FirstOrDefault());
    }
    protected IEnumerator RecurseCoRoutineWithExceptions(IEnumerator coRoutine,
        IList<Exception> exceptions)
    {
        bool completed = false;

        while (!completed)
        {
            object currentValue = null;

            try
            {
                completed = !coRoutine.MoveNext();

                if (!completed)
                {
                    currentValue = coRoutine.Current;
                }
            }
            catch (Exception exception)
            {
                completed = true;
                exceptions.Add(exception);
            }
            if (!completed)
            {
                if (currentValue is IEnumerator)
                {
                    yield return this.RecurseCoRoutineWithExceptions(
                        (IEnumerator)currentValue,
                        exceptions
                    );
                }
                else
                {
                    yield return currentValue;
                }
            }
        }
    }
}
