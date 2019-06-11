using MulticastMessaging.Messages;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using UwpHttpServer;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DummyWebServer
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<RemoteModelViewModel> ObservedNewModels { private set; get; }
        public ObservableCollection<Guid> ObservedDeletedModelIds { private set; get; }

        public bool HasNewModels => this.ObservedNewModels.Count > 0;

        public bool CanPlaybackTransforms => this.modelForTransformPlayback != null;

        public bool IsPlayingBack
        {
            get => this.isPlayingBack;
            set 
            {
                if (value != this.isPlayingBack)
                {
                    this.isPlayingBack = value;
                    this.FirePropertyChanged(nameof(IsPlayingBack));
                }
            }
        }
        bool isPlayingBack;

        public string GuidForNewModelMessage
        {
            get => this.guidForNewModelMessage;
            set
            {
                if (value != this.guidForNewModelMessage)
                {
                    this.guidForNewModelMessage = value;
                    this.FirePropertyChanged(nameof(GuidForNewModelMessage));
                }
            }
        }
        string guidForNewModelMessage;

        public string GuidForDeleteModelMessage
        {
            get => this.guidForDeleteModelMessage;
            set
            {
                if (value != this.guidForDeleteModelMessage)
                {
                    this.guidForDeleteModelMessage = value;
                    this.FirePropertyChanged(nameof(GuidForNewModelMessage));
                }
            }
        }
        string guidForDeleteModelMessage;

        public string GuidForTransformPlayback
        {
            get => this.guidForTransformPlayback;
            set
            {
                if (value != this.guidForTransformPlayback)
                {
                    this.guidForTransformPlayback = value;
                    this.FirePropertyChanged(nameof(GuidForTransformPlayback));
                }
            }
        }
        string guidForTransformPlayback;

        public RemoteModelViewModel SelectedNewModel
        {
            get => this.selectedNewModel;
            set
            {
                if (this.selectedNewModel != value)
                {
                    this.selectedNewModel = value;
                    this.FirePropertyChanged(nameof(SelectedNewModel));
                }
            }
        }
        RemoteModelViewModel selectedNewModel;

        public MainPage()
        {
            this.InitializeComponent();
            this.GuidForNewModelMessage = "2db8b09e-5350-45b4-9e5c-5fcd2adfad5e";
            this.ObservedNewModels = new ObservableCollection<RemoteModelViewModel>();
            this.ObservedDeletedModelIds = new ObservableCollection<Guid>();
            this.Loaded += OnLoaded;
        }
        async void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.webServer = new StorageFolderWebServer(
                KnownFolders.Objects3D);

            NetworkMessagingProvider.Initialise(
                this.OnNewModelMessage, this.OnDeleteModelMessage, this.OnTransformMessage);
            
            await this.webServer.RunAsync();
        }
        void OnDeleteModelMessage(object message)
        {
            var msg = message as DeleteModelMessage;

            var modelEntry = this.ObservedNewModels.SingleOrDefault(
                m => m.Identifier == msg.ModelIdentifier);

            if (modelEntry != null)
            {
                this.ObservedNewModels.Remove(modelEntry);

                this.ObservedDeletedModelIds.Add(msg.ModelIdentifier);

                this.FirePropertyChanged(nameof(HasNewModels));
            }
        }
        void OnNewModelMessage(object message)
        {
            var msg = message as NewModelMessage;

            var newModel = new RemoteModelViewModel()
            {
                Identifier = msg.ModelIdentifier,
                RemoteHostIp = msg.ServerIPAddress
            };
            newModel.SelectedForPlayback += OnNewModelSelectedForPlayback;

            this.ObservedNewModels.Add(newModel);
            
            this.FirePropertyChanged(nameof(HasNewModels));
        }
        void OnNewModelSelectedForPlayback(object sender, EventArgs e)
        {
            this.modelForTransformPlayback = (RemoteModelViewModel)sender;
            this.FirePropertyChanged(nameof(CanPlaybackTransforms));
        }
        void OnTransformMessage(object obj)
        {
            var message = obj as TransformChangeMessage;

            foreach (var model in this.ObservedNewModels)
            {
                model.OnTransformMessage(message);
            }
        }
        void OnSendNewModelMessage(object sender, RoutedEventArgs e)
        {
            Guid guidValue;
            if (Guid.TryParse(this.GuidForNewModelMessage, out guidValue))
            {
                NetworkMessagingProvider.SendNewModelMessage(guidValue);
            }
        }
        void OnSendDeleteModelMessage(object sender, RoutedEventArgs e)
        {
            Guid guidValue;
            if (Guid.TryParse(this.GuidForDeleteModelMessage, out guidValue))
            {
                NetworkMessagingProvider.SendDeleteModelMessage(guidValue);
            }
        }
        async void OnPlaybackTransformsToModel()
        {
            this.IsPlayingBack = true;

            Guid guidValue;
            if (Guid.TryParse(this.GuidForTransformPlayback, out guidValue))
            {
                await this.modelForTransformPlayback.PlaybackCapturedTransformMessagesAsync(guidValue);
            }
            this.IsPlayingBack = false;
        }
        void FirePropertyChanged(string property)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        StorageFolderWebServer webServer;
        RemoteModelViewModel modelForTransformPlayback;
    }
}