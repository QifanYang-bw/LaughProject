using System;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.UIElements;

public enum ArcEndPointStatus {
    ArcEndPointStatusUnknown   = 0,
    ArcEndPointStatusExpanding = 1,
    ArcEndPointStatusShrinking = 2
}

//[Serializable]
public class ArcLinkModel {
    public bool isBroken;
    public ArcModel LeftArc, RightArc;
    public Wall LinkWall;
    public ArcEndPointStatus LeftStatus, RightStatus;

    public void SetLeftExpand() {
        LeftStatus = ArcEndPointStatus.ArcEndPointStatusExpanding;
        RightStatus = ArcEndPointStatus.ArcEndPointStatusShrinking;
    }
    public void SetRightExpand() {
        RightStatus = ArcEndPointStatus.ArcEndPointStatusExpanding;
        LeftStatus = ArcEndPointStatus.ArcEndPointStatusShrinking;
    }

    public ArcEndPointStatus ArcStatus(ArcModel Arc) {
        if (Arc == LeftArc) {
            return LeftStatus;
        }
        if (Arc == RightArc) {
            return RightStatus;
        }
        return ArcEndPointStatus.ArcEndPointStatusUnknown;
    }
    public void ChainAngle(double DeltaAngle) {
        if (LeftStatus == ArcEndPointStatus.ArcEndPointStatusExpanding) {
            LeftArc.Angle.AngleRange += DeltaAngle;
            RightArc.Angle.AngleRange -= DeltaAngle;
        } else {
            LeftArc.Angle.StartAngle = GeoLib.NormalizeAngle(LeftArc.Angle.StartAngle + DeltaAngle);
            LeftArc.Angle.AngleRange -= DeltaAngle;
            RightArc.Angle.StartAngle = GeoLib.NormalizeAngle(RightArc.Angle.StartAngle - DeltaAngle);
            RightArc.Angle.AngleRange += DeltaAngle;
        }
        LeftArc.Load();
        RightArc.Load();
    }
    public string Description() {
        return String.Format("Arc Left {0}, Right {1}", LeftStatus, RightStatus);
    }
}