module Shader
open System.Numerics
open Silk.NET.OpenGL
open System.IO

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

[<Struct>]
type PointLight =
  {
    position: Vector3
    constant: float32
    linear: float32
    quadratic: float32
    ambient: Vector3
    diffuse: Vector3
    specular: Vector3
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

[<Struct>]
type Shaders =
  {
    cube: Shader
    light: Shader
  }
