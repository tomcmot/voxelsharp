open System
open System.Numerics
open System.Drawing
open Silk.NET.OpenGL
open Silk.NET.Input
open Silk.NET.Windowing

[<EntryPoint>]
let main args =
  let window = Window.Create WindowOptions.Default
  window.Title <- "Secondhand"

  window.add_Load (fun () -> 
    let gl = GL.GetApi window
    let context = new Graphics.Context (gl)
    context.CreateBuffer ()
    let sceneDirLight : Shader.DirLight = 
      {
        direction = Vector3(-0.2f, -1f, -0.3f)
        ambient = Vector3(0.05f, 0.05f, 0.05f)
        diffuse = Vector3(0.4f,0.4f,0.4f)
        specular = Vector3(0.5f,0.5f,0.5f)
      }
    let shaders: Shader.Shaders = 
      {
        cube=
          context.CreateShader
            "texture/vert.glsl" 
            "texture/frag.glsl" 
        light= 
          context.CreateShader
            "light/vert.glsl" 
            "light/frag.glsl" 
      }
    let model =
      let world = Matrix4x4.Identity
      Model.Create
        world
        {
            diffuse = context.LoadTexture "texture/crate.png"
            specular = context.LoadTexture "texture/crate_specular.png"
            shininess = 32f
          }
          shaders.cube
    
    gl.ClearColor Color.Black

    gl.Enable EnableCap.DepthTest
    gl.BlendFunc (BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha)
    
    let input = window.CreateInput()
    let keyboard = input.Keyboards.Item 0
    if keyboard <> null then
        keyboard.add_KeyDown (Client.Systems.Keyboard.keyDown window)
    for mouse in input.Mice do
      Client.Systems.Mouse.lastPosition <- mouse.Position
      mouse.Cursor.CursorMode <- CursorMode.Raw
      mouse.add_MouseMove Client.Systems.Mouse.move
      mouse.add_Scroll Client.Systems.Mouse.scroll
    
    window.add_Render(fun _ -> 
      gl.Clear(uint32 GLEnum.ColorBufferBit ||| uint32 GLEnum.DepthBufferBit)
      model.Render (context, Client.Systems.Camera.camera, sceneDirLight, LightSource.scenePointLights)
      LightSource.Render context shaders.light Client.Systems.Camera.camera
    
    )

    window.add_Update (Client.Systems.Physics.update keyboard)

    window.add_Resize(fun size ->
      gl.Viewport size
    )

    window.add_Closing(fun () ->
      gl.BindBuffer(GLEnum.ArrayBuffer, 0u)
      gl.BindBuffer(GLEnum.ElementArrayBuffer, 0u)
      gl.BindTexture(TextureTarget.Texture2D, 0u)
      gl.BindVertexArray 0u
      gl.UseProgram 0u
      (context :> IDisposable).Dispose()
    )
  )

  window.Run()
  0