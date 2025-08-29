using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class WaveTests
{
    private GameObject CreateEnemyPrefab()
    {
        var enemyPrefab = new GameObject("EnemyPrefab");
        enemyPrefab.AddComponent<Enemy>();
        enemyPrefab.AddComponent<FollowThePath>();
        var e = enemyPrefab.GetComponent<Enemy>();
        e.hitEffect = new GameObject("HitFX");
        e.destructionVFX = new GameObject("DestFX");
        e.Projectile = new GameObject("EnemyProj");
        return enemyPrefab;
    }

    private Transform[] CreatePathPoints()
    {
        var points = new Transform[4];
        for (int i = 0; i < 4; i++)
        {
            var t = new GameObject("WavePath" + i).transform;
            t.position = new Vector3(i, 0, 0);
            points[i] = t;
        }
        return points;
    }

    [UnityTest]
    public IEnumerator Wave_CreatesSpecifiedNumberOfEnemies_AndDestroysItself()
    {
        var waveGO = new GameObject("Wave");
        var wave = waveGO.AddComponent<Wave>();
        wave.enemy = CreateEnemyPrefab();
        wave.count = 3;
        wave.speed = 0f;
        wave.timeBetween = 0f;
        wave.pathPoints = CreatePathPoints();
        wave.rotationByPath = false;
        wave.Loop = false;
        wave.testMode = false;
        wave.shooting = new Shooting { shotChance = 0, shotTimeMax = 0, shotTimeMin = 0 };

        yield return null; // Start & coroutine begin
        yield return null; // finish spawning (timeBetween=0)

        var enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None)
            .Where(e => e.gameObject.name.Contains("EnemyPrefab")).ToArray();
        Assert.AreEqual(3, enemies.Length, "Wave should spawn exact number of enemies.");

        // Coroutine should have destroyed wave (not loop)
        yield return new WaitForSeconds(0.5f);
        Assert.IsTrue(wave == null || wave.gameObject == null, "Wave object should be destroyed after spawning when not looping.");
    }
}