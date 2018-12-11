using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Occludee : MonoBehaviour {

    [SerializeField]
    private bool isStatic = true;

    [SerializeField]
    private bool isGroup = false;

    private List<Renderer> renderers = new List<Renderer>();

    private Bounds bounds = new Bounds();
    private int lastFrame = -1;

	void Start ()
    {
        if (this.isGroup)
        {
            Renderer[] groupRenderers = this.GetComponentsInChildren<Renderer>();
            if (groupRenderers != null)
                renderers.AddRange(groupRenderers);
        }
        else
        {
            Renderer renderer = this.GetComponent<Renderer>();
            if (renderer != null)
                renderers.Add(renderer);
        }


        if (this.isStatic)
            this.CalculateBounds();
    }

    private void OnWillRenderObject()
    {
        AntiPortalCuller culler = Camera.current.GetComponent<AntiPortalCuller>();
        if (culler != null)
        {
            Debug.Log("Occludee.OnWillRenderObject " + Time.frameCount);

            if(this.isStatic == false)
            {
                //a GameObject maybe render multiple times in one frame, just update bounds once
                if (this.lastFrame != Time.frameCount)
                {
                    this.CalculateBounds();
                    this.lastFrame = Time.frameCount;
                }
            }

            culler.AddOccludee(this);
        }
    }

    //only when this.
    private void CalculateBounds()
    {
        this.bounds.center = Vector3.zero;
        this.bounds.extents = Vector3.zero;

        foreach(Renderer renderer in this.renderers)
        {
            this.bounds.Encapsulate(renderer.bounds);
        }
    }

    public Bounds GetBounds()
    {
        return this.bounds;
    }

    public void SetVisable(bool visable)
    {
        foreach (Renderer renderer in this.renderers)
        {
            renderer.enabled = visable;
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
