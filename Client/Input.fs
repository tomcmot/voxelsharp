namespace Client.Systems

open Silk.NET.Windowing
open Silk.NET.Input
open System.Numerics

module Mouse =
    let mutable lastPosition = Vector2(0f,0f)
    let move (_: IMouse) (position: Vector2) = 
        let lookSensitivity = 0.1f
        let xOffset = (position.X - lastPosition.X) * lookSensitivity
        let yOffset = (position.Y - lastPosition.Y) * lookSensitivity
        lastPosition <- position
        Camera.Rotate (xOffset, yOffset)
      
    let scroll (_:IMouse) (scroll: ScrollWheel) =
        Camera.Zoom scroll.Y

module Keyboard =
    let keyDown (window:IWindow) keyboard key code =
            if key = Key.Escape then
                window.Close()