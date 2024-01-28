using System;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using Unity.Mathematics;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.U2D.IK;
using UnityEngine.UIElements;

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
        while (SmallerThan(angle, 0))
            angle += 2 * Math.PI;
        while (GreaterOrEqualThan(angle, 2 * Math.PI))
            angle -= 2 * Math.PI;
        return angle;
    }
    public static double ConvertRadiansToDegrees(double radians) {
        return radians * (180.0 / Math.PI);
    }
    public static double ConvertDegreesToRadians(double radians) {
        return radians * (Math.PI * 2) / 360.0;
    }
    public static bool IsAngleWithinArc(ArcModel arc, double angle) {
        double StartAngle = arc.Angle.StartAngle;
        double EndAngle = arc.Angle.EndAngle;

        bool res;
        if (Math.Abs(Math.PI * 2f - arc.Angle.AngleRange) < 1e-6) {
            res = true;
        } else {
            if (EndAngle - StartAngle >= -1e-6) {
                res = angle >= StartAngle && angle <= EndAngle;
            } else {
                // A 360 flip is included
                double angleExt = angle + Math.PI * 2f;
                double endAngleExt = EndAngle + Math.PI * 2f;
                res = angle >= StartAngle && angle <= endAngleExt || angleExt >= StartAngle && angleExt <= endAngleExt;
            }
        }
        // Debug.LogFormat("IsAngleWithinArc {0} StartAngle {1} EndAngle {2} angle {3}", res, StartAngle, EndAngle, angle);
        return res;
    }

    public static (bool, Vector2) FindSegToSegIntersection(SegmentModel LineA, SegmentModel LineB) {
        Vector2 linePoint1 = LineA.PointA;
        Vector2 lineVec1 = LineA.PointB - LineA.PointA;
        Vector2 linePoint2 = LineB.PointA;
        Vector2 lineVec2 = LineB.PointB - LineB.PointA;

        Vector2 lineVec3 = linePoint2 - linePoint1;
        Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
        Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

        float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

        //is coplanar, and not parrallel
        if (Mathf.Abs(planarFactor) < 1e-5f && crossVec1and2.sqrMagnitude > 1e-5f) {
            float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
            Vector2 intersection = linePoint1 + (lineVec1 * s);

            if (intersection.x - Math.Min(LineA.PointA.x, LineA.PointB.x) > -1e-6 && Math.Max(LineA.PointA.x, LineA.PointB.x) - intersection.x > -1e-6 &&
                intersection.y - Math.Min(LineA.PointA.y, LineA.PointB.y) > -1e-6 && Math.Max(LineA.PointA.y, LineA.PointB.y) - intersection.y > -1e-6 && 
                intersection.x - Math.Min(LineB.PointA.x, LineB.PointB.x) > -1e-6 && Math.Max(LineB.PointA.x, LineB.PointB.x) - intersection.x > -1e-6 &&
                intersection.y - Math.Min(LineB.PointA.y, LineB.PointB.y) > -1e-6 && Math.Max(LineB.PointA.y, LineB.PointB.y) - intersection.y > -1e-6) {
                return (true, intersection);
            } else {
                return (false, Vector2.zero);
            }
        } else {
            return (false, Vector2.zero);
        }
    }
    public static (bool, Vector2) RayLineSegmentIntersection(RayModel ray, SegmentModel lineSegment) {
        float farDist = 1e5f;
        Vector2 farPoint = new Vector2((float)Math.Cos(ray.Direction), (float)Math.Sin(ray.Direction)) * farDist;
        SegmentModel raySeg = new SegmentModel(ray.Origin, farPoint);

        return FindSegToSegIntersection(lineSegment, raySeg);
    }
    public static (bool, Vector2, double, double) PerpendicularIntersection(Vector2 p, SegmentModel seg) {
        Vector2 bc = seg.PointB - seg.PointA;
        Vector2 ba = p - seg.PointA;
        Vector2 nBC = bc.normalized;
        float dBP = Vector3.Dot(ba, nBC);
        Vector2 intersection = seg.PointA + dBP * nBC;

        // Debug.LogFormat("PerpendicularIntersection intersection {0}, seg {1}", intersection, seg.Description());

        if (intersection.x - Math.Min(seg.PointA.x, seg.PointB.x) > -1e-6 && Math.Max(seg.PointA.x, seg.PointB.x) - intersection.x > -1e-6 &&
            intersection.y - Math.Min(seg.PointA.y, seg.PointB.y) > -1e-6 && Math.Max(seg.PointA.y, seg.PointB.y) - intersection.y > -1e-6) {
            double angle = CalculateAngle(p, intersection);
            double dist = (intersection - p).magnitude;
            return (true, intersection, angle, dist);
        } else {
            return (false, Vector2.zero, 0, 0);
        }
    }
    public static (bool, Vector2, double, Vector2, double) ArcCollisionRadiusWithSegment(ArcModel arc, SegmentModel seg) {
        Debug.LogFormat("ArcCollisionRadiusWithSegment seg {0}, arc {1}", seg.Description(), arc.Description());

        RayModel rayA = new RayModel(arc.Center, arc.Angle.StartAngle);
        (bool rayACollision, Vector2 rayAPoint) = RayLineSegmentIntersection(rayA, seg);
        double rayADist = Math.Sqrt(SquaredDistance(rayA.Origin, rayAPoint));
        Debug.LogFormat("ArcCollisionRadiusWithSegment rayA {0} \r\n Collision {1}, Dist {2}, Point {3}", rayA.Description(), rayACollision, rayADist, rayAPoint);

        RayModel rayB = new RayModel(arc.Center, arc.Angle.EndAngle);
        (bool rayBCollision, Vector2 rayBPoint) = RayLineSegmentIntersection(rayB, seg);
        double rayBDist = Math.Sqrt(SquaredDistance(rayB.Origin, rayBPoint));
        Debug.LogFormat("ArcCollisionRadiusWithSegment rayB {0} \r\n Collision {1}, Dist {2}, Point {3}", rayB.Description(), rayBCollision, rayBDist, rayBPoint);

        (bool perpCollision, Vector2 perpIntersectPoint, double perpAngle, double perpDist) = PerpendicularIntersection(arc.Center, seg);
        if (perpCollision) {
            bool isPerpInSegment = IsAngleWithinArc(arc, perpAngle);
            Debug.LogFormat("ArcCollisionRadiusWithSegment perp Collision {0}, isWithin {1}, Angle {2}, Dist {3}", perpCollision, isPerpInSegment, perpAngle, perpDist);
            perpCollision = perpCollision && isPerpInSegment;
        } else {
            Debug.LogFormat("ArcCollisionRadiusWithSegment perp Collision {0}, Angle {1}, Dist {2}", perpCollision, perpAngle, perpDist);
        }
        // The circumstance in which the segment's two endpoint become the point of contact
        bool pointACollision = IsAngleWithinArc(arc, CalculateAngle(arc.Center, seg.PointA));
        double pointADist = 0;
        if (pointACollision) {
            pointADist = Vector2.Distance(seg.PointA, arc.Center);
            Debug.LogFormat("ArcCollisionRadiusWithSegment pointA Collision {0}, Angle {1}, point {2}, \r\n stAngle {3}, edAngle {4}",
                            pointACollision, CalculateAngle(arc.Center, seg.PointA), seg.PointA, arc.Angle.StartAngle, arc.Angle.EndAngle);
        }
        bool pointBCollision = IsAngleWithinArc(arc, CalculateAngle(arc.Center, seg.PointB));
        double pointBDist = 0;
        if (pointBCollision) {
            pointBDist = Vector2.Distance(seg.PointB, arc.Center);
            Debug.LogFormat("ArcCollisionRadiusWithSegment pointB Collision {0}, Angle {1}, point {2}, \r\n stAngle {3}, edAngle {4}",
                             pointBCollision, CalculateAngle(arc.Center, seg.PointB), seg.PointB, arc.Angle.StartAngle, arc.Angle.EndAngle);
        }

        if (!rayACollision && !rayBCollision && !perpCollision && !pointACollision && !pointBCollision) {
            Debug.LogWarningFormat("ArcCollisionRadiusWithSegment Point Not Found: arc {0}, seg {1}", arc.Description(), seg.Description());
            return (false, Vector2.zero, double.PositiveInfinity, Vector2.zero, double.PositiveInfinity);
        }
        double minDist = double.PositiveInfinity;
        Vector2 minPoint = Vector2.zero;
        if (rayACollision && rayADist < minDist) {
            minDist = rayADist;
            minPoint = rayAPoint;
        }
        if (rayBCollision && rayBDist < minDist) {
            minDist = rayBDist;
            minPoint = rayBPoint;
        }
        if (perpCollision && perpDist < minDist) {
            minDist = perpDist;
            minPoint = perpIntersectPoint;
        }
        if (pointACollision && pointADist < minDist) {
            minDist = pointADist;
            minPoint = seg.PointA;
        }
        if (pointBCollision && pointBDist < minDist) {
            minDist = pointBDist;
            minPoint = seg.PointA;
        }

        double maxDist = double.NegativeInfinity;
        Vector2 maxPoint = Vector2.zero;
        if (rayACollision && rayADist > maxDist) {
            maxDist = rayADist;
            maxPoint = rayAPoint;
        }
        if (rayBCollision && rayBDist > maxDist) {
            maxDist = rayBDist;
            maxPoint = rayBPoint;
        }
        if (perpCollision && perpDist > maxDist) {
            maxDist = perpDist;
            maxPoint = perpIntersectPoint;
        }
        if (pointACollision && pointADist > maxDist) {
            maxDist = pointADist;
            maxPoint = seg.PointA;
        }
        if (pointBCollision && pointBDist > maxDist) {
            maxDist = pointBDist;
            maxPoint = seg.PointA;
        }

        Debug.LogWarningFormat("ArcCollisionRadiusWithSegment Point Found: arc {0}, seg {1} \r\n minPos {2}, minDist {3}, maxPos {4}, maxDist {5}",
                                arc.Description(), seg.Description(), minPoint, minDist, maxPoint, maxDist);
        return (true, minPoint, minDist, maxPoint, maxDist);
    }

    public static (bool, Vector2) FindReflectionPoint(Vector2 point, SegmentModel seg) {
        // Calculate the vector from linePoint1 to linePoint2
        Vector2 lineVector = seg.PointB - seg.PointA;
        if (Math.Abs(lineVector.sqrMagnitude) < 1e-5) {
            return (false, Vector2.zero);
        }
        // Calculate the vector from linePoint1 to the point
        Vector2 pointVector = point - seg.PointA;

        // Project pointVector onto lineVector
        float projectionFactor = Vector2.Dot(pointVector, lineVector) / lineVector.sqrMagnitude;
        Vector2 projectionVector = projectionFactor * lineVector;

        // Calculate the vector from the point to the intersection point
        Vector2 intersectionVector = projectionVector - pointVector;

        // Calculate the reflection point
        Vector2 reflectedPoint = point + 2 * intersectionVector;

        return (true, reflectedPoint);
    }
    public static (bool, RayModel) FindReflectedRay(RayModel ray, SegmentModel seg) {
        // Find the reflection of the ray's origin
        (bool isAvailable, Vector2 reflectedOrigin) = FindReflectionPoint(ray.Origin, seg);
        if (!isAvailable) {
            return (false, ray);
        }

        // Calculate the normal vector of the line segment
        Vector2 lineVector = seg.PointB - seg.PointA;
        Vector2 normal = new Vector2(-lineVector.y, lineVector.x).normalized;

        // Calculate the incident vector
        Vector2 incident = new Vector2((float)Math.Cos(ray.Direction), (float)Math.Sin(ray.Direction));

        // Calculate the reflected direction using the formula: R = I - 2 * dot(I, N) * N
        Vector2 reflectedDirectionVector = incident - 2 * Vector2.Dot(incident, normal) * normal;
        double reflectedDirection = Math.Atan2(reflectedDirectionVector.y, reflectedDirectionVector.x);

        // Create the reflected ray
        RayModel reflectedRay = new RayModel(reflectedOrigin, reflectedDirection);
        return (true, reflectedRay);
    }

    public static List<Vector2> FindCircleLineCollision(Vector2 center, float radius, Vector2 p1, Vector2 p2) {
        List<Vector2> collisionPoints = new List<Vector2>();
        float a, b, c;
        float bb4ac;
        float mu1;
        float mu2;

        //  get the distance between X and Z on the segment
        Vector2 dp = new Vector2();
        dp.x = p2.x - p1.x;
        dp.y = p2.y - p1.y;
        //   I don't get the math here
        a = dp.x * dp.x + dp.y * dp.y;
        b = 2 * (dp.x * (p1.x - center.x) + dp.y * (p1.y - center.y));
        c = center.x * center.x + center.y * center.y;
        c += p1.x * p1.x + p1.y * p1.y;
        c -= 2 * (center.x * p1.x + center.y * p1.y);
        c -= radius * radius;
        bb4ac = b * b - 4 * a * c;
        if (Mathf.Abs(a) < float.Epsilon || bb4ac < 0) {
            //  line does not intersect
            return collisionPoints;
        }
        mu1 = (-b + Mathf.Sqrt(bb4ac)) / (2 * a);
        mu2 = (-b - Mathf.Sqrt(bb4ac)) / (2 * a);
        collisionPoints.Add(new Vector2(p1.x + mu1 * (p2.x - p1.x), p1.y + mu1 * (p2.y - p1.y)));
        collisionPoints.Add(new Vector2(p1.x + mu2 * (p2.x - p1.x), p1.y + mu2 * (p2.y - p1.y)));
        return collisionPoints;
    }

    public static bool IsPointOnLineSegment(Vector2 point, Vector2 linePoint1, Vector2 linePoint2) {
        float minX = Mathf.Min(linePoint1.x, linePoint2.x);
        float maxX = Mathf.Max(linePoint1.x, linePoint2.x);
        float minY = Mathf.Min(linePoint1.y, linePoint2.y);
        float maxY = Mathf.Max(linePoint1.y, linePoint2.y);

        return point.x >= minX && point.x <= maxX && point.y >= minY && point.y <= maxY;
    }

    public static (bool, Vector2) FindOneArcSegmentCollision(ArcModel arc, SegmentModel seg) {
        List<Vector2> collisionPointList = GeoLib.FindCircleLineCollision(arc.Center, (float)arc.Radius, seg.PointA, seg.PointB);
        bool found = false;
        Vector2 collisionPoint = Vector2.zero;
        if (collisionPointList.Count > 0) {
            foreach (Vector2 collisionCirclePoint in collisionPointList) {
                double pointAngle = GeoLib.CalculateAngle(arc.Center, collisionCirclePoint);
                if (IsAngleWithinArc(arc, pointAngle) && IsPointOnLineSegment(collisionCirclePoint, seg.PointA, seg.PointB)) {
                    found = true;
                    collisionPoint = collisionCirclePoint;
                }
            }
        }
        return (found, collisionPoint);
    }

    public static bool isPointWithinArc(Vector2 point, ArcModel arc) {
        double pointAngle = GeoLib.CalculateAngle(arc.Center, point);
        return IsAngleWithinArc(arc, pointAngle);
    }

    public static (bool, double) FindPointArcCollision(Vector2 point, ArcModel arc) {
        if (isPointWithinArc(point, arc)) {
            return (true, Math.Sqrt(SquaredDistance(arc.Center, point)));
        } else {
            return (false, double.PositiveInfinity);
        }
    }

    public static bool isEqual(double a, double b) {
        return Math.Abs(a - b) < 1e-3;
    }
    public static bool isEqual(float a, float b) {
        return Math.Abs(a - b) < 1e-3;
    }
    public static bool SmallerOrEqualThan(double d1, double d2) {
        return d2 - d1 > -1e-3;
    }
    public static bool GreaterOrEqualThan(double d1, double d2) {
        return d1 - d2 > -1e-3;
    }
    public static bool SmallerThan(double d1, double d2) {
        return d2 - d1 > -1e-3;
    }
    public static bool GreaterThan(double d1, double d2) {
        return d1 - d2 > -1e-3;
    }
}