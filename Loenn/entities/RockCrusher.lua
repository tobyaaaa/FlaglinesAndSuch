local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")
local drawableSpriteStruct = require("structs.drawable_sprite")

local RockCrusher = {}

RockCrusher.name = "FlaglinesAndSuch/RockCrusher"
RockCrusher.depth = 0


RockCrusher.placements = {
    {
        name = "rockcrusher",
        data = {

        }
    }
}

function RockCrusher.sprite(room, entity)
    local texture = "objects/FlaglinesAndSuch/Sawblade/small00"
    local sprite = drawableSpriteStruct.fromTexture(texture, entity)
    return sprite
end

function RockCrusher.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0

    selectOffset = 6
    selectSize = 12


    -- if there are no nodes, nodeRects is empty, which is fine
    return utils.rectangle(x - selectOffset, y - selectOffset, selectSize, selectSize), {}
end


return RockCrusher