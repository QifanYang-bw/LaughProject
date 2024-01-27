using System;
using Unity.Burst.Intrinsics;
using UnityEditor.VersionControl;
using UnityEngine;
using static UnityEditor.Timeline.TimelinePlaybackControls;

public class GeoLib {
    public static double DotProduct(Vector2 a, Vector2 b) {
        return a.x * b.x + a.y * b.y;
    }
    public static double SquaredDistance(Vector2 a, Vector2 b) {
        return (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y);
    }
    public static double PointToLineSegmentDistance(Vector2 p, Vector2 a, Vector2 b) {
        // If the line segment has zero length, return the distance from the Vector2 to one of the endVector2s
        if (a.x == b.x && a.y == b.y) {
            return Math.Sqrt(SquaredDistance(p, a));
        }

        // Calculate the projection of the Vector2 onto the line
        double t = DotProduct(new Vector2(p.x - a.x, p.y - a.y), new Vector2(b.x - a.x, b.y - a.y)) / SquaredDistance(a, b);

        // If the projection lies outside the line segment, return the minimum distance to the endVector2s
        if (t < 0.0) {
            return Math.Sqrt(SquaredDistance(p, a));
        } else if (t > 1.0) {
            return Math.Sqrt(SquaredDistance(p, b));
        }

        // If the projection lies inside the line segment, return the distance from the Vector2 to the line
        Vector2 projection = new Vector2(a.x + (float)t * (b.x - a.x), a.y + (float)t * (b.y - a.y));
        return Math.Sqrt(SquaredDistance(p, projection));
    }
    public static double CalculateAngle(Vector2 p1, Vector2 p2) {
        double angle = NormalizeAngle(Math.Atan2(p2.y - p1.y, p2.x - p1.x));
        return angle;
    }
    public static double NormalizeAngle(double angle) {
        while (angle < 0)
            angle += 2 * Math.PI;
        while (angle >= 2 * Math.PI)
            angle -= 2 * Math.PI;
        return angle;
    }
    public static double ConvertRadiansToDegrees(double radians) {
        return radians * (180.0 / Math.PI);
    }

    public static bool IsAngleWithinArc(ArcModel arc, double angle) {
        double StartAngle = arc.Angle.StartAngle;
        double EndAngle = arc.Angle.EndAngle;

        bool res;
        if (StartAngle < EndAngle) {
            res = angle >= StartAngle && angle <= EndAngle;
        } else {
            /// A 360 flip is included
            double angleExt = angle + Math.PI * 2f;
            double endAngleExt = EndAngle + Math.PI * 2f;
            res = angle >= StartAngle && angle <= endAngleExt || angleExt >= StartAngle && angleExt <= endAngleExt;
        }
        Debug.LogFormat("IsAngleWithinArc {0} StartAngle {1} EndAngle {2} angle {3}", res, StartAngle, EndAngle, angle);
        return res;
    }
    public static bool IsSegmentWithinArc(ArcModel arc, SegmentModel seg) {
        double segAAngle = GeoLib.CalculateAngle(arc.Center, seg.PointA);
        double segBAngle = GeoLib.CalculateAngle(arc.Center, seg.PointB);

        if (segAAngle - segBAngle > Math.PI) {
            segBAngle += Math.PI * 2f;
        }
        if (segBAngle - segAAngle > Math.PI) {
            segAAngle += Math.PI * 2f;
        }
        if (segBAngle < segAAngle) {
            (segAAngle, segBAngle) = (segBAngle, segAAngle);
        }

        segAAngle = GeoLib.NormalizeAngle(segAAngle);
        segBAngle = GeoLib.NormalizeAngle(segBAngle);

        double StartAngle = arc.Angle.StartAngle;
        double EndAngle = arc.Angle.StartAngle + arc.Angle.AngleRange;

        Debug.LogFormat("IsSegmentWithinArc segA {0} segB {1} arcA {2} arcB {3}", segAAngle, segBAngle, StartAngle, EndAngle);
        bool res;
        res = segAAngle >= StartAngle && segAAngle <= EndAngle || segBAngle >= StartAngle && segBAngle <= EndAngle ||
              segAAngle <= StartAngle && segBAngle >= EndAngle || segAAngle >= StartAngle && segBAngle >= EndAngle;
        if (EndAngle > Math.PI * 2f) {
            segAAngle += Math.PI * 2f;
            segBAngle += Math.PI * 2f;
            Debug.LogFormat("IsSegmentWithinArc new segA {0} segB {1} arcA {2} arcB {3}", segAAngle, segBAngle, StartAngle, EndAngle);
            res = res || (segAAngle >= StartAngle && segAAngle <= EndAngle || segBAngle >= StartAngle && segBAngle <= EndAngle ||
                  segAAngle <= StartAngle && segBAngle >= EndAngle || segAAngle >= StartAngle && segBAngle >= EndAngle);
        }
        return res;
    }

    public class LineSegment2D {
        public (double x, double y) PointA;
        public (double x, double y) PointB;

        public LineSegment2D(double x1, double y1, double x2, double y2) {
            PointA = (x1, y1);
            PointB = (x2, y2);
        }
    }

