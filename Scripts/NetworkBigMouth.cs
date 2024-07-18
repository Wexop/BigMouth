using System;
using System.Collections;
using System.Collections.Generic;
using StaticNetcodeLib;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BigEyes.Scripts;

[StaticNetcode]
public class NetworkBigMouth
{
    
    [ClientRpc]
    public static void SetTeethValueClientRpc(ulong networkId, int value)
    {
        BigMouthPlugin.instance.StartCoroutine(SetValue(networkId, value));
    }
    
    [ClientRpc]
    public static void SetBigMouthValueClientRpc(ulong networkId, int value)
    {
        var networkObjects = Object.FindObjectsByType<BigMouthEnemyAI>(FindObjectsSortMode.None);

        BigMouthEnemyAI bigMouthEnemy = null;
        
        foreach (var g in networkObjects)
        {
            if (g.NetworkObjectId == networkId) bigMouthEnemy = g;
        }
        
        if (bigMouthEnemy != null)
        {
            //Debug.Log($"BIG MOUTH FOUND {networkId} SET VALUE {value}");
            bigMouthEnemy.SetValue(value);
        }
    }
    

    public static IEnumerator SetValue(ulong networkId, int value)
    {

        yield return new WaitForSeconds(1);
        
        var networkObjects = Object.FindObjectsByType<GrabbableObject>(FindObjectsSortMode.None);

        GrabbableObject grabbableObjectFound = null;

        foreach (var g in networkObjects)
        {
            if (g.NetworkObjectId == networkId) grabbableObjectFound = g;
        }

        if (grabbableObjectFound != null)
        {
            Debug.Log($"TEETH FOUND {networkId} SET VALUE {value}");
            grabbableObjectFound.SetScrapValue(value);
        }
    }
}