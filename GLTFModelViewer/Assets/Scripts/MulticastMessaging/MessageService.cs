namespace MulticastMessaging
{
    using Microsoft.MixedReality.Toolkit;
    using Microsoft.MixedReality.Toolkit.Utilities;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;

    [MixedRealityExtensionService(SupportedPlatforms.WindowsUniversal | SupportedPlatforms.WindowsEditor)]
    public class MessageService : BaseExtensionService, IMessageService
    {
        // Note: 239.0.0.0 is the start of the UDP multicast addresses reserved for
        // private use.
        // Note: 49152 is the result I get out of executing;
        //      netsh int ipv4 show dynamicport udp
        // on Windows 10.
        public MessageService(
            IMixedRealityServiceRegistrar registrar,
            string name,
            uint priority,
            BaseMixedRealityProfile profile) : base(registrar, name, priority, profile)
        {

        }
        MessageServiceProfile Profile => base.ConfigurationProfile as MessageServiceProfile;

        public MessageRegistrar MessageRegistrar
        {
            get => this.messageRegistrar;
            set => this.messageRegistrar = value;
        }

        public void Open()
        {
            this.CheckOpen(false);

            this.multicastEndpoint =
                new IPEndPoint(
                    IPAddress.Parse(this.Profile.multicastAddress),
                    this.Profile.multicastPort);

            this.udpClient = new UdpClient(this.multicastEndpoint.Port);

            this.udpClient.MulticastLoopback = false;

            this.udpClient.JoinMulticastGroup(this.multicastEndpoint.Address);

            this.ReceiveInternal();
        }
        public void Close()
        {
            this.CheckOpen();

            this.udpClient.Dispose();

            this.udpClient = null;
        }
        public void Send<T>(T message, Action<bool> callback = null) where T : Message
        {
            var bits = this.Serialize<T>(message);

            this.SendInternalAsync(bits, callback);
        }
        byte[] Serialize<T>(T message) where T : Message
        {
            var stream = new MemoryStream();

            var writer = new BinaryWriter(stream);

            try
            {
                writer.Write(MessageRegistrar.KeyFromMessageType<T>());
                message.Save(writer);
                writer.Flush();

                if (stream.Length > Constants.MAX_UDP_SIZE)
                {
                    throw new ArgumentException("Message size exceeded maximum length");
                }
            }
            finally
            {
                writer.Dispose();
            }
            return (stream.ToArray());
        }
        void CheckOpen(bool open = true)
        {
            if ((this.udpClient == null) && open)
            {
                throw new InvalidOperationException("Not open");
            }
            if ((this.udpClient != null) && !open)
            {
                throw new InvalidOperationException("Already open");
            }
        }
        void DispatchMessage(byte[] bits)
        {
            var stream = new MemoryStream(bits);
            Message message = null;

            using (var reader = new BinaryReader(stream))
            {
                var messageTypeKey = reader.ReadString();
                message = this.messageRegistrar.CreateMessage(messageTypeKey);
                message.Load(reader);

                this.messageRegistrar.InvokeHandlers(message);
            }
        }
        async void SendInternalAsync(byte[] bits, Action<bool> callback)
        {
            bool sent = false;

            try
            {
                var sendCount = await this.udpClient.SendAsync(bits, bits.Length, this.multicastEndpoint);
                Debug.Assert(sendCount == bits.Length);
                sent = true;
            }
            catch (ObjectDisposedException)
            {

            }
            catch (SocketException)
            {
            }
            if (callback != null)
            {
                callback(sent);
            }
        }
        async void ReceiveInternal()
        {
            bool failed = false;

            while (!failed)
            {
                try
                {
                    var result = await this.udpClient.ReceiveAsync();

                    if (result.Buffer != null)
                    {
                        this.DispatchMessage(result.Buffer);
                    }
                }
                catch (SocketException) // TODO: verify that this is the right exception
                {
                    failed = true;
                }
            }
        }
        UdpClient udpClient;
        MessageRegistrar messageRegistrar;
        IPEndPoint multicastEndpoint;
    }
}