using System;
using System.Numerics;
using System.Net.WebSockets;
using System.Threading.Tasks;
using ChainSafe.Gaming.Evm.Transactions;
using ChainSafe.Gaming.Evm.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.Contracts;
using Nethereum.RPC.Reactive.Eth.Subscriptions;
using Nethereum.JsonRpc.WebSocketStreamingClient;
using Nethereum.ABI.FunctionEncoding.Attributes;
using UnityEngine;



namespace ChainSafe.Gaming.Evm.Contracts.Custom
{
    public partial class DepositContract : ICustomContract
    {
        public string Address => OriginalContract.Address;
       
        public string ABI => "[ 	{ 		\"inputs\": [ 			{ 				\"internalType\": \"uint256\", 				\"name\": \"amount\", 				\"type\": \"uint256\" 			} 		], 		\"name\": \"depositToEscrow\", 		\"outputs\": [], 		\"stateMutability\": \"nonpayable\", 		\"type\": \"function\" 	}, 	{ 		\"inputs\": [ 			{ 				\"internalType\": \"address\", 				\"name\": \"user\", 				\"type\": \"address\" 			}, 			{ 				\"internalType\": \"uint256\", 				\"name\": \"flowRate\", 				\"type\": \"uint256\" 			} 		], 		\"name\": \"startStreamToUser\", 		\"outputs\": [], 		\"stateMutability\": \"nonpayable\", 		\"type\": \"function\" 	}, 	{ 		\"inputs\": [ 			{ 				\"internalType\": \"address\", 				\"name\": \"user\", 				\"type\": \"address\" 			} 		], 		\"name\": \"stopStreamToUser\", 		\"outputs\": [], 		\"stateMutability\": \"nonpayable\", 		\"type\": \"function\" 	}, 	{ 		\"inputs\": [ 			{ 				\"internalType\": \"contract ISuperToken\", 				\"name\": \"superToken\", 				\"type\": \"address\" 			}, 			{ 				\"internalType\": \"address\", 				\"name\": \"forwarder\", 				\"type\": \"address\" 			} 		], 		\"stateMutability\": \"nonpayable\", 		\"type\": \"constructor\" 	}, 	{ 		\"inputs\": [ 			{ 				\"internalType\": \"uint256\", 				\"name\": \"amount\", 				\"type\": \"uint256\" 			} 		], 		\"name\": \"withdrawFromEscrow\", 		\"outputs\": [], 		\"stateMutability\": \"nonpayable\", 		\"type\": \"function\" 	}, 	{ 		\"inputs\": [], 		\"name\": \"owner\", 		\"outputs\": [ 			{ 				\"internalType\": \"address\", 				\"name\": \"\", 				\"type\": \"address\" 			} 		], 		\"stateMutability\": \"view\", 		\"type\": \"function\" 	} ]";
        
        public string ContractAddress { get; set; }
        
        public IContractBuilder ContractBuilder { get; set; }

        public Contract OriginalContract { get; set; }
        
        public string WebSocketUrl { get; set; }
        
        public bool Subscribed { get; set; }

        private StreamingWebSocketClient _webSocketClient;
        
        #region Methods

        public async Task DepositToEscrow(BigInteger amount) 
        {
            var response = await OriginalContract.Send("depositToEscrow", new object [] {
                amount
            });
            
            
        }
        public async Task<TransactionReceipt> DepositToEscrowWithReceipt(BigInteger amount) 
        {
            var response = await OriginalContract.SendWithReceipt("depositToEscrow", new object [] {
                amount
            });
            
            return response.receipt;
        }

        public async Task StartStreamToUser(string user, BigInteger flowRate) 
        {
            var response = await OriginalContract.Send("startStreamToUser", new object [] {
                user, flowRate
            });
            
            
        }
        public async Task<TransactionReceipt> StartStreamToUserWithReceipt(string user, BigInteger flowRate) 
        {
            var response = await OriginalContract.SendWithReceipt("startStreamToUser", new object [] {
                user, flowRate
            });
            
            return response.receipt;
        }

        public async Task StopStreamToUser(string user) 
        {
            var response = await OriginalContract.Send("stopStreamToUser", new object [] {
                user
            });
            
            
        }
        public async Task<TransactionReceipt> StopStreamToUserWithReceipt(string user) 
        {
            var response = await OriginalContract.SendWithReceipt("stopStreamToUser", new object [] {
                user
            });
            
            return response.receipt;
        }

