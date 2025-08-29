using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class BonusTests
{
    private Bonus CreateBonus()
    {
        var go = new GameObject("Bonus");
        var bonus = go.AddComponent<Bonus>();
        go.AddComponent<BoxCollider2D>().isTrigger = true;
        return bonus;
    }

    private PlayerShooting CreatePlayerShooting(int weaponPower, int maxPower)
    {
        var go = new GameObject("PlayerShooting");
        var ps = go.AddComponent<PlayerShooting>();
        ps.guns = new Guns
        {
            leftGun = new GameObject("LeftGun"),
            rightGun = new GameObject("RightGun"),
            centralGun = new GameObject("CentralGun")
        };
        ps.guns.leftGun.AddComponent<ParticleSystem>();
        ps.guns.rightGun.AddComponent<ParticleSystem>();
        ps.guns.centralGun.AddComponent<ParticleSystem>();
        ps.fireRate = 1;
        ps.weaponPower = weaponPower;
        ps.maxweaponPower = maxPower;
        ps.projectileObject = new GameObject("ProjectilePrefab");
        PlayerShooting.instance = ps;

        // Initialize Start (private)
        var startMI = typeof(PlayerShooting).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
        startMI.Invoke(ps, null);

        return ps;
    }

    [UnityTest]
    public IEnumerator Bonus_IncreasesWeaponPower_WhenBelowMax()
    {
        var bonus = CreateBonus();
        var playerGO = new GameObject("Player");
        playerGO.tag = "Player";
        playerGO.AddComponent<BoxCollider2D>();
        var ps = CreatePlayerShooting(1, 4);

        // Invoke OnTriggerEnter2D via reflection
        var collideMI = typeof(Bonus).GetMethod("OnTriggerEnter2D", BindingFlags.Instance | BindingFlags.NonPublic);
        collideMI.Invoke(bonus, new object[] { playerGO.GetComponent<Collider2D>() });

        yield return null;
        Assert.AreEqual(2, ps.weaponPower);
        Assert.IsTrue(bonus == null || bonus.gameObject == null, "Bonus should be destroyed after pickup.");
    }

    [UnityTest]
    public IEnumerator Bonus_DoesNotExceedMaxWeaponPower()
    {
        var bonus = CreateBonus();
        var playerGO = new GameObject("Player");
        playerGO.tag = "Player";
        playerGO.AddComponent<BoxCollider2D>();
        var ps = CreatePlayerShooting(4, 4);

        var collideMI = typeof(Bonus).GetMethod("OnTriggerEnter2D", BindingFlags.Instance | BindingFlags.NonPublic);
        collideMI.Invoke(bonus, new object[] { playerGO.GetComponent<Collider2D>() });

        yield return null;
        Assert.AreEqual(4, ps.weaponPower, "Weapon power should not exceed max.");
    }
}