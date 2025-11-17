local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableSpriteStruct = require("structs.drawable_sprite")
local drawableSprite = require("structs.drawable_sprite")
local resortPlatformHelper = require("helpers.resort_platforms")
local utils = require("utils")

local CustomSinkingBlock = {}

CustomSinkingBlock.name = "FlaglinesAndSuch/CustomSinkingBlock"
CustomSinkingBlock.nodeLimits = {1, 1}

CustomSinkingBlock.placements = {
    {
        name = "customsinkingblock",
        data = {
            width = 24,
            height = 24,
            texture = "default",
            Crouching_speed = 60.0,
            Pressed_speed = 30.0,
            Idle_speed = 45.0,
            Unpressed_speed = -50.0,
            Look_up_speed = 30.0,
            HoldableSpeed = 0.0,
            Idle_time = 0.1,
            Accelerations = "",
            no_bg_line = false,
            start_offset = 0
        }
    }
}

local ninePatchOptions = {
    mode = "fill",
    borderMode = "repeat",
    fillMode = "repeat"
}
local frameTexture = "objects/FlaglinesAndSuch/CustomSinkingBlock/%s"

function CustomSinkingBlock.sprite(room, entity)
    local sprites = {}

    local x, y = entity.x or 0, entity.y or 0
    local nodes = entity.nodes or {{x = 0, y = 0}}
    local nodeX, nodeY = nodes[1].x, nodes[1].y

    local width, height = entity.width or 24, entity.height or 24

    local blockSprite = entity.texture or "default"


    --drawing.getRelativeQuad(imageMeta, x, y, width, height)
    --local frame = drawableSpriteStruct.fromTexture(string.format(frameTexture, blockSprite), entity)
    --frame:useRelativeQuad(0, 0, 24, 24)

    local frame = string.format(frameTexture, blockSprite)
    local ninePatch = drawableNinePatch.fromTexture(frame, ninePatchOptions, x, y, width, height)
    
    resortPlatformHelper.addConnectorSprites(sprites, entity, x, y, nodeX, nodeY)
    --resortPlatformHelper.addPlatformSprites(sprites, entity, entity)
    table.insert(sprites, ninePatch)
    return sprites
end

function CustomSinkingBlock.selection(room, entity)
    local nodes = entity.nodes or {}
    local x, y = entity.x or 0, entity.y or 0
    local nodeX, nodeY = nodes[1].x or x, nodes[1].y or y
    local width, height = entity.width or 24, entity.height or 24

    return utils.rectangle(x, y, width, height), {utils.rectangle(nodeX, nodeY, width, height)}
end

return CustomSinkingBlock