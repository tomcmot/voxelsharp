namespace Client.Systems
open System
open System.Numerics

[<Struct>]
type Camera =
  {
    mutable position: Vector3
    mutable direction: Vector3
    mutable front: Vector3
    up: Vector3
    mutable yaw: float32
    mutable pitch: float32
    mutable zoom: float32
  }
    member this.View () =
        Matrix4x4.CreateLookAt (this.position, this.position + this.front, this.up)
    member this.Projection () =
        Matrix4x4.CreatePerspectiveFieldOfView (Single.DegreesToRadians this.zoom, 800f/600f, 0.1f, 200f)

module Camera =
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

  let Rotate (xOffset, yOffset) =
        camera.yaw <- camera.yaw + xOffset
        camera.pitch <- Math.Clamp(camera.pitch - yOffset, -89f, 89f)
        let yaw = Single.DegreesToRadians camera.yaw
        let pitch = Single.DegreesToRadians camera.pitch
        let direction = Vector3(
              cos yaw * cos pitch,
              sin pitch,
              sin yaw * cos pitch
            )
        camera.direction <- direction
        camera.front <- Vector3.Normalize direction
  
  let Zoom offset =
        camera.zoom <- Math.Clamp (camera.zoom + offset, 1.0f, 45f)
