public enum DamageType
{
    None,
    Fire,
    Water,
    Air,
    Earth,
    Order,
    Chaos,
    Life,
    Death
}
//Interface class to allow iteration on diffrent highlight needs.
public interface IHealth
{
    public void TakeDamage(int damage, DamageType Element);
    public void Heal(int healing);
    public void HealPercent(double healing);
    public int ReturnHealth();

    public int ReturnBaseHealth();
}
