
using System;
using GameNetcodeStuff;
using JetBrains.Annotations;
using System.Collections;
using System.Linq;
using BigMouth;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BigEyes.Scripts;

public class BigMouthEnemyAI: EnemyAI
{
    
    public AudioClip angrySound;
    public AudioSource screamSound;
    public GameObject TeethObjectContainer;

    private GameObject fakeItemGameObject;
    private ScanNodeProperties scanNode;

    private bool haveAFakeItem;
    
    private bool deadAnimHaveBeenCalled;

    private int value;
    
    private float attackPlayerTimer = 0;
    private float chasePlayerTimer = BigMouthPlugin.instance.chaseDuration.Value;
    public float aiInterval;
    public bool isPlayerClose;
    
    private static readonly int Angry = Animator.StringToHash("Angry");
    public int lastBehaviorState;
    private static readonly int Dead = Animator.StringToHash("Dead");

    IEnumerator DeadCoroutine()
    {
        yield return new WaitForSeconds(1);
        if (!IsServer || BigMouthPlugin.instance.teehGameObject == null) yield return null;

        GameObject teethObject = Instantiate(BigMouthPlugin.instance.teehGameObject,transform.position, Quaternion.identity);
        var networkObject = teethObject.GetComponent<NetworkObject>();
        networkObject.Spawn();
        NetworkBigMouth.SetTeethValueClientRpc(networkObject.NetworkObjectId, value );
        NetworkObject.Despawn();

    }

    public void SetValue(int value)
    {
        this.value = value;
        scanNode = GetComponentInChildren<ScanNodeProperties>();
        scanNode.subText = $"Value: {value}";
    }

    public void SetFakeItemClient(GameObject gameObject)
    {
        fakeItemGameObject = gameObject;

        var grabable = fakeItemGameObject.GetComponent<GrabbableObject>();
        var scanNodeFakeItem = fakeItemGameObject.GetComponentInChildren<ScanNodeProperties>();
        
        grabable.isInShipRoom = false;
        var pos = fakeItemGameObject.transform.localPosition + grabable.itemProperties.verticalOffset * Vector3.up;
        
        Destroy(grabable);

        fakeItemGameObject.transform.localPosition = pos;
        
        fakeItemGameObject.transform.parent = transform;
        fakeItemGameObject.tag = "PhysicsProp";
        fakeItemGameObject.layer = LayerMask.NameToLayer("Props");
        scanNodeFakeItem.tag = "DoNotSet";
        scanNodeFakeItem.gameObject.layer = LayerMask.NameToLayer("ScanNode");;
        
        
        scanNode.gameObject.SetActive(false);
        haveAFakeItem = true;
        ChangeFakeItemState(false);
    }

    public void SetFakeItem(string name)
    {
        if(IsServer)
        {
            var fakeObject = FindNetworkGameObject(name);
            GrabbableObject grabbableObject = fakeObject.GetComponent<GrabbableObject>();

            fakeItemGameObject = Instantiate(fakeObject, TeethObjectContainer.transform.position, Quaternion.identity, transform);
            fakeItemGameObject.transform.rotation = Quaternion.Euler(grabbableObject.itemProperties.restingRotation);
            var grabable = fakeItemGameObject.GetComponent<GrabbableObject>();
            grabable.isInShipRoom = false;
            var scrapNetwork = fakeItemGameObject.GetComponent<NetworkObject>();

            scrapNetwork.Spawn();

            var scrapMutliplier = RoundManager.Instance.scrapValueMultiplier;
            
            NetworkBigMouth.SetClientFakeItemClientRpc(NetworkObjectId, scrapNetwork.NetworkObjectId, Random.Range(  Mathf.RoundToInt(grabable.itemProperties.minValue * scrapMutliplier), Mathf.RoundToInt(grabable.itemProperties.maxValue * scrapMutliplier)));
        }
    }

    public void ChangeFakeItemState(bool isAngry)
    {
        if(!haveAFakeItem) return;

        TeethObjectContainer.SetActive(isAngry);
        fakeItemGameObject.SetActive(!isAngry);
        
    }

