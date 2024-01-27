using System;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Gameplay.Model;
using UnityEngine;
using UnityEditor;

[Serializable]
public class VoiceCollisionModel {
    public double Radius;
    public Wall Target;
    public Vector2 collisionPoint;

    public VoiceCollisionModel(double radius, Wall target, Vector2 collisionPoint) {
        this.Radius = radius;
        this.Target = target;
        this.collisionPoint = collisionPoint;
    }
}

public class VoiceWave : MonoBehaviour {
    private GameObject wavePrefab;

    private VoiceWaveLineRendererEx rendererEx;

    public ArcModel Arc;
    public ArcLinkModel LeftLink, RightLink;
    public SoundTypes SoundType;

    public float MaximumRadius = 10f;
    public int collisionIndex;
    public List<VoiceCollisionModel> collisionList;
    public Wall sourceWall;

    public float MinimumExpansionSpeed = 1.0f;
    [Header("Runtime Variables")]
    public float ExpansionSpeed = 1.0f;
    public float SpeedRatio = 1.0f;

    public float InitialStrength = 10;
    [Header("Runtime Variables")]
    public float RuntimeStrength = 10;
    // strength value per sec
    public float StrengthRatio = 1.0f;

    public bool isHidden;

    // Debug Vars
    public List<Wall> wallList;
    private List<NPC> _npcList;
    private List<Microphone> _microphoneList;

    private void Start() {
        Arc.Center = transform.position;
        Arc.Load();

        RuntimeStrength = InitialStrength;
        // copy a list for modify
        _npcList = LevelManager.instance != null ? new List<NPC>(LevelManager.instance.Npcs) : new List<NPC>(transform.parent.GetComponentsInChildren<NPC>());

        _microphoneList = new List<Microphone>(transform.parent.GetComponentsInChildren<Microphone>());
        Debug.Log($"microphoes:{_microphoneList.Count}");

        rendererEx = GetComponent<VoiceWaveLineRendererEx>();
        rendererEx.arc = Arc;

        wavePrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/VoiceWave.prefab", typeof(GameObject));

        ExamineCollision();
    }

    private void ExamineCollision() {
        foreach (Wall wall in wallList) {
            // Check whether the wall is the source it comes from
            if (wall == sourceWall || wall == null) {
                continue;
            }
            (bool collision, Vector2 collisionPoint, double radius) = GeoLib.ArcCollisionRadiusWithSegment(Arc, wall.Seg);
            Debug.LogFormat("Arc ExamineCollision with seg {0}, res {1}, rad {2}",
                            Arc.Description(), collision, radius);
            if (!collision || collision && (radius > MaximumRadius || radius < Arc.Radius)) {
                continue;
            }
            // A collision will happen in the future, at least for now
            VoiceCollisionModel model = new VoiceCollisionModel(radius, wall, collisionPoint);
            collisionList.Add(model);
        }
        collisionList.Sort((p1, p2) => p1.Radius.CompareTo(p2.Radius));
        collisionIndex = 0;
    }

