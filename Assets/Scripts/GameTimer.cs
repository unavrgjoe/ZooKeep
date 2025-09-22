using System;
using System.Collections;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    public static event Action Tick2s;

    public float intervalSeconds = 2f;

    Coroutine _loop;

    void OnEnable() => _loop = StartCoroutine(Loop());
    void OnDisable() { if (_loop != null) StopCoroutine(_loop); _loop = null; }

    IEnumerator Loop()
    {
        // Reuse WaitForSeconds to avoid per-tick allocs
        var wait = new WaitForSeconds(intervalSeconds);
        while (true)
        {
            Debug.Log("Invkoing 2 second Action");
            Tick2s?.Invoke();
            yield return wait;
        }
    }
}