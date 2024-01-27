using System;
using UnityEngine;

[Serializable]
public class ArcAngleModel {
    public double StartAngle, AngleRange;

    [Header("Runtime Variables")]
    public double EndAngle;

    public ArcAngleModel ArcAngleModelWithStartEnd(double StartAngle, double EndAngle) {
        double angleRange;
        if (Math.Abs(this.StartAngle - this.EndAngle) > 1e-6) {
            angleRange = GeoLib.NormalizeAngle(EndAngle - StartAngle);
        } else {
            angleRange = Math.PI * 2f;
        }
        ArcAngleModel model = new(StartAngle, angleRange);
        return model;
    }

    public ArcAngleModel(double StartAngle, double AngleRange) {
        this.StartAngle = StartAngle;
        this.AngleRange = AngleRange;
        Load();
    }

    public void Load() {
        StartAngle = GeoLib.NormalizeAngle(this.StartAngle);
        EndAngle = GeoLib.NormalizeAngle(StartAngle + AngleRange);
    }
}

[Serializable]
public class ArcModel {
    public Vector2 Center;
    public double Radius;
    public ArcAngleModel Angle;

    public void ContructModel(Vector2 Center, double Radius, double StartAngle, double AngleRange) {
        this.Center = Center;
        this.Radius = Radius;
        this.Angle = new ArcAngleModel(StartAngle, AngleRange);
    }
    public ArcModel(Vector2 Center, double Radius, double StartAngle, double AngleRange) {
        ContructModel(Center, Radius, StartAngle, AngleRange);
    }
    public ArcModel(Vector2 Center, double Radius) {
        ContructModel(Center, Radius, 0f, Math.PI * 2f);
    }
    public void Load() {
        Angle.Load();
    }
    public string Description() {
        return String.Format("Arc Center {0}, Radius {1}, StartAngle {2}, AngleRange {3}, EndAngle {4}",
                             Center, Radius, Angle.StartAngle, Angle.AngleRange, Angle.EndAngle);
    }

    public bool isEmpty() {
        return Angle.AngleRange <= 1e-5f;
    }
}