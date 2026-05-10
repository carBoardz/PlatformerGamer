require("LuaPlayerStateBase")
LuaPlayerStateBase:subClass("PlayerWalk_RunState")

function PlayerWalk_RunState:new()
	local obj = self.base.new(self)
	csharp.stateMachine:LuaRisterState("PlayerWalk_RunState", obj)
	return obj
end

function PlayerWalk_RunState:Enter( )
	self.base.Enter(self)
	local csharp = self.csharp
	csharp:OnBufferComplete()
end

function PlayerWalk_RunState:Exit( )
	self.base.Exit(self)
end

function PlayerWalk_RunState:OnUpdate( )
	self.base.OnUpdate(self)
	local csharp = self.csharp
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
	self.base.OnFixedUpdate(self)
end

function PlayerWalk_RunState:OnLateUpdate( )
	self.base.OnLateUpdate(self)
end

return PlayerWalk_RunState