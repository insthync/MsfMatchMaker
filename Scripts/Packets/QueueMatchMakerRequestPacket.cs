using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Barebones.Networking;

public class QueueMatchMakerRequestPacket : SerializablePacket
{
    public Dictionary<string, string> properties;

    public override void ToBinaryWriter(EndianBinaryWriter writer)
    {
        writer.Write(properties);
    }

    public override void FromBinaryReader(EndianBinaryReader reader)
    {
        properties = reader.ReadDictionary();
    }
}
