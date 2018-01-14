using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Barebones.Networking;

public class QueueMatchMakerStartPacket : SerializablePacket
{
    public string gameModeName;
    public Dictionary<string, string> properties = new Dictionary<string, string>();

    public override void ToBinaryWriter(EndianBinaryWriter writer)
    {
        writer.Write(gameModeName);
        writer.Write(properties);
    }

    public override void FromBinaryReader(EndianBinaryReader reader)
    {
        gameModeName = reader.ReadString();
        properties = reader.ReadDictionary();
    }
}
