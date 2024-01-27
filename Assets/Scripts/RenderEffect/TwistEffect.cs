using Assets.Scripts;
using UnityEngine;

public class TwistEffect : PostEffectsBase {
    private float _wellStrengthMultiplier = 2f;

    private bool _isTwisting;
    private float _startTime;
    private float _twistStrength;
    private float _clockwise;

    // 放大强度
    [Range(-2.0f, 2.0f), Tooltip("放大强度")]
    public float zoomFactor = 0.4f;

    // 放大镜大小
    [Range(-2.0f, 2.0f), Tooltip("放大镜大小")]
    public float size = 0.08f;

    // 凸镜边缘强度
    [Range(0.0001f, 0.1f), Tooltip("凸镜边缘强度")]
    public float edgeFactor = 0.07f;

    // 遮罩中心位置
    private Vector2 pos = new Vector4(0.5f, 0.5f);

    void Start() {
        //找到对应的Shader文件  
        shader = Shader.Find("screenEffect/Zoom");
    }

    // 渲染屏幕
    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        if (material && _isTwisting)
        {
            // 把鼠标坐标传递给Shader
            material.SetVector("_Pos", pos);
            material.SetFloat("_ZoomFactor", zoomFactor);
            material.SetFloat("_EdgeFactor", edgeFactor);
            material.SetFloat("_Size", size);
            material.SetFloat("_TimeDiff", Time.time - _startTime);
            material.SetFloat("_Radius", _twistStrength);
            material.SetFloat("_Clockwise", _clockwise);
            // 渲染
            Graphics.Blit(source, destination, material);
        } else {
            Graphics.Blit(source, destination);
        }
    }

    /// <summary>
    /// 创造漩涡渲染效果
    /// </summary>
    private void TwistPlane(float x, float y, float twistStrength = 20, bool clockwise = false) {
        _isTwisting = true;
        _twistStrength = twistStrength;
        _clockwise = clockwise == true ? -1 : 1;
        pos = new Vector2(x, y);
        _startTime = Time.time;
    }
}