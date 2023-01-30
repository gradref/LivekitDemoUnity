using System;
using UnityEngine;

public class LocalPlayer : Player
{
    public delegate void CharacterMovementUpdatedHandler( Vector3 position, Vector3 velocity, float rotation );

    public CharacterMovementUpdatedHandler CharacterMovementUpdated;

    public delegate void CharacterAnimationUpdatedHandler( int booleans, float animSpeed );

    public CharacterAnimationUpdatedHandler CharacterAnimationUpdated;
    public InputStatus InputStatus = new InputStatus();

    [Tooltip( "The follow target set in the Cinemachine Virtual Camera that the camera will follow" )]
    public Transform CinemachineCameraTarget;


    private float animationBlend;
    private float verticalVelocity;
    private float cameraRotationVelocity;
    private float cameraTargetRotation;
    private float cinemachineTargetYaw;
    private float cinemachineTargetPitch;
    private float jumpTimeoutDelta;
    private double fallTimeoutDelta;
    private bool Grounded;
    private bool animationUpdated;

    protected void Start()
    {
        this.cinemachineTargetYaw = this.CinemachineCameraTarget.rotation.eulerAngles.y;
    }

    public void Update()
    {
        this.animationUpdated = false;
        this.JumpAndGravity();
        this.GroundedCheck();
        this.Move();
        this.CharacterMovementUpdated?.Invoke(
            this.transform.position, this.Controller.velocity, this.transform.rotation.eulerAngles.y );
        if ( this.animationUpdated )
        {
            int b = this.GetAnimationBooleans();
            float s = this.GetAnimationSpeed();
            this.CharacterAnimationUpdated?.Invoke( this.GetAnimationBooleans(), this.GetAnimationSpeed() );
        }
    }

    private void LateUpdate()
    {
        this.CameraRotation();
    }

    #region Player Update

    /// <summary>
    /// Handle the player movement
    /// </summary>
    private void Move()
    {
        float targetSpeed = this.InputStatus.SprintPressed ? this.Settings.SprintSpeed : this.Settings.MoveSpeed;
        if ( !this.InputStatus.Moving ) targetSpeed = 0.0f;

        // a reference to the players current horizontal velocity
        float currentHorizontalSpeed = new Vector3( this.Controller.velocity.x, 0.0f, this.Controller.velocity.z ).magnitude;

        float speedOffset = 0.1f;
        float speed = targetSpeed;
        // accelerate or decelerate to target speed
        if ( currentHorizontalSpeed < targetSpeed - speedOffset ||
             currentHorizontalSpeed > targetSpeed + speedOffset )
        {
            // creates curved result rather than a linear one giving a more organic speed change
            // note T in Lerp is clamped, so we don't need to clamp our speed
            speed = Mathf.Lerp( currentHorizontalSpeed, targetSpeed,
                Time.deltaTime * this.Settings.SpeedChangeRate );

            // round speed to 3 decimal places
            speed = Mathf.Round( speed * 1000f ) / 1000f;
        }

        this.animationBlend = Mathf.Lerp( this.animationBlend, targetSpeed, Time.deltaTime * this.Settings.SpeedChangeRate );
        if ( this.animationBlend < 0.01f ) this.animationBlend = 0f;

        // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is a move input rotate player when the player is moving
        if ( this.InputStatus.Moving )
        {
            Camera mainCamera = Camera.main;
            if ( mainCamera )
            {
                this.cameraTargetRotation = Mathf.Atan2( this.InputStatus.Movement.x, this.InputStatus.Movement.z ) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
            }

            float rotation = Mathf.SmoothDampAngle( this.transform.eulerAngles.y, this.cameraTargetRotation, ref this.cameraRotationVelocity,
                this.Settings.RotationSmoothTime );

            // rotate to face input direction relative to camera position
            this.transform.rotation = Quaternion.Euler( 0.0f, rotation, 0.0f );
        }


        Vector3 targetDirection = Quaternion.Euler( 0.0f, this.cameraTargetRotation, 0.0f ) * Vector3.forward;

        // move the player
        this.Controller.Move( targetDirection.normalized * ( speed * Time.deltaTime ) +
                              new Vector3( 0.0f, this.verticalVelocity, 0.0f ) * Time.deltaTime );

        // update animator if using character

        this.UpdateAnimation( this.animIDSpeed, this.animationBlend );
        //   this.Animator.SetFloat( this.animIDMotionSpeed, 1f );
    }

