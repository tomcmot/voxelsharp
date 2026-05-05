module State

open System.Numerics

open Engine
open Graphics
open Shader
open Silk.NET.OpenGL

let makeOffset i =
    let struct(x,y,z) = World.idx i
    Vector3(float32 x * 16f, float32 y * 16f, float32 z *  16f)
type ChunkMesh =
    {
        mutable vertices: Vertex array
        mutable indices: uint array
        mutable lights: PointLight array
        vao: uint
        vbo: uint
        ebo: uint
    }

let private makeChunkMesh (context: Context) =
    let vao, vbo, ebo = context.CreateBuffers ()
    {
        vertices = [||]
        indices = [||]
        lights = [||]
        vao = vao
        vbo = vbo
        ebo = ebo
    }
type WorldState(context: Context) =
    let world = World.generateWorld ()    
    let material = 
        {
            diffuse = context.LoadTexture "texture/crate.png"
            specular = context.LoadTexture "texture/crate_specular.png"
            shininess = 32f
          }
    let mutable meshes = Array.init world.chunks.Length (fun _ -> makeChunkMesh context)
    let mutable dirty = Array.create world.chunks.Length 0xFFFFFFFFFFFFUL
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
    let mutable pointLights = [||]
    let MaxMeshesPerFrame = 4
    
    member _.GenerateMeshes () =
        let mutable meshed = 0
        let mutable i = 0
        while i < world.chunks.Length && meshed < MaxMeshesPerFrame do
            if dirty[i]<> 0UL then
                let chunk = world.chunks[i]
                let vertices, indices = ChunkRenderer.generateMeshGreedy (makeOffset i) chunk.chunk
                meshes[i].vertices <- vertices
                meshes[i].indices <- indices
                context.UpdateBuffer meshes[i].vao meshes[i].vbo meshes[i].ebo vertices indices
                meshes[i].lights <- ChunkRenderer.getLightPositions (makeOffset i) chunk.chunk |> genPointLights |> Array.ofSeq
                pointLights <- meshes |> Array.collect (fun m -> m.lights)
                dirty[i] <- 0UL
                meshed <- meshed + 1
            i <- i + 1
    member _.Render (camera : Client.Systems.Camera, dirLight) =
        let viewProj = camera.View () * camera.Projection ()
        let planes = AABB.extractFrustumPlanes viewProj
        context.Use shader
        context.SetUniform (shader, "model", transform)
        context.SetUniform (shader, "view", camera.View())
        context.SetUniform (shader, "projection", camera.Projection ())
        context.SetUniform (shader, "normal", normal)
        context.SetUniform (shader, "viewPos", camera.position)
        context.SetMaterial (shader, material)
        context.SetDirLight (shader, dirLight)
        context.SetPointLights (shader, shaderBuffer, pointLights)
        for i = 0 to meshes.Length - 1 do
            let mesh = meshes[i]
            let aabb = AABB.chunkBounds (makeOffset i)
            if AABB.intersectsFrustum aabb planes then
                context.BindVertexArray mesh.vao
                context.BindBuffer (BufferTargetARB.ArrayBuffer, mesh.vbo)
                context.BindBuffer (BufferTargetARB.ElementArrayBuffer, mesh.ebo)
                context.DrawElements(PrimitiveType.Triangles, uint mesh.indices.Length, DrawElementsType.UnsignedInt, 0n.ToPointer())