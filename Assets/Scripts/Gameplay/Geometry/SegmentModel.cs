using System;
using UnityEngine;

[Serializable]
public class SegmentModel {
    public Vector2 PointA, PointB;

    public SegmentModel(Vector2 PointA, Vector2 PointB) {
        this.PointA = PointA;
        this.PointB = PointB;
    }
    public string Description() {
        return String.Format("Segment PointA {0}, PointB {1}", PointA, PointB);
    }
}