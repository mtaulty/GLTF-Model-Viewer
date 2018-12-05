using System;
using System.Threading.Tasks;
#if ENABLE_WINMD_SUPPORT
using Windows.UI.Popups;
#endif // ENABLE_WINMD_SUPPORT

internal static class MessageDialogHelper
{
    internal static async Task<bool> AskYesNoQuestionAsync(string title, string question)
    {
#if ENABLE_WINMD_SUPPORT
        var completed = new TaskCompletionSource<bool>();

        UnityEngine.WSA.Application.InvokeOnUIThread(
            async () =>            
            {
                var dialog = new MessageDialog(question, title);

                bool accepted = false;

                dialog.Commands.Add(
                    new UICommand("Yes", _ => { completed.SetResult(true); }));

                dialog.Commands.Add(
                    new UICommand("No", _ => { completed.SetResult(false); }));

                dialog.CancelCommandIndex = 1;

                await dialog.ShowAsync();
            },
            true
        );
        await completed.Task;

        return(completed.Task.Result);
#else
        throw new InvalidOperationException("Sorry, only UWP support here");
#endif // ENABLE_WINMD_SUPPORT
    }
}
