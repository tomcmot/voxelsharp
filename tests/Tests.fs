module Tests

open System
open Xunit
open Engine

let ``GetVoxel tests`` () =
    Assert.Equal(Chunk.Block.Box, Chunk.getVoxel Chunk.plane 0 0 0)
    Assert.Equal(Chunk.Block.Air, Chunk.getVoxel Chunk.plane 1 0 0)

let ``SetVoxel tests`` () =
    let copy = Array.copy Chunk.plane
    Chunk.setVoxel copy Chunk.Block.Box 1 2 3
    Assert.Equal(Chunk.Block.Box, Chunk.getVoxel copy 1 2 3)