PlayerCrouchState = {}
PlayerCrouchState.__index = PlayerCrouchState

local csharp = self.self

function PlayerCrouchState.new()
	local self = setmetatable({}, PlayerCrouchState)
	csharp.stateMachine:LuaRisterState("PlayerCrouchState", self)
	return self
end

function PlayerCrouchState:Enter( )
	local csharp = self.self
	csharp:OnBufferComplete()
end

function PlayerCrouchState:Exit( )
	
end

function PlayerCrouchState:OnUpdate( )
	local csharp = self.self
    -- 对应 C# 的 base.OnBufferComplete();
	csharp:OnBufferComplete()

	-- 对应 C# 的 if (controller.HasMoveInput)
	if csharp.controller.HasMoveInput then
	    -- 有蹲伏输入
	    if csharp.controller.HasCrouchInput then
	        -- 有奔跑输入 → 蹲伏慢跑
	        if csharp.controller.HasRunInput then
	            csharp.stateMachine:SwitchState("PlayerCrouchState")
	        end
	    -- 无蹲伏输入，但有奔跑输入 → 普通慢跑
	    elseif csharp.controller.HasRunInput then
	        csharp.stateMachine:SwitchState("PlayerWalkState")
	    end
	end
end

function PlayerCrouchState:OnFixedUpdate( )
	
end

function PlayerCrouchState:OnLateUpdate( )
	
end

return PlayerCrouchState