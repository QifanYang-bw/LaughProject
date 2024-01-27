Shader "screenEffect/Zoom"
{
    // ---------------------------【属性】---------------------------
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        //_Radius ("_Radius",Float) =5
    }
    // ---------------------------【子着色器】---------------------------
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always
        // ---------------------------【渲染通道】---------------------------
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            //顶点输入结构体
            struct VertexInput
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            // 顶点输出结构体
            struct VertexOutput
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            // 变量申明
            sampler2D _MainTex;
            float2 _Pos;//引力场的位置中心
            float _ZoomFactor;
            float _Clockwise;
            float _EdgeFactor;
            float _Size;
            float _TimeDiff;
            float _Radius;   //扭曲的弧度（扭曲强度）
            // ---------------------------【顶点着色器】---------------------------
            VertexOutput vert (VertexInput v)
            {
                VertexOutput o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // float2 whirl(float2 uv, float2 center) {
			// 	//坐标系从左下角变换到中心
			// 	uv += float2(-0.5, -0.5);   
			// 	//弧度值 X UV点到中心的距离 X 时间变化
			// 	float tempRad=_Radius*_Time.y*length(uv - center)*5; 
			// 	//旋转矩阵
			// 	float2x2 TransMatrix = {
			// 		cos(tempRad),sin(tempRad),-sin(tempRad),cos(tempRad)
			// 	};
			// 	//对UV进行旋转变换
			// 	float2 uv2 = mul(TransMatrix, uv);
			// 	//映射回原来的坐标空间
			// 	uv2 += float2(0.5, 0.5);			
			// 	return uv2;
			// }
            float2 swirl(float2 uv, float2 center)
            {
                float2 dir = uv - center;
                float r = length(dir);
                float angle = (atan2(dir.y, dir.x) + _TimeDiff * _Radius * r) * _Clockwise;
                return center + r * float2(cos(angle), sin(angle));
            }

            // ---------------------------【片元着色器】---------------------------
            fixed4 frag (VertexOutput i) : SV_Target
            {
                
                //屏幕长宽比 缩放因子 
                float2 scale = float2(_ScreenParams.x / _ScreenParams.y, 1);
                // 放大区域中心
                float2 center = _Pos;
                float2 dir = center-i.uv;
                
                //当前像素到中心点的距离
                float dis = length(dir * scale);

                // 是否在放大镜区域
                //// fixed atZoomArea = 1-step(_Size,dis);
                // float atZoomArea = smoothstep(_Size + _EdgeFactor,_Size,dis );
                // fixed4 col = tex2D(_MainTex, i.uv + dir * _ZoomFactor * atZoomArea );

                //是否在扭曲区域
                fixed atZoomArea = 1-step(_Size,dis);
                fixed4 col = tex2D(_MainTex, i.uv);
                if(0 != atZoomArea){
                    //col = tex2D(_MainTex, whirl(i.uv, center));
                    col = tex2D(_MainTex, swirl(i.uv, center));
                }

                return col;
            }
            ENDCG
        }
    }
}

