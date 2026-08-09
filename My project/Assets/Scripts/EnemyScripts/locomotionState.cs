using StarterAssets;
using System;
public class locomotionState : BaseState
{
    public locomotionState(ThirdPersonController enemyController) : base(playerController)
    {
    }

    public override void OnEnter() 
    {
        //call anything
    }

    public override void FixedUpdate()  
    {
        // add better patrol logic
        enemyController.Move();
    }

}


