using System;
using UnityEngine;

public enum TrashType {Organic, Recyclable, Hazardous, General}

public class Trash : MonoBehaviour
{
    [SerializeField] public TrashType typeOfTrash;

    private void Start()
    {
        Debug.Log("Start is called");
    }
}
