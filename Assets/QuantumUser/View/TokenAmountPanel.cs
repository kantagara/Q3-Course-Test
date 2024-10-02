using System;
using System.Collections;
using System.Numerics;
using ChainSafe.Gaming.Evm.Contracts;
using ChainSafe.Gaming.Evm.Contracts.BuiltIn;
using ChainSafe.Gaming.UnityPackage;
using DG.Tweening;
using Scripts.EVM.Token;
using TMPro;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Quantum
{
    public class TokenAmountPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text amount;
        [SerializeField] private TMP_Text tokenStreamingToggle;
        [SerializeField] private Transform coinTransform;

        private Coroutine _coroutine;
        private Erc20Contract _erc20;
        private Vector3 _coinScale;
        private Vector3 _amountScale;

        private async void Awake()
        {
            Web3Unity.Web3.Erc20.BuildContract("0xC5FCF18Dc1CD147f5Ad3a95210cc61Cb024C5Bf1");
            TokenStreamingChecker.OnTokenStreamingStarted += TokenStreamingStarted;
            TokenStreamingChecker.OnTokenStreamingStopped += TokenStreamingStopped;
            _coinScale = coinTransform.localScale;
            _amountScale = amount.transform.localScale;
            var balance = await Web3Unity.Web3.Erc20.GetBalanceOf("0xC5FCF18Dc1CD147f5Ad3a95210cc61Cb024C5Bf1");
            var ethBalance = (decimal)balance / (decimal)Math.Pow(10, 18);
            amount.text = ethBalance.ToString("0.############") + " MTT";
        }

        private void OnDestroy()
        {
            TokenStreamingChecker.OnTokenStreamingStarted -= TokenStreamingStarted;
            TokenStreamingChecker.OnTokenStreamingStopped -= TokenStreamingStopped;
        }

        private void TokenStreamingStopped()
        {
            if (_coroutine != null)
            {
                _sequence?.Kill();
                tokenStreamingToggle.text = "Token Streaming Stopped!";
                tokenStreamingToggle.color = Color.red;

                _sequence = DOTween.Sequence().Append(tokenStreamingToggle.transform.DOScale(Vector3.one, 1))
                    .AppendInterval(1).OnComplete(() => tokenStreamingToggle.transform.localScale = Vector3.zero);
                StopCoroutine(_coroutine);
            }
        }

        private Sequence _sequence;

        private void TokenStreamingStarted()
        {
            if (_coroutine == null)
            {
                _sequence?.Kill();
                tokenStreamingToggle.text = "Token Streaming Started!";
                tokenStreamingToggle.color = Color.green;

                _sequence = DOTween.Sequence().Append(tokenStreamingToggle.transform.DOScale(Vector3.one, 1))
                    .AppendInterval(1).OnComplete(() => tokenStreamingToggle.transform.localScale = Vector3.zero);

                StartCoroutine(CheckBalanceConstantly());
            }
        }

        private WaitForSeconds _wfs = new(2);
        private decimal _prevBalance = 0;
        private Tween _coinTween, _textTween;

        private IEnumerator CheckBalanceConstantly()
        {
            while (true)
            {
                
                // Create a task to fetch the balance
                var getBalanceTask = Web3Unity.Web3.Erc20.GetBalanceOf("0xC5FCF18Dc1CD147f5Ad3a95210cc61Cb024C5Bf1");

                // Wait for the task to complete
                yield return new WaitUntil(() => getBalanceTask.IsCompleted);
                
                // Wait for some time (assuming _wfs is your WaitForSeconds object)
                yield return _wfs;
                
                // Check if the task completed successfully
                if (getBalanceTask.Exception == null)
                {
                    // Get the result (balance)
                    var balance = getBalanceTask.Result;
                    var ethBalance = (decimal)balance / (decimal)Math.Pow(10, 18);
                    amount.text = ethBalance.ToString("0.############") + " MTT";
                    if (ethBalance != _prevBalance)
                    {
                        _coinTween?.Kill();
                        _textTween?.Kill();
                        _coinTween = coinTransform.DOScale(_coinScale * 1.2f, 1f).SetLoops(1, LoopType.Yoyo);
                        amount.color = Color.green;
                        _textTween = amount.transform.DOScale(_coinScale * 1.2f, 1f).SetLoops(1, LoopType.Yoyo)
                            .OnComplete(
                                () =>
                                {
                                    amount.color = Color.white;
                                    amount.transform.localScale = _amountScale;
                                    coinTransform.localScale = _coinScale;
                                }).OnKill(() =>
                            {
                                amount.color = Color.white;
                                amount.transform.localScale = _amountScale;
                                coinTransform.localScale = _coinScale;
                            });
                    }

                    _prevBalance = ethBalance;
                }
                else
                {
                    // Log the error if any
                    Debug.LogError($"Error fetching balance: {getBalanceTask.Exception}");
                }
            }
        }
    }
}