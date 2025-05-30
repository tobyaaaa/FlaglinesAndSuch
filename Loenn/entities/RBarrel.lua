local utils = require("utils")

local RocketBarrel = {}

RocketBarrel.name = "FlaglinesAndSuch/RocketBarrel"
RocketBarrel.depth = 0
RocketBarrel.texture = "objects/FlaglinesAndSuch/RBarrel/rbarrel02"
RocketBarrel.placements = {
    {
        name = "rbarrelnormal"
    }
}


function RocketBarrel.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return utils.rectangle(x - 12, y - 12, 24, 24)
end


return RocketBarrel