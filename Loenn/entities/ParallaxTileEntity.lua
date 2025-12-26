local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local matrixLib = require("utils.matrix")
local drawableSprite = require("structs.drawable_sprite")
local connectedEntities = require("helpers.connected_entities")

local parallaxTileEntity = {}

parallaxTileEntity.name = "FlaglinesAndSuch/parallaxTileEntity"
parallaxTileEntity.warnBelowSize = {8, 8}

parallaxTileEntity.fieldInformation = {
    tiletype = {
        options = fakeTilesHelper.getTilesOptions(),
        editable = false
    },
    offsetX = {
        fieldType = "integer"
    },
    offsetY = {
        fieldType = "integer"
    },
    anchoringMode = {
        options = { "room origin", "map origin" },
        editable = false
    },
}
parallaxTileEntity.placements = {
    {
        name = "ptileentitynormal",
        data = {
            width = 16,
            height = 16,
            tiletype = "3",
            scroll = 0.8,
            offsetX = 0,
            offsetY = 0,
            anchoringMode="room origin"
        }
    }
}

parallaxTileEntity.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", false, "tilesFg", {1.0, 1.0, 1.0, 0.7})
return parallaxTileEntity