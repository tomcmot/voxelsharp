module Model
open Silk.NET.OpenGL
open System.Numerics
open System

type Model =
    {
        vertices: array<float32>
        vao: uint32
        context: GL
        mutable shader: Shader.Shader
        mutable transform: Matrix4x4
    }
    member this.Render (camera: Camera.Camera) =
      this.shader.Use()
      this.context.BindVertexArray this.vao
      this.shader.SetUniformM4 ("model", this.transform)
      this.shader.SetUniformM4 ("view", camera.View())
      this.shader.SetUniformM4 ("projection", camera.Projection ())
      this.context.DrawArrays(PrimitiveType.Triangles, 0, 36u )

    member this.UpdateUniform (k, v) =
        this.shader.uniforms <- this.shader.uniforms |> Map.add k v

    member this.UpdateTransform t =
        this.transform <- t
let private vertices = 
    [|
        -0.5f; -0.5f; -0.5f;  0.0f;  0.0f; -1.0f;
         0.5f; -0.5f; -0.5f;  0.0f;  0.0f; -1.0f;
         0.5f;  0.5f; -0.5f;  0.0f;  0.0f; -1.0f;
         0.5f;  0.5f; -0.5f;  0.0f;  0.0f; -1.0f;
        -0.5f;  0.5f; -0.5f;  0.0f;  0.0f; -1.0f;
        -0.5f; -0.5f; -0.5f;  0.0f;  0.0f; -1.0f;
        -0.5f; -0.5f;  0.5f;  0.0f;  0.0f;  1.0f;
         0.5f; -0.5f;  0.5f;  0.0f;  0.0f;  1.0f;
         0.5f;  0.5f;  0.5f;  0.0f;  0.0f;  1.0f;
         0.5f;  0.5f;  0.5f;  0.0f;  0.0f;  1.0f;
        -0.5f;  0.5f;  0.5f;  0.0f;  0.0f;  1.0f;
        -0.5f; -0.5f;  0.5f;  0.0f;  0.0f;  1.0f;
        -0.5f;  0.5f;  0.5f; -1.0f;  0.0f;  0.0f;
        -0.5f;  0.5f; -0.5f; -1.0f;  0.0f;  0.0f;
        -0.5f; -0.5f; -0.5f; -1.0f;  0.0f;  0.0f;
        -0.5f; -0.5f; -0.5f; -1.0f;  0.0f;  0.0f;
        -0.5f; -0.5f;  0.5f; -1.0f;  0.0f;  0.0f;
        -0.5f;  0.5f;  0.5f; -1.0f;  0.0f;  0.0f;
         0.5f;  0.5f;  0.5f;  1.0f;  0.0f;  0.0f;
         0.5f;  0.5f; -0.5f;  1.0f;  0.0f;  0.0f;
         0.5f; -0.5f; -0.5f;  1.0f;  0.0f;  0.0f;
         0.5f; -0.5f; -0.5f;  1.0f;  0.0f;  0.0f;
         0.5f; -0.5f;  0.5f;  1.0f;  0.0f;  0.0f;
         0.5f;  0.5f;  0.5f;  1.0f;  0.0f;  0.0f;
        -0.5f; -0.5f; -0.5f;  0.0f; -1.0f;  0.0f;
         0.5f; -0.5f; -0.5f;  0.0f; -1.0f;  0.0f;
         0.5f; -0.5f;  0.5f;  0.0f; -1.0f;  0.0f;
         0.5f; -0.5f;  0.5f;  0.0f; -1.0f;  0.0f;
        -0.5f; -0.5f;  0.5f;  0.0f; -1.0f;  0.0f;
        -0.5f; -0.5f; -0.5f;  0.0f; -1.0f;  0.0f;
        -0.5f;  0.5f; -0.5f;  0.0f;  1.0f;  0.0f;
         0.5f;  0.5f; -0.5f;  0.0f;  1.0f;  0.0f;
         0.5f;  0.5f;  0.5f;  0.0f;  1.0f;  0.0f;
         0.5f;  0.5f;  0.5f;  0.0f;  1.0f;  0.0f;
        -0.5f;  0.5f;  0.5f;  0.0f;  1.0f;  0.0f;
        -0.5f;  0.5f; -0.5f;  0.0f;  1.0f;  0.0f;
    |]

let private stride = uint32 (6 * sizeof<float32>)

let Create (gl:GL) transform normal isLight =
    let vbo = gl.GenBuffer()
    let vao = gl.GenVertexArray()
    let shader =
        if isLight then
            Shader.Create "light/vert.glsl" "light/frag.glsl" gl Map.empty
        else
            Shader.Create 
                "texture/vert.glsl" 
                "texture/frag.glsl" 
                gl 
                (
                    Map.ofList 
                        [
                            "lightColor", Shader.Triple (1.0f, 1f, 1.0f)
                            "lightPos", Shader.Triple (1.2f, 1.0f, 2.0f)
                            "objectColor", Shader.Triple(1.0f, 0.5f, 0.3f)
                            "normal", Shader.Mat4 normal
                        ]
                    )

    gl.BindVertexArray vao
    gl.BindBuffer(GLEnum.ArrayBuffer, vbo)
    gl.BufferData(BufferTargetARB.ArrayBuffer, ReadOnlySpan vertices, BufferUsageARB.StaticDraw)
    gl.VertexAttribPointer(0u, 3, VertexAttribPointerType.Float, false, stride, IntPtr.Zero.ToPointer())
    gl.EnableVertexAttribArray 0u
    if not isLight then
        let normalOffset = nativeint (3 * sizeof<float32>)
        gl.VertexAttribPointer(1u, 3, VertexAttribPointerType.Float, false, stride, normalOffset.ToPointer())
        gl.EnableVertexAttribArray 1u
    {
        vertices= vertices
        vao= vao
        context= gl
        shader=shader
        transform=transform
    }