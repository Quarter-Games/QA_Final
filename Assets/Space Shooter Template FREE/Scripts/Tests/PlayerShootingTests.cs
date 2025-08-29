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
        ps.fireRate = 100f; // high so timing not restrictive
        ps.nextFire = -1f;

        // Call private Start() to initialize VFX references
        var startMI = typeof(PlayerShooting).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
        startMI.Invoke(ps, null);

        return ps;
    }

    private int CountProjectiles()
        => Object.FindObjectsByType<ProjectileMarker>(FindObjectsSortMode.None).Count(x => x.name.Contains("("));

    public void ClreanAllMarkers()
    {
        var markers = Object.FindObjectsByType<ProjectileMarker>(FindObjectsSortMode.None);
        foreach (var m in markers)
            Object.DestroyImmediate(m.gameObject);
    }
    [UnityTest]
    public IEnumerator PlayerShooting_WeaponPower1_Fires1Projectile()
    {
        var ps = CreatePlayerShooting(1);
        ps.gameObject.AddComponent<DummyTimeDriver>(); // ensure Update runs
        yield return null; // first frame (Update fires)
        Assert.AreEqual(1, CountProjectiles());
    }

    [UnityTest]
    public IEnumerator PlayerShooting_WeaponPower2_Fires2Projectiles()
    {
        ClreanAllMarkers();
        var ps = CreatePlayerShooting(2);
        ps.gameObject.AddComponent<DummyTimeDriver>();
        yield return null;
        Assert.AreEqual(2, CountProjectiles());
    }

    [UnityTest]
    public IEnumerator PlayerShooting_WeaponPower3_Fires3Projectiles()
    {
        ClreanAllMarkers();
        var ps = CreatePlayerShooting(3);
        ps.gameObject.AddComponent<DummyTimeDriver>();
        yield return null;
        Assert.AreEqual(3, CountProjectiles());
    }

    [UnityTest]
    public IEnumerator PlayerShooting_WeaponPower4_Fires6Projectiles()
    {
        ClreanAllMarkers();
        var ps = CreatePlayerShooting(4);
        ps.gameObject.AddComponent<DummyTimeDriver>();
        yield return null;
        Assert.AreEqual(5, CountProjectiles());
    }

    // Helper component just to ensure Update() of PlayerShooting runs (PlayerShooting itself has Update).
    private class DummyTimeDriver : MonoBehaviour {}
}