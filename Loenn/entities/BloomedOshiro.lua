local utils = require("utils")
local drawableSpriteStruct = require("structs.drawable_sprite")

local BloomedOshiro = {}
BloomedOshiro.name = "FlaglinesAndSuch/BloomedOshiro"
BloomedOshiro.depth = 100
BloomedOshiro.texture = "objects/FlaglinesAndSuch/bloomedoshiro/boss13"
BloomedOshiro.placements = {
    {
        name = "BloomedOshiro",
        data = {
            Right=false,
            Tween=false
        }
    }
}

function BloomedOshiro.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return utils.rectangle(x - 15, y - 16, 31, 40)
end

return BloomedOshiro