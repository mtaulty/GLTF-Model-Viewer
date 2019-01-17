using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using UwpHttpServer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace DummyWebServer
{
    public sealed partial class MainPage : Page
    {
        class RecordedEntry
        {
            public TransformChangeMessage TransformChange { get; set; }
            public TimeSpan Interval { get; set; }
        }

        public MainPage()
        {
            this.InitializeComponent();
            this.recordedEntries = new List<RecordedEntry>();
            this.Loaded += OnLoaded;
        }
        async void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.webServer = new StorageFolderWebServer(
                KnownFolders.Objects3D);

            NetworkMessagingProvider.Initialise(this.OnTransformMessage);
            
            await this.webServer.RunAsync();
        }
        void OnTransformMessage(object obj)
        {
            if (this.isCapturing)
            {
                var interval = DateTime.Now - this.lastTransformMessageTime;
                this.lastTransformMessageTime = DateTime.Now;

                this.recordedEntries.Add(
                    new RecordedEntry()
                    {
                        Interval = interval,
                        TransformChange = (TransformChangeMessage)obj
                    }
                );
            }
        }
        void OnStopCapturingTransformMessages(object sender, RoutedEventArgs e)
        {
            this.isCapturing = false;
        }
        void OnSendNewModelMessage(object sender, RoutedEventArgs e)
        {
            NetworkMessagingProvider.SendNewModelMessage(
                Guid.Parse(this.txtGuidNewModel.Text));
        }
        async void OnCaptureTransformMessages(object sender, RoutedEventArgs e)
        {
            this.lastTransformMessageTime = DateTime.Now;
            this.isCapturing = true;
        }
        async void OnPlaybackCapturedTransformMessages(object sender, RoutedEventArgs e)
        {
            foreach (var entry in this.recordedEntries)
            {
                await Task.Delay(entry.Interval);

                NetworkMessagingProvider.SendTransformChangeMessage(
                    Guid.Parse(this.txtGuidNewModel.Text),
                    entry.TransformChange.Scale,
                    entry.TransformChange.Rotation,
                    entry.TransformChange.Translation);
            }
        }
        List<RecordedEntry> recordedEntries;
        bool isCapturing;
        DateTime lastTransformMessageTime;
        StorageFolderWebServer webServer;
    }
}

//Vector3 scale = new Vector3();
//scale.x = scale.y = scale.z = 1.0f;
//Vector3 translation = new Vector3();
//Quaternion rotation = new Quaternion();
//rotation.w = 1;

//for (int i = -2; i < 2; i++)
//{
//    scale.x += i / 10.0f;
//    scale.y += 0.5f;
//    scale.z += 0.5f;
//    translation.x += 0.2f;
//    translation.y += 0.2f;
//    translation.z += 0.2f;

//    NetworkMessagingProvider.SendTransformChangeMessage(
//        Guid.Parse("2db8b09e-5350-45b4-9e5c-5fcd2adfad5e"),
//        scale,
//        rotation,
//        translation);

//    await Task.Delay(1000);
//}
