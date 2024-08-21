using System;
using System.Collections;
using System.Collections.Generic;
using BigMouth;
using StaticNetcodeLib;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BigMouth.Scripts;

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
    
    [ClientRpc]
    public static void SetClientFakeItemClientRpc(ulong bigMouthId, ulong scrapId, int value)
    {
        var networkObjects = Object.FindObjectsByType<GrabbableObject>(FindObjectsSortMode.None);

        GrabbableObject grabbableObjectFound = null;

        foreach (var g in networkObjects)
        {
            if (g.NetworkObjectId == scrapId) grabbableObjectFound = g;
        }
        
        var networkObjectsBigMouth = Object.FindObjectsByType<BigMouthEnemyAI>(FindObjectsSortMode.None);

        BigMouthEnemyAI bigMouthEnemy = null;
        
        foreach (var g in networkObjectsBigMouth)
        {
            if (g.NetworkObjectId == bigMouthId) bigMouthEnemy = g;
        }

        if (grabbableObjectFound != null && bigMouthEnemy != null)
        {
            grabbableObjectFound.SetScrapValue(value);
            grabbableObjectFound.grabbable = false;
            
            bigMouthEnemy.SetFakeItemClient(grabbableObjectFound.gameObject);
        }
    }
    
    [ClientRpc]
    public static void SetBigFakeItemClientRpc(ulong networkId, string name)
    {
        var networkObjects = Object.FindObjectsByType<BigMouthEnemyAI>(FindObjectsSortMode.None);

        BigMouthEnemyAI bigMouthEnemy = null;
        
        foreach (var g in networkObjects)
        {
            if (g.NetworkObjectId == networkId) bigMouthEnemy = g;
        }
        
        if (bigMouthEnemy != null)
        {
            Debug.Log($"BIG MOUTH FOUND {networkId} SET FAKE ITEM {name}");
            bigMouthEnemy.SetFakeItem(name);
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
            ScanNodeProperties scanNodeProperties = grabbableObjectFound.GetComponentInChildren<ScanNodeProperties>();
            scanNodeProperties.headerText = "BigMouth body";
        }
    }
}