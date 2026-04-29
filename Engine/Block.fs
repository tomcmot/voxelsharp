module Engine.Block

type Block =
    | Void = 0u
    | Air = 1u
    | Light = 2u
    | Box = 3u

let isTransparent (block:Block) =
    block < Block.Light