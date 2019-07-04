using Microsoft.MixedReality.Toolkit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ISingleShotSpeechRecognitionService : IMixedRealityExtensionService
{
    Task RecogniseAndDispatchCommandsAsync(InputActionHandlerPair[] actionsAndHandlers);
}
