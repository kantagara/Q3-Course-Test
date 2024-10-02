using System;
using System.Collections;
using System.Collections.Generic;
using ChainSafe.Gaming.UnityPackage;
using UnityEngine;

public class EnableDisableObjectsWhenWeb3Initialized : MonoBehaviour
{
    [SerializeField] private GameObject[] objectsWhenWeb3Exists;
    [SerializeField] private GameObject[] objectsWhenWeb3DoesntExist;


    private void Awake()
    {
        var enabledCollection = Web3Unity.Connected ? objectsWhenWeb3Exists : objectsWhenWeb3DoesntExist;
        var disabledCollection = objectsWhenWeb3Exists == enabledCollection
            ? objectsWhenWeb3DoesntExist
            : objectsWhenWeb3Exists;
        foreach (var o in enabledCollection)
        {
            o.SetActive(true);
        }

        foreach (var o in disabledCollection)
        {
            o.SetActive(false);
        }
    }

    
}
