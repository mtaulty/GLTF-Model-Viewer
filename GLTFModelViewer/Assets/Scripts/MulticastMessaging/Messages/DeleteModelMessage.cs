using System.IO;
using System.Net;
using System;
using MulticastMessaging;

namespace MulticastMessaging.Messages
{
    public class DeleteModelMessage : Message
    {
        public Guid ModelIdentifier { get; set; }

        public override void Load(BinaryReader reader)
        {
            this.ModelIdentifier = Guid.Parse(reader.ReadString());
        }

        public override void Save(BinaryWriter writer)
        {
            writer.Write(this.ModelIdentifier.ToString());
        }
    }
}