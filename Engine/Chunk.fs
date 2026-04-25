module Chunk

type Chunk = uint array
type Direction =
    | PosX | PosY | PosZ
    | NegX | NegY | NegZ

let directions = [| PosX; PosY; PosZ; NegX; NegY; NegZ|]

let inline idx i =
    struct(uint8 (i &&& 0xF), uint8 (i >>> 4 &&& 0xF),uint8 (i >>> 8 &&& 0xF))

let decodeIndex f (i :int) =
    let struct(x,y,z) = idx i
    f x y z
let init f =
    Array.init (16*16*16) (decodeIndex f)

let inline private encodeIndex (x: byte) (y : byte) (z : byte) =
    int (uint z <<< 8 ||| (uint y <<< 4) ||| uint x)
let getVoxel (chunk: Chunk) x y z =
    chunk[encodeIndex x y z]

let setVoxel (chunk: Chunk) value x y z =
    chunk[encodeIndex x y z] <- value

let positionAt (x:byte) (y:byte) (z:byte) dir = 
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
        else chunk[encodeIndex (byte x) (byte y) (byte z)]
    safeGet (positionAt x y z dir)

let plane =
    init (fun x y z -> 
        if y = 0uy then 1u else 0u
    )
