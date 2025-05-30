local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local CustomReflectionStatue = {}

CustomReflectionStatue.name = "FlaglinesAndSuch/CustomReflectionStatue"
CustomReflectionStatue.depth = 8999
CustomReflectionStatue.nodeVisibility = "always"
CustomReflectionStatue.nodeLineRenderType = "line"
CustomReflectionStatue.nodeLimits = {5, 5}
CustomReflectionStatue.placements = {
    {
        name = "statue",
        data = {
            fake_heart = false,
            base_code = "U,L,DR,UR,L,UL",
            gem_override_colors = "DL,ffffff",
            hint_mural_sprites = "objects/reflectionHeart/hint",
            dash_flavour_sounds = true,
            Override_codes = ""
        }
    }
}

local statueTexture = "objects/reflectionHeart/statue"
local torchTexture = "objects/reflectionHeart/torch00"
local gemTexture = "objects/reflectionHeart/gem"

--local codeColors = {}
local function createCodeColors(codeColors)
codeColors["U"] = {240 / 255, 240 / 255, 240 / 255}
    codeColors["L"] = {145 / 255, 113 / 255, 242 / 255}
    codeColors["DR"] = {10 / 255, 68 / 255, 224 / 255}
    codeColors["UR"] = {179 / 255, 45 / 255, 0 / 255}
    codeColors["UL"] = {255 / 255, 205 / 255, 55 / 255}
    codeColors["D"] = {255 / 255, 255 / 255, 255 / 255}
    codeColors["DL"] = {255 / 255, 255 / 255, 255 / 255}
    codeColors["R"] = {255 / 255, 255 / 255, 255 / 255}
end

--modified version of Ja's example code
local function setOverrideColors(overrides, codeColors)
    local split = string.split(overrides, ",")()
    for i = 1, #split, 2 do
        codeColors[split[i]] = utils.getColor(split[i+1])
    end
end

local function hintTexture(index, entity)
    return string.format("%s%02d", entity.hint_mural_sprites, index)
end

function CustomReflectionStatue.sprite(room, entity)
    local sprite = drawableSprite.fromTexture(statueTexture, entity)

    sprite:setJustification(0.5, 1.0)

    return sprite
end


function CustomReflectionStatue.nodeSprite(room, entity, node, nodeIndex)
    local codeColors = {}
    createCodeColors(codeColors)
    setOverrideColors(entity.Override_codes, codeColors)

    if nodeIndex <= 4 then
        local torchSprite = drawableSprite.fromTexture(torchTexture, node)
        local hintSprite = drawableSprite.fromTexture(hintTexture(nodeIndex - 1, entity), node)

        hintSprite:setJustification(0.5, 0.5)
        hintSprite:addPosition(0, 28)

        torchSprite:setJustification(0.0, 0.0)
        torchSprite:addPosition(-32, -64)

        return {torchSprite, hintSprite}

    else
        local sprites = {}
        local codes = string.split(entity.base_code, ",")()
        local codeLength = #codes

        for i = 0, codeLength - 1 do
            local gemSprite = drawableSprite.fromTexture(gemTexture, node)
            local offsetX = (i - (codeLength - 1) / 2) * 24

            gemSprite:addPosition(offsetX, 0)
            gemSprite:setColor(codeColors[codes[i+1]])

            sprites[i + 1] = gemSprite
        end

        return sprites
    end
end

return CustomReflectionStatue