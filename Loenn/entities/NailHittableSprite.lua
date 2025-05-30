local utils = require("utils")
local rect = require('structs.drawable_rectangle')
local drawablesprite = require('structs.drawable_sprite')

local NailHittableSprite = {}

NailHittableSprite.name = "FlaglinesAndSuch/NailHittableSprite"
NailHittableSprite.depth = 0
NailHittableSprite.texture = "glass"
NailHittableSprite.placements = {
    {
        name = "nailnormal",
        data = {
            width = 16,
            height = 16,
            SpritesXMLEntry = "glass",
            HitSound = "event:/tobyaaa_HKnail/sword_tink",
            Breakable = false,
            DoMomentum = true,
            RefillDash = false,
            spriteTopLeft = false,
            DebrisTileID = "1"
        }
    }
}

function NailHittableSprite.sprite(room, entity)
    local incolor = {1, 1, 1, 0.2}
    local outcolor = {1, 1, 1, 0.5}
    local sprite = "ahorn/FlaglinesAndSuch/nailsprite"

    local drawsprite = drawablesprite.fromTexture(sprite, entity):setPosition(entity.x + entity.width/2, entity.y+ entity.height/2)
    if entity.spriteTopLeft then 
        drawsprite = drawablesprite.fromTexture(sprite, entity):setPosition(entity.x, entity.y)
    end
    
    return {rect.fromRectangle("bordered",entity.x,entity.y, entity.width, entity.height, incolor, outcolor), drawsprite}
end

return NailHittableSprite