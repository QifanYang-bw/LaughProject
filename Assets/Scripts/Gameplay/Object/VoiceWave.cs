using System;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Gameplay.Model;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine.Assertions;

[Serializable]
public class VoiceCollisionModel {
    public double MinRadius, MaxRadius;
    public Wall Target;
    public Vector2 MinCollisionPoint, MaxCollisionPoint;

    public VoiceCollisionModel(Wall target, double MinRadius, Vector2 MinCollisionPoint, double MaxRadius, Vector2 MaxCollisionPoint) {
        this.Target = target;
        this.MinRadius = MinRadius;
        this.MinCollisionPoint = MinCollisionPoint;
        this.MaxRadius = MaxRadius;
        this.MaxCollisionPoint = MaxCollisionPoint;
    }
}

public class VoiceWave : MonoBehaviour {
    [NonSerialized]
    private VoiceWaveLineRendererEx rendererEx;

    public ArcModel Arc;
    public SoundTypes SoundType;

    public float MaximumRadius = 10f;
    public float MinimumExpansionSpeed = 1.0f;
    public float SpeedRatio = 1.0f;

    public float InitialStrength = 10;
    // strength value per sec
    public float StrengthRatio = 1.0f;

    public bool isHidden;

    [Header("Runtime Variables")]
    public float ExpansionSpeed = 1.0f;
    public float RuntimeStrength = 10;

    // Debug Vars
    [SerializeField]
    private List<Wall> WallBanList;
    [SerializeField]
    private List<VoiceCollisionModel> _collisionList;
    [SerializeField]
    private List<NPC> _npcList;
    [SerializeField]
    private List<Microphone> _microphoneList;
    [SerializeField]
    private List<Switch> _switchList;

    public ArcLinkModel LeftLink, RightLink;

    private void Awake() {
        WallBanList = new List<Wall>();
    }

    private void Start() {
        Arc.Center = transform.position;
        Arc.Load();

        RuntimeStrength = InitialStrength;

        _npcList = LevelManager.instance != null ? new List<NPC>(LevelManager.instance.NpcList) :
                                            new List<NPC>(transform.parent.GetComponentsInChildren<NPC>());
        _microphoneList = LevelManager.instance != null ? new List<Microphone>(LevelManager.instance.MicrophoneList) :
                          new List<Microphone>(transform.parent.GetComponentsInChildren<Microphone>());
        _switchList = LevelManager.instance != null ? new List<Switch>(LevelManager.instance.SwitchList) :
            new List<Switch>(transform.parent.GetComponentsInChildren<Switch>());

        rendererEx = GetComponent<VoiceWaveLineRendererEx>();
        rendererEx.arc = Arc;

        UpdateUIByType();

        ExamineCollision();
    }

    private void ExamineCollision() {
        if (!LevelManager.instance) {
            return;
        }
        _collisionList.Clear();
        foreach (Wall wall in LevelManager.instance.WallList) {
            // Check whether the wall is the source it comes from
            if (wall == null || WallBanList.Contains(wall)) {
                continue;
            }
            (bool collision, Vector2 MinCollisionPoint, double MinRadius, Vector2 MaxCollisionPoint, double MaxRadius) = GeoLib.ArcCollisionRadiusWithSegment(Arc, wall.Seg);
            Debug.LogFormat("VoiceWave ExamineCollision with seg {0}, res {1}, minPos {2}, minDist {3}, maxPos {4}, maxDist {5}",
                             wall.Seg, collision, MinCollisionPoint, MinRadius, MaxCollisionPoint, MaxRadius);
            if (collision && (Arc.Radius > MaxRadius) && !WallBanList.Contains(wall)) {
                WallBanList.Add(wall);
            }
            if (!collision || collision && (Arc.Radius > MaxRadius || MaxRadius > MaximumRadius)) {
                continue;
            }
            // A collision will happen in the future, at least for now
            VoiceCollisionModel model = new VoiceCollisionModel(wall, MinRadius, MinCollisionPoint, MaxRadius, MaxCollisionPoint);
            _collisionList.Add(model);
        }
    }

