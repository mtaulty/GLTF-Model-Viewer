using System;

namespace DummyWebServer
{
    public class RecordedEntry
    {
        public TransformChangeMessage TransformChange { get; set; }
        public TimeSpan Interval { get; set; }
    }
}

//void OnStopCapturingTransformMessages(object sender, RoutedEventArgs e)
//{
//    this.isCapturing = false;
//}
//async void OnCaptureTransformMessages(object sender, RoutedEventArgs e)
//{
//    this.lastTransformMessageTime = DateTime.Now;
//    this.isCapturing = true;
//}
//async void OnPlaybackCapturedTransformMessages(object sender, RoutedEventArgs e)
//{
//    foreach (var entry in this.recordedEntries)
//    {
//        await Task.Delay(entry.Interval);

//        NetworkMessagingProvider.SendTransformChangeMessage(
//            Guid.Parse(this.txtGuidNewModel.Text),
//            entry.TransformChange.Scale,
//            entry.TransformChange.Rotation,
//            entry.TransformChange.Translation);
//    }
//}
//List<RecordedEntry> recordedEntries;
//bool isCapturing;
//DateTime lastTransformMessageTime;
