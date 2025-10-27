using System.Collections.Generic;
using BepInEx;
using System.IO;
using System.Reflection;
using BepInEx.Configuration;
using BigMouth.Utils;
using LethalConfig;
using LethalConfig.ConfigItems;
using LethalConfig.ConfigItems.Options;
using UnityEngine;
using LethalLib.Modules;
using NetworkPrefabs = LethalLib.Modules.NetworkPrefabs;

namespace BigMouth;

[BepInPlugin(GUID, NAME, VERSION)]
[BepInDependency("evaisa.lethallib", "0.15.1")]
[BepInDependency(StaticNetcodeLib.StaticNetcodeLib.Guid)]
public class BigMouthPlugin : BaseUnityPlugin
{

    const string GUID = "wexop.bigmouth";
    const string NAME = "BigMouth";
    const string VERSION = "1.1.3";

    public GameObject teehGameObject;
    public List<string> everyScrapsItems = new List<string>();

    public static BigMouthPlugin instance;

    public ConfigEntry<string> spawnMoonRarity;
    
    public ConfigEntry<float> playerDetectionDistance;
    
    public ConfigEntry<int> minTeethValue;
    public ConfigEntry<int> maxTeethValue;
    
    public ConfigEntry<float> chaseDuration;
    public ConfigEntry<float> attackPlayerDelay;
    public ConfigEntry<int> attackDamage;
    
    public ConfigEntry<float> angrySpeed;
    public ConfigEntry<float> angryAcceleration;

    public ConfigEntry<bool> canBeEveryItem;
    public ConfigEntry<string> itemDisabled;

    void Awake()
    {
        instance = this;
        
        Logger.LogInfo($"BigMouth starting....");

        string assetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "bigmouth");
        AssetBundle bundle = AssetBundle.LoadFromFile(assetDir);
        
        Logger.LogInfo($"BigMouth bundle found !");

        RegisterConfigs();
        RegisterMonster(bundle);
        
