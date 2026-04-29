module Shader
open System.Numerics
open Silk.NET.OpenGL
open System.IO
open System.Runtime.InteropServices

type ColorRGB = Vector3

[<Struct>]
type Material =
  {
    diffuse: uint32
    specular: uint32
    shininess: float32
  }

[<Struct>]
type DirLight =
  {
    direction: Vector3
    ambient: Vector3
    diffuse: Vector3
    specular: Vector3
  }

[<Struct; StructLayout(LayoutKind.Sequential)>]
type PointLight =
  {
    position: Vector3
    constant: float32
    ambient: Vector3
    linear: float32
    diffuse: Vector3
    quadratic: float32
    specular: Vector3
    _pad: float32
  }

type Shader = uint32

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

let Create vertPath fragPath (gl: GL) =
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

  handle

let genPointLights positions = 
  positions
  |> Seq.map (fun position ->
    {
      position= position
      ambient= Vector3(0.05f, 0.05f, 0.05f)
      diffuse= Vector3(0.8f, 0.8f, 0.8f)
      specular = Vector3(1f, 1f, 1f)
      constant= 1f
      linear= 0.09f
      quadratic= 0.032f
      _pad=0f
    }
  )
  |> Array.ofSeq