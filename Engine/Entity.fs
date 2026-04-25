module Entity

let mutable private maxId = 0u
let mutable private freeIds: uint32 list = []

let createEntity () =
    match freeIds with
        | x::xs ->
            freeIds <- xs
            x
        | [] -> 
            maxId <- maxId + 1u
            maxId

let deleteEntity id =
    freeIds <- id::freeIds
