using System;
using System.Collections;
using System.Collections.Generic;
using LiveKit;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable CS0618
public class NetManager : MonoBehaviour
{
    public delegate void PacketReceivedHandler( RemoteParticipant participant, IPacket packet, DataPacketKind kind );

    public PacketReceivedHandler PacketReceived;
    public Action<string> TokenGenerated;
    public Action<Room> RoomCreated;
    public Action ConnectionEnstablished;
    public Action ConnetionFailed;

    public Room LivekitRoom { get; private set; }
    private readonly PacketReader packetReader = new PacketReader();
    private readonly PacketWriter packetWriter = new PacketWriter();
    private const string livekitUrl = "wss://lkdemo.livekit.cloud";

    protected void Awake()
    {
        Application.ExternalEval( "socket.isReady = true;" );
    }

    /// <summary>
    /// Retrive a livekit token from server
    /// </summary>
    public void GetLivekitToken( string username, string room )
    {
        Dictionary<string, string> dic = new Dictionary<string, string>()
        {
            ["name"] = username,
            ["room"] = room
        };
        Application.ExternalCall( "socket.emit", "TOKEN_REQUEST", new JSONObject( dic ) );
    }


    /// <summary>
    /// Receive Token generated from the server
    /// </summary>
    public void OnTokenGenerated( string token )
    {
        Debug.Log( "Token Received: " + token );
        this.TokenGenerated?.Invoke( token );
    }


    /// <summary>
    /// Connect to livekit cloud room
    /// </summary>
    /// <param name="token">token generated from the server</param>
    /// <returns></returns>
    public IEnumerator ConnectLivekitRoom( string token )
    {
        this.LivekitRoom = new Room();
        this.RoomCreated?.Invoke( this.LivekitRoom );
        ConnectOperation connectionOperation = this.LivekitRoom.Connect( livekitUrl, token );
        yield return connectionOperation;
        if ( !connectionOperation.IsError )
        {
            Debug.Log( "Connected" );

            this.LivekitRoom.DataReceived += this.DataReceived;
            this.ConnectionEnstablished?.Invoke();
        }
        else
        {
            Debug.Log( "Connection Failed: " + connectionOperation.Error.Message );
            this.ConnetionFailed?.Invoke();
        }
    }

    private void DataReceived( byte[] data, RemoteParticipant participant, DataPacketKind? kind )
    {
        if ( participant == null )
        {
            Debug.Log( "Received a packet coming from the Server API ? ( Ignoring ..) " );
            return;
        }

        IPacket packet = this.packetReader.UnserializePacket( data );
        if ( packet == null )
        {
            Debug.LogError( $"Failed to unserialize incoming packet from {participant.Sid}" );
            return;
        }

        if ( kind != null )
        {
            this.PacketReceived?.Invoke( participant, packet, (DataPacketKind)kind );
        }
    }

    public JSPromise SendPacket<T>( T packet, DataPacketKind kind, params RemoteParticipant[] participants ) where T : IPacket
    {
        var data = this.packetWriter.SerializePacket( packet );
        return this.LivekitRoom.LocalParticipant.PublishData( data.Array, data.Offset, data.Count, kind, participants );
    }
}