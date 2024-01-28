using Assets.Scripts;
using Assets.Scripts.Gameplay.Model;
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

    public float imageSizeRatio = 3f;

    public float RotateAngle = 90f;


    private void Awake() {
        UpdateGeometryPosition();
    }

    private void Start() {
        if (!LevelManager.instance) {
            return;
        }
        LevelManager.instance.AddWall(this);
        GetComponent<SpriteRenderer>().sprite = AssetHelper.instance.WallSprites[(int)Type];
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
            case WallTypes.BrokenGlass:
                return WallTypes.Shattered;
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

    public void UpdateGeometryPosition() {
        float boardHalfLength = transform.lossyScale.x * .5f * imageSizeRatio;
        Vector2 boardHalfSizeShiftVec = new Vector2(boardHalfLength * (float)Math.Cos(GeoLib.ConvertDegreesToRadians(transform.rotation.eulerAngles.z)),
                                                    boardHalfLength * (float)Math.Sin(GeoLib.ConvertDegreesToRadians(transform.rotation.eulerAngles.z)));

        Vector2 p1 = transform.position + (Vector3)boardHalfSizeShiftVec;
        Vector2 p2 = transform.position - (Vector3)boardHalfSizeShiftVec;
        Seg = new SegmentModel(p1, p2);
    }

    private void UpdateType(WallTypes type)
    {
        Type = type;
        GetComponent<SpriteRenderer>().sprite = AssetHelper.instance.WallSprites[(int)Type];
    }

    public void OnWaveWillCollide(VoiceWave wave) {
        UpdateType(GetNextType(wave));
    }

    public bool ShouldSplitOnCollision(VoiceWave wave)
    {
        return Type != WallTypes.Shattered;
    }

    public void OnWaveDidCollide(VoiceWave wave) {
        // update wave
        UpdateWaveByType(wave);

        if (Type == WallTypes.Shattered)
        {
            Debug.Log("wall Shattered");
            Destroy(gameObject);
        }
    }

    public void TriggerRotate()
    {
        transform.Rotate(0, 0, RotateAngle);
        UpdateGeometryPosition();
    }

}


