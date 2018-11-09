using System;
using System.IO;
using System.Threading.Tasks;

#if ENABLE_WINMD_SUPPORT
using Windows.Storage;
using Windows.Storage.Pickers;
#endif 

internal static class FileDialogHelper
{
    internal static async Task<string> PickGLTFFileAsync()
    {
#if ENABLE_WINMD_SUPPORT
        var pickCompleted = new TaskCompletionSource<string>();

        UnityEngine.WSA.Application.InvokeOnUIThread(
            async () =>
            {
                Stream stream = null;
                FileOpenPicker picker = new FileOpenPicker();
                picker.SuggestedStartLocation = PickerLocationId.Objects3D;
                picker.FileTypeFilter.Add(".glb");
                picker.FileTypeFilter.Add(".gltf");
                picker.FileTypeFilter.Add("*");
                picker.ViewMode = PickerViewMode.Thumbnail;
                picker.CommitButtonText = "Select Model";

                var file = await picker.PickSingleFileAsync();
                string filePath = null;

                if (file != null)
                {
                    filePath = file.Path;
                }
                pickCompleted.SetResult(filePath);
            },
            true
        );

        await pickCompleted.Task;

        return (pickCompleted.Task.Result);

#else             
        throw new InvalidOperationException(
            "Sorry, no file dialog support for other platforms here");
#endif
    }
}
