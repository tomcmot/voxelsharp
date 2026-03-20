namespace Engine
open System
open System.Numerics

module Entity =
    type Shape =
        | Box of Vector3 * Vector3
    type Components = { position: Vector3 option array; bounds: Shape array; velocity: Vector3 option array }
    let mutable components = {position= [||]; bounds=[||]; velocity=[||]}

module World =

    type Voxel =
        | Air
        | Solid
        | Fluid
    let world : Voxel array3d = Array3D.create 16 16 16 Air 

module Physics =
    let private query () =
        let count = Entity.components.position.Length
        seq {
            for i = 0 to count do
                if Entity.components.position[i].IsSome && Entity.components.velocity[i].IsSome then
                    yield i, Entity.components.position[i].Value, Entity.components.position[i].Value
        }
    
    let private updatePosition i (p: Vector3) =
        Entity.components.position[i] <- Some p

    let private roundAway a b =
        if b < 0f then
            int (ceil a)
        else
            int (floor a)

    let run (delta: float32) =
        for id, pos, vel in query() do
            let bottom = roundAway pos.X vel.X, roundAway pos.Y vel.Y, roundAway pos.Z vel.Z
            updatePosition id (pos + delta * vel)
        