    private void Update() {
        UpdateParams();

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
        double afterRadius = Arc.Radius;

        /*if (RightLink == null) {
            Debug.LogFormat("VoiceWave RightLink is null");
        } else {
            Debug.LogFormat("VoiceWave RightLink.ArcStatus {0}", RightLink.ArcStatus(Arc));
        }*/
        if (RightLink != null && RightLink.ArcStatus(Arc) == ArcEndPointStatus.ArcEndPointStatusShrinking) {
            (bool collision, Vector2 collisionPoint) = GeoLib.FindOneArcSegmentCollision(Arc, RightLink.LinkWall.Seg);
            if (collision) {
                double newAngle = GeoLib.CalculateAngle(Arc.Center, collisionPoint);
                double newAngleDelta = GeoLib.NormalizeAngle(newAngle - Arc.Angle.StartAngle);
                //Debug.LogFormat("VoiceWave update Right angle: collisionPoint {0}, startAngle {1}, newAngle {2}, angle delta {3}",
                //                 collisionPoint, Arc.Angle.StartAngle, newAngle, newAngleDelta);
                if (newAngleDelta < 0) {
                    Debug.LogAssertionFormat("VoiceWave update angle delta < 0: collisionPoint {0}, arc {1}, new Angle {2}, angle delta {3}",
                                              collisionPoint, Arc, newAngle, newAngleDelta);
                    newAngleDelta = Math.Abs(newAngleDelta);
                }
                Assert.IsFalse(newAngleDelta > Math.PI, $"{newAngleDelta} > Math.PI");
                //Debug.LogFormat("VoiceWave update Chain newAngle {0} angleDelta {1} \r\n Arc {2}",
                //                 newAngle, newAngleDelta, Arc.Description());
                RightLink.ChainAngle(newAngleDelta);
            } else {
                RightLink.isBroken = true;
                RightLink = null;
            }
        }
        /*if (LeftLink == null) {
            Debug.LogFormat("VoiceWave LeftLink is null");
        } else {
            Debug.LogFormat("VoiceWave LeftLink.ArcStatus {0}", LeftLink.ArcStatus(Arc));
        }*/
        if (LeftLink != null && LeftLink.ArcStatus(Arc) == ArcEndPointStatus.ArcEndPointStatusShrinking) {
            (bool collision, Vector2 collisionPoint) = GeoLib.FindOneArcSegmentCollision(Arc, LeftLink.LinkWall.Seg);
            if (collision) {
                double newAngle = GeoLib.CalculateAngle(Arc.Center, collisionPoint);
                double newAngleDelta = GeoLib.NormalizeAngle(Arc.Angle.EndAngle - newAngle);

                if (newAngleDelta < 0) {
                    Debug.LogAssertionFormat("VoiceWave update angle delta < 0: collisionPoint {0}, arc {1}, new Angle {2}, angle delta {3}",
                                              collisionPoint, Arc, newAngle, newAngleDelta);
                    newAngleDelta = Math.Abs(newAngleDelta);
                }
                Assert.IsFalse(newAngleDelta > Math.PI, $"{newAngleDelta} > Math.PI");
                LeftLink.ChainAngle(newAngleDelta);
            } else {
                LeftLink.isBroken = true;
                LeftLink = null;
            }
        }

        if (!IsStaticWave()) {
            Debug.LogFormat("VoiceWave {0} is not static, reexamine", gameObject.name);
            ExamineCollision();
        }
        foreach (VoiceCollisionModel model in _collisionList) {
            if (LeftLink != null && LeftLink.LinkWall == model.Target) {
                continue;
            }
            if (RightLink != null && RightLink.LinkWall == model.Target) {
                continue;
            }
            if (GeoLib.SmallerThan(Arc.Radius, model.MinRadius) || GeoLib.GreaterThan(Arc.Radius, model.MaxRadius)) {
                continue;
            }
            Wall collisionWall = model.Target;
            SegmentModel seg = model.Target.Seg;
            Debug.LogFormat("VoiceWave {0} enter collision model with seg {1}", gameObject.name, seg.Description());

            collisionWall.OnWaveWillCollide(this);
            if (!collisionWall.ShouldSplitOnCollision(this)) {
                Debug.LogFormat("VoiceWave {0} does not split with seg {1}", gameObject.name, seg.Description());
                continue;
            }

            RayModel startRay = new RayModel(Arc.Center, Arc.Angle.StartAngle);
            (bool isAvailable, RayModel reflectedRay) = GeoLib.FindReflectedRay(startRay, seg);
            if (!isAvailable) {
                Debug.LogAssertionFormat("Arc FindReflectedRay Failed. Arc {0}, Ray {1}, seg {2}",
                                         Arc.Description(), startRay.Description(), seg.Description());
                continue;
            }

            double reflectedCollisionAngle = GeoLib.CalculateAngle(reflectedRay.Origin, model.MinCollisionPoint);

            ArcModel reflectedArc = new ArcModel(reflectedRay.Origin, Arc.Radius, reflectedCollisionAngle, 0f);
            VoiceWave reflectedWave = CreateWaveFromSelf(reflectedArc, collisionWall);
            double collisionAngle = GeoLib.CalculateAngle(Arc.Center, model.MinCollisionPoint);

            Debug.LogFormat("Arc ReflectedArc reflected. Arc {0}, \r\n Reflected {1}, collisionAngle {2}, stAngle {3}, edAngle {4}",
                             Arc.Description(), reflectedArc.Description(), collisionAngle, Arc.Angle.StartAngle, Arc.Angle.EndAngle);
            bool shouldDestroy = false;
            if (GeoLib.isEqual(collisionAngle, Arc.Angle.StartAngle)) {
                ArcLinkModel linkModel = new ArcLinkModel();
                SetRightLink(linkModel);
                reflectedWave.SetRightLink(linkModel);
                linkModel.LeftArc = Arc;
                linkModel.RightArc = reflectedArc;
                linkModel.SetRightExpand();
                linkModel.LinkWall = collisionWall;
                WallBanList.Add(collisionWall);
                    
                Debug.LogFormat("Arc ReflectedArc Right Link Created.");
                reflectedWave.gameObject.name = "RightReflected";
                //Debug.Break();
            } else if (GeoLib.isEqual(collisionAngle, Arc.Angle.EndAngle)) {
                ArcLinkModel linkModel = new ArcLinkModel();
                SetLeftLink(linkModel);
                reflectedWave.SetLeftLink(linkModel);
                linkModel.LeftArc = reflectedArc;
                linkModel.RightArc = Arc;
                linkModel.SetLeftExpand();
                linkModel.LinkWall = collisionWall;
                WallBanList.Add(collisionWall);

                Debug.LogFormat("Arc ReflectedArc Left Link Created.");
                reflectedWave.gameObject.name = "LeftReflected";
                //Debug.Break();
            } else {
                double rangeOne = GeoLib.NormalizeAngle(collisionAngle - Arc.Angle.StartAngle);
                ArcModel splitArcOne = new ArcModel(Arc.Center, Arc.Radius, Arc.Angle.StartAngle, rangeOne);
                double rangeTwo = GeoLib.NormalizeAngle(Arc.Angle.EndAngle - collisionAngle);
                Debug.LogFormat("Arc ReflectedArc reflected. One {0} Two {1} compare {2}, calc {3}",
                                 rangeOne, rangeTwo, GeoLib.GreaterOrEqualThan(Arc.Angle.EndAngle, collisionAngle), Arc.Angle.EndAngle - collisionAngle);
                ArcModel splitArcTwo = new ArcModel(Arc.Center, Arc.Radius, collisionAngle, rangeTwo);

                ArcLinkModel twoLinkModel = new ArcLinkModel();
                VoiceWave splitWaveTwo = CreateWaveFromSelf(splitArcTwo, collisionWall);
                reflectedWave.SetRightLink(twoLinkModel);
                splitWaveTwo.SetRightLink(twoLinkModel);
                twoLinkModel.LeftArc = splitArcTwo;
                twoLinkModel.RightArc = reflectedArc;
                twoLinkModel.SetRightExpand();
                twoLinkModel.LinkWall = collisionWall;

                ArcLinkModel oneLinkModel = new ArcLinkModel();
                VoiceWave splitWaveOne = CreateWaveFromSelf(splitArcOne, collisionWall);
                reflectedWave.SetLeftLink(oneLinkModel);
                splitWaveOne.SetLeftLink(oneLinkModel);
                oneLinkModel.LeftArc = reflectedArc;
                oneLinkModel.RightArc = splitArcOne;
                oneLinkModel.SetLeftExpand();
                oneLinkModel.LinkWall = collisionWall;

                Debug.LogFormat("Arc Splitted: Right {0}, \r\nLeft {1}.", splitArcTwo.Description(), splitArcOne.Description());

                Debug.LogFormat("Arc twoLinkModel {0}", twoLinkModel.Description());
                Debug.LogFormat("Arc oneLinkModel {0}", oneLinkModel.Description());
                //Debug.Break();
                shouldDestroy = true;
            }
            collisionWall.OnWaveDidCollide(reflectedWave);
            if (shouldDestroy) {
                Debug.LogFormat("Arc Double Reflected; Self {0} destroyed.", Arc.Description());
                Discard();
            }
        }

        ExamineMicrophoneCollision(beforeRadius, afterRadius);
        ExamineNpcCollision(beforeRadius, afterRadius);
        ExamineSwitchCollision(beforeRadius, afterRadius);

        if (RuntimeStrength <= 0) {
            Destroy(gameObject);
        }
    }

