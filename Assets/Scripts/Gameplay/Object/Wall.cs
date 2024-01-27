using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;

public class Wall : MonoBehaviour {
    public SegmentModel seg;

    private void Awake() {
        float boardHalfLength = transform.lossyScale.x * .5f;
        Vector2 boardHalfSizeShiftVec = new Vector2(boardHalfLength * (float)Math.Cos(GeoLib.ConvertDegreesToRadians(transform.rotation.eulerAngles.z)),
                                                    boardHalfLength * (float)Math.Sin(GeoLib.ConvertDegreesToRadians(transform.rotation.eulerAngles.z)));

        Debug.LogFormat("Wall boardHalfSizeShiftVec {0} angle {1}", boardHalfSizeShiftVec, transform.rotation.eulerAngles.z);
        Vector2 p1 = transform.position + (Vector3)boardHalfSizeShiftVec;
        Vector2 p2 = transform.position - (Vector3)boardHalfSizeShiftVec;
        seg = new SegmentModel(p1, p2);

        //RayModel rayModel = new RayModel(-3.71, -1.74, 4.71238898038469);
        //(bool rayCollision, double rayDist) = GeoLib.RayLineSegmentIntersection(rayModel, seg);
        //Debug.LogFormat("VoiceWave Awake RayLineSegmentIntersection {0} rayDist {1}", rayCollision, rayDist);
    }
}
