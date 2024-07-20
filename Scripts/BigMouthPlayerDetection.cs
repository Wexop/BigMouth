using System;
using BigMouth;
using UnityEngine;

namespace BigEyes.Scripts;

public class BigMouthPlayerDetection: MonoBehaviour
{
    public BigMouthEnemyAI MouthEnemyAI;

    public void Start()
    {
        var width = BigMouthPlugin.instance.playerDetectionDistance.Value;

        transform.localScale = new Vector3(width, 4.65f, width);
    }

    public void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player")) MouthEnemyAI.PlayerIsClose(true, other);
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            if (!MouthEnemyAI.isPlayerClose) MouthEnemyAI.isPlayerClose = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) MouthEnemyAI.isPlayerClose = false;
    }
}