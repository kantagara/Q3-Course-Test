using System.Threading.Tasks;
using ChainSafe.Gaming.UnityPackage.Connection;
using ChainSafe.Gaming.Web3;
using UnityEngine;

public class ToggleGameObjectsAfterWeb3Initialized : MonoBehaviour, IWeb3InitializedHandler
{
    [SerializeField] private GameObject[] objectsToEnable;
    [SerializeField] private GameObject[] objectsToDisable;
    public int Priority => 0;
    
    public Task OnWeb3Initialized(Web3 web3)
    {
        foreach (var o in objectsToEnable)
            o.SetActive(true);
        
        foreach (var o in objectsToDisable)
            o.SetActive(false);
        
        return Task.CompletedTask;
    }
}
