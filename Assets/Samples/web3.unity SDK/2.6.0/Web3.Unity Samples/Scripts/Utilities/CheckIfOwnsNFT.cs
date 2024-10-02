using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using ChainSafe.Gaming.Evm.Contracts.Custom;
using ChainSafe.Gaming.UnityPackage.Connection;
using ChainSafe.Gaming.Web3;
using UnityEngine;

public class CheckIfOwnsNFT : MonoBehaviour, IWeb3InitializedHandler
{
    [SerializeField] private string address = "0x87b42e2c286f26ed8a67cd1138650fdb1bfc0e18";
    public int Priority => 0;

    public async Task OnWeb3Initialized(Web3 web3)
    {
        var contract = await web3.ContractBuilder.Build<DepositContract>("0x2a7a584f06db97700636e9d26e8912ae4f35b905");
        await contract.DepositToEscrow(BigInteger.Pow(10, 18));
        Debug.LogError("HAH");
    }
}