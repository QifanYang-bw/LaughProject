using System;
using UnityEngine;

public enum ArcEndPointStatus {
    ArcEndPointStatusUnknown   = 0,
    ArcEndPointStatusExpanding = 1,
    ArcEndPointStatusShrinking = 2
}

[Serializable]
public class ArcLinkModel {
    public ArcModel LeftArc, RightArc;
    public ArcEndPointStatus LeftStatus, RightStatus;

    public ArcEndPointStatus ArcStatus(ArcModel Arc) {
        if (Arc == LeftArc) {
            return LeftStatus;
        }
        if (Arc == LeftArc) {
            return RightStatus;
        }
        return ArcEndPointStatus.ArcEndPointStatusUnknown;
    }
}