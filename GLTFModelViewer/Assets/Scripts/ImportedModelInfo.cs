using Microsoft.MixedReality.Toolkit.Utilities.Gltf.Schema;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ImportedModelInfo
{
    public ImportedModelInfo(
        string fullFilePath,
        GltfObject gltfObject)
    {
        this.BaseDirectoryPath = Path.GetDirectoryName(fullFilePath);

        this.relativeLoadedFilePaths =
            gltfObject.buffers.Select(b => b.uri).Concat(gltfObject.images.Select(i => i.uri)).ToList();

        this.GameObject = gltfObject.GameObjectReference;
    }
    public string BaseDirectoryPath { get; private set; }
    public IReadOnlyList<string> RelativeLoadedFilePaths => this.relativeLoadedFilePaths.AsReadOnly();
    public GameObject GameObject { get; set; }

    List<string> relativeLoadedFilePaths;
}