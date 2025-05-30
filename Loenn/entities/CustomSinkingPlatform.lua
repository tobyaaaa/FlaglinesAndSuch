local resortPlatformHelper = require("helpers.resort_platforms")
local utils = require("utils")

local CustomSinkingPlatform = {}

CustomSinkingPlatform.name = "FlaglinesAndSuch/CustomSinkingPlatform"
CustomSinkingPlatform.nodeLimits = {1, 1}

CustomSinkingPlatform.placements = {
    {
        name = "customsinkingplatform",
        data = {
            width = 24,
            texture = "default",
            Crouching_speed = 60.0,
            Pressed_speed = 30.0,
            Idle_speed = 45.0,
            Unpressed_speed = -50.0,
            Look_up_speed = 30.0,
            HoldableSpeed = 0.0,
            Idle_time = 0.1,
            Accelerations = "",
            no_bg_line = false
        }
    }
}

function CustomSinkingPlatform.sprite(room, entity)
    local sprites = {}

    local x, y = entity.x or 0, entity.y or 0
    local nodes = entity.nodes or {{x = 0, y = 0}}
    local nodeX, nodeY = nodes[1].x, nodes[1].y
    
    resortPlatformHelper.addConnectorSprites(sprites, entity, x, y, nodeX, nodeY)
    resortPlatformHelper.addPlatformSprites(sprites, entity, entity)
    return sprites
end

function CustomSinkingPlatform.nodeSprite(room, entity, node)
    return resortPlatformHelper.addPlatformSprites({}, entity, node)
end

CustomSinkingPlatform.selection = resortPlatformHelper.getSelection

return CustomSinkingPlatform