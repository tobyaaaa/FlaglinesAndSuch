local drawableSpriteStruct = require("structs.drawable_sprite")
local drawing = require("utils.drawing")
local utils = require("utils")

local PlatformJellyfish = {}

PlatformJellyfish.name = "FlaglinesAndSuch/PlatformJelly"
PlatformJellyfish.depth = 0
PlatformJellyfish.fieldInformation = {
    sound_index = {
        platformSound = "integer"
    }
}
PlatformJellyfish.placements = {
    {
        name = "pjellynormal",
        data = {
            bubble = false,
            tutorial = false,
            platformSound = 12,
            noGrabbing = false,
            restLower = false,
            OverrideSprite = "",
            bigHitbox = false,
            NewHitboxBehavior = false,
            noRotation = false
        }
    }
}

function PlatformJellyfish.sprite(room, entity)
    local mainTexture = drawableSpriteStruct.fromTexture("objects/FlaglinesAndSuch/PlatformJelly/idle0", entity)
    local JumpThruL = drawableSpriteStruct.fromTexture("objects/jumpthru/wood", entity)
    local JumpThruM = drawableSpriteStruct.fromTexture("objects/jumpthru/wood", entity)
    local JumpThruR = drawableSpriteStruct.fromTexture("objects/jumpthru/wood", entity)
    JumpThruL:setColor({1,1,1,0.7})
    JumpThruM:setColor({1,1,1,0.7})
    JumpThruR:setColor({1,1,1,0.7})
    JumpThruL:addPosition(-12, -16)
    JumpThruL:useRelativeQuad(0, 8, 8, 8)
    JumpThruM:addPosition(-4, -16)
    JumpThruM:useRelativeQuad(8, 8, 8, 8)
    JumpThruR:addPosition(4, -16)
    JumpThruR:useRelativeQuad(16, 8, 8, 8)
    local sprites = {}
    table.insert(sprites, mainTexture)
    table.insert(sprites, JumpThruL)
    table.insert(sprites, JumpThruM)
    table.insert(sprites, JumpThruR)
    return sprites
end

function PlatformJellyfish.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return utils.rectangle(x - 13, y - 14, 28, 17)
end


return PlatformJellyfish