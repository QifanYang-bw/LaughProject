using System;
using System.Collections.Generic;
using Assets.Scripts.Gameplay.Model;
using UnityEngine;

public class VoiceWave : MonoBehaviour {
    private VoiceWaveLineRendererEx rendererEx;

    public ArcModel Arc;
    public ArcLinkModel LeftLink, RightLink;
    public SoundTypes SoundType;

    public float maximumRadius = 10f;
    public float expansionSpeed = 1.0f;

    // Debug Vars
    public List<Wall> wallList;

    private void Awake() {
        Arc.Center = transform.position;
        Arc.Load();

        rendererEx = GetComponent<VoiceWaveLineRendererEx>();
        rendererEx.arc = Arc;
    }

    private void ExamineCollision() {

    }

    private void Update() {
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
}
