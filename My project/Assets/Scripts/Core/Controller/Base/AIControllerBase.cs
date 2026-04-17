using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIControllerBase : ControllerBase
{
    public AIHotLogic aiHotLogic { get; protected set; }
    public AIControlleStateMechine aiControlleStateMechine { get; protected set; }
    protected override void Init()
    {
        base.Init();
        aiControlleStateMechine = new AIControlleStateMechine(this as AIController);
    }
}
