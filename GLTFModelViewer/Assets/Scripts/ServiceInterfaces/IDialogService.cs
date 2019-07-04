using Microsoft.MixedReality.Toolkit;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public interface IDialogService : IMixedRealityExtensionService
{
    Task<bool> AskYesNoQuestionAsync(string title, string question);
}
