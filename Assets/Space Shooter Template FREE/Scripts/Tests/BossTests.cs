using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class BossTests
{
    private class HitMarker : MonoBehaviour { }
    private class DestructionMarker : MonoBehaviour { }
    private class ProjectileMarker : MonoBehaviour { }

    private GameObject CreatePlayer(int damageTakenDestroys = 1)
    {
        var playerGO = new GameObject("Player");
        playerGO.tag = "Player";
        var player = playerGO.AddComponent<Player>();
        player.destructionFX = new GameObject("PlayerFX");
        Player.instance = player;
        return playerGO;
    }

    private Boss CreateBoss(int health, int shotChance = 0)
    {
        var go = new GameObject("Boss");
        var boss = go.AddComponent<Boss>();
        boss.health = health;

        boss.hitEffect = new GameObject("BossHitFX");
        boss.hitEffect.AddComponent<HitMarker>();

        boss.destructionVFX = new GameObject("BossDestFX");
        boss.destructionVFX.AddComponent<DestructionMarker>();

        var proj = new GameObject("BossProjectilePrefab");
        proj.AddComponent<ProjectileMarker>();
        proj.AddComponent<Projectile>().damage = 3;
        boss.Projectile = proj;

        boss.shotChance = shotChance;
        boss.shotTimeMin = 0f;
        boss.shotTimeMax = 0f;

        boss.moveSpeed = 5f;
        boss.changeDirectionTime = 0.05f;
        boss.maxMovementRadius = 2f;

        return boss;
    }

    private void InvokePrivateStart(MonoBehaviour mb)
    {
        typeof(Boss).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.Invoke(mb, null);
    }

    [TearDown]
    public void TearDown()
    {
        Player.instance = null;
        foreach (var obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            Object.DestroyImmediate(obj);
    }

    [UnityTest]
    public IEnumerator Boss_GetDamage_SpawnsHitEffect_UntilDestroyed()
    {
        CreatePlayer();
        var boss = CreateBoss(health: 2);
        InvokePrivateStart(boss);
        yield return null;

        boss.GetDamage(1);
        yield return null;
        Assert.IsNotNull(boss, "Boss should survive first hit.");
        Assert.Greater(Object.FindObjectsByType<HitMarker>(FindObjectsSortMode.None)
            .Count(h => h.name.Contains("(Clone)")), 0, "Hit effect should spawn.");

        boss.GetDamage(1);
        yield return null;
        Assert.IsTrue(boss == null, "Boss should be destroyed after lethal damage.");
        Assert.Greater(Object.FindObjectsByType<DestructionMarker>(FindObjectsSortMode.None)
            .Count(h => h.name.Contains("(Clone)")), 0, "Destruction VFX should spawn.");
    }

    [UnityTest]
    public IEnumerator Boss_ActivateShooting_SpawnsProjectile_WhenChance100()
    {
        CreatePlayer();
        var boss = CreateBoss(health: 1, shotChance: 100);
        InvokePrivateStart(boss);
        // Directly invoke private ActivateShooting to avoid timing issues
        typeof(Boss).GetMethod("ActivateShooting", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.Invoke(boss, null);
        yield return null;

        Assert.Greater(Object.FindObjectsByType<ProjectileMarker>(FindObjectsSortMode.None)
            .Count(p => p.name.Contains("(Clone)")), 0, "Projectile should be instantiated.");
    }

    [UnityTest]
    public IEnumerator Boss_ActivateShooting_NoProjectile_WhenChance0()
    {
        CreatePlayer();
        var boss = CreateBoss(health: 1, shotChance: 0);
        InvokePrivateStart(boss);
        typeof(Boss).GetMethod("ActivateShooting", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.Invoke(boss, null);
        yield return null;

        Assert.AreEqual(0, Object.FindObjectsByType<ProjectileMarker>(FindObjectsSortMode.None)
            .Count(p => p.name.Contains("(Clone)")), "No projectile should be instantiated.");
    }

    [UnityTest]
    public IEnumerator Boss_PlayerCollision_DamagesPlayer()
    {
        var playerGO = CreatePlayer();
        var player = Player.instance;
        var boss = CreateBoss(health: 1);
        InvokePrivateStart(boss);
        yield return null;

        var box = playerGO.AddComponent<BoxCollider2D>();
        var collideMI = typeof(Boss).GetMethod("OnTriggerEnter2D", BindingFlags.Instance | BindingFlags.NonPublic);
        collideMI.Invoke(boss, new object[] { box });

        yield return null;
        Assert.IsTrue(player == null || player.gameObject == null, "Player should be destroyed by boss collision damage.");
    }

    [UnityTest]
    public IEnumerator Boss_Movement_StaysWithinRadius()
    {
        CreatePlayer();
        var boss = CreateBoss(health: 5);
        var startPos = boss.transform.position;
        InvokePrivateStart(boss);
        yield return null;

        // Simulate several frames
        for (int i = 0; i < 120; i++)
            yield return null;

        float distance = Vector2.Distance(startPos, boss.transform.position);
        Assert.LessOrEqual(distance, boss.maxMovementRadius + 0.1f, "Boss should remain within movement radius.");
    }
}