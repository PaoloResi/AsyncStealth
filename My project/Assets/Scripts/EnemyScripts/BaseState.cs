using StarterAssets;

public abstract class BaseState : IState
{
    protected readonly ThirdPersonController enemyController;

    protected BaseState(ThirdPersonController enemyController)
    {
        this.enemyController = enemyController;
    }
    public virtual void OnEnter()
    {

    }

    public virtual void Update()
    {

    }

    public virtual void FixedUpdate()
    {

    }

    public virtual void OnExit()
    {

    }
}