    public static (bool, double) RayLineSegmentIntersection(RayModel ray, SegmentModel lineSegment) {
        double r_px = ray.Origin.x;
        double r_py = ray.Origin.y;
        double r_dx = Math.Cos(ray.Direction);
        double r_dy = Math.Sin(ray.Direction);

        double s_px = lineSegment.PointA.x;
        double s_py = lineSegment.PointA.y;
        double s_dx = lineSegment.PointB.x - lineSegment.PointA.x;
        double s_dy = lineSegment.PointB.y - lineSegment.PointA.y;

        double det = (-s_dx * r_dy + r_dx * s_dy);
        if (Math.Abs(det) < 1e-9) {
            return (false, double.PositiveInfinity);
        }

        double u = (-r_dy * (r_px - s_px) + r_dx * (r_py - s_py)) / det;
        double t = (s_dx * (r_py - s_py) - s_dy * (r_px - s_px)) / det;

        Debug.LogFormat("RayLineSegmentIntersection u {0}, t {1}", u, t);

        if (t >= 0 && u >= 0 && u <= 1) {
            double distance = Math.Sqrt(r_dx * r_dx + r_dy * r_dy) * t;
            return (true, distance);
        }

        return (false, double.PositiveInfinity);
    }

    public static (bool, double, double) PerpendicularIntersection(Vector2 p, SegmentModel seg) {
        // Calculate the directional vectors of the line segment
        double dx = seg.PointB.x - seg.PointA.x;
        double dy = seg.PointB.y - seg.PointA.y;

        // Calculate the unit vector of the line segment
        double length = Math.Sqrt(dx * dx + dy * dy);
        double ux = dx / length;
        double uy = dy / length;

        // Project the point p onto the line segment
        double t = (p.x - seg.PointA.x) * ux + (p.y - seg.PointA.y) * uy;

        // Check if the projection is within the line segment
        if (t >= 0 && t <= length) {
            // Calculate the intersection point
            Vector2 intersection = new Vector2(seg.PointA.x + (float)(t * ux), seg.PointA.y + (float)(t * uy));

            // Calculate the distance between the point and the intersection
            double dxp = intersection.x - p.x;
            double dyp = intersection.y - p.y;
            double distance = Math.Sqrt(dxp * dxp + dyp * dyp);

            // Calculate the angle
            double angle = NormalizeAngle(Math.Atan2(dyp, dxp));
            Debug.LogFormat("PerpendicularIntersection dxp {0}, dyp {1}, angle {2}", dxp, dyp, angle);

            return (true, angle, distance);
        } else {
            return (false, 0, double.PositiveInfinity);
        }
    }
    public static (bool, double) ArcCollisionRadiusWithSegment(ArcModel arc, SegmentModel seg) {
        bool inArc = IsSegmentWithinArc(arc, seg);
        if (!inArc) {
            return (false, double.PositiveInfinity);
        }

        RayModel rayA = new RayModel(arc.Center, arc.Angle.StartAngle);
        (bool rayACollision, double rayADist) = RayLineSegmentIntersection(rayA, seg);
        Debug.LogFormat("ArcCollisionRadiusWithSegment rayA Collision {0}, Dist {1}", rayACollision, rayADist);
        RayModel rayB = new RayModel(arc.Center, arc.Angle.EndAngle);
        Debug.LogFormat("ArcCollisionRadiusWithSegment arc {0}, rayB {1}", arc.Description(), rayB.Description());
        (bool rayBCollision, double rayBDist) = RayLineSegmentIntersection(rayB, seg);
        Debug.LogFormat("ArcCollisionRadiusWithSegment rayB Collision {0}, Dist {1}", rayBCollision, rayBDist);
        (bool perpCollision, double perpAngle, double perpDist) = PerpendicularIntersection(arc.Center, seg);
        if (perpCollision) {
            bool isPerpInSegment = IsAngleWithinArc(arc, perpAngle);
            Debug.LogFormat("ArcCollisionRadiusWithSegment perp Collision {0}, isWithin {1}, Angle {2}, Dist {3}", perpCollision, isPerpInSegment, perpAngle, perpDist);
            perpCollision = perpCollision && isPerpInSegment;
        } else {
            Debug.LogFormat("ArcCollisionRadiusWithSegment perp Collision {0}, Angle{1}, Dist {2}", perpCollision, perpAngle, perpDist);
        }
        Debug.LogFormat("ArcCollisionRadiusWithSegment perp Collision {0}, Angle{1}, Dist {2}", perpCollision, perpAngle, perpDist);

        if (!rayACollision && !rayBCollision && !perpCollision) {
            Debug.LogAssertionFormat("ArcCollisionRadiusWithSegment pass check but point non found: arc {0}, seg {1}", arc, seg);
            return (false, double.PositiveInfinity);
        }

        double dist = double.PositiveInfinity;
        if (rayACollision && rayADist < dist) {
            dist = rayADist;
        }
        if (rayBCollision && rayBDist < dist) {
            dist = rayBDist;
        }
        if (perpCollision && perpDist < dist) {
            dist = perpDist;
        }

        return (true, dist);
    }

}