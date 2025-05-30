local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableSpriteStruct = require("structs.drawable_sprite")
local drawing = require("utils.drawing")
local utils = require("utils")
local enums = require("consts.celeste_enums")

local CustomWhiteBlock = {}

CustomWhiteBlock.name = "FlaglinesAndSuch/CustomWhiteBlock"
CustomWhiteBlock.depth = 8990
CustomWhiteBlock.warnBelowSize = {16, 16}
CustomWhiteBlock.fieldInformation = {
    sound_index = {
        options = enums.tileset_sound_ids,
        fieldType = "integer"
    }
}
CustomWhiteBlock.placements = {
    {
        name = "normal",
        data = {
            width = 16,
            height = 16,
            hold_time = 3.0,
            sound_index = 27,
            permasolid_bgtiles = false
        }
    }
}

local ninePatchOptions = {
    mode = "fill",
    borderMode = "repeat",
    fillMode = "repeat"
}
local frameTexture = "objects/FlaglinesAndSuch/CustomWhiteBlock/block"

function CustomWhiteBlock.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 24, entity.height or 24

    local frame = frameTexture

    local ninePatch = drawableNinePatch.fromTexture(frame, ninePatchOptions, x, y, width, height)
    local sprites = ninePatch:getDrawableSprite()
    return sprites
end

function CustomWhiteBlock.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 24, entity.height or 24

    return utils.rectangle(x, y, width, height)
end

return CustomWhiteBlock