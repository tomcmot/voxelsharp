module Graphics
open System
open System.Numerics
open Silk.NET.OpenGL
open System.Runtime.InteropServices
open System.IO
open StbImageSharp

type Context(gl:GL) =
    let mutable shaders = []
    let mutable textures = []
    let mutable buffers = []

    member _.CreateShader vert frag =
        let handle = Shader.Create vert frag gl
        shaders <- handle :: shaders
        handle

    member _.CreateBuffer (vertices) =
        let vao, vbo = Mesh.Create gl vertices
        buffers <- vao :: vbo :: buffers
        vao, vbo

    member _.UpdateBuffer vao vbo (vertices: float32 array) =
      gl.BindVertexArray vao
      gl.BindBuffer (BufferTargetARB.ArrayBuffer, vbo)
      gl.BufferData (BufferTargetARB.ArrayBuffer, ReadOnlySpan vertices, BufferUsageARB.StaticDraw)

    member _.CreateShaderBuffer () =
      let ssbo = gl.GenBuffer()
      gl.BindBuffer (BufferTargetARB.ShaderStorageBuffer, ssbo)
      gl.BindBufferBase (BufferTargetARB.ShaderStorageBuffer, 0u, ssbo)
      buffers <- ssbo :: buffers
      ssbo
      
    member _.LoadTexture path =
        let inputbytes= File.ReadAllBytes path
        let result = ImageResult.FromMemory (inputbytes, ColorComponents.RedGreenBlueAlpha)
        let handle = gl.GenTexture()
        gl.BindTexture (TextureTarget.Texture2D, handle)
        gl.TexImage2D (TextureTarget.Texture2D, 0, InternalFormat.Rgba, uint result.Width, uint result.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ReadOnlySpan result.Data)
        gl.TextureParameter(handle, TextureParameterName.TextureWrapS, int TextureWrapMode.Repeat)
        gl.TextureParameter(handle, TextureParameterName.TextureWrapT, int TextureWrapMode.Repeat)
        gl.TextureParameter(handle, TextureParameterName.TextureMinFilter, int TextureMinFilter.NearestMipmapNearest)
        gl.TextureParameter(handle, TextureParameterName.TextureMagFilter, int TextureMagFilter.Nearest)

        gl.GenerateMipmap TextureTarget.Texture2D
        gl.BindTexture(TextureTarget.Texture2D, 0u)

        handle
    member _.BindVertexArray vao =
        gl.BindVertexArray vao

    member _.DrawArrays (mode : PrimitiveType, first, count) = gl.DrawArrays(mode, first, count)
    member _.Use program =
      gl.UseProgram program
    member _.SetUniform (program, k: string, i: int) =
      let loc = gl.GetUniformLocation (program, k)
      gl.Uniform1(loc, i)
    member _.SetUniform (program, k: string, f: float32) =
      let loc = gl.GetUniformLocation (program, k)
      gl.Uniform1(loc , f)
    member _.SetUniform (program, k: string, v2: Vector2) =
      gl.Uniform2(gl.GetUniformLocation (program, k), v2)
    member _.SetUniform (program, k: string, v3: Vector3) =
      let loc = gl.GetUniformLocation (program, k)
      if loc = 0 then failwith "loc is 0"
      gl.Uniform3(loc, v3)
    member _.SetUniform (program, key: string, m4) =
        let location = gl.GetUniformLocation (program, key)
        let mutable auxVal = m4    
        let s = ReadOnlySpan &auxVal
        let span = MemoryMarshal.Cast<Matrix4x4, float32> s
        gl.UniformMatrix4 (location, false, span)

    member this.SetMaterial (program, mat: Shader.Material) =
      this.SetUniform(program, "material.diffuse", 0)
      this.SetUniform(program, "material.specular", 1)
      this.SetUniform(program, "material.shininess", mat.shininess)
      gl.ActiveTexture TextureUnit.Texture0
      gl.BindTexture (TextureTarget.Texture2D, mat.diffuse)
      gl.ActiveTexture TextureUnit.Texture1
      gl.BindTexture (TextureTarget.Texture2D, mat.specular)
      
    member this.SetDirLight (program, light: Shader.DirLight) =
      this.SetUniform(program, "dirLight.direction", light.direction)
      this.SetUniform(program, "dirLight.ambient", light.ambient)
      this.SetUniform(program, "dirLight.diffuse", light.diffuse)
      this.SetUniform(program, "dirLight.specular", light.specular)
    member this.SetPointLights (program, buffer, lights: array<Shader.PointLight>) =
      this.SetUniform (program, "numLights", lights.Length)
      gl.BindBuffer (BufferTargetARB.ShaderStorageBuffer, buffer)
      let span = ReadOnlySpan lights
      gl.BufferData (BufferTargetARB.ShaderStorageBuffer, span, BufferUsageARB.DynamicDraw)
      
    interface IDisposable with
        member _.Dispose () = 
            shaders |> List.iter (fun shader -> gl.DeleteProgram shader)
            textures |> List.iter (fun tex -> gl.DeleteTexture tex)
            buffers |> List.iter (fun buffer -> gl.DeleteBuffer buffer)