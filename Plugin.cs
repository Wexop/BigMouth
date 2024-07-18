using System.Collections.Generic;
using BepInEx;
using System.IO;
using System.Reflection;
using BepInEx.Configuration;
using LethalConfig;
using LethalConfig.ConfigItems;
using LethalConfig.ConfigItems.Options;
using UnityEngine;
using LethalLib.Modules;
using NetworkPrefabs = LethalLib.Modules.NetworkPrefabs;

namespace BigEyes
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency("evaisa.lethallib", "0.15.1")]
    [BepInDependency(StaticNetcodeLib.StaticNetcodeLib.Guid)]
    public class BigMouthPlugin : BaseUnityPlugin
    {

        const string GUID = "wexop.bigmouth";
        const string NAME = "BigMouth";
        const string VERSION = "1.0.0";

        public GameObject teehGameObject;

        public static BigMouthPlugin instance;

        public ConfigEntry<int> chanceSpawnEntry;

        void Awake()
        {
            instance = this;
            
            Logger.LogInfo($"BigMouth starting....");

            string assetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "bigmouth");
            AssetBundle bundle = AssetBundle.LoadFromFile(assetDir);
            
            Logger.LogInfo($"BigMouth bundle found !");
            
            chanceSpawnEntry = Config.Bind("General", "SpawnChance", 50, "Chance for big mouth to spawn. You need to restart the game.");
            CreateIntConfig(chanceSpawnEntry);

            //bigeyes
            EnemyType bigMouth = bundle.LoadAsset<EnemyType>("Assets/LethalCompany/Mods/BigMouth/BigMouth.asset");
            Logger.LogInfo($"{bigMouth.name} FOUND");
            Logger.LogInfo($"{bigMouth.enemyPrefab} prefab");
            NetworkPrefabs.RegisterNetworkPrefab(bigMouth.enemyPrefab);
            Utilities.FixMixerGroups(bigMouth.enemyPrefab);

            var dic = new Dictionary<Levels.LevelTypes, int>();
            dic.Add(Levels.LevelTypes.All, chanceSpawnEntry.Value);
            Enemies.RegisterEnemy(bigMouth, Enemies.SpawnType.Default, dic);
            
            
            Logger.LogInfo($"BigMouth is ready!");
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


    }
}