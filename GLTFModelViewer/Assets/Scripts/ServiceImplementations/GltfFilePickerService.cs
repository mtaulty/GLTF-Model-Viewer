using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;

#if ENABLE_WINMD_SUPPORT
using Windows.Storage;
using Windows.Storage.Pickers;
#endif

[MixedRealityExtensionService(SupportedPlatforms.WindowsEditor | SupportedPlatforms.WindowsUniversal)]
public class GltfFilePickerService : BaseExtensionService, IGltfFilePickerService
{
    public GltfFilePickerService(
        IMixedRealityServiceRegistrar registrar, 
        string name = null, 
        uint priority = 10, 
        BaseMixedRealityProfile profile = null) : base(registrar, name, priority, profile)
    {
    }
    public async Task<string> PickFileAsync()
    {
        var returnValue = string.Empty;

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

                pickCompleted.SetResult(file?.Path);
            },
            true
        );

        await pickCompleted.Task;

        returnValue = pickCompleted.Task.Result;
#else

#if UNITY_EDITOR
        returnValue = EditorUtility.OpenFilePanelWithFilters(
            "Select GLTF File",
            string.Empty,
            new string[] { "GLTF Files", "gltf,glb", "All Files", "*" });

        if (string.IsNullOrEmpty(returnValue))
        {
            returnValue = null;
        }
#else
        throw new InvalidOperationException(
            "Sorry, no file dialog support for other platforms here");
#endif // UNITY_EDITOR

#endif // ENABLE_WINMD_SUPPORT

        // If we have a NULL return value then we assume (for the UWP case) that
        // the file dialog got cancelled whereas if we have a good file path then
        // we need to check it's in the right folder.
        if ((returnValue != null) && !IsValidFilePath(returnValue))
        {
            returnValue = string.Empty;
        }
        return (returnValue);
    }
    static bool IsValidFilePath(string filePath)
    {
        var valid = !string.IsNullOrEmpty(filePath);

#if ENABLE_WINMD_SUPPORT        
        var known3DObjectsFolder = KnownFolders.Objects3D.Path.ToLower().TrimEnd('\\');

        valid = valid && filePath.ToLower().StartsWith(known3DObjectsFolder);
#endif 
        return (valid);
    }
}