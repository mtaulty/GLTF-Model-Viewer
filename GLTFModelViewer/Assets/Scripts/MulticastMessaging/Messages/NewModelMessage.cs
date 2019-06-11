using System.IO;
using System.Net;
using System;

using MulticastMessaging;

namespace MulticastMessaging.Messages
{
    public class NewModelMessage : Message
    {
        public IPAddress ServerIPAddress { get; set; }

        public Guid ModelIdentifier { get; set; }

        public override void Load(BinaryReader reader)
        {
            this.ServerIPAddress = IPAddress.Parse(reader.ReadString());
            this.ModelIdentifier = Guid.Parse(reader.ReadString());
        }

        public override void Save(BinaryWriter writer)
        {
            writer.Write(this.ServerIPAddress.ToString());
            writer.Write(this.ModelIdentifier.ToString());
        }
    }
}