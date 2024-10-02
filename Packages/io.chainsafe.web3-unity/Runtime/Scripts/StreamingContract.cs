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
    public partial class StreamingContract : ICustomContract
    {
        public string Address => OriginalContract.Address;
       
        public string ABI => "[{\"inputs\":[{\"internalType\":\"contract ISuperfluid\",\"name\":\"host\",\"type\":\"address\"}],\"stateMutability\":\"nonpayable\",\"type\":\"constructor\"},{\"inputs\":[],\"name\":\"CFA_FWD_INVALID_FLOW_RATE\",\"type\":\"error\"},{\"inputs\":[{\"internalType\":\"contract ISuperToken\",\"name\":\"token\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"sender\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"receiver\",\"type\":\"address\"},{\"internalType\":\"int96\",\"name\":\"flowrate\",\"type\":\"int96\"},{\"internalType\":\"bytes\",\"name\":\"userData\",\"type\":\"bytes\"}],\"name\":\"createFlow\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"contract ISuperToken\",\"name\":\"token\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"sender\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"receiver\",\"type\":\"address\"},{\"internalType\":\"bytes\",\"name\":\"userData\",\"type\":\"bytes\"}],\"name\":\"deleteFlow\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"contract ISuperToken\",\"name\":\"token\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"getAccountFlowInfo\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"lastUpdated\",\"type\":\"uint256\"},{\"internalType\":\"int96\",\"name\":\"flowrate\",\"type\":\"int96\"},{\"internalType\":\"uint256\",\"name\":\"deposit\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"owedDeposit\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"contract ISuperToken\",\"name\":\"token\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"getAccountFlowrate\",\"outputs\":[{\"internalType\":\"int96\",\"name\":\"flowrate\",\"type\":\"int96\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"contract ISuperToken\",\"name\":\"token\",\"type\":\"address\"},{\"internalType\":\"int96\",\"name\":\"flowrate\",\"type\":\"int96\"}],\"name\":\"getBufferAmountByFlowrate\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"bufferAmount\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"contract ISuperToken\",\"name\":\"token\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"sender\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"receiver\",\"type\":\"address\"}],\"name\":\"getFlowInfo\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"lastUpdated\",\"type\":\"uint256\"},{\"internalType\":\"int96\",\"name\":\"flowrate\",\"type\":\"int96\"},{\"internalType\":\"uint256\",\"name\":\"deposit\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"owedDeposit\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"contract ISuperToken\",\"name\":\"token\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"sender\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"flowOperator\",\"type\":\"address\"}],\"name\":\"getFlowOperatorPermissions\",\"outputs\":[{\"internalType\":\"uint8\",\"name\":\"permissions\",\"type\":\"uint8\"},{\"internalType\":\"int96\",\"name\":\"flowrateAllowance\",\"type\":\"int96\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"contract ISuperToken\",\"name\":\"token\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"sender\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"receiver\",\"type\":\"address\"}],\"name\":\"getFlowrate\",\"outputs\":[{\"internalType\":\"int96\",\"name\":\"flowrate\",\"type\":\"int96\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"contract ISuperToken\",\"name\":\"token\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"flowOperator\",\"type\":\"address\"}],\"name\":\"grantPermissions\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"contract ISuperToken\",\"name\":\"token\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"flowOperator\",\"type\":\"address\"}],\"name\":\"revokePermissions\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"contract ISuperToken\",\"name\":\"token\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"receiver\",\"type\":\"address\"},{\"internalType\":\"int96\",\"name\":\"flowrate\",\"type\":\"int96\"}],\"name\":\"setFlowrate\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"contract ISuperToken\",\"name\":\"token\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"sender\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"receiver\",\"type\":\"address\"},{\"internalType\":\"int96\",\"name\":\"flowrate\",\"type\":\"int96\"}],\"name\":\"setFlowrateFrom\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"contract ISuperToken\",\"name\":\"token\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"sender\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"receiver\",\"type\":\"address\"},{\"internalType\":\"int96\",\"name\":\"flowrate\",\"type\":\"int96\"},{\"internalType\":\"bytes\",\"name\":\"userData\",\"type\":\"bytes\"}],\"name\":\"updateFlow\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"contract ISuperToken\",\"name\":\"token\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"flowOperator\",\"type\":\"address\"},{\"internalType\":\"uint8\",\"name\":\"permissions\",\"type\":\"uint8\"},{\"internalType\":\"int96\",\"name\":\"flowrateAllowance\",\"type\":\"int96\"}],\"name\":\"updateFlowOperatorPermissions\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"}]";
        
        public string ContractAddress { get; set; }
        
        public IContractBuilder ContractBuilder { get; set; }

        public Contract OriginalContract { get; set; }
        
        public string WebSocketUrl { get; set; }
        
        public bool Subscribed { get; set; }

        private StreamingWebSocketClient _webSocketClient;
        
        #region Methods

        public async Task<bool> CreateFlow(string token, string sender, string receiver, BigInteger flowrate, byte[] userData) 
        {
            var response = await OriginalContract.Send<bool>("createFlow", new object [] {
                token, sender, receiver, flowrate, userData
            });
            
            return response;
        }
        public async Task<(bool , TransactionReceipt receipt)> CreateFlowWithReceipt(string token, string sender, string receiver, BigInteger flowrate, byte[] userData) 
        {
            var response = await OriginalContract.SendWithReceipt<bool>("createFlow", new object [] {
                token, sender, receiver, flowrate, userData
            });
            
            return (response.response, response.receipt);
        }

        public async Task<bool> DeleteFlow(string token, string sender, string receiver, byte[] userData) 
        {
            var response = await OriginalContract.Send<bool>("deleteFlow", new object [] {
                token, sender, receiver, userData
            });
            
            return response;
        }
        public async Task<(bool , TransactionReceipt receipt)> DeleteFlowWithReceipt(string token, string sender, string receiver, byte[] userData) 
        {
            var response = await OriginalContract.SendWithReceipt<bool>("deleteFlow", new object [] {
                token, sender, receiver, userData
            });
            
            return (response.response, response.receipt);
        }

        public async Task<(BigInteger lastUpdated, BigInteger flowrate, BigInteger deposit, BigInteger owedDeposit)> GetAccountFlowInfo(string token, string account) 
        {
            var response = await OriginalContract.Call("getAccountFlowInfo", new object [] {
                token, account
            });
            
            return ((BigInteger)response[0], (BigInteger)response[1], (BigInteger)response[2], (BigInteger)response[3]);
        }


        public async Task<BigInteger> GetAccountFlowrate(string token, string account) 
        {
            var response = await OriginalContract.Call<BigInteger>("getAccountFlowrate", new object [] {
                token, account
            });
            
            return response;
        }


        public async Task<BigInteger> GetBufferAmountByFlowrate(string token, BigInteger flowrate) 
        {
            var response = await OriginalContract.Call<BigInteger>("getBufferAmountByFlowrate", new object [] {
                token, flowrate
            });
            
            return response;
        }


        public async Task<(BigInteger lastUpdated, BigInteger flowrate, BigInteger deposit, BigInteger owedDeposit)> GetFlowInfo(string token, string sender, string receiver) 
        {
            var response = await OriginalContract.Call("getFlowInfo", new object [] {
                token, sender, receiver
            });
            
            return ((BigInteger)response[0], (BigInteger)response[1], (BigInteger)response[2], (BigInteger)response[3]);
        }


        public async Task<(BigInteger permissions, BigInteger flowrateAllowance)> GetFlowOperatorPermissions(string token, string sender, string flowOperator) 
        {
            var response = await OriginalContract.Call("getFlowOperatorPermissions", new object [] {
                token, sender, flowOperator
            });
            
            return ((BigInteger)response[0], (BigInteger)response[1]);
        }


        public async Task<BigInteger> GetFlowrate(string token, string sender, string receiver) 
        {
            var response = await OriginalContract.Call<BigInteger>("getFlowrate", new object [] {
                token, sender, receiver
            });
            
            return response;
        }


        public async Task<bool> GrantPermissions(string token, string flowOperator) 
        {
            var response = await OriginalContract.Send<bool>("grantPermissions", new object [] {
                token, flowOperator
            });
            
            return response;
        }
        public async Task<(bool , TransactionReceipt receipt)> GrantPermissionsWithReceipt(string token, string flowOperator) 
        {
            var response = await OriginalContract.SendWithReceipt<bool>("grantPermissions", new object [] {
                token, flowOperator
            });
            
            return (response.response, response.receipt);
        }

        public async Task<bool> RevokePermissions(string token, string flowOperator) 
        {
            var response = await OriginalContract.Send<bool>("revokePermissions", new object [] {
                token, flowOperator
            });
            
            return response;
        }
        public async Task<(bool , TransactionReceipt receipt)> RevokePermissionsWithReceipt(string token, string flowOperator) 
        {
            var response = await OriginalContract.SendWithReceipt<bool>("revokePermissions", new object [] {
                token, flowOperator
            });
            
            return (response.response, response.receipt);
        }

        public async Task<bool> SetFlowrate(string token, string receiver, BigInteger flowrate) 
        {
            var response = await OriginalContract.Send<bool>("setFlowrate", new object [] {
                token, receiver, flowrate
            });
            
            return response;
        }
        public async Task<(bool , TransactionReceipt receipt)> SetFlowrateWithReceipt(string token, string receiver, BigInteger flowrate) 
        {
            var response = await OriginalContract.SendWithReceipt<bool>("setFlowrate", new object [] {
                token, receiver, flowrate
            });
            
            return (response.response, response.receipt);
        }

        public async Task<bool> SetFlowrateFrom(string token, string sender, string receiver, BigInteger flowrate) 
        {
            var response = await OriginalContract.Send<bool>("setFlowrateFrom", new object [] {
                token, sender, receiver, flowrate
            });
            
            return response;
        }
        public async Task<(bool , TransactionReceipt receipt)> SetFlowrateFromWithReceipt(string token, string sender, string receiver, BigInteger flowrate) 
        {
            var response = await OriginalContract.SendWithReceipt<bool>("setFlowrateFrom", new object [] {
                token, sender, receiver, flowrate
            });
            
            return (response.response, response.receipt);
        }

        public async Task<bool> UpdateFlow(string token, string sender, string receiver, BigInteger flowrate, byte[] userData) 
        {
            var response = await OriginalContract.Send<bool>("updateFlow", new object [] {
                token, sender, receiver, flowrate, userData
            });
            
            return response;
        }
        public async Task<(bool , TransactionReceipt receipt)> UpdateFlowWithReceipt(string token, string sender, string receiver, BigInteger flowrate, byte[] userData) 
        {
            var response = await OriginalContract.SendWithReceipt<bool>("updateFlow", new object [] {
                token, sender, receiver, flowrate, userData
            });
            
            return (response.response, response.receipt);
        }

        public async Task<bool> UpdateFlowOperatorPermissions(string token, string flowOperator, BigInteger permissions, BigInteger flowrateAllowance) 
        {
            var response = await OriginalContract.Send<bool>("updateFlowOperatorPermissions", new object [] {
                token, flowOperator, permissions, flowrateAllowance
            });
            
            return response;
        }
        public async Task<(bool , TransactionReceipt receipt)> UpdateFlowOperatorPermissionsWithReceipt(string token, string flowOperator, BigInteger permissions, BigInteger flowrateAllowance) 
        {
            var response = await OriginalContract.SendWithReceipt<bool>("updateFlowOperatorPermissions", new object [] {
                token, flowOperator, permissions, flowrateAllowance
            });
            
            return (response.response, response.receipt);
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
