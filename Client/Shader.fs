module Shader
open System.Numerics
open System
open System.Runtime.InteropServices
open Silk.NET.OpenGL
open System.IO

type Uniform =
  | Single of float32
  | Pair of float32 * float32
  | Triple of float32 * float32 * float32
  | Quad of float32 * float32 * float32 * float32
  | Mat4 of Matrix4x4

[<Struct>]
type Shader =
    {
        context: GL
        program: uint
        mutable uniforms: Map<string, Uniform>
    }
    member this.Use () =
      let prog = this.program
      let ctx = this.context
      let SetUniformM4 = this.SetUniformM4
      this.context.UseProgram this.program
      this.uniforms
      |> Map.iter (fun k v ->
        let loc = ctx.GetUniformLocation(prog, k)
        match v with
        | Single f -> ctx.Uniform1(loc, f)
        | Pair (a,b) -> ctx.Uniform2(loc, a, b)
        | Triple (a,b,c) -> ctx.Uniform3(loc, a,b,c)
        | Quad (a,b,c,d) -> ctx.Uniform4(loc, a,b ,c,d)
        | Mat4 m -> SetUniformM4 (k,m)
      )
    member this.SetUniformM4 (key: string, value) =
        let location = this.context.GetUniformLocation (this.program, key)
        let mutable auxVal = value    
        let s = ReadOnlySpan &auxVal
        let span = MemoryMarshal.Cast<Matrix4x4, float32> s
        this.context.UniformMatrix4 (location, false, span)

    interface IDisposable with        
        member this.Dispose(): unit = 
            this.context.DeleteProgram this.program
        

let private compileShader path (kind:ShaderType) (gl: GL) =
  let source = File.ReadAllText path
  let shader = gl.CreateShader kind
  gl.ShaderSource (shader, source)
  gl.CompileShader shader
  let code = gl.GetShader(shader, GLEnum.CompileStatus)
  if code <> int GLEnum.True then 
    let msg = gl.GetShaderInfoLog shader

    failwith (sprintf "Error compiling shader %d:\n%s\n" shader msg)
  shader

let Create vertPath fragPath (gl: GL) (uniforms:Map<string,Uniform>) =
  let vertexShader = compileShader vertPath ShaderType.VertexShader gl
  let fragmentShader = compileShader fragPath ShaderType.FragmentShader gl
  let handle = gl.CreateProgram()
  gl.AttachShader(handle, vertexShader)
  gl.AttachShader(handle, fragmentShader)
  gl.LinkProgram handle

  let code = gl.GetProgram(handle, GLEnum.LinkStatus)
  if code <> int GLEnum.True then failwith (sprintf "Error linking program %d" handle)

  gl.DetachShader(handle, vertexShader)
  gl.DetachShader(handle, fragmentShader)
  gl.DeleteShader vertexShader
  gl.DeleteShader fragmentShader

  {context=gl; program= handle; uniforms=uniforms}
