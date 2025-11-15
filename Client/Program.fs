open System
open System.Numerics
open System.Drawing
open System.IO
open Silk.NET.OpenGL
open Silk.NET.Input
open Silk.NET.Windowing
open StbImageSharp
open System.Runtime.InteropServices

let ortho = Matrix4x4.CreateOrthographic (800f, 600f, 0.1f, 100f)
let proj = Matrix4x4.CreatePerspectiveFieldOfView (Single.DegreesToRadians 45f, 800f/600f, 0.1f, 100f)
let model = Matrix4x4.CreateRotationX (Single.DegreesToRadians -55f) * Matrix4x4.CreateRotationY(Single.DegreesToRadians -24f) * Matrix4x4.CreateRotationZ(Single.DegreesToRadians 25f)
let view = Matrix4x4.CreateTranslation (Vector3 (0f, 0f, -3f))

let setUniformM4 (gl:GL) transform value =
    let mutable auxVal = value    
    let s = ReadOnlySpan &auxVal
    let span = MemoryMarshal.Cast<Matrix4x4, float32> s
    gl.UniformMatrix4 (transform, false, span)

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

let compileShader path (kind:ShaderType) (gl: GL) =
  let source = File.ReadAllText path
  let shader = gl.CreateShader kind
  gl.ShaderSource (shader, source)
  gl.CompileShader shader
  let code = gl.GetShader(shader, GLEnum.CompileStatus)
  if code <> int GLEnum.True then 
    let msg = gl.GetShaderInfoLog shader

    failwith (sprintf "Error compiling shader %d:\n%s\n" shader msg)
  shader

let createShader vertPath fragPath (gl: GL) =
  let vertexShader = compileShader vertPath ShaderType.VertexShader gl
  let fragmentShader = compileShader fragPath ShaderType.FragmentShader gl
  let handle = gl.CreateProgram()
  gl.AttachShader(handle, vertexShader)
  gl.AttachShader(handle, fragmentShader)
  gl.LinkProgram handle

  let code = gl.GetProgram(handle, GLEnum.LinkStatus)
  if code <> int GLEnum.True then failwith (sprintf "Error linking program %d" handle)

  gl.DetachShader(handle, vertexShader)
  gl.DetachShader(handle, fragmentShader)
  gl.DeleteShader vertexShader
  gl.DeleteShader fragmentShader

  handle

[<Struct>]
type Vertex = 
  {
    x:float32;
    y:float32;
    z:float32;
    s:float32;
    t:float32
  }

