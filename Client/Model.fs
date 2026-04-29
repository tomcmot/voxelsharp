module Model
open Silk.NET.OpenGL
open System.Numerics
let private normal x =
      let success, r = Matrix4x4.Invert x
      if not success then failwith "could not invert"
      Matrix4x4.Transpose r
type Model =
    {
        shader: Shader.Shader
        vao: uint32
        vertices: uint
        mutable transform: Matrix4x4
        mutable normal: Matrix4x4
        mutable material: Shader.Material
    }
    member this.Render (context: Graphics.Context, camera: Client.Systems.Camera, dirLight :Shader.DirLight, shaderBuffer, pointLights: struct(int * int * int) seq) =
      context.Use this.shader
      context.BindVertexArray this.vao
      context.SetUniform (this.shader, "model", this.transform)
      context.SetUniform (this.shader, "view", camera.View())
      context.SetUniform (this.shader, "projection", camera.Projection ())
      context.SetUniform (this.shader, "normal", this.normal)
      context.SetUniform (this.shader, "viewPos", camera.position)
      context.SetMaterial (this.shader, this.material)
      context.SetDirLight (this.shader, dirLight)
      context.SetPointLights (this.shader, shaderBuffer, Shader.genPointLights pointLights)
      context.DrawArrays(PrimitiveType.Triangles, 0, this.vertices )
    member this.UpdateTransform t =
        this.transform <- t
        this.normal <- normal t

let Create vao transform material shader vertices =
    {
        vao = vao
        shader=shader
        vertices=vertices
        transform=transform
        normal= normal transform
        material = material
    }