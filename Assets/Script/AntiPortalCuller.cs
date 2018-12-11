using System;
using System.Collections.Generic;
using UnityEngine;

public class AntiPortalCuller : MonoBehaviour {

    [SerializeField]
    private int minRendererNum = 0;

    [SerializeField]
    private int maxOccluderNum = 16;

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




        for (int i = 0; i < this.occludees.Count; i++)
            this.visableFlag[i] = false;


        for (int i = 0; i < this.occludees.Count; i++)
        {
            this.occludees[i].SetVisable(this.visableFlag[i]);
        }

        //this.occludees.Clear();
        //this.occluders.Clear();
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
}
