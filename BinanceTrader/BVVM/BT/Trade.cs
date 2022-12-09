/*
*MIT License
*
*Copyright (c) 2022 S Christison
*
*Permission is hereby granted, free of charge, to any person obtaining a copy
*of this software and associated documentation files (the "Software"), to deal
*in the Software without restriction, including without limitation the rights
*to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
*copies of the Software, and to permit persons to whom the Software is
*furnished to do so, subject to the following conditions:
*
*The above copyright notice and this permission notice shall be included in all
*copies or substantial portions of the Software.
*
*THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
*IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
*FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
*AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
*LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
*OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
*SOFTWARE.
*/

using BinanceAPI;
using BinanceAPI.Enums;
using BinanceAPI.Objects.Spot.SpotData;
using BTNET.BV.Base;
using BTNET.BV.Enum;
using BTNET.BVVM.Helpers;
using BTNET.BVVM.Log;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace BTNET.BVVM.BT
{
    internal class Trade : Core
    {
        private const string FAILED = "Failed";
        private const string FAILED_SELL = "Failed to sell base asset automatically, please sell manually";
        private const string RESTART = "Restart Binance Trader";

        public static void Buy()
        {
            _ = PlaceOrderAsync(Static.SelectedSymbolViewModel.SymbolView.Symbol, TradeVM.OrderQuantity, Static.CurrentTradingMode, BorrowVM.BorrowBuy, OrderSide.Buy, TradeVM.UseLimitBuyBool, TradeVM.SymbolPriceBuy);
        }

        public static void Sell()
        {
            _ = PlaceOrderAsync(Static.SelectedSymbolViewModel.SymbolView.Symbol, TradeVM.OrderQuantity, Static.CurrentTradingMode, BorrowVM.BorrowSell, OrderSide.Sell, TradeVM.UseLimitSellBool, TradeVM.SymbolPriceSell);
        }

        public static bool SellAllFreeBase()
        {
            try
            {
                if (BorrowVM.FreeBase > 0)
                {
                    byte count = new DecimalHelper(BorrowVM.FreeBase.Normalize()).Scale;
                    decimal count2 = QuoteVM.QuantityTickSizeScale;

                    if (count <= count2)
                    {
                        _ = PlaceOrderAsync(Static.SelectedSymbolViewModel.SymbolView.Symbol, BorrowVM.FreeBase, Static.CurrentTradingMode, false, OrderSide.Sell);
                    }
                    else if (count2 == 0 && count > 0)
                    {
                        _ = PlaceOrderAsync(Static.SelectedSymbolViewModel.SymbolView.Symbol, decimal.Floor(BorrowVM.FreeBase), Static.CurrentTradingMode, false, OrderSide.Sell);
                    }
                    else
                    {
                        WriteLog.Error(FAILED_SELL);
                        _ = Prompt.ShowBox(FAILED_SELL, FAILED);
                        NotifyVM.Notification(FAILED_SELL, Static.Red);
                        return false;
                    }
                }

            }
            catch (Exception ex)
            {
                WriteLog.Error(ex);
                return false;
            }

            return true;
        }

        public static void SellAllFreeBaseAndClear()
        {
            try
            {
                if (SellAllFreeBase())
                {
                    lock (MainOrders.OrderUpdateLock)
                    {
                        if (Orders.Current.Count > 0)
                        {
                            Collection<OrderBase> orderBases = Orders.Current;

                            if (SettingsVM.KeepFirstOrderIsChecked == true)
                            {
                                InvokeUI.CheckAccess(() =>
                                {
                                    orderBases.RemoveAt(0);
                                });
                            }

                            Deleted.HideOrderBulk(orderBases);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.Error(ex);
            }
        }

        public static async Task<BinancePlacedOrder?> PlaceOrderAsync(string symbol, decimal quantity, TradingMode tradingmode, bool borrow, OrderSide side, bool useLimit = false, decimal? price = null)
        {
            //#if DEBUG || DEBUG_SLOW

            //            OrderType type = useLimit ? OrderType.Limit : OrderType.Market;
            //            decimal? usePrice = type == OrderType.Limit ? price : null;
            //            await Task.Delay(1);
            //            WriteLog.Info(
            //                "TEST ORDER: Symbol :" + symbol +
            //                " | OrderQuantity :" + quantity +
            //                " | SymbolPrice :" + price +
            //                " | BorrowBuy :" + borrow +
            //                " | UseLimit :" + useLimit +
            //                " | Type: " + type +
            //                " | Side: " + side +
            //                " | usePrice: " + usePrice +
            //                " | SelectedTab :" + (Static.CurrentlySelectedSymbolTab == SelectedTab.Buy ? "Buy"
            //                : Static.CurrentlySelectedSymbolTab == SelectedTab.Sell ? "Sell"
            //                : Static.CurrentlySelectedSymbolTab == SelectedTab.Settle ? "Settle"
            //                : Static.CurrentlySelectedSymbolTab == SelectedTab.Error ? "Mode" : "Error") +
            //                " | SelectedTradingMode :" + (Static.CurrentTradingMode == TradingMode.Spot ? " Spot"
            //                : Static.CurrentTradingMode == TradingMode.Margin ? " Margin"
            //                : Static.CurrentTradingMode == TradingMode.Isolated ? " Isolated"
            //                : " Error")
            //            );

            //            NotifyVM.Notification("Order Placed:" + symbol + " | Quantity:" + quantity, Static.Green);
            //            return null;

            //#else

            if (quantity == 0)
            {
                _ = Prompt.ShowBox(RESTART, FAILED);
                return null;
            }

            decimal? usePrice = useLimit ? price : null;
            OrderType type = useLimit ? OrderType.Limit : OrderType.Market;
            TimeInForce? tif = useLimit ? TimeInForce.GoodTillCancel : null;


            var result = tradingmode switch
            {
                TradingMode.Spot =>
                await Client.Local.Spot.Order.PlaceOrderAsync(symbol, side, type, quantity: quantity, price: usePrice, receiveWindow: 3000, timeInForce: tif),

                TradingMode.Margin =>
                !borrow ? await Client.Local.Margin.Order.PlaceMarginOrderAsync(symbol, side, type, quantity: quantity, price: usePrice, receiveWindow: 3000, timeInForce: tif)
                : await Client.Local.Margin.Order.PlaceMarginOrderAsync(symbol, side, type, quantity: quantity, price: usePrice, sideEffectType: SideEffectType.MarginBuy, receiveWindow: 3000, timeInForce: tif),

                TradingMode.Isolated =>
                !borrow ? await Client.Local.Margin.Order.PlaceMarginOrderAsync(symbol, side, type, quantity: quantity, price: usePrice, isIsolated: true, receiveWindow: 3000, timeInForce: tif)
                : await Client.Local.Margin.Order.PlaceMarginOrderAsync(symbol, side, type, quantity: quantity, price: usePrice, isIsolated: true, sideEffectType: SideEffectType.MarginBuy, receiveWindow: 3000, timeInForce: tif),

                _ => null,
            };

            if (result != null)
            {
                if (result.Success)
                {
                    WriteLog.Info("Order Placed!: Symbol :" + symbol +
                        " | OrderQuantity :" + quantity +
                        " | SymbolPrice :" + (useLimit ? price : "Market") +
                        " | Type: " + type +
                        " | Side: " + side);

                    NotifyVM.Notification("Order Placed:" + symbol + " | Quantity:" + quantity, Static.Green);
                    return result.Data;
                }
                else
                {
                    _ = Prompt.ShowBox($"Order placing failed: {result.Error?.Message}", FAILED);
                    NotifyVM.Notification($"Order placing failed: {result.Error?.Message}", Static.Red);
                }
            }
            else
            {
                _ = Prompt.ShowBox($"Order placing failed: Internal Error, Please " + RESTART, FAILED);
            }

            return null;
            //#endif
        }
    }
}
