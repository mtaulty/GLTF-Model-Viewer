using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace DummyWebServer
{
    public class RemoteModelViewModel : PropertyChangeBase
    {
        public event EventHandler SelectedForPlayback;

        public bool CanRecordTransforms => !this.isRecordingTransforms;

        public bool CanPlayBackRecordedTransforms =>
            !this.isRecordingTransforms && this.recordedEntries.Count > 0;

        public int RecordedTransformCount =>this.recordedEntries.Count;

        public Guid Identifier
        {
            get => this.identifier;
            set => this.SetProperty(ref this.identifier, value);
        }
        public IPAddress RemoteHostIp
        {
            get => this.remoteHostIp;
            set => this.SetProperty(ref this.remoteHostIp, value);
        }
        public bool IsRecordingTransforms
        {
            get => this.isRecordingTransforms;
            set
            {
                if (base.SetProperty(ref this.isRecordingTransforms, value))
                {
                    base.FirePropertyChanged(nameof(CanRecordTransforms));
                    base.FirePropertyChanged(nameof(CanPlayBackRecordedTransforms));
                }
            }
        }
        public void OnRecordTransforms()
        {
            this.IsRecordingTransforms = true;
            this.recordedEntries = new List<RecordedEntry>();
            this.lastTransformMessageTime = DateTime.Now;
        }
        public void OnStopRecordingTransforms()
        {
            this.IsRecordingTransforms = false;
        }
        public void OnSetAsCurrentTransforms()
        {
            if (!this.IsRecordingTransforms)
            {
                this.SelectedForPlayback?.Invoke(this, EventArgs.Empty);
            }
        }
        public void OnTransformMessage(TransformChangeMessage message)
        {
            if (this.IsRecordingTransforms && (message.ModelIdentifier == this.identifier))
            {
                var interval = DateTime.Now - this.lastTransformMessageTime;
                this.lastTransformMessageTime = DateTime.Now;

                this.recordedEntries.Add(
                    new RecordedEntry()
                    {
                        Interval = interval, 
                        TransformChange = message
                    }
                );
                base.FirePropertyChanged(nameof(RecordedTransformCount));
            }
        }
        public async Task PlaybackCapturedTransformMessagesAsync(
            Guid modelIdentifier)
        {
            foreach (var entry in this.recordedEntries)
            {
                await Task.Delay(entry.Interval);

                NetworkMessagingProvider.SendTransformChangeMessage(
                    modelIdentifier,
                    entry.TransformChange.Scale,
                    entry.TransformChange.Rotation,
                    entry.TransformChange.Translation);
            }
        }
        public async Task OnRequestModelAsync()
        {
            var modelDownloader = new HttpModelDownloader(
                this.Identifier,
                this.RemoteHostIp);

            await modelDownloader.DownloadModelToLocalStorageAsync();
        }
        Guid identifier;
        IPAddress remoteHostIp;
        bool isRecordingTransforms;
        List<RecordedEntry> recordedEntries;
        DateTime lastTransformMessageTime;
    }
}
