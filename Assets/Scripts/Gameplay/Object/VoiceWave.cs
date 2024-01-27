using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[Serializable]
public class VoiceCollisionModel {
    public double Radius;
    public Wall Target;
    public VoiceCollisionModel(double radius, Wall target) {
        this.Radius = radius;
        this.Target = target;
    }
}

public class VoiceWave : MonoBehaviour {
    private GameObject wavePrefab;

    private VoiceWaveLineRendererEx rendererEx;

    public ArcModel Arc;
    public ArcLinkModel LeftLink, RightLink;

    public float maximumRadius = 10f;
    public float expansionSpeed = 1.0f;

    public int collisionIndex;
    public List<VoiceCollisionModel> collisionList;
    public Wall sourceWall;

    // Debug Vars
    public List<Wall> wallList;

    private void Start() {
        Arc.Center = transform.position;
        Arc.Load();

        rendererEx = GetComponent<VoiceWaveLineRendererEx>();
        rendererEx.arc = Arc;

        wavePrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/VoiceWave.prefab", typeof(GameObject));

        ExamineCollision();
    }

    private void ExamineCollision() {
        foreach (Wall wall in wallList) {
            // Check whether the wall is the source it comes from
            if (wall == sourceWall) {
                continue;
            }
            (bool collision, double radius) = GeoLib.ArcCollisionRadiusWithSegment(Arc, wall.seg);
            //Debug.LogFormat("Arc ExamineCollision with seg {0}, res {1}, rad {2}",
            //                Arc.Description(), collision, radius);
            if (!collision || collision && (radius > maximumRadius || radius < Arc.Radius)) {
                continue;
            }
            // A collision will happen in the future, at least for now
            VoiceCollisionModel model = new VoiceCollisionModel(radius, wall);
            collisionList.Add(model);
        }
        collisionList.Sort((p1, p2) => p1.Radius.CompareTo(p2.Radius));
        collisionIndex = 0;
    }

    private void Update() {
        if (collisionIndex >= collisionList.Count) {
            Expand(Time.deltaTime * expansionSpeed);
            return;
        }
        while (collisionIndex < collisionList.Count && Arc.Radius > collisionList[collisionIndex].Radius) {
            Wall collisionWall = collisionList[collisionIndex].Target;
            SegmentModel seg = collisionList[collisionIndex].Target.seg;

            ArcLinkModel model = new ArcLinkModel();
            RayModel startRay = new RayModel(Arc.Center, Arc.Angle.StartAngle);
            (bool isAvailable, RayModel reflectedRay) = GeoLib.FindReflectedRay(startRay, seg);
            if (!isAvailable) {
                Debug.LogAssertionFormat("Arc FindReflectedRay Failed. Arc {0}, Ray {1}, seg {2}",
                                         Arc.Description(), startRay.Description(), seg.Description());
                continue;
            }
            double reflectedStartAngle = GeoLib.NormalizeAngle(reflectedRay.Direction - Arc.Angle.AngleRange);
            ArcModel reflectedArc = new ArcModel(reflectedRay.Origin, Arc.Radius, reflectedStartAngle, Arc.Angle.AngleRange);

            GameObject newWaveObject = Instantiate(wavePrefab, transform.parent);
            VoiceWave newWave = newWaveObject.GetComponent<VoiceWave>();
            newWave.transform.position = reflectedArc.Center;
            newWave.Arc = reflectedArc;
            newWave.expansionSpeed = expansionSpeed;
            newWave.maximumRadius = maximumRadius;

            newWave.sourceWall = collisionWall;
            // TODO: Move to global
            newWave.wallList = wallList;

            Debug.LogFormat("Arc ReflectedArc reflected. Arc {0}, Reflected {1}",
                             Arc.Description(), reflectedArc.Description());

            collisionIndex++;
        }
        Expand(Time.deltaTime * expansionSpeed);
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
        Arc.Radius += RadDelta;
        rendererEx.SetupCircle();
    }
}
