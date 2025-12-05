module Camera
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
    member this.Walk (speed: float32) = 
        this.position <- this.position + speed * this.front
    member this.Strafe (speed: float32) =
        this.position <- this.position + Vector3.Normalize(Vector3.Cross(this.front, this.up)) * speed
    member this.Zoom offset =
        this.zoom <- Math.Clamp (this.zoom + offset, 1.0f, 45f)

    member this.Rotate (xOffset, yOffset) =
        this.yaw <- this.yaw + xOffset
        this.pitch <- Math.Clamp(this.pitch + yOffset, -89f, 89f)
        let yaw = Single.DegreesToRadians this.yaw
        let pitch = Single.DegreesToRadians this.pitch
        let direction = Vector3(
              cos yaw * cos pitch,
              sin pitch,
              sin yaw * cos pitch
            )
        this.direction <- direction
        this.front <- Vector3.Normalize direction
    
    member this.View () =
        Matrix4x4.CreateLookAt (this.position, this.position + this.front, this.up)
    member this.Projection () =
        Matrix4x4.CreatePerspectiveFieldOfView (Single.DegreesToRadians this.zoom, 800f/600f, 0.1f, 100f)

let Create () =
  {
    position = Vector3 (0f, 0f, 3f)
    direction = Vector3(0f, 0f, 0f)
    front = Vector3(0f,0f,-1f)
    up = Vector3(0f,1f,0f)
    yaw = -90f
    pitch = 0f
    zoom = 45f
  } 
