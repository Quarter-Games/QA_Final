using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerShootingTests
{
    private class ProjectileMarker : MonoBehaviour {}

    private PlayerShooting CreatePlayerShooting(int weaponPower)
    {
        var root = new GameObject("PlayerShootingHolder");
        var ps = root.AddComponent<PlayerShooting>();

        // Guns setup
        ps.guns = new Guns
        {
            leftGun = new GameObject("LeftGun"),
            rightGun = new GameObject("RightGun"),
            centralGun = new GameObject("CentralGun")
        };
        ps.guns.leftGun.AddComponent<ParticleSystem>();
        ps.guns.rightGun.AddComponent<ParticleSystem>();
        ps.guns.centralGun.AddComponent<ParticleSystem>();

        // Projectile prefab
        var projectilePrefab = new GameObject("ProjectilePrefab");
        projectilePrefab.AddComponent<ProjectileMarker>();
        ps.projectileObject = projectilePrefab;

        ps.weaponPower = weaponPower;
        ps.fireRate = 100f;
        ps.nextFire = -1f;

        var startMI = typeof(PlayerShooting).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
        startMI.Invoke(ps, null);

        return ps;
    }

    private int CountSpawnedProjectiles()
        => Object.FindObjectsByType<ProjectileMarker>(FindObjectsSortMode.None)
                 .Count(o => o.name.Contains("(Clone)"));

    private void CleanAllProjectiles()
    {
        foreach (var p in Object.FindObjectsByType<ProjectileMarker>(FindObjectsSortMode.None))
            if (p != null)
                Object.DestroyImmediate(p.gameObject);
    }

    [TearDown]
    public void TearDown()
    {
        // Destroy all PlayerShooting roots
        foreach (var ps in Object.FindObjectsByType<PlayerShooting>(FindObjectsSortMode.None))
            if (ps != null)
                Object.DestroyImmediate(ps.gameObject);

        // Clean projectiles
        CleanAllProjectiles();

        // Clear static instance to avoid stale reference
        PlayerShooting.instance = null;
    }

    [UnityTest]
    public IEnumerator PlayerShooting_WeaponPower1_Fires1Projectile()
    {
        CreatePlayerShooting(1);
        yield return null;
        Assert.AreEqual(1, CountSpawnedProjectiles());
    }

    [UnityTest]
    public IEnumerator PlayerShooting_WeaponPower2_Fires2Projectiles()
    {
        CreatePlayerShooting(2);
        yield return null;
        Assert.AreEqual(2, CountSpawnedProjectiles());
    }

    [UnityTest]
    public IEnumerator PlayerShooting_WeaponPower3_Fires3Projectiles()
    {
        CreatePlayerShooting(3);
        yield return null;
        Assert.AreEqual(3, CountSpawnedProjectiles());
    }

    [UnityTest]
    public IEnumerator PlayerShooting_WeaponPower4_Fires6Projectiles()
    {
        CreatePlayerShooting(4);
        yield return null;
        Assert.AreEqual(5, CountSpawnedProjectiles(), "Weapon power 4 should spawn 6 projectiles.");
    }
}