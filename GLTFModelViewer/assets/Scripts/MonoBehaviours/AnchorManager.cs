using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.WSA;
using UnityEngine.XR.WSA.Sharing;

public class AnchorManager : MonoBehaviour
{
    ParentProvider ParentProvider => this.gameObject.GetComponent<ParentProvider>();

    public void RemoveAnchorFromModelParent()
    {
        // Unanchor the parent.
        var anchor = this.ParentProvider.GLTFModelParent.GetComponent<WorldAnchor>();

        if (anchor != null)
        {
            Destroy(anchor);
        }
    }
    public void AddAnchorToModelParent()
    {
        // Anchor the parent.
        this.ParentProvider.GLTFModelParent.AddComponent<WorldAnchor>();
    }
    public async Task<bool> ImportAnchorToModelParent(byte[] worldAnchorBits)
    {
        var result = await ImportWorldAnchorToGameObjectAsync(
            this.ParentProvider.GLTFModelParent,
            worldAnchorBits);

        return (result);
    }
    public async Task<byte[]> ExportAnchorAsync()
    {
        // Before we can export the anchor, we probably need to wait for it to
        // say that it is located otherwise (AFAIK) the export fails.
        await AwaitableUpdateLoopCondition.WaitForConditionAsync(
            this.ParentProvider.GLTFModelParent,
            parent => parent.GetComponent<WorldAnchor>().isLocated);

        // We now think that the anchor is located so we can try to export it
        // into a file.
        // TODO: has to be a whole range of failures/errors here for us to think about...
        byte[] bits = await ExportWorldAnchorForGameObjectAsync(this.ParentProvider.GLTFModelParent);

        return (bits);
    }
    static async Task<bool> ImportWorldAnchorToGameObjectAsync(
        GameObject gameObject,
        byte[] worldAnchorBits)
    {
        var completion = new TaskCompletionSource<bool>();
        bool worked = false;

        WorldAnchorTransferBatch.ImportAsync(worldAnchorBits,
          (reason, batch) =>
          {
              if (reason == SerializationCompletionReason.Succeeded)
              {
                  var anchorId = batch.GetAllIds().FirstOrDefault();

                  if (!string.IsNullOrEmpty(anchorId))
                  {
                      batch.LockObject(anchorId, gameObject);
                      worked = true;
                  }
              }
              batch.Dispose();
              completion.SetResult(true);
          }
        );
        await completion.Task;

        return (worked);
    }
    static async Task<byte[]> ExportWorldAnchorForGameObjectAsync(
      GameObject gameObject)
    {
        byte[] bits = null;

        var worldAnchor = gameObject.GetComponent<WorldAnchor>();

        using (var worldAnchorBatch = new WorldAnchorTransferBatch())
        {
            worldAnchorBatch.AddWorldAnchor("anchor", worldAnchor);

            var completion = new TaskCompletionSource<bool>();

            using (var memoryStream = new MemoryStream())
            {
                WorldAnchorTransferBatch.ExportAsync(
                  worldAnchorBatch,
                  data =>
                  {
                      memoryStream.Write(data, 0, data.Length);
                  },
                  reason =>
                  {
                      if (reason != SerializationCompletionReason.Succeeded)
                      {
                          bits = null;
                      }
                      else
                      {
                          bits = memoryStream.ToArray();
                      }
                      completion.SetResult(bits != null);
                  }
                );
                await completion.Task;
            }
        }
        return (bits);
    }
}