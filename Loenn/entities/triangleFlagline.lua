local drawableSprite = require("structs.drawable_sprite")
local flaglineHelper = require("helpers.flagline")
local utils = require("utils")

local TriangleFlagline = {}

TriangleFlagline.name = "FlaglinesAndSuch/TriangleFlagline"
TriangleFlagline.depth = 8999
TriangleFlagline.nodeVisibility = "never"
TriangleFlagline.nodeLimits = {1, 1}
TriangleFlagline.fieldInformation = {
    WireColor = {
        fieldType = "color"
    },
    PinColor = {
        fieldType = "color"
    },
    MinSpacing = {
        fieldType = "integer"
    },
    MaxSpacing = {
        fieldType = "integer"
    },
    Depth = {
        fieldType = "integer"
    }
}
TriangleFlagline.placements = {
    {
        name = "sohelpme",
        data = {
            FlagColors = "d85f2f,d82f63,2fd8a2,d8d62f",
            WireColor = "474c70",
            PinColor = "676e6e",
            MinSpacing = 2,
            MaxSpacing = 10,
            FlagDroopAmount = 0.2,
            RandomColors = false,
            Depth = 8999
        }
    }
}

local flagLineOptions = {
    lineColor = {128 / 255, 128 / 255, 163 / 255},
    pinColor = {128 / 255, 128 / 255, 128 / 255},
    colors = {
        {216 / 255, 95 / 255, 47 / 255},
        {216 / 255, 47 / 255, 99 / 255},
        {47 / 255, 216 / 255, 162 / 255},
        {216 / 255, 214 / 255, 47 / 255}
    },
    minFlagHeight = 0,
    maxFlagHeight = 0,
    minFlagLength = 0,
    maxFlagLength = 0,
    minSpace = 0,
    maxSpace = 0,
    droopAmount = 0
}


--local statueTexture = "objects/reflectionHeart/statue" -- temp sprite
function TriangleFlagline.sprite(room, entity)
    --local sprite = drawableSprite.fromTexture(statueTexture, entity)
    flagLineOptions.maxFlagHeight = 10
    flagLineOptions.minFlagHeight = 10
    flagLineOptions.maxFlagLength = 10
    flagLineOptions.minFlagLength = 10
    
    if entity.MinSpacing <= 0 or entity.MaxSpacing <= 0 then
        flagLineOptions.minSpace = 1
        flagLineOptions.maxSpace = 1
    else
        flagLineOptions.minSpace = entity.MinSpacing
        flagLineOptions.maxSpace = entity.MaxSpacing
    end
    flagLineOptions.droopAmount = entity.FlagDroopAmount
    local pins, pr, pg, pb, pa = utils.parseHexColor(entity.PinColor)
    if pins then
        flagLineOptions.pinColor = {pr, pg, pb, pa}
    end
    local wire, wr, wg, wb, wa = utils.parseHexColor(entity.WireColor)
    if wire then
        flagLineOptions.WireColor = {wr, wg, wb, wa}
    end
    local realcolors = {}
    for i in string.gmatch(entity.FlagColors, "([^,]+)") do
        iy, ir, ig, ib, ia = utils.parseHexColor(i)
        if iy then
            table.insert(realcolors, {ir, ig, ib, ia})
        end
    end
    if next(realcolors) ~= nil then
        flagLineOptions.colors = realcolors
    end

    --local sprites = flaglineHelper.getFlagLineSprites(room, entity, flagLineOptions)
    --table.insert(sprites, sprite)
    --(no longer needed because lonn is too based and doesn't have bad errors like ahorn)
    return flaglineHelper.getFlagLineSprites(room, entity, flagLineOptions)
end
TriangleFlagline.selection = flaglineHelper.getFlaglineSelection

return TriangleFlagline