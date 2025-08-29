using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class FollowThePathTests
{
    private Transform[] CreateLinearPath(Vector3 start, int count, float step)
    {
        var arr = new Transform[count];
        for (int i = 0; i < count; i++)
        {
            var p = new GameObject("PathPoint" + i).transform;
            p.position = start + new Vector3(0, step * i, 0);
            arr[i] = p;
        }
        return arr;
    }

    [UnityTest]
    public IEnumerator FollowThePath_DestroysAfterCompletion_WhenNoLoop()
    {
        var go = new GameObject("Follower");
        var follow = go.AddComponent<FollowThePath>();
        follow.path = CreateLinearPath(Vector3.zero, 4, 1f);
        follow.speed = 500f; // very fast
        follow.loop = false;
        follow.rotationByPath = false;
        follow.SetPath();

        // Wait a few frames to allow completion & destruction
        yield return new WaitForSeconds(1);

        Assert.IsTrue(follow == null || follow.gameObject == null, "Follower should be destroyed after finishing path when not looping.");
    }

    [UnityTest]
    public IEnumerator FollowThePath_LoopsWithoutDestruction()
    {
        var go = new GameObject("FollowerLoop");
        var follow = go.AddComponent<FollowThePath>();
        follow.path = CreateLinearPath(Vector3.zero, 4, 1f);
        follow.speed = 500f;
        follow.loop = true;
        follow.rotationByPath = false;
        follow.SetPath();

        for (int i = 0; i < 10; i++) yield return null;

        Assert.IsNotNull(follow, "Follower should persist when loop is true.");
    }
}