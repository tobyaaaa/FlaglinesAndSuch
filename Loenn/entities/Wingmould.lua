local utils = require("utils")

local Wingmould = {}

Wingmould.name = "FlaglinesAndSuch/Wingmould"
Wingmould.depth = 0
Wingmould.texture = "objects/FlaglinesAndSuch/Wingmould/idle00"

Wingmould.ignoredFields = {"no_nail_kelper"}
Wingmould.placements = {
    {
        name = "normal",
        data = {
            OverrideSprite = "",
            no_nail_kelper=false
        }
    }
}


function Wingmould.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return utils.rectangle(x - 12, y - 12, 24, 24)
end


return Wingmould