    private void JumpAndGravity()
    {
        if ( this.Grounded )
        {
            // reset the fall timeout timer
            float _fallTimeoutDelta = this.Settings.FallTimeout;

            // update animator if using character
            this.UpdateAnimation( this.animIDJump, false );
            this.UpdateAnimation( this.animIDFreeFall, false );

            // stop our velocity dropping infinitely when grounded
            if ( this.verticalVelocity < 0.0f )
            {
                this.verticalVelocity = -2f;
            }

            // Jump
            if ( this.InputStatus.JumpPressed && this.jumpTimeoutDelta <= 0.0f )
            {
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                this.verticalVelocity = Mathf.Sqrt( this.Settings.JumpHeight * -2f * this.Settings.Gravity );

                // update animator if using character
                this.UpdateAnimation( this.animIDJump, true );
            }

            // jump timeout
            if ( this.jumpTimeoutDelta >= 0.0f )
            {
                this.jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            // reset the jump timeout timer
            this.jumpTimeoutDelta = this.Settings.JumpTimeout;

            // fall timeout
            if ( this.fallTimeoutDelta >= 0.0f )
            {
                this.fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                // update animator if using character
                this.UpdateAnimation( this.animIDFreeFall, true );
            }

            // if we are not grounded, do not jump
            this.InputStatus.JumpPressed = false;
        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if ( this.verticalVelocity < this.Settings.TerminalVelocity )
        {
            this.verticalVelocity += this.Settings.Gravity * Time.deltaTime;
        }
    }

    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3( this.transform.position.x, this.transform.position.y - this.Settings.GroundedOffset, this.transform.position.z );
        this.Grounded = Physics.CheckSphere( spherePosition, this.Settings.GroundedRadius, this.Settings.GroundLayers,
            QueryTriggerInteraction.Ignore );
        // update animator if using character
        this.UpdateAnimation( this.animIDGrounded, this.Grounded );
    }

    private void CameraRotation()
    {
        // if there is an input and camera position is not fixed
        if ( this.InputStatus.Look.sqrMagnitude >= 0.01f )
        {
            this.cinemachineTargetYaw += this.InputStatus.Look.x;
            this.cinemachineTargetPitch += this.InputStatus.Look.y;
        }

        // clamp our rotations so our values are limited 360 degrees
        this.cinemachineTargetYaw = ClampAngle( this.cinemachineTargetYaw, float.MinValue, float.MaxValue );
        this.cinemachineTargetPitch = ClampAngle( this.cinemachineTargetPitch, this.Settings.BottomClamp, this.Settings.TopClamp );

        // Cinemachine will follow this target
        this.CinemachineCameraTarget.transform.rotation = Quaternion.Euler( this.cinemachineTargetPitch + this.Settings.CameraAngleOverride, this.cinemachineTargetYaw, 0.0f );
    }

    #endregion


    private void UpdateAnimation( int id, float value )
    {
        if ( !Mathf.Approximately( this.Animator.GetFloat( id ), value ) )
        {
            this.animationUpdated = true;
            this.Animator.SetFloat( id, value );
        }
    }

    private void UpdateAnimation( int id, bool value )
    {
        if ( this.Animator.GetBool( id ) != value )
        {
            this.animationUpdated = true;
            this.Animator.SetBool( id, value );
        }
    }


    private static float ClampAngle( float lfAngle, float lfMin, float lfMax )
    {
        if ( lfAngle < -360f ) lfAngle += 360f;
        if ( lfAngle > 360f ) lfAngle -= 360f;
        return Mathf.Clamp( lfAngle, lfMin, lfMax );
    }

    public void SetupPosition( Vector3 position, Vector3 velocity, float rotation )
    {
        this.transform.position = position;
        Debug.Log( "setup position done" );
    }
}