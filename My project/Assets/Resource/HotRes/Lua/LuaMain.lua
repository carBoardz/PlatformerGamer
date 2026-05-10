
local ConfigBundleName = "configassets";
local PlayerBundleName = "player";

require("InitClass")
require("PlayerDate")

abMgr:LoadABOnlyAsync(ConfigBundleName, function(ok)
    loadConfigDone = ok
end)

abMgr:LoadABOnlyAsync(PlayerBundleName, function(ok)
    loadPlayerDone = ok
end)

