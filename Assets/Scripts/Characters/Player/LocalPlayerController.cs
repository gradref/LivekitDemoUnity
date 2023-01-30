using UnityEngine;

[RequireComponent( typeof(LocalPlayer) )]
public class LocalPlayerController : MonoBehaviour
{
    private InputStatus inputStatus;

    private void Awake()
    {
        this.inputStatus = this.GetComponent<LocalPlayer>().InputStatus;
    }

    private void Update()
    {
        float h = Input.GetAxisRaw( "Horizontal" );
        float v = Input.GetAxisRaw( "Vertical" );
        this.inputStatus.Movement = new Vector3( h, 0f, v );
        if ( this.inputStatus.Movement != Vector3.zero )
        {
            this.inputStatus.Movement.Normalize();
            this.inputStatus.Moving = true;
        }
        else
        {
            this.inputStatus.Moving = false;
        }

        this.inputStatus.JumpPressed = Input.GetKeyDown( KeyCode.Space );
        this.inputStatus.SprintPressed = Input.GetKey( KeyCode.LeftShift );
        if ( Input.GetMouseButton( 0 ) )
        {
            this.ControlCamera( true );
        }

        if ( Input.GetMouseButton( 1 ) || Input.GetKeyDown( KeyCode.Escape ) )
        {
            this.ControlCamera( false );
        }

        if ( Application.isFocused && !Cursor.visible )
        {
            h = Input.GetAxis( "Mouse X" );
            v = Input.GetAxis( "Mouse Y" );
            this.inputStatus.Look = new Vector2( h, -v );
        }
        else
        {
            this.inputStatus.Look = Vector2.zero;
        }
    }

    private void ControlCamera( bool control )
    {
        if ( control )
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
    }
}