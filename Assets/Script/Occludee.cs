using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Occludee : MonoBehaviour {

    private Renderer rend;
	void Start ()
    {
        rend = this.GetComponent<Renderer>();
    }

    public Renderer GetRenderer()
    {
        return this.rend;
    }

    private void OnWillRenderObject()
    {
        AntiPortalCuller culler = Camera.current.GetComponent<AntiPortalCuller>();
        if (culler != null)
        {
            Debug.Log("Occludee.OnWillRenderObject " + Time.frameCount);

            culler.AddOccludee(this);
        }
    }

    private void OnDrawGizmos()
    {
        Renderer renderer = this.GetComponent<Renderer>();
        if (renderer != null)
        {
            Bounds bound = renderer.bounds;
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(bound.center, bound.size);
        }
    }
}
