module Engine.AABB

open System.Numerics

type AABB =
    {
        min: Vector3
        max: Vector3
    }

let chunkBounds offset =
    {min = offset; max = offset + Vector3(Chunk.ChunkDim |> float32)}

type Plane =
    {
        Normal: Vector3
        Distance: float32
    }

let normalizePlane (plane: Plane) =
    let length = plane.Normal.Length()
    { Normal = plane.Normal / length; Distance = plane.Distance / length }

let extractFrustumPlanes (viewProj: Matrix4x4) =
    let m = viewProj
    let left   = { Normal = Vector3(m.M14 + m.M11, m.M24 + m.M21, m.M34 + m.M31); Distance = m.M44 + m.M41 }
    let right  = { Normal = Vector3(m.M14 - m.M11, m.M24 - m.M21, m.M34 - m.M31); Distance = m.M44 - m.M41 }
    let bottom = { Normal = Vector3(m.M14 + m.M12, m.M24 + m.M22, m.M34 + m.M32); Distance = m.M44 + m.M42 }
    let top    = { Normal = Vector3(m.M14 - m.M12, m.M24 - m.M22, m.M34 - m.M32); Distance = m.M44 - m.M42 }
    let near   = { Normal = Vector3(m.M14 + m.M13, m.M24 + m.M23, m.M34 + m.M33); Distance = m.M44 + m.M43 }
    let far    = { Normal = Vector3(m.M14 - m.M13, m.M24 - m.M23, m.M34 - m.M33); Distance = m.M44 - m.M43 }
    [ left; right; bottom; top; near; far ]
    |> List.map normalizePlane

let positiveVertex (aabb: AABB) (normal: Vector3) =
    Vector3(
        (if normal.X >= 0f then aabb.max.X else aabb.min.X),
        (if normal.Y >= 0f then aabb.max.Y else aabb.min.Y),
        (if normal.Z >= 0f then aabb.max.Z else aabb.min.Z)
    )

let planeDistance plane point =
    Vector3.Dot(plane.Normal, point) + plane.Distance

let intersectsFrustum (aabb: AABB) (planes: Plane list) =
    planes
    |> List.forall (fun plane ->
        let p = positiveVertex aabb plane.Normal
        planeDistance plane p >= 0f
    )