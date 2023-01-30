using System;
using UnityEngine;
using Random = UnityEngine.Random;


public class Player : MonoBehaviour
{
    public CharacterController Controller;
    public Animator Animator;
    public PlayerSettings Settings;
    protected int animIDSpeed;

    protected enum PlayerAnimationsBooleans
    {
        Grounded,
        Jump,
        FreeFall,
    }

    protected int[] animationsIds = new int[3];

    protected virtual void Awake()
    {
        this.AssignAnimationIDs();
    }

    private void OnFootstep( AnimationEvent animationEvent )
    {
        if ( animationEvent.animatorClipInfo.weight > 0.5f )
        {
            if ( this.Settings.FootstepAudioClips.Length > 0 )
            {
                var index = Random.Range( 0, this.Settings.FootstepAudioClips.Length );
                AudioSource.PlayClipAtPoint( this.Settings.FootstepAudioClips[index], this.transform.TransformPoint( this.Controller.center ), this.Settings.FootstepAudioVolume );
            }
        }
    }

    private void OnLand( AnimationEvent animationEvent )
    {
        if ( animationEvent.animatorClipInfo.weight > 0.5f )
        {
            AudioSource.PlayClipAtPoint( this.Settings.LandingAudioClip, this.transform.TransformPoint( this.Controller.center ), this.Settings.FootstepAudioVolume );
        }
    }


    private void AssignAnimationIDs()
    {
        this.animIDSpeed = Animator.StringToHash( "Speed" );
        this.animationsIds[(int)PlayerAnimationsBooleans.Grounded] = Animator.StringToHash( "Grounded" );
        this.animationsIds[(int)PlayerAnimationsBooleans.Jump] = Animator.StringToHash( "Jump" );
        this.animationsIds[(int)PlayerAnimationsBooleans.FreeFall] = Animator.StringToHash( "FreeFall" );
        //  this.animIDMotionSpeed = Animator.StringToHash( "MotionSpeed" );
    }

    public int GetAnimationBooleans()
    {
        int flags = 0;
        for ( int i = 0; i < this.animationsIds.Length; i++ )
        {
            if ( this.Animator.GetBool( this.animationsIds[i] ) )
                flags |= ( 1 << i );
        }

        return flags;
    }

    public void SetAnimationBooleans( int booleans )
    {
        for ( int i = 0; i < this.animationsIds.Length; i++ )
        {
            bool status = ( ( booleans >> i ) & 1 ) == 1;
            this.Animator.SetBool( this.animationsIds[i], status );
        }
    }

    public void SetAnimationSpeed( float speed )
    {
        this.Animator.SetFloat( this.animIDSpeed, speed );
    }

    public float GetAnimationSpeed()
    {
        return this.Animator.GetFloat( this.animIDSpeed );
    }

    public Vector3 GetPosition()
    {
        return this.transform.position;
    }
}