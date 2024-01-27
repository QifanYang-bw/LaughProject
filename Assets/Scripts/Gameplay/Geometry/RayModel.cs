using System;
using UnityEngine;

[Serializable]
public class RayModel {
    public Vector2 Origin;
    public double Direction;

    public RayModel(double x, double y, double direction) {
        Origin = new Vector2((float)x, (float)y);
        Direction = direction;
    }
    public RayModel(float x, float y, float direction) {
        Origin = new Vector2(x, y);
        Direction = direction;
    }
    public RayModel(Vector2 origin, double direction) {
        Origin = origin;
        Direction = direction;
    }
    public string Description() {
        return String.Format("Ray Origin {0}, Direction {1}", Origin, Direction);
    }
}
