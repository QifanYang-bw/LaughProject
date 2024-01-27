using System.Drawing;
using UnityEngine;
using static UnityEditor.PlayerSettings;

[RequireComponent(typeof(Wall))]
public class WallRendererEx : MonoBehaviour {
    public SegmentModel seg;

#if UNITY_EDITOR
    private void OnDrawGizmos() {
        Gizmos.color = UnityEngine.Color.magenta;
        Gizmos.DrawLine(seg.PointA, seg.PointB);
        Gizmos.color = UnityEngine.Color.white;
    }
#endif
}