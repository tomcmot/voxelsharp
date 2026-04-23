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
        mutable transform: Matrix4x4
        mutable normal: Matrix4x4
        mutable material: Shader.Material
    }
    member this.Render (context: Graphics.Context, camera: Client.Systems.Camera, dirLight :Shader.DirLight, pointLights: array<Shader.PointLight>) =
      context.Use this.shader
      context.BindVertexArray Mesh.cubeVao
      context.SetUniform (this.shader, "model", this.transform)
      context.SetUniform (this.shader, "view", camera.View())
      context.SetUniform (this.shader, "projection", camera.Projection ())
      context.SetUniform (this.shader, "normal", this.normal)
      context.SetUniform (this.shader, "viewPos", camera.position)
      context.SetMaterial (this.shader, this.material)
      context.SetDirLight (this.shader, dirLight)
      context.SetPointLights (this.shader, pointLights)
      context.DrawArrays(PrimitiveType.Triangles, 0, 36u )
    member this.UpdateTransform t =
        this.transform <- t
        this.normal <- normal t

let Create transform material shader =
    {
        shader=shader
        transform=transform
        normal= normal transform
        material = material
    }