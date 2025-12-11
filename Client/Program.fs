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
    let mutable camera = Camera.Create ()

    let model =
      let world = Matrix4x4.Identity
      Model.Create
        gl 
        world
        {
            diffuse = Texture.Load gl "texture/crate.png"
            specular = Vector3(0.5f, 0.5f, 0.5f)
            shininess = 32f
          }
          {
            position= Vector3(1.2f, 1.0f, 2.0f)
            ambient= Vector3(0.2f,0.2f, 0.2f)
            diffuse= Vector3(0.5f,0.5f,0.5f)
            specular=Vector3(1f,1f,1f)
          }
          camera.position
    let light =
      let world =
          Matrix4x4.CreateScale 0.2f *
          Matrix4x4.CreateTranslation(Vector3(1.2f, 1.0f, 2.0f))
      LightSource.Create
        gl
        world
        (Vector3(1f,1f,1f))


    gl.ClearColor Color.Black

    gl.Enable EnableCap.DepthTest
    gl.BlendFunc (BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha)
    
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
        camera.Rotate (xOffset, yOffset)
      )
      mouse.add_Scroll (fun _ scroll -> 
        camera.Zoom scroll.Y
      )
    
    window.add_Render(fun _ -> 
      gl.Clear(uint32 GLEnum.ColorBufferBit ||| uint32 GLEnum.DepthBufferBit)
      model.Render camera
      light.Render camera
    
    )

    window.add_Update (fun delta ->
      let moveSpeed = float32 (2.5 * delta)
      if keyboard.IsKeyPressed Key.W then
        camera.Walk moveSpeed
      if keyboard.IsKeyPressed Key.S then
        camera.Walk -moveSpeed
      if keyboard.IsKeyPressed Key.A then
        camera.Strafe -moveSpeed
      if keyboard.IsKeyPressed Key.D then
        camera.Strafe moveSpeed

      model.viewPos <- camera.position
      let time = float32 window.Time
      let pos = Vector3(3f * sin time, 1f, 3f * cos time - 0.5f)
      light.UpdateTransform (
          Matrix4x4.CreateScale 0.2f * Matrix4x4.CreateTranslation(pos))
      // light.color <- Vector3(max 0.1f (sin (time * 2f)), max 0.1f (sin(time * 0.7f)), max 0.1f (sin(time * 1.3f)))
      // model.lighting.diffuse <- light.color * 0.5f
      // model.lighting.ambient <- light.color * 0.2f
      model.lighting.position <- pos
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
    )
  )

  window.Run()
  0