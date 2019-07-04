using MulticastMessaging;
using System;
using System.IO;
using UnityEngine;

namespace MulticastMessaging.Messages
{
    public class TransformChangeMessage : Message
    {
        public Guid ModelIdentifier { get; set; }
        public Vector3 Scale { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Translation { get; set; }

        public override void Load(BinaryReader reader)
        {
            this.ModelIdentifier = Guid.Parse(reader.ReadString());
            this.Scale = ReadVector3(reader);
            this.Rotation = ReadQuaternion(reader);
            this.Translation = ReadVector3(reader);
        }
        public override void Save(BinaryWriter writer)
        {
            writer.Write(this.ModelIdentifier.ToString());
            WriteVector3(writer, this.Scale);
            WriteQuaternion(writer, this.Rotation);
            WriteVector3(writer, this.Translation);
        }
        static Quaternion ReadQuaternion(BinaryReader reader)
        {
            var quaternion = new Quaternion();
            quaternion.x = reader.ReadSingle();
            quaternion.y = reader.ReadSingle();
            quaternion.z = reader.ReadSingle();
            quaternion.w = reader.ReadSingle();
            return (quaternion);
        }
        static void WriteQuaternion(BinaryWriter writer, Quaternion quaternion)
        {
            writer.Write(quaternion.x);
            writer.Write(quaternion.y);
            writer.Write(quaternion.z);
            writer.Write(quaternion.w);
        }
        static void WriteVector3(BinaryWriter writer, Vector3 vector)
        {
            writer.Write(vector.x);
            writer.Write(vector.y);
            writer.Write(vector.z);
        }
        static Vector3 ReadVector3(BinaryReader reader)
        {
            var vector = new Vector3();
            vector.x = reader.ReadSingle();
            vector.y = reader.ReadSingle();
            vector.z = reader.ReadSingle();
            return (vector);
        }
    }
}
