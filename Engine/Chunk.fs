module Chunk

type Chunk = uint array
type Direction =
    | PosX | PosY | PosZ
    | NegX | NegY | NegZ

let directions = [| PosX; PosY; PosZ; NegX; NegY; NegZ|]
let decodeIndex f (i :int) =
    let x = uint8 (i &&& 0b1111)
    let y = uint8 (i &&& 0b11110000 >>> 4)
    let z = uint8 (i &&& 0b111100000000 >>> 8)
    f x y z
let init f =
    Array.init (16*16*16) (decodeIndex f)

let private encodeIndex (x: byte) (y : byte) (z : byte) =
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

let getNeighbors (chunk: Chunk) x y z =
    let target = getVoxel chunk x y z
    target, seq {
        for i in [-1;0;1] do
            for j in [-1; 0; 1] do
                for k in [-1; 0; 1] do
                    if i <> 0 && j <> 0 && k <> 0 then 
                        let xi = int x + i
                        let yj = int y + j
                        let zk = int z + k
                        if // convert to access neighboring chunks
                            xi > -1 && xi < 16 &&
                            yj > -1 && yj < 16 &&
                            zk > -1 && zk < 16
                        then
                            yield getVoxel chunk (byte xi) (byte yj) (byte zk), i, j, k
    }

let plane =
    init (fun x y z -> 
        if y = 0uy then 1u else 0u
    )