    private void ExamineMicrophoneCollision(double beforeRadius, double afterRadius) {
        if (_microphoneList.Count == 0)
            return;
        List<Microphone> hitList = new List<Microphone>();
        foreach (Microphone microphone in _microphoneList) {
            (bool collision, double dist) = GeoLib.FindPointArcCollision(microphone.transform.position, Arc);
            if (!collision) {
                continue;
            }
            if (dist - beforeRadius > -1e-5  && afterRadius - dist > -1e-5) {
                Debug.LogFormat("ExamineMicrophoneCollision hit microphone {0}", microphone.gameObject.name);
                microphone.OnVoiceWaveHit(this);
                hitList.Add(microphone);
            }
        }
        _microphoneList.RemoveAll(item => hitList.Contains(item));
    }

    private void ExamineNpcCollision(double beforeRadius, double afterRadius) {
        if (_npcList.Count == 0)
            return;
        List<NPC> hitList = new List<NPC>();
        foreach (NPC npc in _npcList) {
            (bool collision, double dist) = GeoLib.FindPointArcCollision(npc.transform.position, Arc);
            //Debug.LogFormat("ExamineNpcCollision hit npc {0} res {1} dist {2}", npc.gameObject.name, collision, dist);
            if (!collision) {
                continue;
            }
            if (dist - beforeRadius > -1e-5 && afterRadius - dist > -1e-5) {
                Debug.LogFormat("ExamineNpcCollision hit npc {0}", npc.gameObject.name);
                npc.OnVoiceWaveHit(this);
                hitList.Add(npc);
            }
        }
        _npcList.RemoveAll(item => hitList.Contains(item));
    }

