public abstract class HexSelectState
{
    public abstract void EnterState(HexSelectManager manager);
    public abstract void UpdateState(HexSelectManager manager);
    public abstract void ExitState(HexSelectManager manager);
}