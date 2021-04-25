// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/Tile Shader" {
    Properties{
        _MainTex("Base (RGB)", 2D) = "white" {}
    }
        SubShader{
            Tags { "RenderType" = "Opaque" }
            LOD 150

        CGPROGRAM
        #pragma surface surf Lambert noforwardadd

        sampler2D _MainTex;

        struct Input {
            float2 uv_MainTex;
            float3 worldPos;
        };

        void vert(inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.worldPos = mul(unity_ObjectToWorld, v.vertex);
        }


        void surf(Input IN, inout SurfaceOutput o) {
            
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb;
            if (IN.worldPos.y < 0.5)
                o.Albedo = o.Albedo + (IN.worldPos.y * 0.15f);
            o.Alpha = c.a;
        }
        ENDCG
    }

        Fallback "Mobile/VertexLit"
}