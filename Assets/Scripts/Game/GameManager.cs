using System.Collections;
using System.Collections.Generic;
using LiveKit;
using UnityEngine;
using UnityEngine.Timeline;

public class GameManager : MonoBehaviour
{
    [Tooltip( "Distance at which other players can be heard" )]
    public float HearDistance = 15f;

    public LobbyPanel LobbyPanel;
    public GameUI GameUI;

    [Tooltip( "Player random spawn points" )]
    public List<Transform> SpawnPoints = new List<Transform>();

    public GameObject RemotePlayerPrefab;
    public GameObject LocalPlayerPrefab;
    public NetManager NetManager;

    private GameObjectPool<RemotePlayer> playersPool;
    private LocalPlayer localPlayer;
    private readonly Dictionary<string, NetPlayer> networkPlayers = new Dictionary<string, NetPlayer>();


    //public 
    private void Start()
    {
        DontDestroyOnLoad( this );
        Panels.SetActivePanel( this.LobbyPanel.gameObject );
        this.NetManager.TokenGenerated += this.ConnectToLivekitRoom;
        this.NetManager.ConnectionEnstablished += this.ConnectionEnstablished;
        this.NetManager.RoomCreated += this.RoomCreated;
        this.NetManager.ConnetionFailed += this.ConnectionFailed;
        this.NetManager.PacketReceived += this.PacketReceived;
        this.playersPool = new GameObjectPool<RemotePlayer>( this.RemotePlayerPrefab, 2 );
    }

    private void FixedUpdate()
    {
        foreach ( KeyValuePair<string, NetPlayer> netPlayer in this.networkPlayers )
        {
            Participant participant = netPlayer.Value.Participant;
            if ( participant == null || participant is LocalParticipant )
                continue;

            RemoteAudioTrack track = participant.GetTrack( TrackSource.Microphone )?.Track as RemoteAudioTrack;
            if ( track == null )
                continue;

            float dist = Vector3.Distance( Camera.main.transform.position, netPlayer.Value.GetPosition() );
            float volume = 1f - Mathf.Clamp( dist / this.HearDistance, 0f, 1f );
            track.SetVolume( volume );
        }
    }

    public void OnJoinButtonPressed()
    {
        string username = this.LobbyPanel.Username;
        string room = this.LobbyPanel.Room;
        if ( !string.IsNullOrEmpty( username ) && !string.IsNullOrEmpty( username ) )
        {
            this.NetManager.GetLivekitToken( this.LobbyPanel.Username, this.LobbyPanel.Room );
        }
    }

    private void RoomCreated( Room room )
    {
        room.ParticipantConnected += participant => { Debug.Log( $"Participant connected : {participant.Sid}" ); };

        room.ParticipantDisconnected += participant =>
        {
            if ( this.networkPlayers.TryGetValue( participant.Sid, out NetPlayer player ) )
            {
                player.Disconnected.Invoke();
                this.networkPlayers.Remove( participant.Sid );
            }
        };

        room.TrackSubscribed += ( track, publication, participant ) =>
        {
            if ( track.Kind == TrackKind.Audio )
                track.Attach();
        };

        room.TrackUnsubscribed += ( track, publication, participant ) =>
        {
            if ( track.Kind == TrackKind.Audio )
                track.Detach();
        };
        this.LobbyPanel.AlertMessage( "Connected to Livekit room" );
    }


    private void ConnectToLivekitRoom( string token )
    {
        this.StartCoroutine( this.NetManager.ConnectLivekitRoom( token ) );
    }

    private void ConnectionEnstablished()
    {
        this.StartCoroutine( this.Join() );
    }

    private IEnumerator Join()
    {
        //Dispatch ready
        Debug.Log( "Dispatch Ready" );
        yield return this.NetManager.SendPacket( new ReadyPacket(), DataPacketKind.RELIABLE );
        yield return this.SetupAudioTrack();
        this.JoinGame();
    }

    private void JoinGame()
    {
        this.GenerateLocalPlayer();
        Panels.SetActivePanel( this.GameUI.gameObject );
        if ( this.localPlayer )
        {
            //Dispatch join game
            Debug.Log( "Dispatch Join" );
            this.NetManager.SendPacket( this.GetJoinPacket( this.localPlayer ), DataPacketKind.RELIABLE );
        }
    }

    private IEnumerator SetupAudioTrack()
    {
        JSPromise<LocalAudioTrack> audioTrack = Client.CreateLocalAudioTrack( new AudioCaptureOptions()
        {
            EchoCancellation = true,
            NoiseSuppression = new ConstrainBoolean() { Ideal = true }
        } );
        yield return audioTrack;
        LocalParticipant localPartecipant = this.NetManager.LivekitRoom.LocalParticipant;
        yield return localPartecipant.PublishTrack( audioTrack.ResolveValue );
        yield return localPartecipant.SetMicrophoneEnabled( true );
    }

    private void ConnectionFailed()
    {
        this.LobbyPanel.AlertMessage( "Failed to connect Livekit" );
    }

