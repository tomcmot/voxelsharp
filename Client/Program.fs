open System
open System.Numerics
open System.Drawing
open Silk.NET.OpenGL
open Silk.NET.Input
open Silk.NET.Windowing

let private lightPositions = [|
        Vector3( 0.7f,  0.2f,  2.0f)
        Vector3( 2.3f, -3.3f, -4.0f)
        Vector3(-4.0f,  2.0f, -12.0f)
        Vector3( 0.0f,  0.0f, -3.0f)
|]


[<EntryPoint>]
let main args =
  let window = Window.Create WindowOptions.Default
  window.Title <- "Secondhand"

  window.add_Load (fun () -> 
    let gl = GL.GetApi window
    let mutable camera = Camera.Create ()

    let sceneDirLight : Shader.DirLight = 
      {
        direction = Vector3(-0.2f, -1f, -0.3f)
        ambient = Vector3(0.05f, 0.05f, 0.05f)
        diffuse = Vector3(0.4f,0.4f,0.4f)
        specular = Vector3(0.5f,0.5f,0.5f)
      }
    let scenePointLights : array<Shader.PointLight> =
      [|
        {
          position= lightPositions[0]
          ambient= Vector3(0.05f, 0.05f, 0.05f)
          diffuse= Vector3(0.8f, 0.8f, 0.8f)
          specular = Vector3(1f, 1f, 1f)
          constant= 1f
          linear= 0.09f
          quadratic= 0.032f
        }
        {
          position= lightPositions[1]
          ambient= Vector3(0.05f, 0.05f, 0.05f)
          diffuse= Vector3(0.8f, 0.8f, 0.8f)
          specular = Vector3(1f, 1f, 1f)
          constant= 1f
          linear= 0.09f
          quadratic= 0.032f
        }
        {
          position= lightPositions[2]
          ambient= Vector3(0.05f, 0.05f, 0.05f)
          diffuse= Vector3(0.8f, 0.8f, 0.8f)
          specular = Vector3(1f, 1f, 1f)
          constant= 1f
          linear= 0.09f
          quadratic= 0.032f
        }
        {
          position= lightPositions[3]
          ambient= Vector3(0.05f, 0.05f, 0.05f)
          diffuse= Vector3(0.8f, 0.8f, 0.8f)
          specular = Vector3(1f, 1f, 1f)
          constant= 1f
          linear= 0.09f
          quadratic= 0.032f
        }
      |]
    let model =
      let world = Matrix4x4.Identity
      Model.Create
        gl 
        world
        {
            diffuse = Texture.Load gl "texture/crate.png"
            specular = Texture.Load gl "texture/crate_specular.png"
            shininess = 32f
          }
          camera.position
    let lights =
      lightPositions
      |> Array.map (fun pos ->
        let world =
            Matrix4x4.CreateScale 0.2f *
            Matrix4x4.CreateTranslation pos
        LightSource.Create
          gl
          world
          (Vector3(1f,1f,1f))
      )


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
      model.Render (camera, sceneDirLight, scenePointLights)
      lights |> Array.iter (fun light -> light.Render camera)
    
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