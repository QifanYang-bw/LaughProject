using UnityEngine;
public class BlurEffect : PostEffectsBase {
    [Header("Radial Blur")]
    [Range(0, 1)]
    public float intensity = 0.0f;                         // 效果强度
    [Range(0f, 1f)]
    public float fadeRadius = 0.38f;                    // 半径范围内的都淡出
    [Range(0f, 1f)]
    public float sampleDistance = 0.25f;                // 径向模糊每次采样的距离
    [Range(1, 40)]
    public int blurDownSample = 40;                     // 模糊降低输出采样率

    void Start()
    {
        //找到对应的Shader文件  
        shader = Shader.Find("screenEffect/RadialBlur");
        Camera.main.depthTextureMode |= DepthTextureMode.Depth;
    }

    // 渲染屏幕
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!material)
        {
            Graphics.Blit(source, destination);
            return;
        }
        var rw = Screen.width / blurDownSample;
        var rh = Screen.height / blurDownSample;
        RenderTexture downSampleRT = RenderTexture.GetTemporary(rw, rh, 0);
        downSampleRT.filterMode = FilterMode.Bilinear;
        downSampleRT.name = "downSampleRT";
        Graphics.Blit(source, downSampleRT);
        material.SetFloat("_Intensity", intensity);
        material.SetFloat("_FadeRadius", fadeRadius);
        material.SetFloat("_SampleDistance", sampleDistance);
        material.SetTexture("_DownSampleRT", downSampleRT);
        material.SetTexture("_SrcTex", source);
        Graphics.Blit(null, destination, material, 0);
        RenderTexture.ReleaseTemporary(downSampleRT);

    }
}