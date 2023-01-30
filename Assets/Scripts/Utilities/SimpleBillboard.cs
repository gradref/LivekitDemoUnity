using UnityEngine;

public class SimpleBillboard : MonoBehaviour
{
    private void Update()
    {
        if ( Camera.main != null )
        {
            Vector3 dir = this.transform.position - Camera.main.transform.position;
            this.transform.rotation = Quaternion.LookRotation( dir.normalized, Vector3.up );
        }
    }
}