module Mesh
open System
open Silk.NET.OpenGL
let mutable private cubeVbo: uint32 = 0u
let private vertices = 
    [|
        -0.5f; -0.5f; -0.5f;  0.0f;  0.0f; -1.0f;
         0.5f; -0.5f; -0.5f;  0.0f;  0.0f; -1.0f;
         0.5f;  0.5f; -0.5f;  0.0f;  0.0f; -1.0f;
         0.5f;  0.5f; -0.5f;  0.0f;  0.0f; -1.0f;
        -0.5f;  0.5f; -0.5f;  0.0f;  0.0f; -1.0f;
        -0.5f; -0.5f; -0.5f;  0.0f;  0.0f; -1.0f;
        -0.5f; -0.5f;  0.5f;  0.0f;  0.0f;  1.0f;
         0.5f; -0.5f;  0.5f;  0.0f;  0.0f;  1.0f;
         0.5f;  0.5f;  0.5f;  0.0f;  0.0f;  1.0f;
         0.5f;  0.5f;  0.5f;  0.0f;  0.0f;  1.0f;
        -0.5f;  0.5f;  0.5f;  0.0f;  0.0f;  1.0f;
        -0.5f; -0.5f;  0.5f;  0.0f;  0.0f;  1.0f;
        -0.5f;  0.5f;  0.5f; -1.0f;  0.0f;  0.0f;
        -0.5f;  0.5f; -0.5f; -1.0f;  0.0f;  0.0f;
        -0.5f; -0.5f; -0.5f; -1.0f;  0.0f;  0.0f;
        -0.5f; -0.5f; -0.5f; -1.0f;  0.0f;  0.0f;
        -0.5f; -0.5f;  0.5f; -1.0f;  0.0f;  0.0f;
        -0.5f;  0.5f;  0.5f; -1.0f;  0.0f;  0.0f;
         0.5f;  0.5f;  0.5f;  1.0f;  0.0f;  0.0f;
         0.5f;  0.5f; -0.5f;  1.0f;  0.0f;  0.0f;
         0.5f; -0.5f; -0.5f;  1.0f;  0.0f;  0.0f;
         0.5f; -0.5f; -0.5f;  1.0f;  0.0f;  0.0f;
         0.5f; -0.5f;  0.5f;  1.0f;  0.0f;  0.0f;
         0.5f;  0.5f;  0.5f;  1.0f;  0.0f;  0.0f;
        -0.5f; -0.5f; -0.5f;  0.0f; -1.0f;  0.0f;
         0.5f; -0.5f; -0.5f;  0.0f; -1.0f;  0.0f;
         0.5f; -0.5f;  0.5f;  0.0f; -1.0f;  0.0f;
         0.5f; -0.5f;  0.5f;  0.0f; -1.0f;  0.0f;
        -0.5f; -0.5f;  0.5f;  0.0f; -1.0f;  0.0f;
        -0.5f; -0.5f; -0.5f;  0.0f; -1.0f;  0.0f;
        -0.5f;  0.5f; -0.5f;  0.0f;  1.0f;  0.0f;
         0.5f;  0.5f; -0.5f;  0.0f;  1.0f;  0.0f;
         0.5f;  0.5f;  0.5f;  0.0f;  1.0f;  0.0f;
         0.5f;  0.5f;  0.5f;  0.0f;  1.0f;  0.0f;
        -0.5f;  0.5f;  0.5f;  0.0f;  1.0f;  0.0f;
        -0.5f;  0.5f; -0.5f;  0.0f;  1.0f;  0.0f;
    |]

let private stride = uint32 (6 * sizeof<float32>)
let CubeVao (gl:GL) includeNormal =
    if cubeVbo = 0u then 
        cubeVbo <-  gl.GenBuffer()
        gl.BindBuffer(GLEnum.ArrayBuffer, cubeVbo)
        gl.BufferData(BufferTargetARB.ArrayBuffer, ReadOnlySpan vertices, BufferUsageARB.StaticDraw)
    let vao = gl.GenVertexArray()
    gl.BindBuffer(GLEnum.ArrayBuffer, cubeVbo)
    gl.BindVertexArray vao
    gl.VertexAttribPointer(0u, 3, VertexAttribPointerType.Float, false, stride, IntPtr.Zero.ToPointer())
    gl.EnableVertexAttribArray 0u
    if includeNormal then
        let normalOffset = nativeint (3 * sizeof<float32>)
        gl.VertexAttribPointer(1u, 3, VertexAttribPointerType.Float, false, stride, normalOffset.ToPointer())
        gl.EnableVertexAttribArray 1u
    vao