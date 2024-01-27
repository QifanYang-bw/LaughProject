using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

public class Wall : MonoBehaviour {
    public SegmentModel Seg;

    public enum WallTypes
    {
        Normal,
        Silent,
        Enhance,
        Glass,
        BrokenGlass,
        Shattered,
    }
    public WallTypes Type;
    // enhance
    public float EnhanceStrength = 2.0f;
    // glass
    public float ReduceStrength = 2.0f;
    // glass
    public float DestoryGlassLimitStrength = 3.0f;


    private void Awake() {
        float boardHalfLength = transform.lossyScale.x * .5f;
        Vector2 boardHalfSizeShiftVec = new Vector2(boardHalfLength * (float)Math.Cos(GeoLib.ConvertDegreesToRadians(transform.rotation.eulerAngles.z)),
                                                    boardHalfLength * (float)Math.Sin(GeoLib.ConvertDegreesToRadians(transform.rotation.eulerAngles.z)));

        //Debug.LogFormat("Wall boardHalfSizeShiftVec {0} angle {1}", boardHalfSizeShiftVec, transform.rotation.eulerAngles.z);
        Vector2 p1 = transform.position + (Vector3)boardHalfSizeShiftVec;
        Vector2 p2 = transform.position - (Vector3)boardHalfSizeShiftVec;
        Seg = new SegmentModel(p1, p2);

        //RayModel rayModel = new RayModel(-3.71, -1.74, 4.71238898038469);
        //(bool rayCollision, double rayDist) = GeoLib.RayLineSegmentIntersection(rayModel, seg);
        //Debug.LogFormat("VoiceWave Awake RayLineSegmentIntersection {0} rayDist {1}", rayCollision, rayDist);
    }
    public void OnVoiceWaveHit(VoiceWave voiceWave)
    {
        var nextType = GetNextType(voiceWave);
        // update wave
        UpdateWaveByType(voiceWave);
        UpdateType(nextType);
    }

    private WallTypes GetNextType(VoiceWave voiceWave)
    {
        switch (Type)
        {
            case WallTypes.Glass:
                if (voiceWave.RuntimeStrength > DestoryGlassLimitStrength)
                {
                    return WallTypes.Shattered;
                }
                else
                {
                    return WallTypes.BrokenGlass;
                }
                break;
            case WallTypes.BrokenGlass:
                return WallTypes.Shattered;
                break;
            default:
                return Type;
        }
    }

    private void UpdateWaveByType(VoiceWave voiceWave)
    {
        switch (Type)
        {
            case WallTypes.Normal:
                break;
            case WallTypes.Silent:
                voiceWave.RuntimeStrength = 0;
                break;
            case WallTypes.Enhance:
                voiceWave.RuntimeStrength += EnhanceStrength;
                break;
            case WallTypes.Glass:
                voiceWave.RuntimeStrength -= ReduceStrength;
                break;
            case WallTypes.BrokenGlass:
                voiceWave.RuntimeStrength -= ReduceStrength;
                break;
            case WallTypes.Shattered:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void UpdateType(WallTypes type)
    {
        Type = type;
        // todo lwttai update ui
    }
}
