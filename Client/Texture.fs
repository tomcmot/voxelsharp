module Texture
open Silk.NET.OpenGL
open StbImageSharp
open System
open System.IO
let Load (gl: GL) path =
    let inputbytes= File.ReadAllBytes path
    let result = ImageResult.FromMemory (inputbytes, ColorComponents.RedGreenBlueAlpha)
    let handle = gl.GenTexture()
    gl.BindTexture (TextureTarget.Texture2D, handle)
    gl.TexImage2D (TextureTarget.Texture2D, 0, InternalFormat.Rgba, uint result.Width, uint result.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ReadOnlySpan result.Data)
    gl.TextureParameter(handle, TextureParameterName.TextureWrapS, int TextureWrapMode.Repeat)
    gl.TextureParameter(handle, TextureParameterName.TextureWrapT, int TextureWrapMode.Repeat)
    gl.TextureParameter(handle, TextureParameterName.TextureMinFilter, int TextureMinFilter.NearestMipmapNearest)
    gl.TextureParameter(handle, TextureParameterName.TextureMagFilter, int TextureMagFilter.Nearest)

    gl.GenerateMipmap TextureTarget.Texture2D
    gl.BindTexture(TextureTarget.Texture2D, 0u)

    handle