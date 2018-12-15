using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiPortalHelper
{
    private static Mesh cube = null;
    public static Mesh GetCube()
    {
        if(AntiPortalHelper.cube == null)
        {
            GameObject cubeGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cubeGo.hideFlags = HideFlags.HideAndDontSave;
            cubeGo.SetActive(false);
            cube = cubeGo.GetComponent<MeshFilter>().sharedMesh;
        }

        return AntiPortalHelper.cube;
    }


    public static Vector3[] GetBoundsWire(Bounds bounds)
    {
        Vector3[] wire = new Vector3[24];

        Vector3 min = bounds.min;
        Vector3 max = bounds.max;
        //front
        wire[0] = new Vector3(min.x, min.y, min.z);
        wire[1] = new Vector3(max.x, min.y, min.z);

        wire[2] = new Vector3(min.x, min.y, min.z);
        wire[3] = new Vector3(min.x, max.y, min.z);

        wire[4] = new Vector3(min.x, max.y, min.z);
        wire[5] = new Vector3(max.x, max.y, min.z);

        wire[6] = new Vector3(max.x, max.y, min.z);
        wire[7] = new Vector3(max.x, min.y, min.z);

        //back
        wire[8] = new Vector3(min.x, min.y, max.z);
        wire[9] = new Vector3(max.x, min.y, max.z);

        wire[10] = new Vector3(min.x, min.y, max.z);
        wire[11] = new Vector3(min.x, max.y, max.z);

        wire[12] = new Vector3(min.x, max.y, max.z);
        wire[13] = new Vector3(max.x, max.y, max.z);

        wire[14] = new Vector3(max.x, max.y, max.z);
        wire[15] = new Vector3(max.x, min.y, max.z);

        //
        wire[16] = new Vector3(min.x, min.y, min.z);
        wire[17] = new Vector3(min.x, min.y, max.z);

        wire[18] = new Vector3(min.x, max.y, min.z);
        wire[19] = new Vector3(min.x, max.y, max.z);

        wire[20] = new Vector3(max.x, min.y, min.z);
        wire[21] = new Vector3(max.x, min.y, max.z);

        wire[22] = new Vector3(max.x, max.y, min.z);
        wire[23] = new Vector3(max.x, max.y, max.z);

        return wire;
    }
}
