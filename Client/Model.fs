module Model
open Silk.NET.OpenGL
open System.Numerics
let private normal x =
      let success, r = Matrix4x4.Invert x
      if not success then failwith "could not invert"
      Matrix4x4.Transpose r
type Model =
    {
        vao: uint32
        context: GL
        shader: Shader.Shader
        mutable transform: Matrix4x4
        mutable normal: Matrix4x4
        mutable material: Shader.Material
    }
    member this.Render (camera: Client.Systems.Camera, dirLight :Shader.DirLight, pointLights: array<Shader.PointLight>) =
      this.shader.Use()
      this.context.BindVertexArray this.vao
      this.shader.SetUniform ("model", this.transform)
      this.shader.SetUniform ("view", camera.View())
      this.shader.SetUniform ("projection", camera.Projection ())
      this.shader.SetUniform ("normal", this.normal)
      this.shader.SetUniform ("viewPos", camera.position)
      this.shader.SetMaterial this.material
      this.shader.SetDirLight dirLight
      this.shader.SetPointLights pointLights
      this.context.DrawArrays(PrimitiveType.Triangles, 0, 36u )
    member this.UpdateTransform t =
        this.transform <- t
        this.normal <- normal t

let Create (gl:GL) transform material =
    let shader =
        Shader.Create 
            "texture/vert.glsl" 
            "texture/frag.glsl" 
            gl
    {
        vao= Mesh.CubeVao gl
        context= gl
        shader=shader
        transform=transform
        normal= normal transform
        material = material
    }