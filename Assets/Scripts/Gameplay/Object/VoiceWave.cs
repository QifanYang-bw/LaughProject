using System;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Gameplay.Model;
using UnityEngine;

public class VoiceWave : MonoBehaviour {
    private VoiceWaveLineRendererEx rendererEx;

    public ArcModel Arc;
    public ArcLinkModel LeftLink, RightLink;
    public SoundTypes SoundType;

    public float maximumRadius = 10f;
    public float MinimumExpansionSpeed = 1.0f;
    [Header("Runtime Variables")]
    public float expansionSpeed = 1.0f;
    public float SpeedRatio = 1.0f;

    public float InitialStrength = 10;
    [Header("Runtime Variables")]
    public float RuntimeStrength = 10;
    // strength value per sec
    public float StrengthRatio = 1.0f;

    // Debug Vars
    public List<Wall> wallList;
    private List<NPC> _npcList;
    private List<Microphone> _microphoneList;

    private void Awake() {
        Arc.Center = transform.position;
        Arc.Load();

        rendererEx = GetComponent<VoiceWaveLineRendererEx>();
        rendererEx.arc = Arc;

        RuntimeStrength = InitialStrength;
    }

    private void Start()
    {
        // copy a list for modify
        _npcList = new List<NPC>(LevelManager.instance.Npcs);
        _microphoneList = new List<Microphone>(transform.parent.GetComponentsInChildren<Microphone>());
        Debug.Log($"microphoes:{_microphoneList.Count}");
    }

    private void ExamineCollision() {
        ExamineMicrophoneCollision();
        ExamineNpcCollision();
    }

    private void ExamineMicrophoneCollision()
    {
        if (_microphoneList.Count == 0)
            return;
        List<Microphone> needRemoved = new List<Microphone>();
        foreach (var microphone in _microphoneList)
        {
            if (!Arc.IsPointInsideArcRange(microphone.transform.position))
            {
                Debug.Log($"ExamineMicrophoneCollision microphone not in range {microphone.transform.position}");
                needRemoved.Add(microphone);
                continue;
            }

            if (!Arc.IsContainPoint(microphone.transform.position))
                continue;
            Debug.Log("ExamineMicrophoneCollision hit microphone");
            microphone.OnVoiceWaveHit(this);
            needRemoved.Add(microphone);
        }

        _microphoneList.RemoveAll(item => needRemoved.Contains(item));
    }

    private void ExamineNpcCollision()
    {
        if (_npcList.Count == 0)
            return;
        List<NPC> needRemoved = new List<NPC>();
        foreach (var npc in _npcList)
        {
            if (!Arc.IsPointInsideArcRange(npc.transform.position))
            {
                needRemoved.Add(npc);
                continue;
            }

            if (!Arc.IsContainPoint(npc.transform.position))
                continue;
            npc.OnVoiceWaveHit(this);
            needRemoved.Add(npc);
        }

        _npcList.RemoveAll(item => needRemoved.Contains(item));
    }

    private void Update() {
        UpdateStrength();
        UpdateSpeed();

        //(bool collision, double radius) = GeoLib.ArcCollisionRadiusWithSegment(Arc, wall.seg);

        //Debug.LogFormat("VoiceWave PredictedDistanceToArc {0} {1}", collision, radius);

        //if (collision && Arc.Radius >= radius || Arc.Radius > maximumRadius) {
        //    Arc.Angle.StartAngle += Arc.Angle.AngleRange;
        //    if (Arc.Angle.StartAngle > Math.PI * 2f) {
        //        Arc.Angle.StartAngle -= Math.PI * 2f;
        //    }
        //    Arc.Radius = 1;
        //    Arc.Load();
        //    return;
        //}
        Arc.Radius += Time.deltaTime * expansionSpeed;
        rendererEx.SetupCircle();

        ExamineCollision();

        if (RuntimeStrength <= 0)
            Destroy(gameObject);
    }

    public bool IsRemovable() {
        if (!Arc.isEmpty()) {
            return false;
        }
        if (LeftLink != null) {
            if (LeftLink.ArcStatus(Arc) == ArcEndPointStatus.ArcEndPointStatusExpanding) {
                return false;
            }
        }
        if (RightLink != null) {
            if (RightLink.ArcStatus(Arc) == ArcEndPointStatus.ArcEndPointStatusExpanding) {
                return false;
            }
        }
        return true;
    }

    public void Expand(double RadDelta) {
        if (RadDelta < 0) {
            Debug.LogAssertionFormat("Arc Expand delta {0} < 0", RadDelta);
            return;
        }
    }

    private void UpdateSpeed()
    {
        expansionSpeed = Math.Max(MinimumExpansionSpeed, RuntimeStrength * SpeedRatio);
    }

    private void UpdateStrength()
    {
        RuntimeStrength -= Time.deltaTime * StrengthRatio;
    }
}
