local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")
local drawableSpriteStruct = require("structs.drawable_sprite")

local MarbleTrack = {}

MarbleTrack.name = "FlaglinesAndSuch/MarbleTrack"
MarbleTrack.depth = 0
MarbleTrack.nodeLimits = {1, -1}
MarbleTrack.nodeLineRenderType = "line"

MarbleTrack.placements = {
    {
        name = "marbletrack",
        data = {
            
        }
    }
}


return MarbleTrack