using System;
using System.Collections.Generic;
using UnityEngine;

public class VoiceWave : MonoBehaviour {
    private VoiceWaveLineRendererEx rendererEx;

    public ArcModel arc;
    public Wall wall;

    public float expansionSpeed = 1.0f;

    private void Awake() {
        arc.Center = transform.position;
        arc.Load();

        rendererEx = GetComponent<VoiceWaveLineRendererEx>();
        rendererEx.arc = arc;
    }

    private void Update() {
        (bool collision, double radius) = GeoLib.ArcCollisionRadiusWithSegment(arc, wall.seg);

        Debug.LogFormat("VoiceWave PredictedDistanceToArc {0} {1}", collision, radius);

        if (collision && arc.Radius >= radius || arc.Radius > 10f) {
            arc.Angle.StartAngle += arc.Angle.AngleRange;
            if (arc.Angle.StartAngle > Math.PI * 2f) {
                arc.Angle.StartAngle -= Math.PI * 2f;
            }
            arc.Radius = 1;
            arc.Load();
            return;
        }
        arc.Radius += Time.deltaTime * expansionSpeed;
        rendererEx.SetupCircle();
    }
}
