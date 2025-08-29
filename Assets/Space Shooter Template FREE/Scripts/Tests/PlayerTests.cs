using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerTests
{
    private class FXMarker : MonoBehaviour {}

    [UnityTest]
    public IEnumerator Player_GetDamage_SpawnsFX_And_DestroysPlayer()
    {
        // Arrange
        var playerGO = new GameObject("Player");
        var player = playerGO.AddComponent<Player>();

        var fxPrefab = new GameObject("FXPrefab");
        fxPrefab.AddComponent<FXMarker>();
        player.destructionFX = fxPrefab;

        // Act
        player.GetDamage(1);
        yield return null; // allow Destroy & Instantiate to process

        // Assert
        var spawnedFX = Object.FindObjectsOfType<FXMarker>();
        Assert.AreEqual(1, spawnedFX.Length, "Destruction FX was not instantiated exactly once.");
        Assert.IsTrue(playerGO == null, "Player GameObject should be destroyed after taking damage.");

        Object.DestroyImmediate(fxPrefab);
        foreach (var fx in spawnedFX)
            Object.DestroyImmediate(fx.gameObject);
    }
}
