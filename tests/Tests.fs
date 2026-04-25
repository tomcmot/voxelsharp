module Tests

open System
open Xunit

[<Fact>]
let ``My test`` () =
    Assert.True(true)

[<Fact>]
let ``plane is a plane`` () =
    let expected = Array.init (16*16*16) (fun i ->
        if i &&& 0b000011110000 = 0 then 1u else 0u
    )
    Assert.Equal(expected.Length, Chunk.plane.Length)
    Assert.Equivalent(expected, Chunk.plane)
    for i = 0 to Chunk.plane.Length-1 do
        Assert.True(expected[i] = Chunk.plane[i], sprintf "At %i, expected %i actual %i\n" i expected[i] Chunk.plane[i])

let ``GetVoxel tests`` () =
    Assert.Equal(1u, Chunk.getVoxel Chunk.plane 0uy 0uy 0uy)
    Assert.Equal(0u, Chunk.getVoxel Chunk.plane 1uy 0uy 0uy)

let ``SetVoxel tests`` () =
    let copy = Array.copy Chunk.plane
    Chunk.setVoxel copy 3u 1uy 2uy 3uy
    Assert.Equal(3u, Chunk.getVoxel copy 1uy 2uy 3uy)