using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Occluder : MonoBehaviour
{
    public Vector3 center = Vector3.zero;
    public Vector3 extents = Vector3.one;

    struct Face
    {
        public Plane plane;
        public int[] egdes;
        public int[] egdesCCW;

        //idx shound be clockwise
        public Face(Vector3[] corners, int idx0, int idx1, int idx2, int idx3)
        {
            this.plane = new Plane(corners[idx0], corners[idx1], corners[idx2]);

            this.egdes = new int[4];
            this.egdes[0] = idx0 << 8 | idx1;
            this.egdes[1] = idx1 << 8 | idx2;
            this.egdes[2] = idx2 << 8 | idx3;
            this.egdes[3] = idx3 << 8 | idx0;

            this.egdesCCW = new int[4];
            this.egdesCCW[0] = idx1 << 8 | idx0;
            this.egdesCCW[1] = idx2 << 8 | idx1;
            this.egdesCCW[2] = idx3 << 8 | idx2;
            this.egdesCCW[3] = idx0 << 8 | idx3;
        }
    }

    private Vector3[] corners = new Vector3[8];
    private Face[] faces = new Face[6];

    private List<Plane> cullPlanes = new List<Plane>();

    void Start()
    {
        //     5-------6
        //    /|      /|
        //   / |     / |
        //  4--|----7  |
        //  |  1----|--2
        //  |  /    | /
        //  | /     |/
        //  0-------3

        Vector3 min = this.center - this.extents;
        Vector3 max = this.center + this.extents;
        this.corners[0].Set(min.x, min.y, min.z);
        this.corners[1].Set(min.x, min.y, max.z);
        this.corners[2].Set(max.x, min.y, max.z);
        this.corners[3].Set(max.x, min.y, min.z);
        this.corners[4].Set(min.x, max.y, min.z);
        this.corners[5].Set(min.x, max.y, max.z);
        this.corners[6].Set(max.x, max.y, max.z);
        this.corners[7].Set(max.x, max.y, min.z);

        for (int i = 0; i < this.corners.Length; i++)
        {
            this.corners[i] = this.transform.TransformPoint(this.corners[i]);
        }

        //+X
        this.faces[0] = new Face(this.corners, 7, 6, 2, 3);
        //-X
        this.faces[1] = new Face(this.corners, 5, 4, 0, 1);
        //+Y
        this.faces[2] = new Face(this.corners, 4, 5, 6, 7);
        //-Y
        this.faces[3] = new Face(this.corners, 3, 2, 1, 0);
        //+Z
        this.faces[4] = new Face(this.corners, 6, 5, 1, 2);
        //-Z
        this.faces[5] = new Face(this.corners, 0, 4, 7, 3);
    }


    private float screenArea = 0;
    public float ScreenArea
    {
        get { return this.screenArea; }
    }

    private void OnWillRenderObject()
    {
        Camera camera = Camera.current;
        AntiPortalCuller culler = camera.GetComponent<AntiPortalCuller>();
        if (culler != null)
        {
            Vector2 min = Vector2.positiveInfinity;
            Vector2 max = Vector2.negativeInfinity;
            foreach (Vector3 corner in this.corners)
            {
                Vector3 screenPos = camera.WorldToViewportPoint(corner);

                //if occluder cross with camera's near plane, ignore this occluder
                if (screenPos.z < 0)
                    return;

                if (screenPos.x < min.x)
                    min.x = screenPos.x;
                else if (screenPos.x > max.x)
                    max.x = screenPos.x;

                if (screenPos.y < min.y)
                    min.y = screenPos.y;
                else if (screenPos.y > max.y)
                    max.y = screenPos.y;
            }

            if (min.x < -1.0f)
                min.x = -1.0f;
            if (min.y < -1.0f)
                min.y = -1.0f;

            if (max.x > 1.0f)
                max.x = 1.0f;
            if (max.y > 1.0f)
                max.y = 1.0f;

            this.screenArea = (max.x - min.x) * (max.y - min.y);

            culler.AddOccluder(this);
        }
    }

    //http://www.gamasutra.com/view/feature/131388/rendering_the_great_outdoors_fast_.php?page=3
    HashSet<int> edges = new HashSet<int>();
    public List<Plane> CalculateCullPlanes(Vector3 viewPos, Vector3 viewDir)
    {
        this.cullPlanes.Clear();

        foreach (Face face in this.faces)
        {
            if(Vector3.Dot(face.plane.normal, viewDir) < 0)
            {
                this.cullPlanes.Add(face.plane);

                for(int i = 0; i <4; i++)
                {
                    int edge = face.egdes[i];
                    int edgeCCW = face.egdesCCW[i];
                    if (this.edges.Contains(edgeCCW))
                        this.edges.Remove(edgeCCW);
                    else
                        this.edges.Add(edge);
                }
            }
        }

        foreach (int edge in this.edges)
        {
            Vector3 v0 = this.corners[edge >> 8];
            Vector3 v1 = this.corners[edge & 0x00FF];

            Plane plane = new Plane(v0, v1, viewPos);
            this.cullPlanes.Add(plane);
        }
        this.edges.Clear();

        return this.cullPlanes;
    }

    public Vector3[] GetCorners()
    {
        return this.corners;
    }

#if UNITY_EDITOR
    public Vector3[] GetContour(Vector3 viewPos, Vector3 viewDir)
    {
        foreach (Face face in this.faces)
        {
            if (Vector3.Dot(face.plane.normal, viewDir) < 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    int edge = face.egdes[i];
                    int edgeCCW = face.egdesCCW[i];
                    if (this.edges.Contains(edgeCCW))
                        this.edges.Remove(edgeCCW);
                    else
                        this.edges.Add(edge);
                }
            }
        }

        List<int> contourIdx = new List<int>();
        foreach (int edge in this.edges)
        {
            int idx0 = edge >> 8;
            int idx1 = edge & 0x00FF;

            if (contourIdx.Contains(idx0) == false)
                contourIdx.Add(idx0);

            if (contourIdx.Contains(idx1) == false)
                contourIdx.Add(idx1);
        }
        this.edges.Clear();

        Vector3[] contour = new Vector3[contourIdx.Count];
        for(int i = 0; i < contour.Length; i++)
        {
            contour[i] = this.corners[contourIdx[i]];
        }

        return contour;
    }

    public Vector3[] GetWire()
    {
        Vector3[] wire = new Vector3[24];
        //bottom
        wire[0] = this.corners[0];
        wire[1] = this.corners[1];

        wire[2] = this.corners[1];
        wire[3] = this.corners[2];

        wire[4] = this.corners[2];
        wire[5] = this.corners[3];

        wire[6] = this.corners[3];
        wire[7] = this.corners[0];

        //top
        wire[8] = this.corners[4];
        wire[9] = this.corners[5];

        wire[10] = this.corners[5];
        wire[11] = this.corners[6];

        wire[12] = this.corners[6];
        wire[13] = this.corners[7];

        wire[14] = this.corners[7];
        wire[15] = this.corners[4];

        //vertical
        wire[16] = this.corners[0];
        wire[17] = this.corners[5];

        wire[18] = this.corners[1];
        wire[19] = this.corners[5];

        wire[20] = this.corners[2];
        wire[21] = this.corners[6];

        wire[22] = this.corners[3];
        wire[23] = this.corners[7];

        return wire;
    }

    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.red;
        //Gizmos.DrawWireMesh(AntiPortalHelper.GetCube(), this.transform.position, this.transform.rotation, Vector3.Scale(this.extends, this.transform.localScale)*2.0f);
        Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.5f);
        Gizmos.DrawMesh(AntiPortalHelper.GetCube(), this.transform.position, this.transform.rotation, Vector3.Scale(this.extents, this.transform.localScale) * 2.0f);
        //Gizmos.color = Color.red;
        //Gizmos.DrawWireMesh(AntiPortalHelper.GetCube(), this.transform.position, this.transform.rotation, Vector3.Scale(this.extends, this.transform.localScale) * 2.0f);
    }

    private void OnDrawGizmosSelected()
    {
        //if(this.transform.hasChanged)
        //{
        //    if (this.corners != null)
        //    {
        //        Vector3 min = this.center - extends;
        //        Vector3 max = this.center + extends;
        //        this.corners[0].Set(min.x, min.y, min.z);
        //        this.corners[1].Set(min.x, min.y, max.z);
        //        this.corners[2].Set(max.x, min.y, max.z);
        //        this.corners[3].Set(max.x, min.y, min.z);
        //        this.corners[4].Set(min.x, max.y, min.z);
        //        this.corners[5].Set(min.x, max.y, max.z);
        //        this.corners[6].Set(max.x, max.y, max.z);
        //        this.corners[7].Set(max.x, max.y, min.z);

        //        for (int i = 0; i < this.corners.Length; i++)
        //        {
        //            this.corners[i] = this.transform.TransformPoint(this.corners[i]);
        //        }
        //    }

        //    this.transform.hasChanged = false;
        //}

        if (Application.isPlaying)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 17;
            UnityEditor.Handles.Label(this.transform.position, this.screenArea.ToString(), style);
        }
    }
#endif
}
