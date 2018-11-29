using UnityEngine;
using System;
using System.Threading.Tasks;

public class AwaitableUpdateLoopCondition : MonoBehaviour
{
    public AwaitableUpdateLoopCondition()
    {
        this.completed = new TaskCompletionSource<bool>();
    }
    public Func<GameObject, bool> Predicate { get; set; }

    void Update()
    {
        if ((this.Predicate != null) && this.Predicate(this.gameObject))
        {
            this.completed.SetResult(true);
        }
    }
    public async static Task WaitForConditionAsync(
      GameObject gameObject,
      Func<GameObject, bool> predicate)
    {
        var component = gameObject.AddComponent<AwaitableUpdateLoopCondition>();
        component.Predicate = predicate;
        await component.completed.Task;
        Destroy(component);
    }
    TaskCompletionSource<bool> completed;
}
