module LightSource
open Silk.NET.OpenGL
open System.Numerics

type LightSource =
    {
        vao: uint32
        context: GL
        shader: Shader.Shader
        mutable color: Vector3
        mutable transform: Matrix4x4
    }
    member this.Render (camera: Camera.Camera) =
      this.shader.Use()
      this.context.BindVertexArray this.vao
      this.shader.SetUniform ("model", this.transform)
      this.shader.SetUniform ("view", camera.View())
      this.shader.SetUniform ("projection", camera.Projection ())
      this.shader.SetUniform ("color", this.color)
      this.context.DrawArrays(PrimitiveType.Triangles, 0, 36u )

    member this.UpdateTransform t =
        this.transform <- t

let Create (gl:GL) transform color  =
    let shader =
        Shader.Create 
            "light/vert.glsl" 
            "light/frag.glsl" 
            gl
    {
        vao= Mesh.CubeVao gl
        context= gl
        shader=shader
        color= color
        transform=transform
    }