using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TrainingAlgorithm : MonoBehaviour
{
    [SerializeField]
    private TrainingManager tm;
    
    // Setup is called after all vars are updated and sent to the TrainingAlgorithm
    public virtual void Setup(TrainingManager tm){
        this.tm = tm;
    }
}
