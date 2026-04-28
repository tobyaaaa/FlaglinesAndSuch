local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")
local drawableSpriteStruct = require("structs.drawable_sprite")

local Marble = {}

Marble.name = "FlaglinesAndSuch/Marble"
Marble.depth = 0

Marble.placements = {
    {
        name = "marble",
        data = {}
    }
}


return Marble