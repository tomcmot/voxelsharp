namespace Engine

module Shapes2D =
    [<Struct>]
    type Point = {x: float; y: float}
    [<Struct>]
    type Triangle = {a: Point; b: Point; c: Point}
    [<Struct>]
    type Edge = {p: Point; q: Point}

    type Polygon = Point array

module Geometry =
    open Shapes2D
    let distanceSq p q =
        let dx = p.x - q.x
        let dy = p.y - q.y
        dx * dx + dy * dy

    let tile (domain: float) p =
        seq {
            for i in [-1.;0;1] do
                for j in [-1.;0;1] do
                    {x= p.x + domain * i; y= p.y + domain * j}
        }
    let wrappedDistanceSq domain p q =
        let qs = tile domain q
        qs 
        |> Seq.map (fun q' -> q', distanceSq p q') 
        |> Seq.minBy snd

module Delauney =
    open Shapes2D
    type VoronoiCell =
        {points: Point array}
    type VoronoiDiagram =
        {cells: VoronoiCell array; edges: Edge array; triangles: Triangle array}
    let triangulate domain (points: Point array) =
        let origin = {x=0;y=0}
        let invalidTri = {a=origin; b=origin; c=origin}
        let invalidEdge = {p=origin; q=origin}
        let triangles = Array.create (3 * points.Length) invalidTri
        let edges = Array.create (2 * points.Length) invalidEdge
        let cells : VoronoiCell array= Array.create points.Length {points= [||]}
        {cells=cells; edges=edges; triangles=triangles}