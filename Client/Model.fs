module Model
open Silk.NET.OpenGL
open System.Numerics
open System
open System.IO
open StbImageSharp

type Model =
    {
        vertices: array<float32>
        vao: uint32
        context: GL
        shader: Shader.Shader
        transform: Matrix4x4
        texture: uint32
    }
    member this.Render (camera: Camera.Camera) =
      this.shader.Use()
      this.context.BindVertexArray this.vao
      this.context.BindTexture (TextureTarget.Texture2D, this.texture)
      this.shader.SetUniformM4 ("model", this.transform)
      this.shader.SetUniformM4 ("view", camera.View())
      this.shader.SetUniformM4 ("projection", camera.Projection ())
      this.context.DrawArrays(PrimitiveType.Triangles, 0, 36u )

let private LoadTexture (gl:GL) =
    let inputbytes= File.ReadAllBytes "silk.png"
    printf "length: %i\n" inputbytes.Length
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
let Create (gl:GL) (vertices: float32 array) prefix stride transform =
    let vbo = gl.GenBuffer()
    let vao = gl.GenVertexArray()
    let shader = Shader.Create (prefix  + "vert.glsl") (prefix + "frag.glsl") gl
    gl.BindVertexArray vao

    gl.BindBuffer(GLEnum.ArrayBuffer, vbo)
    gl.BufferData(BufferTargetARB.ArrayBuffer, ReadOnlySpan vertices, BufferUsageARB.StaticDraw)
    gl.VertexAttribPointer(0u, 3, VertexAttribPointerType.Float, false, stride, IntPtr.Zero.ToPointer())
    gl.EnableVertexAttribArray 0u
    let offset = nativeint (3*sizeof<float32>)
    gl.VertexAttribPointer(1u, 2, VertexAttribPointerType.Float, false, stride, offset.ToPointer())
    gl.EnableVertexAttribArray 1u
    let texture = LoadTexture gl

    gl.Enable EnableCap.DepthTest
    gl.BlendFunc (BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha)
    {
        vertices= vertices
        vao= vao
        context= gl
        shader=shader
        transform=transform
        texture=texture
    }

let private cubeVertices = 
    [|
      -0.5f; -0.5f; -0.5f; 0.0f; 0.0f;
       0.5f; -0.5f; -0.5f; 1.0f; 0.0f;
       0.5f;  0.5f; -0.5f; 1.0f; 1.0f;
       0.5f;  0.5f; -0.5f; 1.0f; 1.0f;
      -0.5f;  0.5f; -0.5f; 0.0f; 1.0f;
      -0.5f; -0.5f; -0.5f; 0.0f; 0.0f;
      -0.5f; -0.5f;  0.5f; 0.0f; 0.0f;
       0.5f; -0.5f;  0.5f; 1.0f; 0.0f;
       0.5f;  0.5f;  0.5f; 1.0f; 1.0f;
       0.5f;  0.5f;  0.5f; 1.0f; 1.0f;
      -0.5f;  0.5f;  0.5f; 0.0f; 1.0f;
      -0.5f; -0.5f;  0.5f; 0.0f; 0.0f;
      -0.5f;  0.5f;  0.5f; 1.0f; 0.0f;
      -0.5f;  0.5f; -0.5f; 1.0f; 1.0f;
      -0.5f; -0.5f; -0.5f; 0.0f; 1.0f;
      -0.5f; -0.5f; -0.5f; 0.0f; 1.0f;
      -0.5f; -0.5f;  0.5f; 0.0f; 0.0f;
      -0.5f;  0.5f;  0.5f; 1.0f; 0.0f;
       0.5f;  0.5f;  0.5f; 1.0f; 0.0f;
       0.5f;  0.5f; -0.5f; 1.0f; 1.0f;
       0.5f; -0.5f; -0.5f; 0.0f; 1.0f;
       0.5f; -0.5f; -0.5f; 0.0f; 1.0f;
       0.5f; -0.5f;  0.5f; 0.0f; 0.0f;
       0.5f;  0.5f;  0.5f; 1.0f; 0.0f;
      -0.5f; -0.5f; -0.5f; 0.0f; 1.0f;
       0.5f; -0.5f; -0.5f; 1.0f; 1.0f;
       0.5f; -0.5f;  0.5f; 1.0f; 0.0f;
       0.5f; -0.5f;  0.5f; 1.0f; 0.0f;
      -0.5f; -0.5f;  0.5f; 0.0f; 0.0f;
      -0.5f; -0.5f; -0.5f; 0.0f; 1.0f;
      -0.5f;  0.5f; -0.5f; 0.0f; 1.0f;
       0.5f;  0.5f; -0.5f; 1.0f; 1.0f;
       0.5f;  0.5f;  0.5f; 1.0f; 0.0f;
       0.5f;  0.5f;  0.5f; 1.0f; 0.0f;
      -0.5f;  0.5f;  0.5f; 0.0f; 0.0f;
      -0.5f;  0.5f; -0.5f; 0.0f; 1.0f;
      |]
let CreateCube (gl:GL) transform = 
    Create gl cubeVertices "" (uint32 (5 * sizeof<float32>)) transform 