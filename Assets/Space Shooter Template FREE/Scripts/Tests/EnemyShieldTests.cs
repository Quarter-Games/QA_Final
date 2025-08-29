using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EnemyShieldTests
{
    private class HitMarker : MonoBehaviour { }
    private class DestructionMarker : MonoBehaviour { }

    private Enemy CreateShieldedEnemy(int health, float shieldInterval = 0.05f)
    {
        var go = new GameObject("EnemyWithShield");
        var enemy = go.AddComponent<Enemy>();
        enemy.health = health;

        enemy.hitEffect = new GameObject("HitFX");
        enemy.hitEffect.AddComponent<HitMarker>();

        enemy.destructionVFX = new GameObject("DestFX");
        enemy.destructionVFX.AddComponent<DestructionMarker>();

        enemy.Projectile = new GameObject("EnemyProjectile");
        enemy.Projectile.AddComponent<Projectile>().damage = 1;

        enemy.shotChance = 0;
        enemy.shotTimeMin = 10f;
        enemy.shotTimeMax = 10f;

        enemy.shieldVFX = new GameObject("ShieldVFX");
        enemy.shieldVFX.SetActive(true);
        enemy.shieldActivationInterval = shieldInterval;

        // Manually invoke Start (private)
        typeof(Enemy).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.Invoke(enemy, null);

        return enemy;
    }

    [UnityTest]
    public IEnumerator Enemy_ShieldAbsorbsFirstHit_ThenDisables()
    {
        var enemy = CreateShieldedEnemy(health: 5);
        yield return null; // allow Start & first ActivateShield

        // First damage should be absorbed by shield (health unchanged)
        enemy.GetDamage(2);
        yield return null;

        Assert.AreEqual(5, enemy.health, "Shield should absorb the first damage completely.");
        Assert.IsFalse(enemy.shieldVFX.activeSelf, "Shield VFX should be deactivated after absorbing damage.");

        // Second damage should now reduce health
        enemy.GetDamage(2);
        yield return null;

        Assert.AreEqual(3, enemy.health, "Health should decrease after shield is gone.");
    }

    [UnityTest]
    public IEnumerator Enemy_ShieldDoesNotReactivate_AfterDeactivation()
    {
        var enemy = CreateShieldedEnemy(health: 4, shieldInterval: 0.05f);
        yield return null; // Start + initial shield activation

        enemy.GetDamage(1); // absorbed
        yield return null;

        Assert.IsFalse(enemy.shieldVFX.activeSelf, "Shield should be off after first damage.");

        // Wait longer than several shield intervals
        yield return new WaitForSeconds(0.2f);

        // Damage again (should reduce health now)
        enemy.GetDamage(1);
        yield return null;

        Assert.AreEqual(3, enemy.health, "Health should have decreased (shield must not reactivate).");
        Assert.IsFalse(enemy.shieldVFX.activeSelf, "Shield should remain off.");
    }
}