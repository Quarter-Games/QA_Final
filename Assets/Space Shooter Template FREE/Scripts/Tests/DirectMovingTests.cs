using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class DirectMovingTests
{
    [UnityTest]
    public IEnumerator DirectMoving_MovesUpBySpeedTimesDeltaTime()
    {
        var go = new GameObject("Mover");
        var dm = go.AddComponent<DirectMoving>();
        dm.speed = 10f;

        var startY = go.transform.position.y;
        yield return null; // allow one Update frame
        var movedY = go.transform.position.y - startY;

        // Use tolerance because deltaTime varies
        Assert.That(movedY, Is.InRange(10f * Time.deltaTime * 0.9f, 10f * Time.deltaTime * 1.1f),
            "Object did not move approximately speed * deltaTime on Y axis.");
    }
}