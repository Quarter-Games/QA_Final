using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EnemyTests
{
    private class HitMarker : MonoBehaviour { }
    private class DestructionMarker : MonoBehaviour { }
    private class ProjectileMarker : MonoBehaviour { }

    private Enemy CreateEnemy(int health, int shotChance = 0)
    {
        var go = new GameObject("Enemy");
        var enemy = go.AddComponent<Enemy>();
        enemy.health = health;

        enemy.hitEffect = new GameObject("HitEffect");
        enemy.hitEffect.AddComponent<HitMarker>();

        enemy.destructionVFX = new GameObject("DestructionVFX");
        enemy.destructionVFX.AddComponent<DestructionMarker>();

        var proj = new GameObject("EnemyProjectilePrefab");
        proj.AddComponent<ProjectileMarker>();
        proj.AddComponent<Projectile>().damage = 3;
        enemy.Projectile = proj;

        enemy.shotChance = shotChance;
        enemy.shotTimeMin = 0f;
        enemy.shotTimeMax = 0f;

        return enemy;
    }

    [UnityTest]
    public IEnumerator Enemy_GetDamage_SpawnsHitEffect_UntilDestroyed()
    {
        var enemy = CreateEnemy(2);
        yield return null;
        // Force shield off via reflection (set private bool isShielded = false)
        var shieldField = typeof(Enemy).GetField("isShielded", BindingFlags.Instance | BindingFlags.NonPublic);
        if (shieldField != null) shieldField.SetValue(enemy, false);
        // Also ensure shield VFX (if any) is disabled
        if (enemy.shieldVFX != null) enemy.shieldVFX.SetActive(false);

        enemy.GetDamage(1); // should not destroy
        yield return null;
        Assert.IsNotNull(enemy, "Enemy should still exist.");
        Assert.Greater(Object.FindObjectsByType<HitMarker>(FindObjectsSortMode.None).Count(h => h.name.Contains("(Clone)")), 0,
            "Hit effect clone should be instantiated.");

        enemy.GetDamage(1); // should destroy

        yield return new WaitForSeconds(0.5f);
        Assert.IsTrue(enemy == null || enemy.transform == null, "Enemy should be destroyed after lethal damage.");
        Assert.Greater(Object.FindObjectsByType<DestructionMarker>(FindObjectsSortMode.None).Count(h => h.name.Contains("(Clone)")), 0,
            "Destruction VFX clone should be instantiated.");
    }

    [UnityTest]
    public IEnumerator Enemy_ActivateShooting_SpawnsProjectile_WhenChance100()
    {
        var enemy = CreateEnemy(1, 100);
        var startMI = typeof(Enemy).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
        startMI.Invoke(enemy, null);
        var shootMI = typeof(Enemy).GetMethod("ActivateShooting", BindingFlags.Instance | BindingFlags.NonPublic);
        shootMI.Invoke(enemy, null);
        yield return null;
        Assert.Greater(Object.FindObjectsByType<ProjectileMarker>(FindObjectsSortMode.None)
            .Count(p => p.name.Contains("(Clone)")), 0, "Projectile should be instantiated.");
    }

    [UnityTest]
    public IEnumerator Enemy_ActivateShooting_NoProjectile_WhenChance0()
    {
        var enemy = CreateEnemy(1, 0);
        var shootMI = typeof(Enemy).GetMethod("ActivateShooting", BindingFlags.Instance | BindingFlags.NonPublic);
        shootMI.Invoke(enemy, null);
        yield return null;
        Assert.AreEqual(0, Object.FindObjectsByType<ProjectileMarker>(FindObjectsSortMode.None)
            .Count(p => p.name.Contains("(Clone)")), "No projectile should be instantiated.");
    }

    [UnityTest]
    public IEnumerator Enemy_PlayerCollision_CallsPlayerDamage()
    {
        var playerGO = new GameObject("Player");
        playerGO.tag = "Player";
        var player = playerGO.AddComponent<Player>();
        player.destructionFX = new GameObject("PlayerFX");
        var enemy = CreateEnemy(1);
        Player.instance = player;

        var collideMI = typeof(Enemy).GetMethod("OnTriggerEnter2D", BindingFlags.Instance | BindingFlags.NonPublic);
        var playerCollider = playerGO.AddComponent<BoxCollider2D>();
        collideMI.Invoke(enemy, new object[] { playerCollider });

        yield return null;
        Assert.IsTrue(player == null || player.gameObject == null, "Player should be destroyed after collision damage.");
    }
}