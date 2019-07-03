using Microsoft.MixedReality.Toolkit;
using System.Threading.Tasks;

public interface IGltfFilePickerService : IMixedRealityExtensionService
{
    Task<string> PickFileAsync();
}
