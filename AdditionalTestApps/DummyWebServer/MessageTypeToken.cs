namespace MulticastMessaging
{
    using System.IO;

    public class MessageTypeKey : Persistable
    {
        internal string Key { get; set; }

        public override void Load(BinaryReader reader)
        {
            this.Key = reader.ReadString();
        }
        public override void Save(BinaryWriter writer)
        {
            writer.Write(this.Key);
        }
    }
}