    public void GetNetworkPrefab()
    {
        BigMouthPlugin.instance.everyScrapsItems.Clear();
        RoundManager.Instance.currentLevel.spawnableScrap.ToList().ForEach(
            prefab =>
            {
                GrabbableObject grabbableObject = prefab.spawnableItem.spawnPrefab.GetComponent<GrabbableObject>();
                if (grabbableObject != null)
                {
                    if (grabbableObject.itemProperties.isScrap &&
                        BigMouthPlugin.instance.CanTransformInItem(grabbableObject.itemProperties.itemName))
                        BigMouthPlugin.instance.everyScrapsItems.Add(grabbableObject.itemProperties.itemName);
                    if (grabbableObject.itemProperties.itemName == "Teeth")
                    {
                        BigMouthPlugin.instance.teehGameObject = prefab.spawnableItem.spawnPrefab;
                    }
                }

            });
        
        /* if (BigMouthPlugin.instance.teehGameObject == null)
        {
            NetworkManager.NetworkConfig.Prefabs.NetworkPrefabsLists.ForEach(
                list => list.PrefabList.ToList().ForEach(
                    prefab =>
                    {
                        GrabbableObject grabbableObject = prefab.Prefab.GetComponent<GrabbableObject>();
                        if (grabbableObject != null)
                        {
                            if(grabbableObject.itemProperties.isScrap && BigMouthPlugin.instance.CanTransformInItem(grabbableObject.itemProperties.itemName)) BigMouthPlugin.instance.everyScrapsItems.Add(grabbableObject.itemProperties.itemName);
                            if (grabbableObject.itemProperties.itemName == "Teeth")
                            {
                                BigMouthPlugin.instance.teehGameObject = prefab.Prefab;
                            }
                        } 

                    }) 
            );
        }*/
    }

    public GameObject FindNetworkGameObject(string itemName)
    {

        GameObject gameObject = BigMouthPlugin.instance.teehGameObject;
        
        RoundManager.Instance.currentLevel.spawnableScrap.ToList().ForEach(
            prefab =>
            {
                GrabbableObject grabbableObject = prefab.spawnableItem.spawnPrefab.GetComponent<GrabbableObject>();
                if (grabbableObject != null)
                {
                    if (grabbableObject.itemProperties.itemName == itemName)
                    {
                        gameObject = prefab.spawnableItem.spawnPrefab;
                    }
                }

            });
        
       /* NetworkManager.NetworkConfig.Prefabs.NetworkPrefabsLists.ForEach(
            list => list.PrefabList.ToList().ForEach(
                prefab =>
                {
                    GrabbableObject grabbableObject = prefab.Prefab.GetComponent<GrabbableObject>();
                    if (grabbableObject != null)
                    {
                        if (grabbableObject.itemProperties.itemName == itemName)
                        {
                            gameObject = prefab.Prefab;
                        }
                    } 

                }) 
        ); */

        return gameObject;
    }

    
    public override void Start()
    {
        base.Start();
        agent.stoppingDistance = 1f;

        GetNetworkPrefab();
        FindNetworkGameObject("Teeth");

        if (IsServer)
        {
            NetworkBigMouth.SetBigMouthValueClientRpc(NetworkObjectId, Random.Range(BigMouthPlugin.instance.minTeethValue.Value, BigMouthPlugin.instance.maxTeethValue.Value));

            if (BigMouthPlugin.instance.canBeEveryItem.Value)
            {
                NetworkBigMouth.SetBigFakeItemClientRpc(NetworkObjectId,
                    BigMouthPlugin.instance.everyScrapsItems[
                        Random.Range(0, BigMouthPlugin.instance.everyScrapsItems.Count)]);
            }
            
        }
    }

