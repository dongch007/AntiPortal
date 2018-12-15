using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupedOccludee : Occludee {

    private List<Renderer> renderers = new List<Renderer>();

    void Start()
    {
        this.GetComponentsInChildren<Renderer>(this.renderers);

        if (this.isStatic)
            this.CalculateBounds();
    }

    private void OnWillRenderObject()
    {
        AntiPortalCuller culler = Camera.current.GetComponent<AntiPortalCuller>();
        if (culler != null)
        {
            if (this.isStatic == false)
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

    private void CalculateBounds()
    {
        this.bounds = this.renderers[0].bounds;

        for (int i = 1; i < this.renderers.Count; i++)
        {
            this.bounds.Encapsulate(this.renderers[i].bounds);
        }
    }

    public override void SetVisable(bool visable)
    {
        if (this.isVisable != visable)
        {
            foreach (Renderer renderer in this.renderers)
            {
                renderer.enabled = visable;
            }

            this.isVisable = visable;
        }
    }

    public override int GetRendererNum()
    {
        return this.renderers.Count;
    }

    private void OnDrawGizmosSelected()
    {
        Renderer[] groupRenderers = this.GetComponentsInChildren<Renderer>();
        if (groupRenderers != null)
        {
            Bounds bound = groupRenderers[0].bounds;
            for (int i = 1; i < groupRenderers.Length; i++)
            {
                bound.Encapsulate(groupRenderers[i].bounds);
            }

            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(bound.center, bound.size);
        }
    }
}
