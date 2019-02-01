using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImportedModelInfo
{
    public ImportedModelInfo(RecordingFileLoader fileLoader)
    {
        this.FileLoader = fileLoader;
    }
    public GameObject GameObject { get; set; }
    public RecordingFileLoader FileLoader { get; private set; }
}