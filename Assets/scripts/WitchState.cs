
public abstract class WitchState 
{
    protected WitchAI witchAI;

    public WitchState(WitchAI witchAI)
    {
        this.witchAI = witchAI;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }
}
