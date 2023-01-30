using System;
using TMPro;
using UnityEngine;

public class RemotePlayer : Player
{
    [SerializeField]
    private TextMeshProUGUI usernameText;

    [SerializeField]
    private GameObject microphoneIcon;

    public Action<RemotePlayer> OnRemovePlayer;

    public void SetUsername( string username )
    {
        this.usernameText.text = username;
    }

    private Vector3 controllerVelocity;


    /// <summary>
    /// Update position controllerVelocity and rotation of the remote player
    /// </summary>
    public void Move( Vector3 position, Vector3 velocity, float rotation )
    {
        this.transform.position = position;
        this.controllerVelocity = velocity;
        this.transform.rotation = Quaternion.Euler( 0f, rotation, 0f );
    }

    public void Update()
    {
        this.Controller.SimpleMove( this.controllerVelocity + Vector3.down * this.Settings.Gravity * Time.deltaTime );
    }

    public void RemovePlayer()
    {
        this.OnRemovePlayer( this );
    }

    public void ChangeAnimation( int flags, float animationSpeed )
    {
        this.SetAnimationBooleans( flags );
        this.SetAnimationSpeed( animationSpeed );
    }

    public void SpeakingChanged( bool speaking )
    {
        this.microphoneIcon.SetActive( speaking );
    }
}