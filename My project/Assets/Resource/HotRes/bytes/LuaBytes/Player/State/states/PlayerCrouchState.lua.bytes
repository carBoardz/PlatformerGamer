require("PlayerCrouchState")
LuaPlayerStateBase:subClass("PlayerCrouchState")

function PlayerCrouchState:new()
	local obj = self.base.new(self)
	csharp.stateMachine:LuaRisterState("PlayerCrouchState", obj)
	return obj
end

function PlayerCrouchState:Enter( )
	self.base.Enter(self)
	local csharp = self.self
	csharp:OnBufferComplete()
end

function PlayerCrouchState:Exit( )
	self.base.Exit(self)
end

function PlayerCrouchState:OnUpdate( )
	self.base.OnUpdate(self)
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
	self.base.OnFixedUpdate(self)
end

function PlayerCrouchState:OnLateUpdate( )
	self.base.OnLateUpdate(self)
end

return PlayerCrouchState