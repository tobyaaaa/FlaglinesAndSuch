local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local matrixLib = require("utils.matrix")
local drawableSprite = require("structs.drawable_sprite")
local connectedEntities = require("helpers.connected_entities")

local FacingBlock = {}

FacingBlock.name = "FlaglinesAndSuch/FacingBlock"
FacingBlock.warnBelowSize = {16, 16}
FacingBlock.fieldInformation = {
    actor_type = {
        options = { "Player", "Seeker", "Theo", "Input" },
        editable = false
    },
    disabled_color = {
        fieldType = "color"
    },
    enabled_color = {
    fieldType = "color"
    },
    sound_index = {
        fieldType = "integer"
    }
}

FacingBlock.placements = {
    {
        name = "right",
        data = {
            width = 16,
            height = 16,
            left = false,
            trap_player = false,
            do_cassette_wobble = true,
            disabled_color = "004400",
            enabled_color = "00ff00",
            sound_index = 35,
            enabled_sprite = "objects/cassetteblock/solid",
            disabled_sprites = "objects/cassetteblock/pressed",
            actor_type = "Player"
        }
    },
    {
        name = "left",
        data = {
            width = 16,
            height = 16,
            left = true,
            trap_player = false,
            do_cassette_wobble = true,
            disabled_color = "004400",
            enabled_color = "00ff00",
            sound_index = 35,
            enabled_sprite = "objects/cassetteblock/solid",
            disabled_sprites = "objects/cassetteblock/pressed",
            actor_type = "Player"
        }
    }
}



local function getSearchPredicate(entity)
    return function(target)
        return entity._name == target._name and entity.left == target.left and entity.actor_type == target.actor_type
    end
end

local function getTileSprite(entity, x, y, frame, color, depth, rectangles)
    local hasAdjacent = connectedEntities.hasAdjacent

    local drawX, drawY = (x - 1) * 8, (y - 1) * 8

    local closedLeft = hasAdjacent(entity, drawX - 8, drawY, rectangles)
    local closedRight = hasAdjacent(entity, drawX + 8, drawY, rectangles)
    local closedUp = hasAdjacent(entity, drawX, drawY - 8, rectangles)
    local closedDown = hasAdjacent(entity, drawX, drawY + 8, rectangles)
    local completelyClosed = closedLeft and closedRight and closedUp and closedDown

    local quadX, quadY = false, false

    if completelyClosed then
        if not hasAdjacent(entity, drawX + 8, drawY - 8, rectangles) then
            quadX, quadY = 24, 0

        elseif not hasAdjacent(entity, drawX - 8, drawY - 8, rectangles) then
            quadX, quadY = 24, 8

        elseif not hasAdjacent(entity, drawX + 8, drawY + 8, rectangles) then
            quadX, quadY = 24, 16

        elseif not hasAdjacent(entity, drawX - 8, drawY + 8, rectangles) then
            quadX, quadY = 24, 24

        else
            quadX, quadY = 8, 8
        end
    else
        if closedLeft and closedRight and not closedUp and closedDown then
            quadX, quadY = 8, 0

        elseif closedLeft and closedRight and closedUp and not closedDown then
            quadX, quadY = 8, 16

        elseif closedLeft and not closedRight and closedUp and closedDown then
            quadX, quadY = 16, 8

        elseif not closedLeft and closedRight and closedUp and closedDown then
            quadX, quadY = 0, 8

        elseif closedLeft and not closedRight and not closedUp and closedDown then
            quadX, quadY = 16, 0

        elseif not closedLeft and closedRight and not closedUp and closedDown then
            quadX, quadY = 0, 0

        elseif not closedLeft and closedRight and closedUp and not closedDown then
            quadX, quadY = 0, 16

        elseif closedLeft and not closedRight and closedUp and not closedDown then
            quadX, quadY = 16, 16
        end
    end

    if quadX and quadY then
        local sprite = drawableSprite.fromTexture(frame, entity)

        sprite:addPosition(drawX, drawY)
        sprite:useRelativeQuad(quadX, quadY, 8, 8)
        sprite:setColor(color)

        sprite.depth = depth

        return sprite
    end
end

function FacingBlock.sprite(room, entity)
    local relevantBlocks = utils.filter(getSearchPredicate(entity), room.entities)

    connectedEntities.appendIfMissing(relevantBlocks, entity)

    local rectangles = connectedEntities.getEntityRectangles(relevantBlocks)

    local sprites = {}

    local width, height = entity.width or 32, entity.height or 32
    local tileWidth, tileHeight = math.ceil(width / 8), math.ceil(height / 8)

    local color = entity.enabled_color
    local frame = entity.enabled_sprite
    local depth = -10

    for x = 1, tileWidth do
        for y = 1, tileHeight do
            local sprite = getTileSprite(entity, x, y, frame, color, depth, rectangles)

            if sprite then
                table.insert(sprites, sprite)
            end
        end
    end

    return sprites
end



return FacingBlock