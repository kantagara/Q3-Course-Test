using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChainSafe.Gaming.UnityPackage;
using ChainSafe.Gaming.UnityPackage.Connection;
using ChainSafe.Gaming.UnityPackage.UI;
using ChainSafe.Gaming.Web3;
using UnityEngine;

public class CloseModalAfterInit : MonoBehaviour, IWeb3InitializedHandler
{
    public int Priority => 0;
    public Task OnWeb3Initialized(Web3 web3)
    {
        Debug.LogWarning(web3.Signer.PublicAddress);
        Web3Unity.ConnectModal.Close();
        return Task.CompletedTask;
    }
}
