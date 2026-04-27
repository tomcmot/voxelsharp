module ChunkRenderer

let getVertexCount (mesh: float32[]): uint =
    uint mesh.Length / 8u

type Face = uint
type GreedyRect = 
    {
        A: int
        B: int
        W: int
        H: int
        BlockId: uint
    }

let voxelForDir (chunk: Chunk.Chunk) (dir : Chunk.Direction) a b slice =
    match dir with
    | Chunk.PosX | Chunk.NegX -> Chunk.getVoxel chunk slice a b
    | Chunk.PosY | Chunk.NegY -> Chunk.getVoxel chunk a slice b
    | Chunk.PosZ | Chunk.NegZ -> Chunk.getVoxel chunk a b slice
let buildMaskForSlice (chunk: Chunk.Chunk) (dir : Chunk.Direction) slice =
    Array2D.init 16 16 (fun a b -> voxelForDir chunk dir (byte a) (byte b) slice)

let greedyRectForSlice (mask : Face [,]) =
    let validRow face i j w =
        let mutable valid = true
        let mutable j' = j
        while valid && j' < j + w do
            if mask[i, j'] <> face then
                valid <- false
            j' <- j' + 1
        valid
            
    let clearPositions i j h w =
        for x = i to i + h - 1 do
            for y = j to j + w - 1 do
                mask[x,y] <- 0u

    let mutable rects = []
    for i = 0 to 15 do
        for j = 0 to 15 do
            let face = mask[i,j]
            if face > 0u then
                let mutable w = 1
                while j+w < 16 && mask[i,j+w] = face  do
                    w <- w + 1
                let mutable h = 1
                while  i + h < 16  && validRow face (i+h) j w do
                    h <- h + 1

                clearPositions i j h w
                rects <- 
                    {
                        A = i
                        B = j
                        H = h
                        W = w
                        BlockId = face
                    } :: rects
    rects
let emitTriangles (dir: Chunk.Direction) slice (rect: GreedyRect) =
    let h = float32 rect.H
    let w = float32 rect.W
    let {A=a; B=b} = rect
    match dir with
    | Chunk.PosX ->
        let x = float32 slice
        let y = float32 a
        let z = float32 b
        let x1 = x+1f
        let y1 = y+h
        let z1 = z+w
        [|
        // Triangle 1: (x1,y,z), (x1,y,z1), (x1,y1,z1)
        // position // normal    //texture coord
        x1; y; z;   1f; 0f; 0f;   0f; 0f;
        x1; y; z1;  1f; 0f; 0f;   w; 0f;
        x1; y1; z1; 1f; 0f; 0f;   w; h;
        // Triangle 2: (x1,y,z), (x1,y1,z1), (x1,y1,z)
        x1; y; z;   1f; 0f; 0f;   0f; 0f;
        x1; y1; z1; 1f; 0f; 0f;   w; h;
        x1; y1; z;  1f; 0f; 0f;   0f; h;
        |]
    | Chunk.PosY -> 
        let x = float32 a
        let y = float32 slice
        let z = float32 b
        let x1 = x+h
        let y1 = y+1f
        let z1 = z+w
        [|
        // Triangle 1: (x,y1,z), (x1,y1,z), (x1,y1,z1)
        // position // normal    //texture coord
        x; y1; z;   0f; 1f; 0f;   0f; 0f;
        x1; y1; z;  0f; 1f; 0f;   w; 0f;
        x1; y1; z1; 0f; 1f; 0f;   w; h;
        // Triangle 2: (x,y1,z), (x1,y1,z1), (x,y1,z1)
        x; y1; z;   0f; 1f; 0f;   0f; 0f;
        x1; y1; z1; 0f; 1f; 0f;   w; h;
        x; y1; z1;  0f; 1f; 0f;   0f; h;
        |]
    | Chunk.PosZ ->
        let x = float32 a
        let y = float32 b
        let z = float32 slice
        let x1 = x+h
        let y1 = y+w
        let z1 = z+1f
    
        [|
        // Triangle 1: (x,y,z1), (x1,y,z1), (x1,y1,z1)
        // position // normal    //texture coord
        x; y; z1;   0f; 0f; 1f;   0f; 0f;
        x1; y; z1;  0f; 0f; 1f;   h; 0f;
        x1; y1; z1; 0f; 0f; 1f;   h; w;
        // Triangle 2: (x,y,z1), (x1,y1,z1), (x,y1,z1)
        x; y; z1;   0f; 0f; 1f;   0f; 0f;
        x1; y1; z1; 0f; 0f; 1f;   h; w;
        x; y1; z1;  0f; 0f; 1f;   0f; w;
        |]
    | Chunk.NegX -> 
        let x = float32 slice
        let y = float32 a
        let z = float32 b
        let y1 = y + h 
        let z1 = z+w
        [|
        // Triangle 1: (x,y1,z1), (x,y1,z), (x,y,z)
        // position // normal    //texture coord
        x; y1; z1; -1f; 0f; 0f;   0f; 0f;
        x; y1; z;  -1f; 0f; 0f;   w; 0f;
        x; y; z;   -1f; 0f; 0f;   w; h;
        // Triangle 2: (x,y1,z1), (x,y,z), (x,y,z1)
        x; y1; z1; -1f; 0f; 0f;   0f; 0f;
        x; y; z;   -1f; 0f; 0f;   w; h;
        x; y; z1;  -1f; 0f; 0f;   0f; h;
        |]
    | Chunk.NegY -> 
        let x = float32 a
        let y = float32 slice
        let z = float32 b
        let x1 = x+h
        let z1 = z+w
        [|
        // Triangle 1: (x1,y,z1), (x,y,z1), (x,y,z)
        // position // normal    //texture coord
        x1; y; z1; 0f; -1f; 0f;   0f; 0f;
        x; y; z1;  0f; -1f; 0f;   w; 0f;
        x; y; z;   0f; -1f; 0f;   w; h;
        // Triangle 2: (x1,y,z1), (x,y,z), (x1,y,z)
        x1; y; z1; 0f; -1f; 0f;   0f; 0f;
        x; y; z;   0f; -1f; 0f;   w; h;
        x1; y; z;  0f; -1f; 0f;   0f; h;
        |]
    | Chunk.NegZ -> 
        let x = float32 a
        let y = float32 b
        let z = float32 slice
        let x1 = x+h
        let y1 = y+w
        [|
        // Triangle 1: (x1,y1,z), (x,y1,z), (x,y,z)
        // position // normal    //texture coord
        x1; y1; z;   0f; 0f; -1f;   0f; 0f;
        x; y1; z;  0f; 0f; -1f;   h; 0f;
        x; y; z; 0f; 0f; -1f;   h; w;
        // Triangle 2: (x1,y1,z), (x,y,z), (x1,y,z)
        x1; y1; z;   0f; 0f; -1f;   0f; 0f;
        x; y; z; 0f; 0f; -1f;   h; w;
        x1; y; z;  0f; 0f; -1f;   0f; w;
        |]

let generateMeshGreedy (chunk: Chunk.Chunk) =
    let verts = ResizeArray<float32> 2000
    for dir in  Chunk.directions do
        for i = 0 to 15 do
            let mask = buildMaskForSlice chunk dir (byte i)
            for rect in greedyRectForSlice mask do
                verts.AddRange(emitTriangles dir i rect)
    verts.ToArray ()