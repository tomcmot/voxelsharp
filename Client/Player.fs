module Client.Systems.Player
open System.Numerics

let mutable position = Vector3(0f,0f,0f)
let moveSpeed = 2.5f
let Walk (delta: float32) = 
        position <- position + moveSpeed * delta * Camera.camera.front
let Strafe (delta: float32) =
        position <- position + moveSpeed * delta * Vector3.Normalize(Vector3.Cross(Camera.camera.front, Camera.camera.up))
    