using System;
using UnityEngine;
using Random = UnityEngine.Random;


public class Player : MonoBehaviour
{
    public CharacterController Controller;
    public Animator Animator;
    public PlayerSettings Settings;

    protected int animIDSpeed;
    protected int animIDGrounded;
    protected int animIDJump;
    protected int animIDFreeFall;

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
            AudioSource.PlayClipAtPoint( this.Settings.LandingAudioClip, this.transform.TransformPoint( Controller.center ), this.Settings.FootstepAudioVolume );
        }
    }

    [Flags]
    public enum AnimationsFlags
    {
        None = 0,
        Grounded = 1,
        Jump = 2,
        FreeFall = 4,
    }

    private void AssignAnimationIDs()
    {
        this.animIDSpeed = Animator.StringToHash( "Speed" );
        this.animIDGrounded = Animator.StringToHash( "Grounded" );
        this.animIDJump = Animator.StringToHash( "Jump" );
        this.animIDFreeFall = Animator.StringToHash( "FreeFall" );
        //  this.animIDMotionSpeed = Animator.StringToHash( "MotionSpeed" );
    }

    public int GetAnimationBooleans()
    {
        AnimationsFlags flags = 0;
        flags |= this.Animator.GetBool( this.animIDGrounded ) ? AnimationsFlags.Grounded : AnimationsFlags.None;
        flags |= this.Animator.GetBool( this.animIDJump ) ? AnimationsFlags.Jump : AnimationsFlags.None;
        flags |= this.Animator.GetBool( this.animIDFreeFall ) ? AnimationsFlags.FreeFall : AnimationsFlags.None;
        return (int)flags;
    }

    public void SetAnimationBooleans( int booleans )
    {
        AnimationsFlags flags = (AnimationsFlags)booleans;
        this.Animator.SetBool( this.animIDGrounded, ( flags & AnimationsFlags.Grounded ) != AnimationsFlags.None );
        this.Animator.SetBool( this.animIDJump, ( flags & AnimationsFlags.Jump ) != AnimationsFlags.None );
        this.Animator.SetBool( this.animIDFreeFall, ( flags & AnimationsFlags.FreeFall ) != AnimationsFlags.None );
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