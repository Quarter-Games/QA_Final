using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ProjectileTests
{
    private Projectile CreateProjectile(bool enemyBullet, bool destroyedByCollision, int damage)
    {
        var go = new GameObject("Projectile");
        var proj = go.AddComponent<Projectile>();
        proj.enemyBullet = enemyBullet;
        proj.destroyedByCollision = destroyedByCollision;
        proj.damage = damage;
        go.AddComponent<BoxCollider2D>().isTrigger = true;
        return proj;
    }

    [UnityTest]
    public IEnumerator Projectile_EnemyBullet_DamagesPlayer_And_Destroys()
    {
        // Player
        var playerGO = new GameObject("Player");
        playerGO.tag = "Player";
        var player = playerGO.AddComponent<Player>();
        player.destructionFX = new GameObject("FX");
        Player.instance = player;

        // Projectile
        var proj = CreateProjectile(true, true, 5);

        // Simulate collision via reflection
        var collideMI = typeof(Projectile).GetMethod("OnTriggerEnter2D", BindingFlags.Instance | BindingFlags.NonPublic);
        var playerCollider = playerGO.AddComponent<BoxCollider2D>();
        collideMI.Invoke(proj, new object[] { playerCollider });

        yield return null;
        Assert.IsTrue(player == null || player.gameObject == null, "Player should be destroyed by damage.");
        Assert.IsTrue(proj == null || proj.gameObject == null, "Projectile should be destroyed if destroyedByCollision is true.");
    }

    [UnityTest]
    public IEnumerator Projectile_PlayerBullet_DamagesEnemy()
    {
        // Enemy
        var enemyGO = new GameObject("Enemy");
        enemyGO.tag = "Enemy";
        enemyGO.AddComponent<BoxCollider2D>();
        var enemy = enemyGO.AddComponent<Enemy>();
        enemy.health = 5;
        enemy.hitEffect = new GameObject("HitFX");
        enemy.destructionVFX = new GameObject("DestFX");
        enemy.Projectile = new GameObject("DummyProj");

        // Player instance (needed only if enemy triggers damage to player, not here)
        var playerGO = new GameObject("Player");
        playerGO.tag = "Player";
        var player = playerGO.AddComponent<Player>();
        player.destructionFX = new GameObject("FX");
        Player.instance = player;

        // Projectile
        var proj = CreateProjectile(false, true, 2);

        var collideMI = typeof(Projectile).GetMethod("OnTriggerEnter2D", BindingFlags.Instance | BindingFlags.NonPublic);
        collideMI.Invoke(proj, new object[] { enemyGO.GetComponent<Collider2D>() });

        yield return null;
        Assert.AreEqual(3, enemy.health, "Enemy health should be reduced by projectile damage.");
        Assert.IsTrue(proj == null || proj.gameObject == null, "Projectile should be destroyed if destroyedByCollision is true.");
    }
}