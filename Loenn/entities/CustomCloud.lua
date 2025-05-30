local drawableSprite = require("structs.drawable_sprite")
local drawableSpriteStruct = require("structs.drawable_sprite")

local CustomCloud = {}

CustomCloud.name = "FlaglinesAndSuch/CustomCloud"
CustomCloud.depth = 0
CustomCloud.fieldInformation = {
    ParticleColor = {
        fieldType = "color"
    },
    FragileParticleColor = {
        fieldType = "color"
    }
}
CustomCloud.placements = {
    {
        name = "normal",
        data = {
            fragile = false,
            small = false,
            ParticleColor = "2c5fcc",
            FragileParticleColor = "5e22ae",
            OverrideSprite = ""
        }
    },
    {
        name = "fragile",
        data = {
            fragile = true,
            small = false,
            ParticleColor = "2c5fcc",
            FragileParticleColor = "5e22ae",
            OverrideSprite = ""
        }
    }

}

local normalScale = 1.0
local smallScale = 29 / 35

local function getDefaultTexture(entity)
    local fragile = entity.fragile

    if fragile then
        return "objects/clouds/fragile00"
    
    else
        return "objects/clouds/cloud00"
    end
end

function CustomCloud.sprite(room, entity)
    local texture = getDefaultTexture(entity)
    local sprite = drawableSpriteStruct.fromTexture(texture, entity)
    local small = entity.small
    local scale = small and smallScale or normalScale

    sprite:setScale(scale, 1.0)

    return sprite
end

return CustomCloud