module Chunk

type Chunk = uint array
type Direction =
    | PosX | PosY | PosZ
    | NegX | NegY | NegZ

let private ChunkDim = 16
let private ChunkSize = ChunkDim * ChunkDim * ChunkDim

let directions = [| PosX; PosY; PosZ; NegX; NegY; NegZ|]

let inline idx i =
    struct(i &&& 0xF, i >>> 4 &&& 0xF,i >>> 8 &&& 0xF)

let init f =
    let decodeIndex f (i :int) =
        let struct(x,y,z) = idx i
        f x y z
    Array.init ChunkSize (decodeIndex f)

let inline private encodeIndex x y z =
    z <<< 8 |||  (y <<< 4) ||| x
let getVoxel (chunk: Chunk) x y z =
    chunk[encodeIndex x y z]

let setVoxel (chunk: Chunk) value x y z =
    chunk[encodeIndex x y z] <- value

let positionAt x y z dir = 
    let x = int x
    let y = int y
    let z = int z
    match dir with
    | PosX -> x+1,y,z
    | PosY -> x,y+1,z
    | PosZ -> x,y,z+1
    | NegX -> x-1,y,z
    | NegY -> x,y-1,z
    | NegZ -> x,y,z-1
let getNeighborAt (chunk:Chunk) x y z dir =
    let safeGet (x,y,z) =
        if x < 0 || y < 0 || z < 0 || x > 15 || y > 15 || z > 15
        then 0u
        else chunk[encodeIndex x y z]
    safeGet (positionAt x y z dir)

let plane =
    init (fun x y z -> 
        if y = 0 then 1u else 0u
    )
