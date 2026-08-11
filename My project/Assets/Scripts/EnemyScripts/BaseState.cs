using StarterAssets;

public abstract class BaseState : IState
{
    protected readonly EnemyController enemyController;

    protected BaseState(EnemyController enemyController)
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

