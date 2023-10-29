using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class CreatureSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public Creature creaturePrefab;
    public GameObject segmentPrefab;

    [Header("Settings")]
    [SerializeField]
    private Vector3 spawnPos;

    
    public CreatureGenotype creatureGenotype;
    public List<CreatureGenotype> creatureGenotypeHistory;

    private static ObjectPool<Segment> segmentPool;
    public static CreatureSpawner instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject); // Can't have two spawners active at once
        }
        else
        {
            instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        CreatureGenotype testCreature = new CreatureGenotype
        {
            name = "Test Creature"
        };

        SegmentGenotype testSegment = new SegmentGenotype
        {
            id = 1,
            jointType = JointType.Fixed,
            r = 255,
            g = 0,
            b = 0
        };

        SegmentConnectionGenotype testConnection1 = new SegmentConnectionGenotype
        {
            destination = 1,
            anchorX = 0.5f,
            anchorY = 0.0f,
            anchorZ = 0.0f,
            scale = 0.5f
        };

        //SpawnCreature(creatureGenotype, (Vector3.up + Vector3.right), null);
        /*creatureGenotypeHistory.Add(creatureGenotype.Clone());
        for (int i = 0; i < 16; i++)
        {
            GetComponent<MutateGenotype>().MutateCreatureGenotype(creatureGenotype, new MutationPreferenceSetting());
            creatureGenotypeHistory.Add(creatureGenotype.Clone());
        }
        SpawnCreature(creatureGenotype, (Vector3.up + Vector3.right) * 2);*/
    }

    bool VerifyCreatureGenotypeIntegrity(CreatureGenotype cg)
    {
        return true;
    }

    public Creature SpawnCreature(CreatureGenotype cg, Vector3 position){

        //test_cg = 

        return SpawnCreature(cg, position, null);
    }

    // Creature & GHOST (ID 0)
    public Creature SpawnCreature(CreatureGenotype cg, Vector3 position, Fitness fitness)
    {
        // Verify
        counter = 0;
        if (!VerifyCreatureGenotypeIntegrity(cg))
        {
            return null;
        }

        // Create recursive limit dict
        Dictionary<byte, byte> recursiveLimitInitial = new Dictionary<byte, byte>();
        foreach (SegmentGenotype segment in cg.segments)
        {
            recursiveLimitInitial[segment.id] = segment.recursiveLimit;
        }

        Creature c = Instantiate(creaturePrefab, Vector3.zero, Quaternion.identity);
        c.name = $"Creature ({cg.name})";
        c.cg = cg.Clone();
        c.transform.parent = transform;
        
        // Add neurons
        SegmentGenotype ghost = cg.GetSegment(0);
        if (ghost != null)
        {
            foreach (NeuronGenotype nm in ghost.neurons)
            {
                nm.nr.relativityNullable = NeuronReferenceRelativity.GHOST;
                c.AddNeuron(nm, null, null, 0);
            }
        }

        SpawnSegment(cg, c, recursiveLimitInitial, position);
        c.InitializeCreature(fitness);
        return c;
    }

    // Keeps track of segment count, used for auto-flagging strange creatures.
    [SerializeField] private int counter = 0;

    public void ReleaseCreature(Creature c){
        Segment[] segments = c.transform.GetComponentsInChildren<Segment>();
        if (segments != null)
        {
            foreach (Segment segment in segments)
            {
                segmentPool.Release(segment);
            }
        }
        Destroy(c.gameObject);
    }

    // Non-root (ID 2>)
    Segment SpawnSegment(CreatureGenotype cg, Creature c, Dictionary<byte, byte> recursiveLimitValues, SegmentConnectionGenotype myConnection, GameObject parentSegment, float parentGlobalScale, bool parentReflect, List<byte> connectionPath)
    {
        counter++;
        // Debug.Log(counter);
        if (counter == 80){
            cg.SaveDebug();
        }

        myConnection.EulerToQuat(); // Debug, remove later (this changes internal rotation storage stuff to make inspector editing easier.)


        byte id = myConnection.destination;
        //Debug.Log($"S: {myConnection.destination} ({recursiveLimitValues[id]})");

        // Find segmentGenotype
        SegmentGenotype currentSegmentGenotype = cg.GetSegment(id);

        if (currentSegmentGenotype == null)
            return null;

        Transform parentTransform = parentSegment.transform;

        int reflectInt = myConnection.reflected ? -1 : 1;
        //bool otherReflectBool = myConnection.reflected ^ (Mathf.Sign(parentTransform.localScale.x)) == -1;
        int parentReflectInt = parentReflect ? -1 : 1;
        bool otherReflectBool = myConnection.reflected ^ parentReflect;
        int otherReflectInt = otherReflectBool ? -1 : 1;


        Vector3 spawnPos = parentTransform.position +
            parentTransform.right * parentTransform.localScale.x * myConnection.anchorX * reflectInt * parentReflectInt +
            parentTransform.up * parentTransform.localScale.y * (myConnection.anchorY + 0.5f) +
            parentTransform.forward * parentTransform.localScale.z * myConnection.anchorZ;


        Quaternion spawnAngle = Quaternion.identity;
        spawnAngle *= parentTransform.rotation;
        spawnAngle *= new Quaternion(myConnection.orientationX, myConnection.orientationY, myConnection.orientationZ, myConnection.orientationW);

        if (otherReflectBool)
        {
            //spawnAngle = Quaternion.LookRotation(Vector3.Reflect(spawnAngle * Vector3.forward, parentTransform.right), parentTransform.up);
            //spawnAngle = Quaternion.LookRotation(Vector3.Reflect(spawnAngle * Vector3.forward, parentTransform.right), Vector3.Reflect(spawnAngle * Vector3.up, parentTransform.right));
            //spawnAngle = spawnAngle.eulerAngles
            //spawnAngle = Quaternion.LookRotation(Vector3.Reflect(spawnAngle * Vector3.forward, parentTransform.right), Vector3.Reflect(spawnAngle * Vector3.up, parentTransform.right));
            //Quaternion mirrorNormalQuat = new Quaternion(parentTransform.right.x, parentTransform.right.y, parentTransform.right.z, 0);
            spawnAngle = Quaternion.LookRotation(Vector3.Reflect(spawnAngle * Vector3.forward, Vector3.right), Vector3.Reflect(spawnAngle * Vector3.up, Vector3.right));
            //spawnAngle = mirrorNormalQuat * spawnAngle;
            //spawnAngle *= Quaternion.Euler(parentTransform.up * 180);
        }
        //spawnAngle *= parentTransform.rotation;
        // GameObject spawnedSegmentGameObject = Instantiate(segmentPrefab, spawnPos, spawnAngle);
        // Segment spawnedSegment = spawnedSegmentGameObject.GetComponent<Segment>();

        Segment spawnedSegment = segmentPool.Get();
        GameObject spawnedSegmentGameObject = spawnedSegment.gameObject;
        spawnedSegmentGameObject.transform.position = spawnPos;
        spawnedSegmentGameObject.transform.rotation = spawnAngle;

        spawnedSegmentGameObject.transform.parent = c.transform;
        spawnedSegmentGameObject.name = $"Segment {currentSegmentGenotype.id}";

        
        spawnedSegment.SetPath(connectionPath);
        spawnedSegment.SetId(id);

        FluidDrag fluidDrag = spawnedSegmentGameObject.GetComponent<FluidDrag>();
        fluidDrag.negYCovered = true;


        Vector3 dimVector = new Vector3(currentSegmentGenotype.dimensionX /* * otherReflectInt*/, currentSegmentGenotype.dimensionY, currentSegmentGenotype.dimensionZ);
        dimVector *= parentGlobalScale * myConnection.scale;
        spawnedSegmentGameObject.transform.localScale = dimVector;
        //spawnedSegment.GetComponent<BoxCollider>().size = dimVector;
        Transform spawnedGraphic = spawnedSegmentGameObject.transform.Find("Graphic");
        //spawnedGraphic.localScale = dimVector;
        spawnedGraphic.GetComponent<Renderer>().material.color = new Color(currentSegmentGenotype.r / 255f, currentSegmentGenotype.g / 255f, currentSegmentGenotype.b / 255f);

        Rigidbody rb = spawnedSegmentGameObject.GetComponent<Rigidbody>();
        rb.mass *= dimVector.x * dimVector.y * dimVector.z;
        
        Rigidbody parentSegmentRigidbody = parentSegment.GetComponent<Rigidbody>();
        switch (currentSegmentGenotype.jointType)
        {
            case (JointType.Fixed):
                {
                    spawnedSegment.AttachFixedJoint(parentSegmentRigidbody);
                }
                break;

            case (JointType.HingeX):
                {
                    spawnedSegment.AttachHingeJoint(new Vector3(1, 0, 0), parentSegmentRigidbody);
                }
                break;

            case (JointType.HingeY):
                {
                    spawnedSegment.AttachHingeJoint(new Vector3(0, 1 * otherReflectInt, 0), parentSegmentRigidbody);
                }
                break;

            case (JointType.HingeZ):
                {
                    spawnedSegment.AttachHingeJoint(new Vector3(0, 0, 1 * otherReflectInt), parentSegmentRigidbody);
                }
                break;

            case (JointType.Spherical):
                {
                    spawnedSegment.AttachSphericalJoint(parentSegmentRigidbody);
                }
                break;

            default:
                break;
        }

        // Check if self-intersecting TODO


        // Change recursiveLimit stuff
        bool runTerminalOnly = false;
        recursiveLimitValues[id]--;
        if (recursiveLimitValues[id] == 0 || !currentSegmentGenotype.connections.Any(scg => scg.destination == currentSegmentGenotype.id))
        {
            runTerminalOnly = true;
        }

        if (cg.stage == TrainingStage.KSS){
            // Add neurons
            foreach (NeuronGenotype nm in currentSegmentGenotype.neurons)
            {
                nm.nr.connectionPath = connectionPath;
                nm.nr.relativityNullable = NeuronReferenceRelativity.TRACED;
                Neuron addedNeuron;
                if (nm.nr.id == 12)
                {
                    addedNeuron = c.AddNeuron(nm, spawnedSegmentGameObject.GetComponent<Joint>(), spawnedSegment, 1);
                }
                else if (nm.nr.id <= 11)
                {
                    addedNeuron = c.AddNeuron(nm, null, spawnedSegment, 1);
                }
                else
                {
                    addedNeuron = c.AddNeuron(nm, null, spawnedSegment, 1);
                }
                spawnedSegment.AddNeuron(addedNeuron);
            }
        }

        // Add Segment and HingeJoint references
        c.segments.Add(spawnedSegmentGameObject.GetComponent<Segment>());
        c.actionMotors.Add(spawnedSegmentGameObject.GetComponent<HingeJoint>());

        foreach (SegmentConnectionGenotype connection in currentSegmentGenotype.connections)
        {

            if (recursiveLimitValues[connection.destination] > 0)
            {
                if (!runTerminalOnly && connection.terminalOnly)
                {
                    continue;
                }
                var recursiveLimitClone = recursiveLimitValues.ToDictionary(entry => entry.Key, entry => entry.Value);
                var connectionPathClone = connectionPath.Select(item => (byte)item).ToList();
                connectionPathClone.Add(connection.id);
                Segment childSegment = SpawnSegment(cg, c, recursiveLimitClone, connection, spawnedSegmentGameObject, parentGlobalScale * myConnection.scale, otherReflectBool, connectionPathClone);
                childSegment.SetCreature(c);
                childSegment.SetParent(connection.id, spawnedSegment);
                spawnedSegment.AddChild(connection.id, childSegment);
            }
        }

        return spawnedSegment;
    }

    // Root (ID 1)
    void SpawnSegment(CreatureGenotype cg, Creature c, Dictionary<byte, byte> recursiveLimitValues, Vector3 position)
    {
        //Debug.Log("S: ROOT");

        // Find segmentGenotype
        SegmentGenotype currentSegmentGenotype = cg.GetSegment(1);
        if (currentSegmentGenotype == null)
            return;

        //Debug.Log(cg.name);
        //Debug.Log(cg.eulerY);
        cg.EulerToQuat(); //Debug, remove later (this changes internal rotation storage stuff to make inspector editing easier.)
        Quaternion spawnAngle = new Quaternion(cg.orientationX, cg.orientationY, cg.orientationZ, cg.orientationW);
        //Debug.Log(spawnAngle);

        // Handle Object Pool for Segments
        if (segmentPool == null)
        {
            Segment segmentPrefab = Resources.Load<Segment>("Pool Prefabs/Segment");

            int maxSegments;
            OptimizationSettings os = EvolutionSettingsPersist.instance.save.ts.optimizationSettings;
            if (os.stage == TrainingStage.KSS){
                KSSSettings kss = (KSSSettings)os;
                maxSegments = kss.mp.maxSegments;
            }
            else
            {
                RLSettings rl = (RLSettings)os;
                // maxSegments = rl.initialGenotype.GetSegmentCount(); // This method doesn't exist.
                maxSegments = 20;
            }
            int envCount = os.numEnvs;

            segmentPool = new ObjectPool<Segment>(() =>
            {
                Segment segment = Instantiate(segmentPrefab);
                DontDestroyOnLoad(segment.gameObject);
                return segment;
            }, segment =>
            {
                SceneManager.MoveGameObjectToScene(segment.gameObject, SceneManager.GetActiveScene());
                segment.Initialize();
                segment.gameObject.SetActive(true);
            }, segment =>
            {
                segment.gameObject.SetActive(false);
                segment.transform.parent = null;
                segment.Release();
                DontDestroyOnLoad(segment.gameObject);
            }, segment =>
            {
                Destroy(segment.gameObject);
            }, true, envCount * Mathf.Min(5, maxSegments), maxSegments * envCount);
        }

        // GameObject spawnedSegmentGameObject = Instantiate(segmentPrefab, position, spawnAngle);
        // Segment spawnedSegment = spawnedSegmentGameObject.GetComponent<Segment>();
        Segment spawnedSegment = segmentPool.Get();
        GameObject spawnedSegmentGameObject = spawnedSegment.gameObject;
        spawnedSegmentGameObject.transform.position = position;
        spawnedSegmentGameObject.transform.rotation = spawnAngle;

        spawnedSegmentGameObject.transform.parent = c.transform;
        spawnedSegmentGameObject.name = $"Segment {currentSegmentGenotype.id}";

        
        spawnedSegment.SetId(1);
        spawnedSegment.SetCreature(c);

        Vector3 dimVector = new Vector3(currentSegmentGenotype.dimensionX, currentSegmentGenotype.dimensionY, currentSegmentGenotype.dimensionZ);
        //spawnedSegment.GetComponent<BoxCollider>().size = dimVector;
        spawnedSegmentGameObject.transform.localScale = dimVector;
        Transform spawnedGraphic = spawnedSegmentGameObject.transform.Find("Graphic");
        //spawnedGraphic.localScale = dimVector;
        spawnedGraphic.GetComponent<Renderer>().material.color = new Color(currentSegmentGenotype.r / 255f, currentSegmentGenotype.g / 255f, currentSegmentGenotype.b / 255f);

        Rigidbody rb = spawnedSegmentGameObject.GetComponent<Rigidbody>();
        rb.mass *= dimVector.x * dimVector.y * dimVector.z;

        // Change recursiveLimit stuff
        bool runTerminalOnly = false;
        recursiveLimitValues[1]--;
        if (recursiveLimitValues[1] == 0 || !currentSegmentGenotype.connections.Any(scg => scg.destination == currentSegmentGenotype.id))
        {
            runTerminalOnly = true;
        }

        List<byte> connectionPath = new List<byte>();

        if (cg.stage == TrainingStage.KSS){
            // Add neurons
            foreach (NeuronGenotype nm in currentSegmentGenotype.neurons)
            {
                nm.nr.connectionPath = connectionPath;
                nm.nr.relativityNullable = NeuronReferenceRelativity.TRACED;
                Neuron addedNeuron;
                if (nm.nr.id == 12)
                {
                    addedNeuron = c.AddNeuron(nm, spawnedSegmentGameObject.GetComponent<HingeJoint>(), spawnedSegment, 1);
                }
                else if (nm.nr.id <= 11)
                {
                    addedNeuron = c.AddNeuron(nm, null, spawnedSegment, 1);
                }
                else
                {
                    addedNeuron = c.AddNeuron(nm, null, spawnedSegment, 1);
                }
                spawnedSegment.AddNeuron(addedNeuron);
            }
        }

        // Add Segment
        c.segments.Add(spawnedSegmentGameObject.GetComponent<Segment>());
        


        foreach (SegmentConnectionGenotype connection in currentSegmentGenotype.connections)
        {

            if (recursiveLimitValues[connection.destination] > 0)
            {
                if (!runTerminalOnly && connection.terminalOnly)
                {
                    continue;
                }
                var recursiveLimitClone = recursiveLimitValues.ToDictionary(entry => entry.Key, entry => entry.Value);
                Segment childSegment = SpawnSegment(cg, c, recursiveLimitClone, connection, spawnedSegmentGameObject, 1, false, new List<byte>() { connection.id });
                childSegment.SetCreature(c);
                childSegment.SetParent(connection.id, spawnedSegment);
                spawnedSegment.AddChild(connection.id, childSegment);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + spawnPos, 0.1f);
    }

}