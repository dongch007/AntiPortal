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
        Debug.Log("OnPreRender " + Time.frameCount);

        if (this.occluders.Count == 0)
            return;

        //need a min count?
        if (this.occludees.Count == 0)
            return;

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
                Bounds bounds = this.occludees[occludeeIdx].GetBounds();
                int num = 0;
                foreach (Plane plane in cullPlanes)
                {
                    if (this.TestPlaneAABBOutside(plane, bounds) == false)
                        break;

                    num++;
                }

                if(num == cullPlanes.Count)
                    this.visableFlag[occludeeIdx] = false;
            }
        }


        for (int i = 0; i < this.occludees.Count; i++)
        {
            this.occludees[i].SetVisable(this.visableFlag[i]);
        }
    }

    private void OnPostRender()
    {
        Debug.Log("OnPostRender " + Time.frameCount);


        foreach (Occludee occludee in this.occludees)
            occludee.SetVisable(true);

        this.occludees.Clear();
        this.occluders.Clear();

        this.visableFlag.Clear();
    }

    private bool TestPlaneAABBOutside(Plane plane, Bounds bounds)
    {
        //float distance = plane.GetDistanceToPoint(bounds.center);
        //float radius = Vector3.Dot(bounds.extents, plane.normal);

        //if ((distance + radius) < 0)
        //    return true;

        //return false; 

        return true;
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {

    }
#endif
}
