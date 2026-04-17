using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using XLua;

public class CharacterControllerBase : ControllerBase
{
    public PlayerHotLogic playerHotLogic { get; protected set; }
    public InputController inputActions { get; protected set; }
    public PlayerMovementStateMachine playerMovementStateMachine { get; protected set; }
    public PlayerAnimationController playerAnimationController { get; protected set; }
    public LuaPlayerState luaPlayerState { get; protected set; }
    public Animator animator;
    public LuaTable CharacterluaTable;
    protected override void Init()
    {
        base.Init();
        inputActions = new InputController();
        inputActions.Enable();
        
        playerMovementStateMachine = new PlayerMovementStateMachine(this as PlayerController);

        animator = GetComponent<Animator>();
        playerAnimationController = GetComponent<PlayerAnimationController>();
        playerAnimationController.Init(this as PlayerController, animator);

        if (CharacterluaTable != null)
        {
            playerMovementStateMachine.LuaRisterState("DefaultState", CharacterluaTable);
        }
    }
}
