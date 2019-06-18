using System;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.Windows.Speech;
using System.Linq;

#if ENABLE_WINMD_SUPPORT
using System.Windows.Input;
using Windows.Media.SpeechRecognition;
using UnityEngine.Windows.Speech;
#endif // ENABLE_WINMD_SUPPORT

[MixedRealityExtensionService(SupportedPlatforms.WindowsUniversal)]
public class SingleShotSpeechRecognitionService : BaseExtensionService, ISingleShotSpeechRecognitionService
{
    public SingleShotSpeechRecognitionService(
        IMixedRealityServiceRegistrar registrar,
        string name,
        uint priority,
        BaseMixedRealityProfile profile) : base(registrar, name, priority, profile)
    {
    }
    /// <summary>
    /// Why am I using my own speech handling rather than relying on MRTK?
    /// I started using those and they worked fine.
    /// However, I found that my speech commands would stop working across invocations of
    /// the file open dialog. They would work *before* and *stop* after.
    /// I spent a lot of time on this and I found that things would *work* under the debugger
    /// but not without it.
    /// That led me to think that this related to suspend/resume and perhaps HoloLens suspends
    /// the app when you move to the file dialog because I notice that dialog running as its
    /// own app on HoloLens.
    /// I tried hard to do work with suspend/resume but I kept hitting problems and so I wrote
    /// my own code where I try quite hard to avoid a single instance of SpeechRecognizer being
    /// used more than once - i.e. I create it, recognise with it & throw it away each time
    /// as this seems to *actually work* better than any other approach I tried.
    /// I also find that SpeechRecognizer.RecognizeAsync can get into a situation where it
    /// returns "Success" and "Rejected" at the same time & once that happens you don't get
    /// any more recognition unless you throw it away and so that's behind my approach.
    /// </summary>
    public async Task<bool> RecogniseAsync(
        Dictionary<string, Func<Task>> keywordsAndHandlers)
    {
        var result = false;

#if ENABLE_WINMD_SUPPORT
        using (var recognizer = new SpeechRecognizer())
        {
            recognizer.Constraints.Add(new SpeechRecognitionListConstraint(keywordsAndHandlers.Keys));

            await recognizer.CompileConstraintsAsync();

            var uwpResult = await recognizer.RecognizeAsync();

            if ((uwpResult != null) && (uwpResult.Confidence != SpeechRecognitionConfidence.Rejected))
            {
                await keywordsAndHandlers?[uwpResult.Text]();
                result = true;
            }            
        }
#endif

#if UNITY_EDITOR

        // Not sure if this sort of 'create and throw away' approach 
        using (var recognizer = new KeywordRecognizer(keywordsAndHandlers.Keys.ToArray()))
        {
            var completed = new TaskCompletionSource<bool>();

            recognizer.OnPhraseRecognized += async (e) =>
            {
                var recognised = false;

                if (e.confidence != ConfidenceLevel.Rejected)
                {
                    await keywordsAndHandlers?[e.text]();
                    recognised = true;
                }
                completed.SetResult(recognised);
            };
            recognizer.Start();

            result = await completed.Task;

            recognizer.Stop();
        }
#endif
        return (result);
    }
}
