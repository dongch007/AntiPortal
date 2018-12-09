using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiPortalCuller : MonoBehaviour {

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

    private List<Renderer> occludedRenderers = new List<Renderer>();

    private void OnPreRender()
    {
        Debug.Log("OnPreRender " + Time.frameCount);

        if (this.occluders.Count == 0)
            return;

        //need a min count?
        if (this.occludees.Count == 0)
            return;

        foreach(Occludee occludee in this.occludees)
        {
            this.occludedRenderers.Add(occludee.GetRenderer());
        }


        foreach (Renderer renderer in this.occludedRenderers)
        {
            renderer.material.color = Color.red;
            //renderer.enabled = false;
        }

        //this.occludees.Clear();
        //this.occluders.Clear();
    }

    private void OnPostRender()
    {
        Debug.Log("OnPostRender " + Time.frameCount);


        foreach (Renderer renderer in this.occludedRenderers)
        {
            renderer.material.color = Color.white;
            //renderer.enabled = true;
        }

        occludedRenderers.Clear();

        this.occludees.Clear();
        this.occluders.Clear();
    }
}
