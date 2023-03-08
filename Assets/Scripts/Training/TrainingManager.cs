using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TrainingSettings {
    public OptimizationSettings optimizationSettings;
    public EnvironmentSettings envSettings;

    public TrainingSettings(OptimizationSettings os, EnvironmentSettings es){
        optimizationSettings = os;
        envSettings = es;
    }
}

[System.Serializable]
public class OptimizationSettings {
    public int num_envs = 1;
}

[System.Serializable]
public class RLSettings : OptimizationSettings {

}

[System.Serializable]
public class KSSSettings : OptimizationSettings {
    
}

/// <summary>
/// Generates Environments, tallies data, Starts and Stops training
/// </summary>
public class TrainingManager : MonoBehaviour
{
    public static TrainingManager instance;

    [SerializeField]
    private TrainingSettings ts;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject); // Can't have two controllers active at once
        }
        else
        {
            instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (EvolutionSettingsPersist.instance == null)
        {
            throw new Exception("No EvolutionSettingsPersist instance found. Try launching from the Menu Scene!");
        }

        ts = EvolutionSettingsPersist.instance.ts;

        GameObject envPrefab = Resources.Load<GameObject>("Prefabs/Envs/" + EnvironmentSettings.envString[ts.envSettings.envCode]);

        if (ts.envSettings.envArrangeType == EnvArrangeType.LINEAR){
            float sizeX = ts.envSettings.sizeX;
            int l = ts.optimizationSettings.num_envs;
            for (int i = 0; i < l; i++)
            {
                GameObject instantiatedEnv = Instantiate(envPrefab, Vector3.right * i * sizeX, envPrefab.transform.rotation);

                Transform oneOff = instantiatedEnv.transform.Find("OneOffHolder");
                if (oneOff != null && i != 0) {
                    Destroy(oneOff.gameObject);
                }
            }
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}