    private void GenerateLocalPlayer()
    {
        Transform target = this.SpawnPoints[Random.Range( 0, this.SpawnPoints.Count )];
        GameObject localPlayerInstance = Instantiate( this.LocalPlayerPrefab );
        if ( localPlayerInstance != null )
        {
            this.localPlayer = localPlayerInstance.GetComponent<LocalPlayer>();
            if ( this.localPlayer != null )
            {
                this.localPlayer.CharacterMovementUpdated = this.UpdateLocalPlayerPositionAndSpeed;
                this.localPlayer.CharacterAnimationUpdated = this.UpdateLocalPlayerAnimation;
                this.localPlayer.SetupPosition( target.position, Vector3.zero, target.rotation.eulerAngles.y );
            }
        }
    }

    private void UpdateLocalPlayerAnimation( int animationBoleans, float animationSpeed )
    {
        AnimationPacket packet = new AnimationPacket()
        {
            AnimationBooleans = animationBoleans,
            AnimationSpeed = animationSpeed
        };
        this.NetManager.SendPacket( packet, DataPacketKind.LOSSY );
    }

    private void UpdateLocalPlayerPositionAndSpeed( Vector3 position, Vector3 velocity, float rotation )
    {
        MovePacket packet = new MovePacket()
        {
            Position = position,
            Rotation = rotation,
            Velocity = velocity
        };
        this.NetManager.SendPacket( packet, DataPacketKind.LOSSY );
    }


    private void AddRemotePlayer( Participant participant, JoinPacket packet )
    {
        //Get a remote player from pool and setup movement and animations
        RemotePlayer remotePlayer = this.playersPool.Get( true );
        remotePlayer.SetUsername( participant.Identity );
        remotePlayer.OnRemovePlayer += this.RecyclePlayer;
        remotePlayer.SetAnimationBooleans( packet.AnimationBooleans );
        remotePlayer.SetAnimationSpeed( packet.AnimationSpeed );
        remotePlayer.Move( packet.Position, packet.Velocity, packet.Rotation );

        //Bind network event to a remote player
        NetPlayer netPlayer = new NetPlayer() { Participant = participant };
        this.networkPlayers.Add( participant.Sid, netPlayer );
        netPlayer.Move = remotePlayer.Move;
        netPlayer.Disconnected = remotePlayer.RemovePlayer;
        netPlayer.AnimationChange = remotePlayer.ChangeAnimation;
        netPlayer.GetPosition = remotePlayer.GetPosition;
        participant.IsSpeakingChanged += remotePlayer.SpeakingChanged;
    }

    private void RecyclePlayer( RemotePlayer remotePlayer )
    {
        this.playersPool.Recycle( remotePlayer );
    }

    private void PacketReceived( RemoteParticipant participant, IPacket ipacket, DataPacketKind kind )
    {
        NetPlayer netPlayer;
        switch ( ipacket )
        {
            //An host is ready in the room -> spawn local player to host device
            case ReadyPacket:
                Debug.Log( $"{participant.Sid} is ready" );
                if ( this.localPlayer != null )
                {
                    this.NetManager.SendPacket( this.GetJoinPacket( this.localPlayer ), DataPacketKind.RELIABLE, participant );
                }

                break;
            //An host joined the game -> spawn host player to local device
            case JoinPacket packet:
                Debug.Log( $"{participant.Sid} is joined the game" );
                this.AddRemotePlayer( participant, packet );
                break;
            //An host player movement -> update host player to local device
            case MovePacket packet:
                if ( this.networkPlayers.TryGetValue( participant.Sid, out netPlayer ) )
                {
                    netPlayer.Move( packet.Position, packet.Velocity, packet.Rotation );
                }

                break;
            //An host player animation -> update host player to local device
            case AnimationPacket packet:
                if ( this.networkPlayers.TryGetValue( participant.Sid, out netPlayer ) )
                {
                    netPlayer.AnimationChange( packet.AnimationBooleans, packet.AnimationSpeed );
                }

                break;
        }
    }

    private JoinPacket GetJoinPacket( Player player )
    {
        return new JoinPacket()
        {
            Velocity = player.Controller.velocity,
            Rotation = player.transform.rotation.eulerAngles.y,
            Position = player.transform.position,
            AnimationBooleans = player.GetAnimationBooleans(),
            AnimationSpeed = player.GetAnimationSpeed()
        };
    }

    private void OnDestroy()
    {
        if ( this.localPlayer )
        {
            this.NetManager.LivekitRoom.LocalParticipant.SetMicrophoneEnabled( false );
            // this.NetManager.LivekitRoom.LocalParticipant.UnpublishTrack(  );
            DestroyImmediate( this.localPlayer );
            this.localPlayer = null;
        }

        this.NetManager.TokenGenerated -= this.ConnectToLivekitRoom;
        this.NetManager.ConnectionEnstablished -= this.ConnectionEnstablished;
        this.NetManager.RoomCreated -= this.RoomCreated;
        this.NetManager.ConnetionFailed -= this.ConnectionFailed;
        this.NetManager.PacketReceived -= this.PacketReceived;
    }
}