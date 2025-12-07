using UnityEngine;

public class KaiHealth : MonoBehaviour, IHealth
{
    public int BaseHealth;
    private int ExtraHealth;
    private int TotalHealth;
    public int CurrentHealth;

    public void Start()
    {
        Attributes SetExtras = GetComponentInParent<Attributes>();
        ExtraHealth = (SetExtras.Grit + (SetExtras.Chutzpah / 2)) * 5;
        TotalHealth = ExtraHealth + BaseHealth;
        CurrentHealth = TotalHealth;
    }

    public void Heal(int healing)
    {
        CurrentHealth += healing;
        if (CurrentHealth > TotalHealth)
        {
            CurrentHealth = TotalHealth;
        }
    }

    public void HealPercent(double healing)
    {
        double percent = TotalHealth * healing;
        CurrentHealth += (int)percent;
        if (CurrentHealth > TotalHealth)
        {
            CurrentHealth = TotalHealth;
        }
    }

    public int ReturnHealth()
    {
        return CurrentHealth;
    }

    public void TakeDamage(int damage, DamageType Element)
    {
        CurrentHealth -= damage;
    }

    public int ReturnBaseHealth()
    {
        return TotalHealth;
    }
}
