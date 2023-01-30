using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LobbyPanel : MonoBehaviour
{
    [SerializeField]
    private Text usernameText;

    [SerializeField]
    private Text roomText;

    [SerializeField]
    private Text alertText;

    public string Username => this.usernameText.text;
    public string Room => this.roomText.text;

    public void AlertMessage( string message )
    {
        this.alertText.text = message;
        this.StopAllCoroutines();
        this.StartCoroutine( this.DismissAlertMessage() );
    }

    public IEnumerator DismissAlertMessage()
    {
        yield return new WaitForSeconds( 3f );
        this.alertText.text = "";
    }

    public void TokenTest()
    {
    }
}