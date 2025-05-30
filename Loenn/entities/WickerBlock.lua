local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableSpriteStruct = require("structs.drawable_sprite")
local drawing = require("utils.drawing")
local utils = require("utils")
local enums = require("consts.celeste_enums")

local WickerBlock = {}

WickerBlock.name = "FlaglinesAndSuch/WickerBlock"
WickerBlock.depth = 8990
WickerBlock.warnBelowSize = {16, 16}
WickerBlock.fieldInformation = {
    sound_index = {
        options = enums.tileset_sound_ids,
        fieldType = "integer"
    }
}
WickerBlock.placements = {
    {
        name = "wickerblock",
        data = {
            width = 16,
            height = 16,
            reinforced = false
        }
    }
}

local ninePatchOptions = {
    mode = "fill",
    borderMode = "repeat",
    fillMode = "repeat"
}
local frameTexture = "objects/FlaglinesAndSuch/WickerBlock/block"

function WickerBlock.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 24, entity.height or 24

    local frame = frameTexture

    local ninePatch = drawableNinePatch.fromTexture(frame, ninePatchOptions, x, y, width, height)
    local sprites = ninePatch:getDrawableSprite()
    return sprites
end

function WickerBlock.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 24, entity.height or 24

    return utils.rectangle(x, y, width, height)
end

return WickerBlock