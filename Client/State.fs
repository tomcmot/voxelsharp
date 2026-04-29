module State

open System.Numerics

open Engine
open Graphics
open Shader

type WorldState(context: Context) =
    let world = World.generateWorld ()    
    let material = 
        {
            diffuse = context.LoadTexture "texture/crate.png"
            specular = context.LoadTexture "texture/crate_specular.png"
            shininess = 32f
          }
    let shader = 
        context.CreateShader
            "texture/vert.glsl" 
            "texture/frag.glsl" 
    
    let shaderBuffer = 
        context.CreateShaderBuffer ()
    let renderers = world.chunks |> Array.mapi (fun i chunk ->
        let struct(x,y,z) = World.idx i
        ChunkRenderer.Renderer(context, chunk.chunk, Vector3(float32 x * 16f, float32 y * 16f, float32 z *  16f), shader, shaderBuffer, material)
    )
    let pointLights = 
        renderers |> Array.collect (fun r -> r.PointLights)

    let MaxMeshesPerFrame = 4
    
    member _.GenerateMeshes () =
        let mutable meshed = 0
        let mutable i = 0
        while i < renderers.Length && meshed < MaxMeshesPerFrame do
            let render = renderers[i]
            if not render.Generated || render.Dirty then
                render.GenerateMesh ()
                meshed <- meshed + 1
            i <- i + 1
    
    member _.Render (camera, dirLight) =
        for renderer in renderers do
            renderer.Render camera dirLight pointLights
