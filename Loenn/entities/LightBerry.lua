local utils = require("utils")
local drawableSpriteStruct = require("structs.drawable_sprite")

local LightBerry = {}
LightBerry.depth = 100

LightBerry.name = "FlaglinesAndSuch/LightBerry"

LightBerry.fieldInformation = {
    checkpointID = {
        fieldType = "integer"
    },
    order = {
        fieldType = "integer"
    }
}
LightBerry.placements = {
    {
        name = "LightBerry",
        data = {
            checkpointID=1,
            order=1,
            isDark=false,
            moon=true
        }
    },
    {
        name = "LightBerryDark",
        data = {
            checkpointID=1,
            order=1,
            isDark=true,
            moon=true
        }
    }
}

local function getTexture(entity)
    local isDark = entity.isDark

    if isDark then
        return "collectables/FlaglinesAndSuch/darkberry/normal00"

    else
        return "collectables/FlaglinesAndSuch/lightberry/normal00"
    end
end
function LightBerry.sprite(room, entity)
    local texture = getTexture(entity)
    local sprite = drawableSpriteStruct.fromTexture(texture, entity)
    local small = entity.small
    local scale = small and smallScale or normalScale

    sprite:setScale(scale, 1.0)

    return sprite
end
function LightBerry.selection(room, entity)
    -- Same size, just need selection
    local sprite = drawableSpriteStruct.fromTexture("collectables/FlaglinesAndSuch/lightberry/normal00", entity)

    return sprite:getRectangle()
end


return LightBerry