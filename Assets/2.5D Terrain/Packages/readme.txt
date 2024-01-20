These packages contain the shader, shadergraph and material variations
for each Unity & ShaderGraph combination. Usually the correct package
is imported automatically at first start.

In case that failed then this table describes which package has to be
used when. Some packages can be used for multiple combinations (even
for newer Unity versions).

More details on the render pipleline versins can be found here:
HDRP: https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@latest
URP: https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest
ShaderGraph: https://docs.unity3d.com/Packages/com.unity.shadergraph@latest

+- Unity -----+- ShaderGraph ----+- Renderer -+- Package -------------------------------------+
| 2019 LTS    | ShaderGraph 7    | BuiltIn    | n.A. (ShaderGraph7 does not support BuiltIn)  |
|             | ShaderGraph 7    | URP 7      | None (the default assets are for URP)         |
|             | ShaderGraph 7    | HDRP 7     | ShaderGraph7-HDRP.unitypackage                |
|             | HLSL Shader      | BuiltIn    | Shader2019-BuiltIn.unitypackage               |
|             | HLSL Shader      | URP 7      | n.A. (please use the ShaderGraph7 shader)     |
|             | HLSL Shader      | HDRP 7     | n.A. (please use the ShaderGraph7 shader)     |
+-------------+------------------+------------+-----------------------------------------------+
| 2020 LTS    | ShaderGraph 10   | BuiltIn    | n.A. (ShaderGraph10 does not support BuiltIn) |
|             | ShaderGraph 10   | URP 10     | None (the default assets are for URP)         |
|             | ShaderGraph 10   | HDRP 10    | ShaderGraph10-HDRP.unitypackage               |
|             | HLSL Shader      | BuiltIn    | Shader2019-BuiltIn.unitypackage               |
|             | HLSL Shader      | URP 10     | n.A. (please use the ShaderGraph7 shader)     |
|             | HLSL Shader      | HDRP 10    | n.A. (please use the ShaderGraph10 shader)    |
+-------------+------------------+------------+-----------------------------------------------+
| 2021 LTS    | ShaderGraph 12   | BuiltIn    | ShaderGraph12-BuiltIn.unitypackage            |
|             | ShaderGraph 12   | URP 12     | None (the default assets are for URP)         |
|             | ShaderGraph 12   | HDRP 12    | ShaderGraph10-HDRP.unitypackage               |
|             | HLSL Shader      | BuiltIn    | Shader2019-BuiltIn.unitypackage               |
|             | HLSL Shader      | URP 12     | n.A. (please use the ShaderGraph10 shader)    |
|             | HLSL Shader      | HDRP 12    | n.A. (please use the ShaderGraph10 shader)    |
+-------------+------------------+------------+-----------------------------------------------+
| 2022 LTS    | ShaderGraph 14   | BuiltIn    | ShaderGraph12-BuiltIn.unitypackage            |
|             | ShaderGraph 14   | URP 14     | None (the default assets are for URP)         |
|             | ShaderGraph 14   | HDRP 14    | ShaderGraph10-HDRP.unitypackage               |
|             | HLSL Shader      | BuiltIn    | Shader2019-BuiltIn.unitypackage               |
|             | HLSL Shader      | URP 14     | n.A. (please use the ShaderGraph10 shader)    |
|             | HLSL Shader      | HDRP 14    | n.A. (please use the ShaderGraph10 shader)    |
+-------------+------------------+------------+-----------------------------------------------+
| 2023 LTS    | ShaderGraph 15   | BuiltIn    | Not in LTS yet, thus not yet supported.       |
|             | ShaderGraph 15   | URP 15     | It may work but it's not being tested.        |
|             | ShaderGraph 15   | HDRP 15    |                                               |
|             | HLSL Shader      | BuiltIn    |                                               |
|             | HLSL Shader      | URP 15     |                                               |
|             | HLSL Shader      | HDRP 15    |                                               |
+-------------+------------------+------------+-----------------------------------------------+
| ...         |                  |
 