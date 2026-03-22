module Client.Systems.Physics
open Silk.NET.Input
let update (keyboard: IKeyboard) (delta: float) =
    let d = float32 delta
    if keyboard.IsKeyPressed Key.W then
        Player.Walk d
        Camera.camera.position <- Player.position
    if keyboard.IsKeyPressed Key.S then
        Player.Walk -d
        Camera.camera.position <- Player.position
    if keyboard.IsKeyPressed Key.A then
        Player.Strafe -d
        Camera.camera.position <- Player.position
    if keyboard.IsKeyPressed Key.D then
        Player.Strafe d
        Camera.camera.position <- Player.position