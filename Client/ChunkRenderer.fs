module ChunkRenderer
open System.Numerics
open Engine.Block
open Engine.Chunk
open Silk.NET.OpenGL

open Shader
let getVertexCount (mesh: float32[]): uint =
    uint mesh.Length / 8u

type GreedyRect = 
    {
        A: int
        B: int
        W: int
        H: int
        BlockId: Block
    }

let voxelForDir (chunk: Chunk) (dir : Direction) a b slice =
    let showVoxel x y z =
        if isTransparent (getNeighborAt chunk x y z dir)
        then getVoxel chunk x y z
        else Block.Void
    match dir with
    | PosX | NegX -> 
        showVoxel slice a b
    | PosY | NegY -> 
        showVoxel a slice b
    | PosZ | NegZ -> 
        showVoxel a b slice
let buildMaskForSlice (chunk: Chunk) (dir : Direction) slice =
    Array2D.init 16 16 (fun a b -> voxelForDir chunk dir a b slice)

let greedyRectForSlice (mask : Block [,]) =
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
                mask[x,y] <- Block.Void

    let mutable rects = []
    for i = 0 to 15 do
        for j = 0 to 15 do
            let face = mask[i,j]
            if not <| isTransparent face then
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
let emitTriangles (offset:Vector3) (dir: Direction) slice (rect: GreedyRect) =
    let h = float32 rect.H
    let w = float32 rect.W
    let {A=a; B=b} = rect
    let t0, tw, th =
        if rect.BlockId = Block.Light then
            -1f,-1f,-1f
        else 
            0f, w, h
    match dir with
    | PosX ->
        let x = float32 slice + offset.X
        let y = float32 a + offset.Y
        let z = float32 b + offset.Z
        let x1 = x+1f
        let y1 = y+h
        let z1 = z+w
        [|
        // Triangle 1: (x1,y,z), (x1,y,z1), (x1,y1,z1)
        // position // normal    //texture coord
        x1; y; z;   1f; 0f; 0f;   t0; t0;
        x1; y; z1;  1f; 0f; 0f;   tw; t0;
        x1; y1; z1; 1f; 0f; 0f;   tw; th;
        // Triangle 2: (x1,y,z), (x1,y1,z1), (x1,y1,z)
        x1; y; z;   1f; 0f; 0f;   t0; t0;
        x1; y1; z1; 1f; 0f; 0f;   tw; th;
        x1; y1; z;  1f; 0f; 0f;   t0; th;
        |]
    | PosY -> 
        let x = float32 a + offset.X
        let y = float32 slice + offset.Y
        let z = float32 b + offset.Z
        let x1 = x+h
        let y1 = y+1f
        let z1 = z+w
        [|
        // Triangle 1: (x,y1,z), (x1,y1,z), (x1,y1,z1)
        // position // normal    //texture coord
        x; y1; z;   0f; 1f; 0f;   t0; t0;
        x1; y1; z;  0f; 1f; 0f;   tw; t0;
        x1; y1; z1; 0f; 1f; 0f;   tw; th;
        // Triangle 2: (x,y1,z), (x1,y1,z1), (x,y1,z1)
        x; y1; z;   0f; 1f; 0f;   t0; t0;
        x1; y1; z1; 0f; 1f; 0f;   tw; th;
        x; y1; z1;  0f; 1f; 0f;   t0; th;
        |]
    | PosZ ->
        let x = float32 a + offset.X
        let y = float32 b + offset.Y
        let z = float32 slice + offset.Z
        let x1 = x+h
        let y1 = y+w
        let z1 = z+1f
    
        [|
        // Triangle 1: (x,y,z1), (x1,y,z1), (x1,y1,z1)
        // position // normal    //texture coord
        x; y; z1;   0f; 0f; 1f;   t0; t0;
        x1; y; z1;  0f; 0f; 1f;   th; t0;
        x1; y1; z1; 0f; 0f; 1f;   th; tw;
        // Triangle 2: (x,y,z1), (x1,y1,z1), (x,y1,z1)
        x; y; z1;   0f; 0f; 1f;   t0; t0;
        x1; y1; z1; 0f; 0f; 1f;   th; tw;
        x; y1; z1;  0f; 0f; 1f;   t0; tw;
        |]
    | NegX -> 
        let x = float32 slice + offset.X
        let y = float32 a + offset.Y
        let z = float32 b + offset.Z
        let y1 = y + h 
        let z1 = z+w
        [|
        // Triangle 1: (x,y1,z1), (x,y1,z), (x,y,z)
        // position // normal    //texture coord
        x; y1; z1; -1f; 0f; 0f;   t0; t0;
        x; y1; z;  -1f; 0f; 0f;   tw; t0;
        x; y; z;   -1f; 0f; 0f;   tw; th;
        // Triangle 2: (x,y1,z1), (x,y,z), (x,y,z1)
        x; y1; z1; -1f; 0f; 0f;   t0; t0;
        x; y; z;   -1f; 0f; 0f;   tw; th;
        x; y; z1;  -1f; 0f; 0f;   t0; th;
        |]
    | NegY -> 
        let x = float32 a + offset.X
        let y = float32 slice + offset.Y
        let z = float32 b + offset.Z
        let x1 = x+h
        let z1 = z+w
        [|
        // Triangle 1: (x1,y,z1), (x,y,z1), (x,y,z)
        // position // normal    //texture coord
        x1; y; z1; 0f; -1f; 0f;   t0; t0;
        x; y; z1;  0f; -1f; 0f;   tw; t0;
        x; y; z;   0f; -1f; 0f;   tw; th;
        // Triangle 2: (x1,y,z1), (x,y,z), (x1,y,z)
        x1; y; z1; 0f; -1f; 0f;   t0; t0;
        x; y; z;   0f; -1f; 0f;   tw; th;
        x1; y; z;  0f; -1f; 0f;   t0; th;
        |]
    | NegZ -> 
        let x = float32 a + offset.X
        let y = float32 b + offset.Y
        let z = float32 slice + offset.Z
        let x1 = x+h
        let y1 = y+w
        [|
        // Triangle 1: (x1,y1,z), (x,y1,z), (x,y,z)
        // position // normal    //texture coord
        x1; y1; z;   0f; 0f; -1f;   t0; t0;
        x; y1; z;  0f; 0f; -1f;   th; t0;
        x; y; z; 0f; 0f; -1f;   th; tw;
        // Triangle 2: (x1,y1,z), (x,y,z), (x1,y,z)
        x1; y1; z;   0f; 0f; -1f;   t0; t0;
        x; y; z; 0f; 0f; -1f;   th; tw;
        x1; y; z;  0f; 0f; -1f;   t0; tw;
        |]

let generateMeshGreedy offset (chunk: Chunk) =
    let verts = ResizeArray<float32> 2000
    for dir in  directions do
        for i = 0 to 15 do
            let mask = buildMaskForSlice chunk dir i
            for rect in greedyRectForSlice mask do
                    verts.AddRange(emitTriangles offset dir i rect)
    verts.ToArray ()

let getLightPositions (coords:Vector3) (chunk: Chunk) =
    seq {
        for i = 0 to chunk.Length - 1 do
            if chunk[i] = Block.Light then
                let struct(x,y,z) = idx i
                yield Vector3(float32 x, float32 y, float32 z) + coords
    }