        Logger.LogInfo($"BigMouth is ready!");
    }

    void RegisterConfigs()
    {
                    
        spawnMoonRarity = Config.Bind("General", "SpawnRarity", 
            "Modded:100,ExperimentationLevel:40,AssuranceLevel:40,VowLevel:40,OffenseLevel:50,MarchLevel:50,RendLevel:100,DineLevel:100,TitanLevel:150,Adamance:90,Embrion:175,Artifice:180", 
            "Chance for BigMouth to spawn for any moon, example => assurance:100,offense:50 . You need to restart the game.");

        CreatStringConfig(spawnMoonRarity, true);
        
        //SPECIAL
        
        canBeEveryItem = Config.Bind("Special", "canBeEveryItem", 
            false, 
            "Big Mouth can transform into any scrap items, even modded one. You don't need to restart the game !");
        CreateBoolConfig(canBeEveryItem);
        
        itemDisabled = Config.Bind("Special", "itemsDisabled", 
            "Body,Apparatus,Hive,Shotgun", 
            "Items that BigMouth cannot transform into. You don't need to restart the game.");
        CreatStringConfig(itemDisabled);
        
        //BEHAVIOR CONFIGS
                    
        playerDetectionDistance = Config.Bind("Custom Behavior", "playerDetectionDistance", 
            4.65f, 
            "Chance for BigMouth to spawn for any moon, example => assurance:100,offense:50 . You don't need to restart the game !");
        CreateFloatConfig(playerDetectionDistance);
                    
        minTeethValue = Config.Bind("Custom Behavior", "minTeethValue", 
            50, 
            "Min teeth scrap item value when BigMouth die. You don't need to restart the game !");
        CreateIntConfig(minTeethValue, 0, 500);
                    
        maxTeethValue = Config.Bind("Custom Behavior", "maxTeethValue", 
            98, 
            "Max teeth scrap item value when BigMouth die. You don't need to restart the game !");
        CreateIntConfig(maxTeethValue, 0, 500);
        
        chaseDuration = Config.Bind("Custom Behavior", "chaseDuration", 
            2f, 
            "BigMouth chase player duration when detect one. You don't need to restart the game !");
        CreateFloatConfig(chaseDuration);
        
        attackPlayerDelay = Config.Bind("Custom Behavior", "attackPlayerDelay", 
            0.25f, 
            "BigMouth attack player delay. You don't need to restart the game !");
        CreateFloatConfig(attackPlayerDelay, 0f, 5f);
        
        attackDamage = Config.Bind("Custom Behavior", "attackDamage", 
            5, 
            "BigMouth attack player delay. You don't need to restart the game !");
        CreateIntConfig(attackDamage);
        
        angrySpeed = Config.Bind("Custom Behavior", "angrySpeed", 
            8f, 
            "BigMouth speed on angry phase. See Unity NavMeshAgent for more infos. You don't need to restart the game !");
        CreateFloatConfig(angrySpeed);
        
        angryAcceleration = Config.Bind("Custom Behavior", "angryAcceleration", 
            8f, 
            "BigMouth acceleration on angry phase. See Unity NavMeshAgent for more infos. You don't need to restart the game !");
        CreateFloatConfig(angryAcceleration);
    }
    
    void RegisterMonster(AssetBundle bundle)
    {
        //bigmouth
        EnemyType bigMouth = bundle.LoadAsset<EnemyType>("Assets/LethalCompany/Mods/BigMouth/BigMouth.asset");
        Logger.LogInfo($"{bigMouth.name} FOUND");
        Logger.LogInfo($"{bigMouth.enemyPrefab} prefab");
        NetworkPrefabs.RegisterNetworkPrefab(bigMouth.enemyPrefab);
        Utilities.FixMixerGroups(bigMouth.enemyPrefab);

        TerminalNode terminalNodeBigEyes = new TerminalNode();
        terminalNodeBigEyes.creatureName = "BigMouth";
        terminalNodeBigEyes.displayText = "He's cute";

        TerminalKeyword terminalKeywordBigEyes = new TerminalKeyword();
        terminalKeywordBigEyes.word = "BigMouth";
        
        
        RegisterUtil.RegisterEnemyWithConfig(spawnMoonRarity.Value, bigMouth,terminalNodeBigEyes , terminalKeywordBigEyes, bigMouth.PowerLevel, bigMouth.MaxCount);

    }
    
    
    private void CreateFloatConfig(ConfigEntry<float> configEntry, float min = 0f, float max = 100f)
    {
        var exampleSlider = new FloatSliderConfigItem(configEntry, new FloatSliderOptions
        {
            Min = min,
            Max = max,
            RequiresRestart = true
        });
        LethalConfigManager.AddConfigItem(exampleSlider);
    }
    
    private void CreateIntConfig(ConfigEntry<int> configEntry, int min = 0, int max = 100)
    {
        var exampleSlider = new IntSliderConfigItem(configEntry, new IntSliderOptions()
        {
            Min = min,
            Max = max,
            RequiresRestart = true
        });
        LethalConfigManager.AddConfigItem(exampleSlider);
    }
    
    private void CreatStringConfig(ConfigEntry<string> configEntry, bool requireRestart = false)
    {
        var exampleSlider = new TextInputFieldConfigItem(configEntry, new TextInputFieldOptions()
        {
            RequiresRestart = requireRestart
        });
        LethalConfigManager.AddConfigItem(exampleSlider);
    }
    
    private void CreateBoolConfig(ConfigEntry<bool> configEntry, bool requireRestart = false)
    {
        var exampleSlider = new BoolCheckBoxConfigItem(configEntry, new BoolCheckBoxOptions()
        {
            RequiresRestart = requireRestart
        });
        LethalConfigManager.AddConfigItem(exampleSlider);
    }

    public bool CanTransformInItem(string name)
    {

        bool enabled = true;
        
        var searchItem = name.ToLower();

        while (searchItem.Contains(" "))
        {
            searchItem = searchItem.Replace(" ", "");
        }

        var itemsNames = itemDisabled.Value.Split(",");
        foreach (var itemsName in itemsNames)
        {
            var nameOfItem = itemsName.ToLower();

            while (nameOfItem.Contains(" "))
            {
                nameOfItem = nameOfItem.Replace(" ", "");
            }

            if (nameOfItem.Contains(searchItem)) enabled = false;

        }

        return enabled;


    }


}
