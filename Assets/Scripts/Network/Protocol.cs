using System;
using System.Collections.Generic;
using UnityEngine;

public class Protocol
{
    public const int MaxPacketSize = 512; // We shouldn't need more for the demo ( header is included and there's no buffer resizing )

    public static readonly Dictionary<Type, ushort> PacketIds = new Dictionary<Type, ushort>();
    public static readonly Dictionary<ushort, Type> PacketType = new Dictionary<ushort, Type>();

    public static void RegisterPacket( ushort id, Type type )
    {
        PacketIds.Add( type, id );
        PacketType.Add( id, type );
    }

    static Protocol()
    {
        RegisterPacket( ReadyPacket.Id, typeof(ReadyPacket) );
        RegisterPacket( JoinPacket.Id, typeof(JoinPacket) );
        RegisterPacket( MovePacket.Id, typeof(MovePacket) );
        RegisterPacket( PaddingPacket.Id, typeof(PaddingPacket) );
        RegisterPacket( AnimationPacket.Id, typeof(AnimationPacket) );
    }
}

public interface IPacket
{
    // In reality, this isn't required for blittable structs but needed for more complex structs ( Containing strings ... ) + endianness
    void Serialize( PacketWriter writer )
    {
    }

    void Deserialize( PacketReader reader )
    {
    }
}

// TODO Remove
public struct PaddingPacket : IPacket
{
    public const ushort Id = 999;
}

public struct ReadyPacket : IPacket
{
    public const ushort Id = 0;
}


public struct JoinPacket : IPacket
{
    public const ushort Id = 1;
    public Vector3 Position;
    public Vector3 Velocity;
    public float Rotation;
    public int AnimationBooleans;
    public float AnimationSpeed;

    public void Serialize( PacketWriter writer )
    {
        writer.WriteVector3( this.Position );
        writer.WriteVector3( this.Velocity );
        writer.WriteSingle( this.Rotation );
        writer.WriteInt( this.AnimationBooleans );
        writer.WriteSingle( this.AnimationSpeed );
    }

    public void Deserialize( PacketReader reader )
    {
        this.Position = reader.ReadVector3();
        this.Velocity = reader.ReadVector3();
        this.Rotation = reader.ReadSingle();
        this.AnimationBooleans = reader.ReadInt();
        this.AnimationSpeed = reader.ReadSingle();
    }
}

public struct MovePacket : IPacket
{
    public const ushort Id = 2;
    public Vector3 Position;
    public Vector3 Velocity;
    public float Rotation;

    public void Serialize( PacketWriter writer )
    {
        writer.WriteVector3( this.Position );
        writer.WriteVector3( this.Velocity );
        writer.WriteSingle( this.Rotation );
    }

    public void Deserialize( PacketReader reader )
    {
        this.Position = reader.ReadVector3();
        this.Velocity = reader.ReadVector3();
        this.Rotation = reader.ReadSingle();
    }
}

public struct AnimationPacket : IPacket
{
    public const ushort Id = 8;
    public int AnimationBooleans;
    public float AnimationSpeed;

    public void Serialize( PacketWriter writer )
    {
        writer.WriteInt( this.AnimationBooleans );
        writer.WriteSingle( this.AnimationSpeed );
    }

    public void Deserialize( PacketReader reader )
    {
        this.AnimationBooleans = reader.ReadInt();
        this.AnimationSpeed = reader.ReadSingle();
    }
}