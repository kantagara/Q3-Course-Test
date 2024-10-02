using System;
using System.Numerics;
using ChainSafe.Gaming.Evm.Contracts.Custom;
using ChainSafe.Gaming.Evm.JsonRpc;
using ChainSafe.Gaming.InProcessSigner;
using ChainSafe.Gaming.InProcessTransactionExecutor.Unity;
using ChainSafe.Gaming.UnityPackage;
using ChainSafe.Gaming.Web3;
using ChainSafe.Gaming.Web3.Build;
using ChainSafe.Gaming.Web3.Unity;
using Microsoft.Extensions.DependencyInjection;
using Nethereum.JsonRpc.Client;
using Nethereum.RPC.Accounts;
using Nethereum.Signer;
using Nethereum.Web3.Accounts;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Quantum
{
    
    public class TempAccountProvider : IAccountProvider
    {
        public IAccount Account { get; set; }
    }
    
    public class TokenStreamingChecker : MonoBehaviour
    {
        private Web3 _web3;
        private CFAForwarder _cfaForwarder;
        public static event Action OnTokenStreamingStarted;
        public static event Action OnTokenStreamingStopped;
        private const string TokenAddress = "0xC5FCF18Dc1CD147f5Ad3a95210cc61Cb024C5Bf1";
        
        private async void Awake()
        {
            if(Web3Unity.Instance == null) return;
            QuantumEvent.Subscribe<EventOnPlayerKilled>(this, PlayerKilled);
            QuantumEvent.Subscribe<EventOnPlayerPickedUpSpecialItem>(this, SpecialItemPickedUp);
            var web3Builder = new Web3Builder(Web3Unity.Web3.ProjectConfig, Web3Unity.Web3.ChainConfig).Configure(s =>
            {
                s.UseUnityEnvironment();
                s.UseRpcProvider();
                TempAccountProvider tempAccountProvider = new TempAccountProvider()
                {
                    Account = new Account("49ac649503238b5921306c301b2bfe3c66b09ef835598558d1031f7f60356fc2")
                };

                s.AddSingleton<IAccountProvider>(tempAccountProvider);
                s.UseInProcessSigner();
                s.UseInProcessTransactionExecutor();
            });
            _web3 = await web3Builder.LaunchAsync();
            
            var account = _web3.ServiceProvider.GetService<IAccountProvider>();
            account.Account.TransactionManager.Client = _web3.ServiceProvider.GetService<IClient>();
            
            _cfaForwarder = await _web3.ContractBuilder.Build<CFAForwarder>("0xcfA132E353cB4E398080B9700609bb008eceB125");
            await StopFlowIfExists(_web3.Signer.PublicAddress, Web3Unity.Web3.Signer.PublicAddress);
        }

        private async void SpecialItemPickedUp(EventOnPlayerPickedUpSpecialItem callback)
        {
            if (!callback.Game.PlayerIsLocal(callback.Player))
            {
                return;
            }
            if(Web3Unity.Instance == null) 
                return;

            var currentChain = Web3Unity.Web3.Chains.Current.ChainId;
            
            await Web3Unity.Web3.Chains.SwitchChain("1993");

            if (await Web3Unity.Web3.Erc1155.GetBalanceOf("0x6e395d192a76d41a99e8e7ebe586787109d6e1ea", "0") > 0)
            {
                await Web3Unity.Web3.Chains.SwitchChain(currentChain);
                await _cfaForwarder.CreateFlow(TokenAddress, _web3.Signer.PublicAddress, Web3Unity.Web3.Signer.PublicAddress, BigInteger.Pow(10, 18), Array.Empty<byte>());
                OnTokenStreamingStarted?.Invoke();
            }
            else
                await Web3Unity.Web3.Chains.SwitchChain(currentChain);
        }

        private async void PlayerKilled(EventOnPlayerKilled callback)
        {
            if(!callback.Game.PlayerIsLocal(callback.Player))
                return;
            if (!callback.Game.Frames.Verified.TryGet(callback.Entity, out Weapon weapon) ||
                weapon.Type != WeaponType.Golden_AK) return;
            if(Web3Unity.Instance == null) return;
            await _cfaForwarder.DeleteFlow(TokenAddress, _web3.Signer.PublicAddress,
                Web3Unity.Web3.Signer.PublicAddress, Array.Empty<byte>());
            OnTokenStreamingStopped?.Invoke();
        }

        private async void OnDestroy()
        {
            if(Web3Unity.Instance == null) return;
            await StopFlowIfExists(_web3.Signer.PublicAddress, Web3Unity.Web3.Signer.PublicAddress);
        }

        private async System.Threading.Tasks.Task StopFlowIfExists(string sender, string receiver)
        {
            var currentFlowRate = (await _cfaForwarder.GetFlowrate(TokenAddress,
                _web3.Signer.PublicAddress, Web3Unity.Web3.Signer.PublicAddress));
            if (currentFlowRate > BigInteger.Zero)
            {
                await _cfaForwarder.DeleteFlow(TokenAddress, sender, receiver,  Array.Empty<byte>());
            }
        }
        
#if UNITY_EDITOR
        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private async void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                await StopFlowIfExists(_web3.Signer.PublicAddress, Web3Unity.Web3.Signer.PublicAddress); // Ensure flow is stopped before exiting play mode
            }
        }
#endif
    }
}
