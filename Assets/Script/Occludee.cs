using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Occludee : MonoBehaviour {

    [SerializeField]
    private bool isStatic = true;

    [SerializeField]
    private bool isGroup = false;

    private List<Renderer> renderers = new List<Renderer>();

    private Bounds bounds;
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
        this.bounds = this.renderers[0].bounds;

        for(int i = 1; i < this.renderers.Count; i++)
        {
            this.bounds.Encapsulate(this.renderers[i].bounds);
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
            //renderer.material.color = visable ? Color.white : Color.red;
        }
    }

    public int GetRendererNum()
    {
        return this.renderers.Count;
    }

    private void OnDrawGizmosSelected()
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
