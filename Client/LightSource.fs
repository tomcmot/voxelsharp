module LightSource
open Silk.NET.OpenGL
open System.Numerics

[<Struct>]
type LightSource =
    {
        mutable color: Vector3
        mutable transform: Matrix4x4
    }

let lightPositions = [|
        Vector3( 0.7f,  0.2f,  2.0f)
        Vector3( 2.3f, -3.3f, -4.0f)
        Vector3(-4.0f,  2.0f, -12.0f)
        Vector3( 0.0f,  0.0f, -3.0f)
|]

let scenePointLights : array<Shader.PointLight> =
    [|
    {
        position= lightPositions[0]
        ambient= Vector3(0.05f, 0.05f, 0.05f)
        diffuse= Vector3(0.8f, 0.8f, 0.8f)
        specular = Vector3(1f, 1f, 1f)
        constant= 1f
        linear= 0.09f
        quadratic= 0.032f
    }
    {
        position= lightPositions[1]
        ambient= Vector3(0.05f, 0.05f, 0.05f)
        diffuse= Vector3(0.8f, 0.8f, 0.8f)
        specular = Vector3(1f, 1f, 1f)
        constant= 1f
        linear= 0.09f
        quadratic= 0.032f
    }
    {
        position= lightPositions[2]
        ambient= Vector3(0.05f, 0.05f, 0.05f)
        diffuse= Vector3(0.8f, 0.8f, 0.8f)
        specular = Vector3(1f, 1f, 1f)
        constant= 1f
        linear= 0.09f
        quadratic= 0.032f
    }
    {
        position= lightPositions[3]
        ambient= Vector3(0.05f, 0.05f, 0.05f)
        diffuse= Vector3(0.8f, 0.8f, 0.8f)
        specular = Vector3(1f, 1f, 1f)
        constant= 1f
        linear= 0.09f
        quadratic= 0.032f
    }
    |]

let lights =
      lightPositions
      |> Array.map (fun pos ->
        let world =
            Matrix4x4.CreateScale 0.2f *
            Matrix4x4.CreateTranslation pos
        {
            transform= world
            color =Vector3(1f,1f,1f)
        }
      )
let Render (context:Graphics.Context) (shader: Shader.Shader) (camera: Client.Systems.Camera) =
      context.Use shader
      context.BindVertexArray Mesh.cubeVao
      for light in lights do
        context.SetUniform (shader, "model", light.transform)
        context.SetUniform (shader, "view", camera.View())
        context.SetUniform (shader, "projection", camera.Projection ())
        context.SetUniform (shader, "color", light.color)
        context.DrawArrays(PrimitiveType.Triangles, 0, 36u )