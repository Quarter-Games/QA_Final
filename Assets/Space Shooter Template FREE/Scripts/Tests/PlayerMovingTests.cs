using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerMovingTests
{
    [UnityTest]
    public IEnumerator PlayerMoving_PositionClampedWithinBorders()
    {
        // Arrange camera
        var camGO = new GameObject("MainCamera");
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5;
        camGO.tag = "MainCamera";

        // Player
        var playerGO = new GameObject("PlayerMover");
        var mover = playerGO.AddComponent<PlayerMoving>();
        mover.borders = new Borders
        {
            minXOffset = 1f,
            maxXOffset = 1f,
            minYOffset = 1f,
            maxYOffset = 1f
        };

        // Allow Start() to run to compute borders
        yield return null;

        // Force position outside expected borders
        playerGO.transform.position = new Vector3(100, 100, 0);

        // Act
        yield return null; // Update executes clamping

        // Assert
        Assert.LessOrEqual(playerGO.transform.position.x, mover.borders.maxX + 0.0001f);
        Assert.GreaterOrEqual(playerGO.transform.position.x, mover.borders.minX - 0.0001f);
        Assert.LessOrEqual(playerGO.transform.position.y, mover.borders.maxY + 0.0001f);
        Assert.GreaterOrEqual(playerGO.transform.position.y, mover.borders.minY - 0.0001f);
    }
}