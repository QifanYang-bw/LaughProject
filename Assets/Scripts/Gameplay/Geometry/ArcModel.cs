using System;
using UnityEngine;
using UnityEngine.Assertions;

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
        Assert.IsFalse(AngleRange < 0, "AngleRange < 0");
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
        return Angle.AngleRange < 1e-3f;
    }

    public bool IsContainPoint(Vector2 point)
    {
        // Calculate the vector from the center of the pie to the point
        Vector2 fromCenterToPoint = point - Center;

        // Calculate the distance from the center of the pie to the point
        float distanceToCenter = fromCenterToPoint.magnitude;

        // Check if the point is within the pie's radius
        if (distanceToCenter > Radius)
        {
            return false;
        }

        return IsPointInsideArcRange(point);
    }

    public bool IsPointInsideArcRange(Vector2 point)
    {
        // Calculate the vector from the center of the pie to the point
        Vector2 fromCenterToPoint = point - Center;

        // Calculate the angle from the positive x-axis to the vector
        float angle = Vector2.SignedAngle(Vector2.right, fromCenterToPoint);

        // Ensure the angle is positive
        if (angle < 0)
        {
            angle += 360;
        }

        var startAngle = GeoLib.ConvertRadiansToDegrees(Angle.StartAngle);
        if (startAngle < 0)
        {
            startAngle += 360;
        }

        var endAngle = startAngle + GeoLib.ConvertRadiansToDegrees(Angle.AngleRange);
        if (endAngle < 0)
        {
            endAngle += 360;
        }

        bool IsWithinRange(double x, double l, double r)
        {
            return l <= x && x <= r;
        }

        if (IsWithinRange(360f, startAngle, endAngle))
        {
            return IsWithinRange(angle, startAngle, 360) || IsWithinRange(angle, 0, endAngle - 360);
        }

        return IsWithinRange(angle, startAngle, endAngle);
    }
}