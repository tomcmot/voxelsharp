module Engine.World

open Engine.Chunk

type ChunkMetadata =
    {
        chunk: Chunk
        mutable dirty: bool
    }

type World =
    {
        chunks: ChunkMetadata array
    }

let worldBounds = 8,1,8
let worldSize = 
    let (x,y,z) = worldBounds
    x*y*z
let idx i =
    let value = 
        struct(i &&& 0b111, 0, i >>> 3 &&& 0b111)
    printf "i %i and coord: %A" i value
    value
let private encodeIndex struct(x,y,z) =
    x ||| (z <<< 3)
// TODO figure out how to do bounds checking
let getChunk (world:World) coords =
    world.chunks[encodeIndex coords]

let markDirty (world: World) coords =
    try 
        (getChunk world coords).dirty <- true
    with e ->
        // probably out of bounds
        printf "%s\n" e.Message

let modifyVoxel (world:World) coords voxelCoords value =
    let metadata = getChunk world coords
    let struct(cx, cy, cz) = coords
    let struct(x,y,z) = voxelCoords
    setVoxel metadata.chunk value x y z
    metadata.dirty <- true
    if x = 0 then markDirty world struct(cx - 1, cy, cz)
    if x = 15 then markDirty world struct(cx + 1, cy, cz)
    if y = 0 then markDirty world struct(cx, cy - 1, cz)
    if y = 15 then markDirty world struct(cx, cy + 1, cz)
    if z = 0 then markDirty world struct(cx, cy, cz - 1)
    if z = 15 then markDirty world struct(cx, cy, cz + 1)

let generateWorld () =
    {
        chunks =
            Array.init worldSize (fun i ->
                {
                    chunk = plane |> Array.copy
                    dirty = true
                }
            )
    }