    public override void Update()
    {

        if (isEnemyDead )
        {
            if (deadAnimHaveBeenCalled) return;
            deadAnimHaveBeenCalled = true;
            SetAnimation();
            if(IsServer) StartCoroutine(DeadCoroutine());
            
            return;
        }
        
        base.Update();
        aiInterval -= Time.deltaTime;
        attackPlayerTimer -= Time.deltaTime;
        chasePlayerTimer -= Time.deltaTime;

        if (lastBehaviorState != currentBehaviourStateIndex)
        {
            ChangeFakeItemState(currentBehaviourStateIndex == 1);
            lastBehaviorState = currentBehaviourStateIndex;
            SetAnimation();
            if (currentBehaviourStateIndex == 1)
            {
                attackPlayerTimer = 1f;
            }
        }
        
        if (GameNetworkManager.Instance.localPlayerController.HasLineOfSightToPosition(transform.position + Vector3.up * 0.25f, 100f, 25))
        {
            if (currentBehaviourStateIndex == 1)
                GameNetworkManager.Instance.localPlayerController.JumpToFearLevel(1f);
        }
        
        
        if (aiInterval <= 0 && IsOwner)
        {
            aiInterval = AIIntervalTime;
            DoAIInterval();
        }
    }

    public void PlayerIsClose(bool close, [CanBeNull] Collider other)
    {
        movingTowardsTargetPlayer = close;
        isPlayerClose = close;
        if (!IsOwner || targetPlayer != null && close) return;
        
        if (close && other != null)
        {
            var player = other.GetComponent<PlayerControllerB>();
            if(player == null || player.isPlayerDead || isEnemyDead) return;
            if(PlayerIsTargetable(player))
            {
                chasePlayerTimer = BigMouthPlugin.instance.chaseDuration.Value;
                SetMovingTowardsTargetPlayer(player);
                SwitchToBehaviourClientRpc(1);
            }
        }
        else
        {
            targetPlayer = null;
            SwitchToBehaviourState(0);
        }
        
    }

    public override void HitEnemy(int force = 1, PlayerControllerB playerWhoHit = null, bool playHitSFX = false, int hitID = -1)
    {
        base.HitEnemy(force, playerWhoHit, playHitSFX, hitID);
        Debug.Log($"ENEMY HIT {isEnemyDead} {enemyHP}");
        if (isEnemyDead)
            return;
        enemyHP -= force;
        if (enemyHP > 0 || !IsOwner) return;
        KillEnemyOnOwnerClient();
    }

    public override void DoAIInterval()
    {
        base.DoAIInterval();
        
        switch (currentBehaviourStateIndex)
        {
            case 0:
            {
                agent.speed = 0f;
                break;
            }
            case 1:
            {
                agent.speed = BigMouthPlugin.instance.angrySpeed.Value;
                agent.acceleration = BigMouthPlugin.instance.angryAcceleration.Value;
                
                if (targetPlayer != null && PlayerIsTargetable(targetPlayer))
                {
                    SetMovingTowardsTargetPlayer(targetPlayer);
                }
                if (chasePlayerTimer <= 0)
                {
                    if(isPlayerClose)
                    {
                        chasePlayerTimer = BigMouthPlugin.instance.chaseDuration.Value / 2;
                        break;
                    }
                    PlayerIsClose(false, null);
                }
                
                break;
            }
            default: break;
                
        }
        

    }

    public void SetAnimation()
    {

        if (isEnemyDead)
        {
            creatureAnimator.SetBool(Dead, true);
            return;
        }

        switch (currentBehaviourStateIndex)
        {
            case 0:
            {
                creatureVoice.clip = null ;
                creatureAnimator.SetBool(Angry, false);
                break;
            }
            case 1:
            {
                screamSound.Play();
                creatureVoice.clip = angrySound ;
                creatureVoice.Play();
                creatureAnimator.SetBool(Angry, true);
                break;
            }
        }
    }
    
    public override void OnCollideWithPlayer(Collider other)
    {
        if(currentBehaviourStateIndex == 0) return;
        var player = MeetsStandardPlayerCollisionConditions(other, false, true);
        if (player != null && attackPlayerTimer <= 0)
        {
            player.DamagePlayer(BigMouthPlugin.instance.attackDamage.Value, causeOfDeath: CauseOfDeath.Crushing);
            attackPlayerTimer = BigMouthPlugin.instance.attackPlayerDelay.Value;
        }
    }
}