using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Segment : MonoBehaviour
{

    public bool isTopEmpty;
    public bool isBottomEmpty;
    public bool isRightEmpty;
    public bool isLeftEmpty;
    public bool isFrontEmpty;
    public bool isBackEmpty;

    public HingeJoint joint;
    public float jointAxisX;
    public float jointAxisY;
    public float jointAxisZ;

    // Start is called before the first frame update
    void Start()
    {
        joint = GetComponent<HingeJoint>();
    }

    void FixedUpdate()
    {
        isTopEmpty = true;
        isBottomEmpty = true;
        isRightEmpty = true;
        isLeftEmpty = true;
        isFrontEmpty = true;
        isBackEmpty = true;

        if (joint != null)
        {
            Vector3 angles = (transform.localRotation * Quaternion.Inverse(joint.connectedBody.transform.localRotation)).eulerAngles;
            //Vector3 angles = Quaternion.FromToRotation(joint.connectedBody.transform.rotation.eulerAngles, transform.rotation.).eulerAngles;
            jointAxisX = angles.x;
            jointAxisY = angles.y;
            jointAxisZ = angles.z;

        }
    }

    public sbyte GetContact(string name)
    {
        bool value = name switch
        {
            "Top" => isTopEmpty,
            "Bottom" => isBottomEmpty,
            "Right" => isRightEmpty,
            "Left" => isLeftEmpty,
            "Front" => isFrontEmpty,
            "Back" => isBackEmpty,
            _ => true
        };
        return (sbyte)(value ? -1 : 1);
    }

    public float GetPhotosensor(int varNumber)
    {
        LightSource lightsource = GameObject.FindObjectOfType<LightSource>();
        if (lightsource == null)
        {
            return 0;
        }
        else
        {
            Vector3 lspos = lightsource.transform.position;
            Vector3 normalVector = (lspos - transform.position).normalized;
            return varNumber switch
            {
                0 => normalVector.x,
                1 => normalVector.y,
                2 => normalVector.z,
                _ => 0,
            };
        }
    }

    public void HandleStay(Collider other, string name)
    {
        if (other.gameObject.layer != 6)
        {
            switch (name)
            {
                case ("Top"):
                    isTopEmpty = false;
                    break;
                case ("Bottom"):
                    isBottomEmpty = false;
                    break;
                case ("Right"):
                    isRightEmpty = false;
                    break;
                case ("Left"):
                    isLeftEmpty = false;
                    break;
                case ("Front"):
                    isFrontEmpty = false;
                    break;
                case ("Back"):
                    isBackEmpty = false;
                    break;
            }
        }
    }
}
