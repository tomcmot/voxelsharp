module State

open System.Numerics

open Engine
open Graphics
open Shader
open Silk.NET.OpenGL

let makeOffset i =
    let struct(x,y,z) = World.idx i
    Vector3(float32 x * 16f, float32 y * 16f, float32 z *  16f)

type WorldState(context: Context) =
    let world = World.generateWorld ()    
    let material = 
        {
            diffuse = context.LoadTexture "texture/crate.png"
            specular = context.LoadTexture "texture/crate_specular.png"
            shininess = 32f
          }
    let mutable meshes = Array.create world.chunks.Length [||]
    let mutable meshCached = [||]
    let mutable dirty = Array.create world.chunks.Length true

    let vao, vbo =
        context.CreateBuffer meshCached
    let shader = 
        context.CreateShader
            "texture/vert.glsl" 
            "texture/frag.glsl" 
    
    let shaderBuffer = 
        context.CreateShaderBuffer ()
    let transform = Matrix4x4.Identity
    let normal = 
        let normal x =
            let success, r = Matrix4x4.Invert x
            if not success then failwith "could not invert"
            Matrix4x4.Transpose r 
        normal transform
    let pointLights = 
        world.chunks |> Array.mapi (fun i chunk -> 
            ChunkRenderer.getLightPositions (makeOffset i) chunk.chunk
        )
        |> Array.collect Array.ofSeq
        |> genPointLights
        |> Array.ofSeq
        

    let MaxMeshesPerFrame = 4
    
    member _.GenerateMeshes () =
        let mutable meshed = 0
        let mutable i = 0
        while i < world.chunks.Length && meshed < MaxMeshesPerFrame do
            if dirty[i] then
                let chunk = world.chunks[i]
                meshes[i] <- ChunkRenderer.generateMeshGreedy (makeOffset i) chunk.chunk
                dirty[i] <- false
                meshed <- meshed + 1
            i <- i + 1
        if meshed > 0 then
            meshCached <- meshes |> Array.collect id
            context.UpdateBuffer vao vbo meshCached
            // todo update point lights
        
    member _.Render (camera : Client.Systems.Camera, dirLight) =
        context.Use shader
        context.BindVertexArray vao
        context.SetUniform (shader, "model", transform)
        context.SetUniform (shader, "view", camera.View())
        context.SetUniform (shader, "projection", camera.Projection ())
        context.SetUniform (shader, "normal", normal)
        context.SetUniform (shader, "viewPos", camera.position)
        context.SetMaterial (shader, material)
        context.SetDirLight (shader, dirLight)
        context.SetPointLights (shader, shaderBuffer, pointLights)
        context.DrawArrays(PrimitiveType.Triangles, 0, uint (meshCached.Length/8))