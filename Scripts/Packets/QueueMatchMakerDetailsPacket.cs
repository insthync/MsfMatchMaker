using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Barebones.Networking;

public class QueueMatchMakerDetailsPacket : SerializablePacket
{
    public int SpawnId;
    public string MachineId;
    public string SpawnCode;
    public int AssignedPort;

    public override void ToBinaryWriter(EndianBinaryWriter writer)
    {
        writer.Write(SpawnId);
        writer.Write(MachineId);
        writer.Write(SpawnCode);
        writer.Write(AssignedPort);
    }

    public override void FromBinaryReader(EndianBinaryReader reader)
    {
        SpawnId = reader.ReadInt32();
        MachineId = reader.ReadString();
        SpawnCode = reader.ReadString();
        AssignedPort = reader.ReadInt32();
    }
}
