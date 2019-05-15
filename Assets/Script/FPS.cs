using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPS : MonoBehaviour {

    public float updateInterval = 0.5F;
    private double lastInterval;
    private int frames = 0;
    private float fps;
    void Start()
    {
        lastInterval = Time.realtimeSinceStartup;
        frames = 0;
    }
    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 26;
        GUI.Label(new Rect(0, 0, 100, 100), " " + fps.ToString("f2"), style);

        if (GUI.Button(new Rect(0, 100, 100, 100), ""))
        {
            AntiPortalCuller culler = Camera.main.GetComponent<AntiPortalCuller>();
            culler.enabled = !culler.enabled;

            Camera.main.useOcclusionCulling = !culler.enabled;
        }

        //if (GUI.Button(new Rect(0, 100, 100, 100), ""))
        //{
        //    Bounds[] b = new Bounds[10];
        //    bool[] v = new bool[10];
        //    NativeCull.Cull(b, v);
        //}
    }
    void Update()
    {
        ++frames;
        float timeNow = Time.realtimeSinceStartup;
        if (timeNow > lastInterval + updateInterval)
        {
            fps = (float)(frames / (timeNow - lastInterval));
            frames = 0;
            lastInterval = timeNow;
        }
    }
}
