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

    private List<Occludee> occludees = new List<Occludee>();
    public void AddOccludee(Occludee occludee)
    {
        this.occludees.Add(occludee);
    }

    private List<Occluder> occluders = new List<Occluder>();
    public void AddOccluder(Occluder occluder)
    {
        if (occluder.ScreenArea < this.minOccluderSceenThreshold)
            return;

        this.occluders.Add(occluder);
    }

    private List<bool> visableFlag = new List<bool>();

    private void OnPreRender()
    {
        if (this.occluders.Count == 0)
            return;

        //need a min count?
        if (this.occludees.Count == 0)
            return;

        float fTime = Time.realtimeSinceStartup;

        for (int i = 0; i < this.occludees.Count; i++)
            this.visableFlag.Add(true);

        Vector3 viewPos = this.transform.position;
        Vector3 viewDir = this.transform.forward;

        this.occluders.Sort((x, y) => -x.ScreenArea.CompareTo(y.ScreenArea));
        int occluderNum = Mathf.Min(this.occluders.Count, this.maxOccluderNum);
        for(int i = 0; i < occluderNum; i++)
        {
            Occluder occluder = this.occluders[i];

            List<Plane> cullPlanes = occluder.CalculateCullPlanes(viewPos, viewDir);
            for (int occludeeIdx = 0; occludeeIdx < this.occludees.Count; occludeeIdx++)
            {
                if (this.visableFlag[occludeeIdx] == false)
                    continue;

                Bounds bounds = this.occludees[occludeeIdx].GetBounds();

                //if (this.CullAABB(cullPlanes, bounds))
                //    this.visableFlag[occludeeIdx] = false;
                this.visableFlag[occludeeIdx] = !this.CullAABB(cullPlanes, bounds);
            }
        }

        for (int i = 0; i < this.occludees.Count; i++)
        {
            if(this.visableFlag[i] == false)
                this.occludees[i].SetVisable(false);
        }

        Debug.Log("Cull cost: " + (Time.realtimeSinceStartup-fTime)*1000);
        int culledNum = 0;
        for (int i = 0; i < this.occludees.Count; i++)
        {
            if (this.visableFlag[i] == false)
                culledNum++;
        }
        Debug.Log("Total: " + this.occludees.Count);
        Debug.Log("Culled: " + culledNum);
    }

    private void OnPostRender()
    {
        for (int i = 0; i < this.occludees.Count; i++)
        {
            if (this.visableFlag[i] == false)
                this.occludees[i].SetVisable(true);
        }

        this.occludees.Clear();
        this.occluders.Clear();

        this.visableFlag.Clear();
    }

    private bool CullAABB(List<Plane> planes, Bounds bounds)
    {
        foreach(Plane plane in planes)
        {
            float distance = plane.GetDistanceToPoint(bounds.center);
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

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
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
#endif
}
