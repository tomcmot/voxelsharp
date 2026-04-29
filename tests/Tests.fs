module Tests

open System
open Xunit
open Engine.Block
open Engine

let ``GetVoxel tests`` () =
    Assert.Equal(Block.Box, Chunk.getVoxel Chunk.plane 0 0 0)
    Assert.Equal(Block.Air, Chunk.getVoxel Chunk.plane 1 0 0)

let ``SetVoxel tests`` () =
    let copy = Array.copy Chunk.plane
    Chunk.setVoxel copy Block.Box 1 2 3
    Assert.Equal(Block.Box, Chunk.getVoxel copy 1 2 3)