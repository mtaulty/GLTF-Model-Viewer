using System;
using System.Collections;

internal interface IRunCoRoutine
{
    void RunCoRoutineWithCallback(IEnumerator coRoutine, Action callback);
}