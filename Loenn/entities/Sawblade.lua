local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")
local drawableSpriteStruct = require("structs.drawable_sprite")

local Sawblade = {}

Sawblade.name = "FlaglinesAndSuch/Sawblade"
Sawblade.depth = 0
Sawblade.nodeLimits = {0, 1}
Sawblade.nodeLineRenderType = "line"
Sawblade.fieldInformation = {
    size = {
        options = { "tiny", "small", "big" },
        editable = true
    },
}
Sawblade.placements = {
    {
        name = "normal",
        data = {
            size = "small",
            move_time = 1.0,
            pause_time = 1.0,
            --start_offset = 0.0,
            time_offset = 0.0,
            easing = true,
            wrap = false,
            draw_track=false,
            track_sprite="default",
            no_nail_flaglines=false,
            no_nail_kuksa=false,
            silent=false
        }
    }
}

function Sawblade.sprite(room, entity)
    local texture = "objects/FlaglinesAndSuch/Sawblade/small00"
    local bladesize = entity.size
    if bladesize == "tiny" then
        texture = "objects/FlaglinesAndSuch/Sawblade/tiny00"
    end
    if bladesize == "big" then
        texture = "objects/FlaglinesAndSuch/Sawblade/big00"
    end
    local sprite = drawableSpriteStruct.fromTexture(texture, entity)
    return sprite
end

function Sawblade.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0

    local bladesize = entity.size
    local selectOffset = 10
    local selectSize = 20
    if bladesize == "tiny" then
        selectOffset = 8
        selectSize = 16
    end
    if bladesize == "big" then
        selectOffset = 24
        selectSize = 48
    end

    -- if there's a node, insert it into a nodeRects table, which we wil return anyway
    local nodeRects = {}
    local nodes = entity.nodes or {}
    if #nodes == 1 then
        local nx, ny = nodes[1].x or 0, nodes[1].y or 0
        table.insert(nodeRects, utils.rectangle(nx - selectOffset, ny - selectOffset, selectSize, selectSize))
    end

    -- if there are no nodes, nodeRects is empty, which is fine
    return utils.rectangle(x - selectOffset, y - selectOffset, selectSize, selectSize), nodeRects
end


return Sawblade