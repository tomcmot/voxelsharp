module ChunkRenderer

let getVertexCount (mesh: float32[]): uint =
    uint mesh.Length / 8u

let getVerticesForDir x y z dir =
    let x = float32 x
    let y = float32 y
    let z = float32 z
    let x1 = x+1f
    let y1 = y+1f
    let z1 = z+1f
    match dir with
    | Chunk.PosX ->
        [|
        // Triangle 1: (x1,y,z), (x1,y,z1), (x1,y1,z1)
        // position // normal    //texture coord
        x1; y; z;   1f; 0f; 0f;   0f; 0f;
        x1; y; z1;  1f; 0f; 0f;   1f; 0f;
        x1; y1; z1; 1f; 0f; 0f;   1f; 1f;
        // Triangle 2: (x1,y,z), (x1,y1,z1), (x1,y1,z)
        x1; y; z;   1f; 0f; 0f;   0f; 0f;
        x1; y1; z1; 1f; 0f; 0f;   1f; 1f;
        x1; y1; z;  1f; 0f; 0f;   0f; 1f;
        |]
    | Chunk.PosY -> 
        [|
        // Triangle 1: (x,y1,z), (x1,y1,z), (x1,y1,z1)
        // position // normal    //texture coord
        x; y1; z;   0f; 1f; 0f;   0f; 0f;
        x1; y1; z;  0f; 1f; 0f;   1f; 0f;
        x1; y1; z1; 0f; 1f; 0f;   1f; 1f;
        // Triangle 2: (x,y1,z), (x1,y1,z1), (x,y1,z1)
        x; y1; z;   0f; 1f; 0f;   0f; 0f;
        x1; y1; z1; 0f; 1f; 0f;   1f; 1f;
        x; y1; z1;  0f; 1f; 0f;   0f; 1f;
        |]
    | Chunk.PosZ ->
        [|
        // Triangle 1: (x,y,z1), (x1,y,z1), (x1,y1,z1)
        // position // normal    //texture coord
        x; y; z1;   0f; 0f; 1f;   0f; 0f;
        x1; y; z1;  0f; 0f; 1f;   1f; 0f;
        x1; y1; z1; 0f; 0f; 1f;   1f; 1f;
        // Triangle 2: (x,y,z1), (x1,y1,z1), (x,y1,z1)
        x; y; z1;   0f; 0f; 1f;   0f; 0f;
        x1; y1; z1; 0f; 0f; 1f;   1f; 1f;
        x; y1; z1;  0f; 0f; 1f;   0f; 1f;
        |]
    | Chunk.NegX -> 
        [|
        // Triangle 1: (x,y1,z1), (x,y1,z), (x,y,z)
        // position // normal    //texture coord
        x; y1; z1; -1f; 0f; 0f;   0f; 0f;
        x; y1; z;  -1f; 0f; 0f;   1f; 0f;
        x; y; z;   -1f; 0f; 0f;   1f; 1f;
        // Triangle 2: (x,y1,z1), (x,y,z), (x,y,z1)
        x; y1; z1; -1f; 0f; 0f;   0f; 0f;
        x; y; z;   -1f; 0f; 0f;   1f; 1f;
        x; y; z1;  -1f; 0f; 0f;   0f; 1f;
        |]
    | Chunk.NegY -> 
        [|
        // Triangle 1: (x1,y,z1), (x,y,z1), (x,y,z)
        // position // normal    //texture coord
        x1; y; z1; 0f; -1f; 0f;   0f; 0f;
        x; y; z1;  0f; -1f; 0f;   1f; 0f;
        x; y; z;   0f; -1f; 0f;   1f; 1f;
        // Triangle 2: (x1,y,z1), (x,y,z), (x1,y,z)
        x1; y; z1; 0f; -1f; 0f;   0f; 0f;
        x; y; z;   0f; -1f; 0f;   1f; 1f;
        x1; y; z;  0f; -1f; 0f;   0f; 1f;
        |]
    | Chunk.NegZ -> 
        [|
        // Triangle 1: (x1,y1,z), (x,y1,z), (x,y,z)
        // position // normal    //texture coord
        x1; y1; z;   0f; 0f; -1f;   0f; 0f;
        x; y1; z;  0f; 0f; -1f;   1f; 0f;
        x; y; z; 0f; 0f; -1f;   1f; 1f;
        // Triangle 2: (x1,y1,z), (x,y,z), (x1,y,z)
        x1; y1; z;   0f; 0f; -1f;   0f; 0f;
        x; y; z; 0f; 0f; -1f;   1f; 1f;
        x1; y; z;  0f; 0f; -1f;   0f; 1f;
        |]

let processNeighbors (chunk: Chunk.Chunk) x y z =
    Chunk.directions 
    |> Array.filter (fun dir ->
        Chunk.getNeighborAt chunk x y z dir = 0u
    )
    |> Array.collect (getVerticesForDir x y z)

let generateMesh (chunk: Chunk.Chunk): float32[] =
    chunk |> Array.mapi (fun i v ->
        if v > 0u then
            Chunk.decodeIndex (processNeighbors chunk) i
        else
            [||]
    ) |> Array.collect id