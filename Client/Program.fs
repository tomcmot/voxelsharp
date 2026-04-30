open System
open System.Numerics
open System.Drawing
open Silk.NET.OpenGL
open Silk.NET.Input
open Silk.NET.Windowing

open State
[<EntryPoint>]
let main args =
  let window = Window.Create WindowOptions.Default
  window.Title <- "Secondhand"

  window.add_Load (fun () -> 
    let gl = GL.GetApi window
    let context = new Graphics.Context (gl)
    
    let sceneDirLight : Shader.DirLight = 
      {
        direction = Vector3(-0.2f, -1f, -0.3f)
        ambient = Vector3(0.05f, 0.05f, 0.05f)
        diffuse = Vector3(0.4f,0.4f,0.4f)
        specular = Vector3(0.5f,0.5f,0.5f)
      }
    let world = WorldState context
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
      world.Render (Client.Systems.Camera.camera, sceneDirLight)
    )

    window.add_Update (fun delta ->
      world.GenerateMeshes()
      Client.Systems.Physics.update keyboard delta)

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