    private void Update() {
        UpdateStrength();
        UpdateSpeed();

        if (LeftLink != null && LeftLink.isBroken) {
            LeftLink = null;
        }
        if (RightLink != null && RightLink.isBroken) {
            RightLink = null;
        }
        if (IsRemovable()) {
            if (LeftLink != null) {
                LeftLink.isBroken = true;
            }
            if (RightLink != null) {
                RightLink.isBroken = true;
            }
            Discard();
            return;
        }
        double beforeRadius = Arc.Radius;
        Expand(Time.deltaTime * ExpansionSpeed);
        if (RightLink == null) {
            Debug.LogFormat("VoiceWave RightLink is null");
        } else {
            Debug.LogFormat("VoiceWave RightLink.ArcStatus {0}", RightLink.ArcStatus(Arc));
        }
        if (RightLink != null && RightLink.ArcStatus(Arc) == ArcEndPointStatus.ArcEndPointStatusShrinking) {
            (bool collision, Vector2 collisionPoint) = GeoLib.FindOneArcSegmentCollision(Arc, RightLink.Segment);
            if (collision) {
                double newAngle = GeoLib.CalculateAngle(Arc.Center, collisionPoint);
                double newAngleDelta = newAngle - Arc.Angle.StartAngle;
                //Debug.LogFormat("VoiceWave update Right angle: collisionPoint {0}, startAngle {1}, newAngle {2}, angle delta {3}",
                //                 collisionPoint, Arc.Angle.StartAngle, newAngle, newAngleDelta);
                if (newAngleDelta < 0) {
                    Debug.LogAssertionFormat("VoiceWave update angle delta < 0: collisionPoint {0}, arc {1}, new Angle {2}, angle delta {3}",
                                              collisionPoint, Arc, newAngle, newAngleDelta);
                    newAngleDelta = Math.Abs(newAngleDelta);
                }
                if (newAngleDelta > Math.PI) {
                    newAngleDelta = Math.PI * 2f - newAngleDelta;
                }
                //Debug.LogFormat("VoiceWave update Chain newAngle {0} angleDelta {1} \r\n Arc {2}",
                //                 newAngle, newAngleDelta, Arc.Description());
                RightLink.ChainAngle(newAngleDelta);
            } else {
                RightLink.isBroken = true;
                RightLink = null;
            }
        }
        if (LeftLink == null) {
            Debug.LogFormat("VoiceWave LeftLink is null");
        } else {
            Debug.LogFormat("VoiceWave LeftLink.ArcStatus {0}", LeftLink.ArcStatus(Arc));
        }
        if (LeftLink != null && LeftLink.ArcStatus(Arc) == ArcEndPointStatus.ArcEndPointStatusShrinking) {
            (bool collision, Vector2 collisionPoint) = GeoLib.FindOneArcSegmentCollision(Arc, LeftLink.Segment);
            if (collision) {
                double newAngle = GeoLib.CalculateAngle(Arc.Center, collisionPoint);
                double newAngleDelta = Arc.Angle.EndAngle - newAngle;

                if (newAngleDelta < 0) {
                    Debug.LogAssertionFormat("VoiceWave update angle delta < 0: collisionPoint {0}, arc {1}, new Angle {2}, angle delta {3}",
                                              collisionPoint, Arc, newAngle, newAngleDelta);
                    newAngleDelta = Math.Abs(newAngleDelta);
                }
                if (newAngleDelta > Math.PI) {
                    newAngleDelta = Math.PI * 2f - newAngleDelta;
                }
                LeftLink.ChainAngle(newAngleDelta);
            } else {
                LeftLink.isBroken = true;
                LeftLink = null;
            }
        }

        // Debug.LogFormat("VoiceWave {0} Update With Center {1}", gameObject.name, Arc.Center);
        if (collisionIndex >= collisionList.Count) {
            return;
        }
        while (collisionIndex < collisionList.Count && Arc.Radius > collisionList[collisionIndex].Radius) {
            VoiceCollisionModel model = collisionList[collisionIndex];
            Wall collisionWall = model.Target;
            SegmentModel seg = model.Target.Seg;
            Debug.LogFormat("VoiceWave {0} enter collision model with seg {1}", gameObject.name, seg);

            collisionWall.OnWaveWillCollide(this);
            if (!collisionWall.ShouldSplitOnCollision(this)) {
                Debug.LogFormat("VoiceWave {0} does not split with seg {1}", gameObject.name, seg);
                collisionIndex++;
                continue;
            }

            RayModel startRay = new RayModel(Arc.Center, Arc.Angle.StartAngle);
            (bool isAvailable, RayModel reflectedRay) = GeoLib.FindReflectedRay(startRay, seg);
            if (!isAvailable) {
                Debug.LogAssertionFormat("Arc FindReflectedRay Failed. Arc {0}, Ray {1}, seg {2}",
                                         Arc.Description(), startRay.Description(), seg.Description());
                continue;
            }

            double reflectedCollisionAngle = GeoLib.CalculateAngle(reflectedRay.Origin, model.collisionPoint);

            ArcModel reflectedArc = new ArcModel(reflectedRay.Origin, Arc.Radius, reflectedCollisionAngle, 0f);
            VoiceWave reflectedWave = CreateWaveFromSelf(reflectedArc, collisionWall);
            Debug.LogFormat("Arc ReflectedArc reflected. Arc {0}, \r\n Reflected {1}",
                             Arc.Description(), reflectedArc.Description());

            double collisionAngle = GeoLib.CalculateAngle(Arc.Center, model.collisionPoint);
            bool shouldDestroy = false;
            if (GeoLib.isEqual(collisionAngle, Arc.Angle.StartAngle)) {
                ArcLinkModel linkModel = new ArcLinkModel();
                SetRightLink(linkModel);
                reflectedWave.SetRightLink(linkModel);
                linkModel.LeftArc = Arc;
                linkModel.RightArc = reflectedArc;
                linkModel.SetRightExpand();
                linkModel.Segment = collisionWall.Seg;
                    
                Debug.LogFormat("Arc ReflectedArc Right Link Created.");
                reflectedWave.gameObject.name = "RightReflected";
            } else if (GeoLib.isEqual(collisionAngle, Arc.Angle.EndAngle)) {
                ArcLinkModel linkModel = new ArcLinkModel();
                SetLeftLink(linkModel);
                reflectedWave.SetLeftLink(linkModel);
                linkModel.LeftArc = reflectedArc;
                linkModel.RightArc = Arc;
                linkModel.SetLeftExpand();
                linkModel.Segment = collisionWall.Seg;
                Debug.LogFormat("Arc ReflectedArc Left Link Created.");
                reflectedWave.gameObject.name = "LeftReflected";
            } else {
                ArcModel splitArcOne = new ArcModel(Arc.Center, Arc.Radius, Arc.Angle.StartAngle, collisionAngle - Arc.Angle.StartAngle);
                ArcModel splitArcTwo = new ArcModel(Arc.Center, Arc.Radius, collisionAngle, Arc.Angle.EndAngle - collisionAngle);

                ArcLinkModel twoLinkModel = new ArcLinkModel();
                VoiceWave splitWaveTwo = CreateWaveFromSelf(splitArcTwo, collisionWall);
                reflectedWave.SetRightLink(twoLinkModel);
                splitWaveTwo.SetRightLink(twoLinkModel);
                twoLinkModel.LeftArc = splitArcTwo;
                twoLinkModel.RightArc = reflectedArc;
                twoLinkModel.SetRightExpand();
                twoLinkModel.Segment = collisionWall.Seg;

                ArcLinkModel oneLinkModel = new ArcLinkModel();
                VoiceWave splitWaveOne = CreateWaveFromSelf(splitArcOne, collisionWall);
                reflectedWave.SetLeftLink(oneLinkModel);
                splitWaveOne.SetLeftLink(oneLinkModel);
                oneLinkModel.LeftArc = reflectedArc;
                oneLinkModel.RightArc = splitArcOne;
                oneLinkModel.SetLeftExpand();
                oneLinkModel.Segment = collisionWall.Seg;

                Debug.LogFormat("Arc Splitted: Right {0}, \r\nLeft {1}.", splitArcTwo.Description(), splitArcOne.Description());

                Debug.LogFormat("Arc twoLinkModel {0}", twoLinkModel.Description());
                Debug.LogFormat("Arc oneLinkModel {0}", oneLinkModel.Description());
                shouldDestroy = true;
            }
            collisionWall.OnWaveDidCollide(reflectedWave);
            if (shouldDestroy) {
                Debug.LogFormat("Arc ReflectedArc this {0} destroyed.", Arc.Description());
                Discard();
            }
            collisionIndex++;
        }

        ExamineMicrophoneCollision();
        ExamineNpcCollision();

        if (RuntimeStrength <= 0)
            Destroy(gameObject);
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

    public bool IsRemovable() {
        if (Arc.Radius > MaximumRadius) {
            return true;
        }
        if (!Arc.isEmpty()) {
            return false;
        }
        if (LeftLink != null) {
            Debug.LogFormat("VoiceWave IsRemovable LeftLink Status {0}", LeftLink.ArcStatus(Arc));
            if (LeftLink.ArcStatus(Arc) == ArcEndPointStatus.ArcEndPointStatusExpanding) {
                return false;
            }
        } else {
            Debug.LogFormat("VoiceWave IsRemovable LeftLink null");
        }
        if (RightLink != null) {
            Debug.LogFormat("VoiceWave IsRemovable RightLink Status {0}", RightLink.ArcStatus(Arc));
            if (RightLink.ArcStatus(Arc) == ArcEndPointStatus.ArcEndPointStatusExpanding) {
                return false;
            }
        } else {
            Debug.LogFormat("VoiceWave IsRemovable RightLink null");
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

    public VoiceWave CreateWaveFromSelf(ArcModel newArc, Wall collisionWall) {
        GameObject newWaveObject = Instantiate(wavePrefab, transform.parent);
        VoiceWave newWave = newWaveObject.GetComponent<VoiceWave>();
        newWave.transform.position = newArc.Center;
        newWave.Arc = newArc;
        newWave.ExpansionSpeed = ExpansionSpeed;
        newWave.MaximumRadius = MaximumRadius;

        newWave.sourceWall = collisionWall;
        // TODO: Move to global
        newWave.wallList = wallList;
        return newWave;
    }

    private void SetLeftLink(ArcLinkModel linkModel) {
        if (LeftLink != null) {
            LeftLink.isBroken = true;
        }
        LeftLink = linkModel;
    }
    private void SetRightLink(ArcLinkModel linkModel) {
        if (RightLink != null) {
            RightLink.isBroken = true;
        }
        RightLink = linkModel;
    }

    private void Discard() {
#if UNITY_EDITOR
        gameObject.SetActive(false);
#else
        Destroy(this.gameObject);
#endif
    }

    private void UpdateSpeed()
    {
        ExpansionSpeed = Math.Max(MinimumExpansionSpeed, RuntimeStrength * SpeedRatio);
    }

    private void UpdateStrength()
    {
        RuntimeStrength -= Time.deltaTime * StrengthRatio;
    }
}
