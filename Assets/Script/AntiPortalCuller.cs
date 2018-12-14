using System;
using System.Collections.Generic;
using UnityEngine;

public class AntiPortalCuller : MonoBehaviour {
    
    [SerializeField]
    private int minRendererNum = 0;

    [SerializeField]
    private int maxOccluderNum = 16;

    [SerializeField]
    private float minOccluderSceenThreshold = 0.001f;

    // Use this for initialization
    void Start () {
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private int rendererNum = 0;
    private List<Occludee> occludees = new List<Occludee>();
    public void AddOccludee(Occludee occludee)
    {
        this.occludees.Add(occludee);

        this.rendererNum += occludee.GetRendererNum();
    }

    private List<Occluder> occluders = new List<Occluder>();
    public void AddOccluder(Occluder occluder)
    {
        if (occluder.ScreenArea < this.minOccluderSceenThreshold)
            return;

        this.occluders.Add(occluder);
    }

    private List<bool> occludeeVisable = new List<bool>();
    private List<bool> occluderVisable = new List<bool>();
    private void OnPreRender()
    {
        if (this.occluders.Count == 0)
            return;

        if (this.rendererNum <= this.minRendererNum)
            return;

        float fTime = Time.realtimeSinceStartup;

        for (int i = 0; i < this.occludees.Count; i++)
            this.occludeeVisable.Add(true);

        for (int i = 0; i < this.occluders.Count; i++)
            this.occluderVisable.Add(true);

        Vector3 viewPos = this.transform.position;
        Vector3 viewDir = this.transform.forward;

        this.occluders.Sort((x, y) => -x.ScreenArea.CompareTo(y.ScreenArea));
        int occluderNum = Mathf.Min(this.occluders.Count, this.maxOccluderNum);
        fTime = Time.realtimeSinceStartup;
        for (int i = 0; i < occluderNum; i++)
        {
            Occluder occluder = this.occluders[i];
            if (this.occluderVisable[i] == false)
                continue;

            List<Plane> cullPlanes = occluder.CalculateCullPlanes(viewPos, viewDir);

            for (int occludeeIdx = 0; occludeeIdx < this.occludees.Count; occludeeIdx++)
            {
                if (this.occludeeVisable[occludeeIdx] == false)
                    continue;

                Bounds bounds = this.occludees[occludeeIdx].GetBounds();

                if (this.CullAABB(cullPlanes, bounds))
                    this.occludeeVisable[occludeeIdx] = false;
            }

            for(int j = i+1; j < occluderNum; j++)
            {
                if (this.CullOccluder(cullPlanes, this.occluders[i]))
                    this.occluderVisable[j] = false;
            }
        }

        Debug.Log("Cull cost: " + (Time.realtimeSinceStartup - fTime) * 1000);

        for (int i = 0; i < this.occludees.Count; i++)
            this.occludees[i].SetVisable(this.occludeeVisable[i]);

        int culledNum = 0;
        for (int i = 0; i < this.occludees.Count; i++)
        {
            if (this.occludeeVisable[i] == false)
                culledNum++;
        }
        Debug.Log("Total: " + this.occludees.Count);
        Debug.Log("Culled: " + culledNum);
    }

    private void OnPostRender()
    {
        if (this.occludeeVisable.Count > 0)
        {
            for (int i = 0; i < this.occludees.Count; i++)
                this.occludees[i].SetVisable(true);

#if UNITY_EDITOR
            //debug mode
            GL.Clear(true, false, Color.black);
            if (this.lineMaterial == null)
                this.lineMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
            this.lineMaterial.SetPass(0);
            GL.Begin(GL.LINES);
            GL.Color(Color.red);
            for (int i = 0; i < this.occludees.Count; i++)
            {
                if (this.occludeeVisable[i] == false)
                {
                    Bounds bound = this.occludees[i].GetBounds();
                    Vector3 min = bound.min;
                    Vector3 max = bound.max;
                    //front
                    GL.Vertex3(min.x, min.y, min.z);
                    GL.Vertex3(max.x, min.y, min.z);

                    GL.Vertex3(min.x, min.y, min.z);
                    GL.Vertex3(min.x, max.y, min.z);

                    GL.Vertex3(min.x, max.y, min.z);
                    GL.Vertex3(max.x, max.y, min.z);

                    GL.Vertex3(max.x, max.y, min.z);
                    GL.Vertex3(max.x, min.y, min.z);

                    //back
                    GL.Vertex3(min.x, min.y, max.z);
                    GL.Vertex3(max.x, min.y, max.z);

                    GL.Vertex3(min.x, min.y, max.z);
                    GL.Vertex3(min.x, max.y, max.z);

                    GL.Vertex3(min.x, max.y, max.z);
                    GL.Vertex3(max.x, max.y, max.z);

                    GL.Vertex3(max.x, max.y, max.z);
                    GL.Vertex3(max.x, min.y, max.z);

                    //
                    GL.Vertex3(min.x, min.y, min.z);
                    GL.Vertex3(min.x, min.y, max.z);

                    GL.Vertex3(min.x, max.y, min.z);
                    GL.Vertex3(min.x, max.y, max.z);

                    GL.Vertex3(max.x, min.y, min.z);
                    GL.Vertex3(max.x, min.y, max.z);

                    GL.Vertex3(max.x, max.y, min.z);
                    GL.Vertex3(max.x, max.y, max.z);
                }
            }
            GL.End();

            this.occludedBounds.Clear();
            for (int i = 0; i < this.occludees.Count; i++)
            {
                if (this.occludeeVisable[i] == false)
                {
                    this.occludedBounds.Add(this.occludees[i].GetBounds());
                }
            }
#endif
        }

        this.occludees.Clear();
        this.occluders.Clear();

        this.occludeeVisable.Clear();
        this.occluderVisable.Clear();
        this.rendererNum = 0;
    }

    //http://old.cescg.org/CESCG-2002/DSykoraJJelinek/
    private bool CullAABB(List<Plane> planes, Bounds bounds)
    {
        foreach (Plane plane in planes)
        {
            float distance = plane.GetDistanceToPoint(bounds.center);
            if (distance >= 0)
                return false;

            //todo
            //precal abs normal
            Vector3 normal = plane.normal;
            normal.x = Mathf.Abs(normal.x);
            normal.y = Mathf.Abs(normal.y);
            normal.z = Mathf.Abs(normal.z);
            float radius = Vector3.Dot(bounds.extents, normal);

            if ((distance + radius) > 0)
                return false;
        }

        return true;
    }

    //List<int> normalFlags = new List<int>();
    //normalFlags.Clear();
    //foreach (Plane plane in cullPlanes)
    //{
    //    normalFlags.Add(plane.normal.x < 0 ? 0 : 1);
    //    normalFlags.Add(plane.normal.y < 0 ? 0 : 1);
    //    normalFlags.Add(plane.normal.z < 0 ? 0 : 1);
    //}
    //Vector3[] v = new Vector3[2];
    //private bool CullAABB(List<Plane> planes, List<int> normalFlags, Bounds bounds)
    //{
    //    v[0] = bounds.min;
    //    v[1] = bounds.max;
    //    int i = 0;
    //    foreach (Plane plane in planes)
    //    {
    //        Vector3 normal = plane.normal;
    //        Vector3 p = new Vector3(this.v[normalFlags[i]].x, this.v[normalFlags[i + 1]].y, this.v[normalFlags[i + 2]].z);

    //        float distance = plane.GetDistanceToPoint(p);

    //        if (distance > 0)
    //            return false;

    //        i += 3;
    //    }

    //    return true;
    //}

    private bool CullOccluder(List<Plane> planes, Occluder occluder)
    {
        return false;
    }

#if UNITY_EDITOR
    Material lineMaterial;
    private void OnDrawGizmos()
    {
        //draw select occluder
        GameObject go = UnityEditor.Selection.activeGameObject;
        if(go != null)
        {
            Occluder occluder = go.GetComponent<Occluder>();
            if (occluder != null)
            {
                Vector3 viewPos = this.transform.position;
                Vector3 viewDir = this.transform.forward;

                Vector3[] contour = occluder.getContour(viewPos, viewDir);
                Gizmos.color = Color.white;
                foreach (Vector3 v in contour)
                {
                    Gizmos.DrawRay(viewPos, (v-viewPos)*100);
                }
            }
        }
    }

    private List<Bounds> occludedBounds = new List<Bounds>();
    private void OnDrawGizmosSelected()
    {
        //Gizmos.color = Color.red;
        Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.5f);
        foreach (Bounds bound in this.occludedBounds)
        {
            //Gizmos.DrawWireCube(bound.center, bound.extents*2);
            Gizmos.DrawCube(bound.center, bound.extents * 2.001f);
        }
    }
#endif
}
