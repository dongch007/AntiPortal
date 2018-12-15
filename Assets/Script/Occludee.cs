using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Occludee : MonoBehaviour {

    [SerializeField]
    protected bool isStatic = true;

    private new Renderer renderer;

    protected Bounds bounds;
    protected int lastFrame = -1;

    protected bool isVisable = true;

    void Start ()
    {
        this.renderer = this.GetComponent<Renderer>();

        if (this.isStatic)
            this.bounds = this.renderer.bounds;
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
                    this.bounds = this.renderer.bounds;
                    this.lastFrame = Time.frameCount;
                }
            }

            culler.AddOccludee(this);
        }
    }

    public Bounds GetBounds()
    {
        return this.bounds;
    }

    public virtual void SetVisable(bool visable)
    {
        if(this.isVisable != visable)
        {
            this.renderer.enabled = visable;
            this.isVisable = visable;
        }
    }

    public bool IsVisable()
    {
        return this.isVisable;
    }

    public virtual int GetRendererNum()
    {
        return 1;
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
