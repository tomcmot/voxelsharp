open System
open System.Numerics
open System.Drawing
open System.IO
open Silk.NET.OpenGL
open Silk.NET.Input
open Silk.NET.Windowing
open StbImageSharp
open System.Runtime.InteropServices

[<Struct>]
type Camera = 
  {
    position: Vector3
    direction: Vector3
    front: Vector3
    up: Vector3
    yaw: float32
    pitch: float32
    zoom: float32
  }

let mutable camera = 
  {
    position = Vector3 (0f, 0f, 3f)
    direction = Vector3(0f, 0f, 0f)
    front = Vector3(0f,0f,-1f)
    up = Vector3(0f,1f,0f)
    yaw = -90f
    pitch = 0f
    zoom = 45f
  }

let ortho = Matrix4x4.CreateOrthographic (800f, 600f, 0.1f, 100f)
let proj = Matrix4x4.CreatePerspectiveFieldOfView (Single.DegreesToRadians 45f, 800f/600f, 0.1f, 100f)
let model = Matrix4x4.CreateRotationX (Single.DegreesToRadians -55f) * Matrix4x4.CreateRotationY(Single.DegreesToRadians -24f) * Matrix4x4.CreateRotationZ(Single.DegreesToRadians 25f)
let view (camera: Camera) = 
  Matrix4x4.CreateLookAt (camera.position, camera.position + camera.front, camera.up)


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
    let keyboard = input.Keyboards.Item 0
    if keyboard <> null then
        keyboard.add_KeyDown (fun keyboard key code ->
            if key = Key.Escape then
                window.Close()
        )
    for mouse in input.Mice do
      let mutable lastPosition = mouse.Position
      mouse.Cursor.CursorMode <- CursorMode.Raw
      mouse.add_MouseMove (fun _ position -> 
        let lookSensitivity = 0.1f
        let xOffset = (position.X - lastPosition.X) * lookSensitivity
        let yOffset = (position.Y - lastPosition.Y) * lookSensitivity
        lastPosition <- position
        camera <- {
          camera with 
            yaw = camera.yaw + xOffset; 
            pitch = Math.Clamp(camera.pitch + yOffset, -89f, 89f)
          }
        let direction = Vector3(
              cos(Single.DegreesToRadians camera.yaw) * cos (Single.DegreesToRadians camera.pitch),
              sin (Single.DegreesToRadians camera.pitch),
              sin (Single.DegreesToRadians camera.yaw) * cos (Single.DegreesToRadians camera.pitch)
            )

        camera <- {
          camera with
            direction = direction
            front = Vector3.Normalize direction
        }
              
      )
      mouse.add_Scroll (fun _ scroll -> 
        camera <- {camera with zoom = Math.Clamp (camera.zoom - scroll.Y, 1.0f, 45f)}
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
      setUniformM4 gl viewLoc (view camera)
      let projLoc = gl.GetUniformLocation (shaderHandle, "projection")
      setUniformM4 gl projLoc proj
      gl.DrawArrays(PrimitiveType.Triangles, 0, 36u )
    
    )

    window.add_Update (fun delta ->
      let moveSpeed = float32 (2.5 * delta)
      if keyboard.IsKeyPressed Key.W then
        camera <- {camera with position = camera.position + moveSpeed * camera.front}
      if keyboard.IsKeyPressed Key.S then
        camera <- {camera with position = camera.position - moveSpeed * camera.front}
      if keyboard.IsKeyPressed Key.A then
        //Move left
        camera <- {camera with position = camera.position - Vector3.Normalize(Vector3.Cross(camera.front, camera.up)) * moveSpeed}
      if keyboard.IsKeyPressed Key.D then
        //Move left
        camera <- {camera with position = camera.position + Vector3.Normalize(Vector3.Cross(camera.front, camera.up)) * moveSpeed}
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