    private void ExamineSwitchCollision(double beforeRadius, double afterRadius)
    {
        if (_switchList.Count == 0)
            return;
        List<Switch> hitList = new List<Switch>();
        foreach (Switch ns in _switchList)
        {
            (bool collision, double dist) = GeoLib.FindPointArcCollision(ns.transform.position, Arc);
            //Debug.LogFormat("ExamineSwitchCollision hit ns {0} res {1} dist {2}", ns.gameObject.name, collision, dist);
            if (!collision)
            {
                continue;
            }
            if (dist - beforeRadius > -1e-5 && afterRadius - dist > -1e-5)
            {
                Debug.LogFormat("ExamineSwitchCollision hit npc {0}", ns.gameObject.name);
                ns.OnVoiceWaveHit(this);
                hitList.Add(ns);
            }
        }
        _switchList.RemoveAll(item => hitList.Contains(item));
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

    public bool IsStaticWave() {
        return LeftLink == null && RightLink == null;
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
        GameObject newWaveObject = Instantiate(AssetHelper.instance.WavePrefab, transform.parent);
        VoiceWave newWave = newWaveObject.GetComponent<VoiceWave>();
        newWave.transform.position = newArc.Center;
        newWave.Arc = newArc;
        newWave.MaximumRadius = MaximumRadius;
        newWave.WallBanList.Add(collisionWall);

        newWave.InitialStrength = RuntimeStrength;
        newWave.RuntimeStrength = RuntimeStrength;
        newWave.MinimumExpansionSpeed = MinimumExpansionSpeed;
        newWave.ExpansionSpeed = ExpansionSpeed;
        newWave.StrengthRatio = StrengthRatio;
        newWave.SpeedRatio = SpeedRatio;

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

    private void UpdateParams()
    {
        RuntimeStrength -= Time.deltaTime * StrengthRatio;
        ExpansionSpeed = Math.Max(MinimumExpansionSpeed, RuntimeStrength * SpeedRatio);
    }

    private void UpdateUIByType()
    {
        GetComponent<LineRenderer>().material = AssetHelper.instance.WaveMaterials[(int)SoundType];
    }
}
