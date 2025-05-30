local utils = require("utils")
local drawableSpriteStruct = require("structs.drawable_sprite")

local ShootingEye = {}

ShootingEye.name = "FlaglinesAndSuch/ShootingEye"
ShootingEye.placements = {
    {
        name = "ShootingEye"
    }
}

local function isBackground(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0

    local tx = math.floor(x / 8) + 1
    local ty = math.floor(y / 8) + 1

    return room.tilesFg.matrix:get(tx, ty, "0") == "0"
end

function ShootingEye.depth(room, entity, viewport)
    return isBackground(room, entity) and 8990 or -10001
end
function ShootingEye.draw(room, entity, viewport)


    local layer = isBackground(room, entity) and "bg" or "fg"

    local eyeSprite = drawableSpriteStruct.fromTexture("scenery/temple/eye/" .. layer .. "_eye", entity)
    local lidSprite = drawableSpriteStruct.fromTexture("scenery/temple/eye/" .. layer .. "_lid00", entity)
    local pupilSprite = drawableSpriteStruct.fromTexture("scenery/temple/eye/" .. layer .. "_pupil", entity)
    pupilSprite.setColor({1,0,0,1})

    eyeSprite:draw()
    lidSprite:draw()
    pupilSprite:draw()
end

function ShootingEye.selection(room, entity)
    -- Same size, just need selection
    local sprite = drawableSpriteStruct.fromTexture("scenery/temple/eye/bg_eye", entity)

    return sprite:getRectangle()
end


return ShootingEye