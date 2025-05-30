local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local HoneyWall = {}

HoneyWall.name = "FlaglinesAndSuch/HoneyWall"
HoneyWall.depth = 1999
HoneyWall.canResize = {false, true}
HoneyWall.placements = {
    {
        name = "honeywallR",
        data = {
            height = 16,
            left = false
        }
    },
    {
        name = "honeywallL",
        data = {
            height = 16,
            left = true
        }
    }
}

function HoneyWall.sprite(room, entity)
    local sprites = {}

    local left = entity.left
    local height = entity.height or 8
    local tileHeight = math.floor(height / 8)
    local offsetX = left and 0 or 8
    local scaleX = left and 1 or -1

    local topTexture = "objects/wallBooster/iceTop00"
    local middleTexture = "objects/wallBooster/iceMid00"
    local bottomTexture = "objects/wallBooster/iceBottom00"

    for i = 2, tileHeight - 1 do
        local middleSprite = drawableSprite.fromTexture(middleTexture, entity)

        middleSprite:addPosition(offsetX, (i - 1) * 8)
        middleSprite:setScale(scaleX, 1)
        middleSprite:setJustification(0.0, 0.0)

        table.insert(sprites, middleSprite)
    end

    local topSprite = drawableSprite.fromTexture(topTexture, entity)
    local bottomSprite = drawableSprite.fromTexture(bottomTexture, entity)

    topSprite:addPosition(offsetX, 0)
    topSprite:setScale(scaleX, 1)
    topSprite:setJustification(0.0, 0.0)

    bottomSprite:addPosition(offsetX, (tileHeight - 1) * 8)
    bottomSprite:setScale(scaleX, 1)
    bottomSprite:setJustification(0.0, 0.0)

    table.insert(sprites, topSprite)
    table.insert(sprites, bottomSprite)

    return sprites
end


function HoneyWall.rectangle(room, entity)
    return utils.rectangle(entity.x, entity.y, 8, entity.height or 8)
end

function HoneyWall.flip(room, entity, horizontal, vertical)
    if horizontal then
        entity.left = not entity.left
        entity.x += (entity.left and 8 or -8)
    end

    return horizontal
end

return HoneyWall