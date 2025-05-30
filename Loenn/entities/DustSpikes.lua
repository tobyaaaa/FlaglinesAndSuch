local spikeHelper = require("helpers.spikes")

local spikeOptions = {
    triggerSpike = true,
    --variant = "tobyaaa_FlaglinesAndSuch_dust_spikes_plugin"
    directionNames = {
        up = "FlaglinesAndSuch/DustSpikesUp",
        down = "FlaglinesAndSuch/DustSpikesDown",
        left = "FlaglinesAndSuch/DustSpikesLeft",
        right = "FlaglinesAndSuch/DustSpikesRight"
    }
}

return spikeHelper.createEntityHandlers(spikeOptions)