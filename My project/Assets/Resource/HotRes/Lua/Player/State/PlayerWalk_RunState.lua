PlayerWalk_RunState = {}
PlayerWalk_RunState.__index = PlayerWalk_RunState

function PlayerWalk_RunState.new()
	local self = setmetatable({}, PlayerWalk_RunState)
	csharp.stateMachine:LuaRisterState("PlayerWalk_RunState", self)
	return self
end

function PlayerWalk_RunState:Enter( )
	local csharp = self.self
	csharp:OnBufferComplete()
end

function PlayerWalk_RunState:Exit( )
	
end

function PlayerWalk_RunState:OnUpdate( )
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

function PlayerWalk_RunState:OnFixedUpdate( )
	
end

function PlayerWalk_RunState:OnLateUpdate( )
	
end

return PlayerWalk_RunState