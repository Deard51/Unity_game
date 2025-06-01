using UnityEngine;

public interface IWeapon
{
    void Use(Vector2 direction);
    int GetDamage();
}
