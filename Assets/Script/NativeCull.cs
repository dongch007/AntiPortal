using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class NativeCull
{
#if (UNITY_IOS || UNITY_IPHONE)
    [DllImport("__Internal")]
#else
    [DllImport("CullTest", EntryPoint = "CullAABB")]
#endif
    internal static extern float CullAABB([In, Out]Bounds[] bounds, [In, Out]bool[] visible, int num);

    public static void Cull(Bounds[] bounds, bool[] visible)
    {
        float fTime = Time.realtimeSinceStartup;

        //GCHandle boundsBuf = GCHandle.Alloc(bounds, GCHandleType.Pinned);
        //GCHandle visibleBuf= GCHandle.Alloc(visible, GCHandleType.Pinned);

        //float t = CullAABB(boundsBuf.AddrOfPinnedObject(), visibleBuf.AddrOfPinnedObject(), bounds.Length);

        //Marshal.WriteByte()

        //boundsBuf.Free();
        //visibleBuf.Free();

        //CullAABB(bounds, visible, bounds.Length);

        Debug.Log("Native time: " + (Time.realtimeSinceStartup-fTime)*1000);
        Debug.Log(System.Runtime.InteropServices.Marshal.SizeOf(visible[0]));

        //Debug.Log(t);
    }
}