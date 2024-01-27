using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class VoiceWaveLineRendererEx : MonoBehaviour {
    public int VertexCount = 40; // 4 vertices == square
    public float LineWidth = 0.2f;
    
    [Header("Runtime Variables")]
    public ArcModel arc;
    [SerializeField]
    private LineRenderer lineRenderer;

    private void Awake() {
        lineRenderer = GetComponent<LineRenderer>();
        SetupCircle();
    }

    public void SetupCircle() {
        lineRenderer.widthMultiplier = LineWidth;

        float angleRange = (float)arc.Angle.AngleRange;
        float deltaTheta = angleRange / VertexCount;
        float theta = (float)arc.Angle.StartAngle;

        //Debug.LogFormat("SetupCircle angleRange {0} deltaTheta {1} theta {2}", angleRange, deltaTheta, theta);

        lineRenderer.positionCount = VertexCount + 1;
        for (int i = 0; i < lineRenderer.positionCount; i++) {
            Vector3 pos = new Vector3((float)arc.Radius * Mathf.Cos(theta), (float)arc.Radius * Mathf.Sin(theta), 0f);
            lineRenderer.SetPosition(i, pos);
            //Debug.LogFormat("SetPosition i {0} pos {1}", i, pos);
            theta += deltaTheta;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos() {
        float angleRange = (float)arc.Angle.AngleRange;
        float deltaTheta = angleRange / VertexCount;
        float theta = (float)arc.Angle.StartAngle;

        Vector3 oldPos = transform.position;
        theta += deltaTheta;
        for (int i = 0; i < VertexCount + 1; i++) {
            Vector3 pos = new Vector3((float)arc.Radius * Mathf.Cos(theta), (float)arc.Radius * Mathf.Sin(theta), 0f);
            Gizmos.DrawLine(oldPos, transform.position + pos);
            oldPos = transform.position + pos;

            theta += deltaTheta;
        }
    }
#endif
}