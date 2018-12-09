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
            cube = cubeGo.GetComponent<MeshFilter>().sharedMesh;
        }

        return AntiPortalHelper.cube;
    }
}
