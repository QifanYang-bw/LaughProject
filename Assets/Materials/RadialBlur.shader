Shader "screenEffect/RadialBlur" {
    CGINCLUDE
    #include "UnityCG.cginc"
    struct appdata {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
    };
    struct v2f {
        float4 vertex : SV_POSITION;
        float2 uv : TEXCOORD0;
    };
    float _Intensity;           // 径向效果的强度
    float _FadeRadius;          // 淡出径向效果的半径范围
    float _SampleDistance;      // 每次采样的距离
    sampler2D _DownSampleRT;    // 原图降采样后的图
    sampler2D _SrcTex;          // 原始图像纹理
    v2f vert(appdata v) {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = v.uv;
        return o;
    }
    fixed4 frag(v2f i) : SV_Target {
        const int sampleCount = 5; // 单想采样次数，乘以2就是真的总次数
        const float invSampleCount = 1.0 / ((float)sampleCount * 2);
        float2 vec = i.uv - 0.5;
        float len = length(vec);
        float fade = smoothstep(0, _FadeRadius, len); // 平滑淡出的径向效果值
        float2 stepDir = normalize(vec) * _SampleDistance; // 每次的采样步长方向
        float stepLenFactor = len * 0.1 * _Intensity; // len : 0~0.5 再乘上 0.1 就是0~0.05，越是靠近中心开，采样距离会越小，模糊度就会相对边缘来说更小
        stepDir *= stepLenFactor; // 控制步长值，stepLenFactor=len * 0.1 * _Intensity中的：0.1是经验数值可以不管，或是外部公开控制也是可以的
        fixed4 sum = 0;
        for (int it = 0; it < sampleCount; it++) {
            float2 appliedStep = stepDir * it;
            sum += tex2D(_DownSampleRT, i.uv + appliedStep); // 正向采样
            sum += tex2D(_DownSampleRT, i.uv - appliedStep); // 反向采样
        }
        sum *= invSampleCount; // 均值模糊
        return lerp(tex2D(_SrcTex, i.uv), sum, fade * _Intensity);
    }
    ENDCG
    SubShader {
        Cull Off ZWrite Off ZTest Always
        Pass {
            NAME "RADIA_BLUR"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }
}

