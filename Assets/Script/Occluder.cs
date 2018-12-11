using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Occluder : MonoBehaviour {

    public Vector3 center = Vector3.zero;
    public Vector3 extends = Vector3.one;

    void Start()
    {

    }

    private void OnWillRenderObject()
    {
        Camera camera = Camera.current;
        AntiPortalCuller culler = camera.GetComponent<AntiPortalCuller>();
        if (culler != null)
        {
            Debug.Log("Occluder.OnWillRenderObject " + Time.frameCount);

            culler.AddOccluder(this);
        }
    }

    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.red;
        //Gizmos.DrawWireMesh(AntiPortalHelper.GetCube(), this.transform.position, this.transform.rotation, Vector3.Scale(this.extends, this.transform.localScale)*2.0f);
        Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.5f);
        Gizmos.DrawMesh(AntiPortalHelper.GetCube(), this.transform.position, this.transform.rotation, Vector3.Scale(this.extends, this.transform.localScale) * 2.0f);
        //Gizmos.color = Color.red;
        //Gizmos.DrawWireMesh(AntiPortalHelper.GetCube(), this.transform.position, this.transform.rotation, Vector3.Scale(this.extends, this.transform.localScale) * 2.0f);
    }
}
