open System
open System.Drawing
open System.IO
open Silk.NET.OpenGL
open Silk.NET.Input
open Silk.NET.Windowing
open StbImageSharp
let private LoadTexture (gl:GL) =
    let inputbytes= File.ReadAllBytes "silk.png"
    printf "length: %i\n" inputbytes.Length
    let result = ImageResult.FromMemory (inputbytes, ColorComponents.RedGreenBlueAlpha)
    let handle = gl.GenTexture()
    gl.BindTexture (TextureTarget.Texture2D, handle)
    gl.TexImage2D (TextureTarget.Texture2D, 0, InternalFormat.Rgba, uint result.Width, uint result.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ReadOnlySpan result.Data)
    // Set the texture wrap mode to repeat.
    // The texture wrap mode defines what should happen when the texture coordinates go outside of the 0-1 range.
    // In this case, we set it to repeat. The texture will just repeatedly tile over and over again.
    // You'll notice we're using S and T wrapping here. This is OpenGL's version of the standard UV mapping you
    // may be more used to, where S is on the X-axis, and T is on the Y-axis.
    gl.TextureParameter(handle, TextureParameterName.TextureWrapS, int TextureWrapMode.Repeat)
    gl.TextureParameter(handle, TextureParameterName.TextureWrapT, int TextureWrapMode.Repeat)

    // The min and mag filters define how the texture should be sampled as it resized.
    // The min, or minification filter, is used when the texture is reduced in size.
    // The mag, or magnification filter, is used when the texture is increased in size.
    // We're using bilinear filtering here, as it produces a generally nice result.
    // You can also use nearest (point) filtering, or anisotropic filtering, which is only available on the min
    // filter.
    // You may notice that the min filter defines a "mipmap" filter as well. We'll go over mipmaps below.
    gl.TextureParameter(handle, TextureParameterName.TextureMinFilter, int TextureMinFilter.NearestMipmapNearest)
    gl.TextureParameter(handle, TextureParameterName.TextureMagFilter, int TextureMagFilter.Nearest)

    // Generate mipmaps for this texture.
    // Note: We MUST do this or the texture will appear as black (this is an option you can change but this is
    // out of scope for this tutorial).
    // What is a mipmap?
    // A mipmap is essentially a smaller version of the existing texture. When generating mipmaps, the texture
    // size is continuously halved, generally stopping once it reaches a size of 1x1 pixels. (Note: there are
    // exceptions to this, for example if the GPU reaches its maximum level of mipmaps, which is both a hardware
    // limitation, and a user defined value. You don't need to worry about this for now, so just assume that
    // the mips will be generated all the way down to 1x1 pixels).
    // Mipmaps are used when the texture is reduced in size, to produce a much nicer result, and to reduce moire
    // effect patterns.
    gl.GenerateMipmap TextureTarget.Texture2D
    // Unbind the texture as we no longer need to update it any further.
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
    //   gl.PolygonMode(GLEnum.FrontAndBack, PolygonMode.Line)
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