[<EntryPoint>]
let main args =
  let vertices = 
    [|
      {x= -0.5f; y= -0.5f; z= -0.5f; s=0.0f; t= 0.0f}
      {x=  0.5f; y= -0.5f; z= -0.5f; s=1.0f; t=0.0f}
      {x=  0.5f; y=  0.5f; z= -0.5f; s=1.0f; t=1.0f}
      {x=  0.5f; y=  0.5f; z= -0.5f; s=1.0f; t=1.0f}
      {x= -0.5f; y=  0.5f; z= -0.5f; s=0.0f; t= 1.0f}
      {x= -0.5f; y= -0.5f; z= -0.5f; s=0.0f; t= 0.0f}
      {x= -0.5f; y= -0.5f; z=  0.5f; s=0.0f; t= 0.0f}
      {x=  0.5f; y= -0.5f; z= 0.5f;  s=1.0f; t=0.0f}
      {x=  0.5f; y=  0.5f; z= 0.5f;  s=1.0f; t=1.0f}
      {x=  0.5f; y=  0.5f; z= 0.5f;  s=1.0f; t=1.0f}
      {x= -0.5f; y=  0.5f; z=  0.5f; s=0.0f; t= 1.0f}
      {x= -0.5f; y= -0.5f; z=  0.5f; s=0.0f; t= 0.0f}
      {x= -0.5f; y=  0.5f; z=  0.5f; s=1.0f; t= 0.0f}
      {x= -0.5f; y=  0.5f; z= -0.5f; s=1.0f; t= 1.0f}
      {x= -0.5f; y= -0.5f; z= -0.5f; s=0.0f; t= 1.0f}
      {x= -0.5f; y= -0.5f; z= -0.5f; s=0.0f; t= 1.0f}
      {x= -0.5f; y= -0.5f; z=  0.5f; s=0.0f; t= 0.0f}
      {x= -0.5f; y=  0.5f; z=  0.5f; s=1.0f; t= 0.0f}
      {x=  0.5f; y=  0.5f; z= 0.5f;  s=1.0f; t=0.0f}
      {x=  0.5f; y=  0.5f; z= -0.5f; s=1.0f; t=1.0f}
      {x=  0.5f; y= -0.5f; z= -0.5f; s=0.0f; t=1.0f}
      {x=  0.5f; y= -0.5f; z= -0.5f; s=0.0f; t=1.0f}
      {x=  0.5f; y= -0.5f; z= 0.5f;  s=0.0f; t=0.0f}
      {x=  0.5f; y=  0.5f; z= 0.5f;  s=1.0f; t=0.0f}
      {x= -0.5f; y= -0.5f; z= -0.5f; s=0.0f; t= 1.0f}
      {x=  0.5f; y= -0.5f; z= -0.5f; s=1.0f; t=1.0f}
      {x=  0.5f; y= -0.5f; z= 0.5f;  s=1.0f; t=0.0f}
      {x=  0.5f; y= -0.5f; z= 0.5f;  s=1.0f; t=0.0f}
      {x= -0.5f; y= -0.5f; z=  0.5f; s=0.0f; t= 0.0f}
      {x= -0.5f; y= -0.5f; z= -0.5f; s=0.0f; t= 1.0f}
      {x= -0.5f; y=  0.5f; z= -0.5f; s=0.0f; t= 1.0f}
      {x=  0.5f; y=  0.5f; z= -0.5f; s=1.0f; t=1.0f}
      {x=  0.5f; y=  0.5f; z= 0.5f;  s=1.0f; t=0.0f}
      {x=  0.5f; y=  0.5f; z= 0.5f;  s=1.0f; t=0.0f}
      {x= -0.5f; y=  0.5f; z=  0.5f; s=0.0f; t= 0.0f}
      {x= -0.5f; y=  0.5f; z= -0.5f; s=0.0f; t= 1.0f}
      |]
  let window = Window.Create WindowOptions.Default
  window.Title <- "Secondhand"

  window.add_Load (fun () -> 
    let gl = GL.GetApi window
    let vbo = gl.GenBuffer()
    let vao = gl.GenVertexArray()
    let shaderHandle = createShader "vert.glsl" "frag.glsl" gl

    gl.ClearColor Color.CornflowerBlue

    gl.BindVertexArray vao

    gl.BindBuffer(GLEnum.ArrayBuffer, vbo)
    gl.BufferData(BufferTargetARB.ArrayBuffer, ReadOnlySpan vertices, BufferUsageARB.StaticDraw)


    let stride = uint32 (5 * sizeof<float32>)

    gl.VertexAttribPointer(0u, 3, VertexAttribPointerType.Float, false, stride, IntPtr.Zero.ToPointer())
    gl.EnableVertexAttribArray 0u
    let offset = nativeint (3*sizeof<float32>)
    printf "offset: %A\n" offset
    gl.VertexAttribPointer(1u, 2, VertexAttribPointerType.Float, false, stride, offset.ToPointer())
    gl.EnableVertexAttribArray 1u
    let input = window.CreateInput()
    for keyboard in input.Keyboards do
        keyboard.add_KeyDown (fun keyboard key code ->
            if key = Key.Escape then
                window.Close()
        )

    let texture = LoadTexture gl
    gl.Enable ( EnableCap.DepthTest)
    gl.BlendFunc (BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha)
    window.add_Render(fun _ -> 
      gl.Clear(uint32 GLEnum.ColorBufferBit ||| uint32 GLEnum.DepthBufferBit)
      gl.UseProgram shaderHandle
      gl.BindVertexArray vao
      gl.BindTexture (TextureTarget.Texture2D, texture)
      let modelLoc = gl.GetUniformLocation (shaderHandle, "model")
      setUniformM4 gl modelLoc model
      let viewLoc = gl.GetUniformLocation (shaderHandle, "view")
      setUniformM4 gl viewLoc view
      let projLoc = gl.GetUniformLocation (shaderHandle, "projection")
      setUniformM4 gl projLoc proj
      gl.DrawArrays(PrimitiveType.Triangles, 0, 36u )
    
    )

    window.add_Resize(fun size ->
      gl.Viewport size
    )

    window.add_Closing(fun () ->
      gl.BindBuffer(GLEnum.ArrayBuffer, 0u)
      gl.BindBuffer(GLEnum.ElementArrayBuffer, 0u)
      gl.BindTexture(TextureTarget.Texture2D, 0u)
      gl.BindVertexArray 0u
      gl.UseProgram 0u
      gl.DeleteBuffer vbo
      gl.DeleteVertexArray vao
      gl.DeleteProgram shaderHandle
    )
  )

  window.Run()
  0