        public async Task WithdrawFromEscrow(BigInteger amount) 
        {
            var response = await OriginalContract.Send("withdrawFromEscrow", new object [] {
                amount
            });
            
            
        }
        public async Task<TransactionReceipt> WithdrawFromEscrowWithReceipt(BigInteger amount) 
        {
            var response = await OriginalContract.SendWithReceipt("withdrawFromEscrow", new object [] {
                amount
            });
            
            return response.receipt;
        }

        public async Task<string> Owner() 
        {
            var response = await OriginalContract.Call<string>("owner", new object [] {
                
            });
            
            return response;
        }



        #endregion
        
        
        #region Event Classes


        #endregion
        
        #region Interface Implemented Methods
        
        public async ValueTask DisposeAsync()
        {
            if(string.IsNullOrEmpty(WebSocketUrl))
                return;
            if(!Subscribed)
                return;
                
            if(Application.platform == RuntimePlatform.WebGLPlayer)
               return;
            Subscribed = false;
            try
            {


            
             if (_webSocketClient != null)
                await _webSocketClient.StopAsync();
            }catch(Exception e)
            {
                Debug.LogError("Caught an exception whilst unsubscribing from events\n" + e.Message);
            }
        }
        
        public async ValueTask InitAsync()
        {
            if(Subscribed)
                return;
            Subscribed = true;

            if(string.IsNullOrEmpty(WebSocketUrl))
            {
                Debug.LogWarning($"WebSocketUrl is not set for this class. Event Subscriptions will not work.");
                return;
            }

            
           
            try
            {
                if(Application.platform == RuntimePlatform.WebGLPlayer)
                {
                   Debug.LogWarning("WebGL Platform is currently not supporting event subscription");
                   return;
                }
                        
                _webSocketClient ??= new StreamingWebSocketClient(WebSocketUrl);
                
              
                
                await _webSocketClient.StartAsync();
                if (_webSocketClient != null && (_webSocketClient.WebSocketState != WebSocketState.None && _webSocketClient.WebSocketState != WebSocketState.Open &&
                                                                     _webSocketClient.WebSocketState != WebSocketState.CloseReceived))
                {
                    Debug.LogWarning(
                        $"Websocket is in an invalid state {_webSocketClient.WebSocketState}. It needs to be in a state None, Open or CloseReceived");
                    return;
                }

    
            }catch(Exception e)
            {
                Debug.LogError("Caught an exception whilst subscribing to events. Subscribing to events will not work in this session\n" + e.Message);
            }
            
        }
        
        [Obsolete("It's not advisable to use this method. Use the pre-generated methods instead.")]
        public IContract Attach(string address)
        {
            return OriginalContract.Attach(address);
        }
        
        [Obsolete("It's not advisable to use this method. Use the pre-generated methods instead.")]
        public Task<object[]> Call(string method, object[] parameters = null, TransactionRequest overwrite = null)
        {
            return OriginalContract.Call(method, parameters, overwrite);
        }
        
        [Obsolete("It's not advisable to use this method. Use the pre-generated methods instead.")]
        public object[] Decode(string method, string output)
        {
            return OriginalContract.Decode(method, output);
        }
        
        [Obsolete("It's not advisable to use this method. Use the pre-generated methods instead.")]
        public Task<object[]> Send(string method, object[] parameters = null, TransactionRequest overwrite = null)
        {
            return OriginalContract.Send(method, parameters, overwrite);
        }
        
        [Obsolete("It's not advisable to use this method. Use the pre-generated methods instead.")]
        public Task<(object[] response, TransactionReceipt receipt)> SendWithReceipt(string method, object[] parameters = null, TransactionRequest overwrite = null)
        {
            return OriginalContract.SendWithReceipt(method, parameters, overwrite);
        }
        
        [Obsolete("It's not advisable to use this method. Use the pre-generated methods instead.")]
        public Task<HexBigInteger> EstimateGas(string method, object[] parameters, TransactionRequest overwrite = null)
        {
            return OriginalContract.EstimateGas(method, parameters, overwrite);
        }
        
        [Obsolete("It's not advisable to use this method. Use the pre-generated methods instead.")]
        public string Calldata(string method, object[] parameters = null)
        {
            return OriginalContract.Calldata(method, parameters);
        }
        
        [Obsolete("It's not advisable to use this method. Use the pre-generated methods instead.")]
        public Task<TransactionRequest> PrepareTransactionRequest(string method, object[] parameters, bool isReadCall = false, TransactionRequest overwrite = null)
        {
            return OriginalContract.PrepareTransactionRequest(method, parameters, isReadCall, overwrite);
        }
        #endregion
    }


}
