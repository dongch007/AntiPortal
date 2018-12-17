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
    private float setVisableTime = 0.0f;
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

        this.occluders.Sort((x, y) => -x.ScreenArea.CompareTo(y.ScreenArea));
        int occluderNum = Mathf.Min(this.occluders.Count, this.maxOccluderNum);
        fTime = Time.realtimeSinceStartup;
        for (int i = 0; i < occluderNum; i++)
        {
            Occluder occluder = this.occluders[i];
            if (this.occluderVisable[i] == false)
                continue;

            List<Plane> cullPlanes = occluder.CalculateCullPlanes(viewPos);

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
                if (this.occluderVisable[j] == false)
                    continue;

                if (this.CullOccluder(cullPlanes, this.occluders[j]))
                    this.occluderVisable[j] = false;
            }
        }

        Debug.Log("Cull cost: " + (Time.realtimeSinceStartup - fTime) * 1000);

        setVisableTime = 0.0f;
        fTime = Time.realtimeSinceStartup;
        for (int i = 0; i < this.occludees.Count; i++)
            this.occludees[i].SetVisable(this.occludeeVisable[i]);
        setVisableTime += Time.realtimeSinceStartup - fTime;
    }

    private void OnPostRender()
    {
        if (this.occludeeVisable.Count > 0)
        {
#if UNITY_EDITOR
            //debug mode
            GL.Clear(true, false, Color.black);
            if (this.lineMaterial == null)
                this.lineMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
            this.lineMaterial.SetPass(0);
            GL.Begin(GL.LINES);
            //Draw culled occludee
            GL.Color(Color.red);
            for (int i = 0; i < this.occludees.Count; i++)
            {
                if (this.occludeeVisable[i] == false)
                {
                    Bounds bound = this.occludees[i].GetBounds();
                    Vector3[] wire = AntiPortalHelper.GetBoundsWire(bound);
                    foreach (Vector3 v in wire)
                    {
                        GL.Vertex(v);
                    }
                }
            }

            //Draw culled occluder
            GL.Color(Color.blue);
            for (int i = 0; i < this.occluders.Count; i++)
            {
                if (this.occluderVisable[i] == false)
                {
                    Vector3[] wire = this.occluders[i].GetWire();
                    foreach(Vector3 v in wire)
                    {
                        GL.Vertex(v);
                    }
                }
            }

            GL.End();

            //draw culled occludee in editor view
            this.occludedBounds.Clear();
            for (int i = 0; i < this.occludees.Count; i++)
            {
                if (this.occludeeVisable[i] == false)
                {
                    this.occludedBounds.Add(this.occludees[i].GetBounds());
                }
            }
#endif

            float fTime = Time.realtimeSinceStartup;
            for (int i = 0; i < this.occludees.Count; i++)
                this.occludees[i].SetVisable(true);
            setVisableTime += Time.realtimeSinceStartup - fTime;
            Debug.Log("Visable cost: " + setVisableTime * 1000);

            int culledNum = 0;
            for (int i = 0; i < this.occludees.Count; i++)
            {
                if (this.occludeeVisable[i] == false)
                    culledNum++;
            }
            Debug.Log("Total: " + this.occludees.Count);
            Debug.Log("Culled: " + culledNum);
        }


        this.occludees.Clear();
        this.occluders.Clear();

        this.occludeeVisable.Clear();
        this.occluderVisable.Clear();
        this.rendererNum = 0;
    }

    //http://old.cescg.org/CESCG-2002/DSykoraJJelinek/
    //todo SIMD https://fgiesen.wordpress.com/2010/10/17/view-frustum-culling/
    private bool CullAABB(List<Plane> planes, Bounds bounds)
    {
        foreach (Plane plane in planes)
        {
            float distance = plane.GetDistanceToPoint(bounds.center);
            if (distance >= 0)
                return false;

            //todo
            //precal abs(normal)
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

    //todo
    //<<Real-time collection detection>> TestOBBPlane
    //private bool CullOccluder(List<Plane> planes, Occluder occluder)
    //{
    //    Vector3 center = occluder.transform.TransformPoint(occluder.center);
    //    //Vector3 extents = occluder.transform.TransformDirection(occluder.extents);
    //    Vector3 extents = occluder.extents;
    //    foreach (Plane plane in planes)
    //    {
    //        float distance = plane.GetDistanceToPoint(center);
    //        if (distance >= 0)
    //            return false;

    //        Vector3 normal = occluder.transform.TransformDirection(plane.normal);
    //        normal.x = Mathf.Abs(normal.x);
    //        normal.y = Mathf.Abs(normal.y);
    //        normal.z = Mathf.Abs(normal.z);
    //        float radius = Vector3.Dot(extents, normal);
    //        //Vector3 normal = plane.normal;
    //        //normal.x = Mathf.Abs(normal.x);
    //        //normal.y = Mathf.Abs(normal.y);
    //        //normal.z = Mathf.Abs(normal.z);
    //        //float radius = Vector3.Dot(extents, normal);

    //        if ((distance + radius) > 0)
    //            return false;
    //    }

    //    return true;
    //}

    private bool CullOccluder(List<Plane> planes, Occluder occluder)
    {
        Vector3[] corners = occluder.GetCorners();

        foreach (Plane plane in planes)
        {
            foreach (Vector3 corner in corners)
            {
                if (plane.GetDistanceToPoint(corner) > 0)
                    return false;
            }
        }

        return true;
    }

#if UNITY_EDITOR
    Material lineMaterial;
    private void OnDrawGizmos()
    {
        if (Application.isPlaying == false)
            return;

        //draw select occluder
        GameObject go = UnityEditor.Selection.activeGameObject;
        if(go != null)
        {
            Occluder occluder = go.GetComponent<Occluder>();
            if (occluder != null)
            {
                Vector3 viewPos = this.transform.position;

                Vector3[] contour = occluder.GetContour(viewPos);
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
        //sizeof()
    }
#endif
}
