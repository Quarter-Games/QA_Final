using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerTests
{
    private class FXMarker : MonoBehaviour { }

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
        var spawnedFX = Object.FindObjectsByType<FXMarker>(FindObjectsSortMode.None);
        Assert.AreEqual(2, spawnedFX.Length, "Expected exactly 2 FX objects (original prefab + instantiated clone).");
        Assert.IsTrue(playerGO == null, "Player GameObject should be destroyed after taking damage.");

        Object.DestroyImmediate(fxPrefab);
        foreach (var fx in spawnedFX)
            if (fx != null)
                Object.DestroyImmediate(fx.gameObject);
    }
}
