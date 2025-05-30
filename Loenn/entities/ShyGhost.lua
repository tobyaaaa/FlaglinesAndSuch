local utils = require("utils")
local drawableSpriteStruct = require("structs.drawable_sprite")

local ShyGhost = {}
ShyGhost.depth = 100

ShyGhost.texture = "objects/FlaglinesAndSuch/shyghost/chase00"
ShyGhost.name = "FlaglinesAndSuch/ShyGhost"
ShyGhost.fieldInformation = {
    speed = {
        fieldType = "integer"
    }
}
ShyGhost.placements = {
    {
        name = "ShyGhost",
        data = {
            speed=60,
            reversed=false,
            right=false
        }
    }
}

function ShyGhost.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return utils.rectangle(x - 10, y - 10, 20, 20)
end



function ShyGhost.scale(room, entity)
    local right = entity.right

    return right and 1 or -1, 1
end

function ShyGhost.flip(room, entity, horizontal, vertical)
    if horizontal then
        entity.right = not entity.right
    end

    return horizontal
end

return ShyGhost