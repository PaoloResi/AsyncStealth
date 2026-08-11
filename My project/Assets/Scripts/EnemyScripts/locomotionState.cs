using StarterAssets;
using System;
public class locomotionState : BaseState
{
    public locomotionState(EnemyController enemyController) : base(enemyController)
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


