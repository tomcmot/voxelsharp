open System
open FSharp.NativeInterop
open System.Numerics
open System.Drawing
open System.IO
open Silk.NET.OpenGL
open Silk.NET.Input
open Silk.NET.Windowing
open StbImageSharp

let setUniformM4 (gl:GL) transform value = 
      let mutable auxVal = value
      let valPtr = NativePtr.toNativeInt<Matrix4x4> &&auxVal
      let ptr = NativePtr.ofNativeInt<float32> valPtr 
      gl.UniformMatrix4 (transform,  1u, false, ptr)

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
  if code <> int GLEnum.True then failwith (sprintf "Error compiling shader %d" shader)
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
        { x=  0.5f; y=  0.5f; z= 0.0f; s= 1.0f; t= 1.0f;};
        { x=  0.5f; y= -0.5f; z= 0.0f; s= 1.0f; t= 0.0f;};
        { x= -0.5f; y= -0.5f; z= 0.0f; s= 0.0f; t= 0.0f;};
        { x= -0.5f; y=  0.5f; z= 0.0f; s= 0.0f; t= 1.0f;};  
    |]
  let indices = [|
    0u; 1u; 3u; 1u; 2u; 3u;
  |]
  let window = Window.Create WindowOptions.Default
  window.Title <- "Secondhand"

  window.add_Load (fun () -> 
    let gl = GL.GetApi window
    let ebo = gl.GenBuffer ()
    let vbo = gl.GenBuffer()
    let vao = gl.GenVertexArray()
    let shaderHandle = createShader "vert.glsl" "frag.glsl" gl

    gl.ClearColor Color.CornflowerBlue

    gl.BindVertexArray vao

    gl.BindBuffer(GLEnum.ArrayBuffer, vbo)
    gl.BufferData(BufferTargetARB.ArrayBuffer, ReadOnlySpan vertices, BufferUsageARB.StaticDraw)

    gl.BindBuffer (GLEnum.ElementArrayBuffer, ebo)
    gl.BufferData(BufferTargetARB.ElementArrayBuffer, ReadOnlySpan indices, BufferUsageARB.StaticDraw)

    let stride = uint32 (5 * sizeof<float32>)

    gl.VertexAttribPointer(0u, 3, VertexAttribPointerType.Float, false, stride, IntPtr.Zero.ToPointer())
    gl.EnableVertexAttribArray 0u
    let offset = IntPtr.Add(IntPtr.Zero, 3*sizeof<float32>)
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
    gl.Enable EnableCap.Blend
    gl.BlendFunc (BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha)
    window.add_Render(fun _ -> 
      gl.Clear(uint32 GLEnum.ColorBufferBit)
      gl.UseProgram shaderHandle
      gl.BindVertexArray vao
      gl.BindTexture (TextureTarget.Texture2D, texture)
      let transform = gl.GetUniformLocation (shaderHandle, "uTransform" )
      let t1 = Matrix4x4.Identity * Matrix4x4.CreateTranslation(Vector3(0.5f, 0.5f, 0f))
      setUniformM4 gl transform t1
    //   gl.PolygonMode(GLEnum.FrontAndBack, PolygonMode.Line)
      gl.DrawElements(PrimitiveType.Triangles, uint indices.Length, DrawElementsType.UnsignedInt, 0n.ToPointer() )
      let t2 = Matrix4x4.Identity * Matrix4x4.CreateTranslation(Vector3(-0.5f, -0.5f, 0f))
      setUniformM4 gl transform t2
      gl.DrawElements(PrimitiveType.Triangles, uint indices.Length, DrawElementsType.UnsignedInt, 0n.ToPointer() )

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