module Shader
open System.Numerics
open System
open System.Runtime.InteropServices
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

[<Struct>]
type Shader =
    {
        context: GL
        program: uint
    }
    member this.Use () =
      this.context.UseProgram this.program
    member this.SetUniform (k: string, i: int) =
      let loc = this.context.GetUniformLocation (this.program, k)
      this.context.Uniform1(loc, i)
    member this.SetUniform (k: string, f: float32) =
      let loc = this.context.GetUniformLocation (this.program, k)
      this.context.Uniform1(loc , f)
      printf "%s: %i: %A\n" k loc f
    member this.SetUniform (k: string, v2: Vector2) =
      this.context.Uniform2(this.context.GetUniformLocation (this.program, k), v2)
    member this.SetUniform (k: string, v3: Vector3) =
      let loc = this.context.GetUniformLocation (this.program, k)
      if loc = 0 then failwith "loc is 0"
      this.context.Uniform3(loc, v3)
      printf "%s: %i: %A\n" k loc v3
    member this.SetUniform (key: string, m4) =
        let location = this.context.GetUniformLocation (this.program, key)

        printf "%s: %i: %A\n" key location m4
        let mutable auxVal = m4    
        let s = ReadOnlySpan &auxVal
        let span = MemoryMarshal.Cast<Matrix4x4, float32> s
        this.context.UniformMatrix4 (location, false, span)

    member this.SetMaterial (mat: Material) =
      this.SetUniform("material.diffuse", 0)
      this.SetUniform("material.specular", 1)
      this.SetUniform("material.shininess", mat.shininess)
      this.context.ActiveTexture TextureUnit.Texture0
      this.context.BindTexture (TextureTarget.Texture2D, mat.diffuse)
      this.context.ActiveTexture TextureUnit.Texture1
      this.context.BindTexture (TextureTarget.Texture2D, mat.specular)
      
    member this.SetDirLight (light: DirLight) =
      this.SetUniform("dirLight.direction", light.direction)
      this.SetUniform ("dirLight.ambient", light.ambient)
      this.SetUniform("dirLight.diffuse", light.diffuse)
      this.SetUniform("dirLight.specular", light.specular)
    member this.SetPointLights (lights: array<PointLight>) =
      for i = 0 to lights.Length - 1 do
        let location = sprintf "pointLights[%i]." i
        this.SetUniform(location + "position", lights[i].position)
        this.SetUniform(location + "ambient", lights[i].ambient)
        this.SetUniform(location + "diffuse", lights[i].diffuse)
        this.SetUniform(location + "specular", lights[i].specular)
        this.SetUniform(location + "constant", lights[i].constant)
        this.SetUniform(location + "linear", lights[i].linear)
        this.SetUniform(location + "quadratic", lights[i].quadratic)
        
 
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

  {context=gl; program= handle;}
