module Cube 
open System.Numerics
open Silk.NET.OpenGL

let mutable private vao = 0u
let draw (gl: GL) (position: Vector3) =
    if vao = 0u then
        vao <- Mesh.CubeVao gl
    ()