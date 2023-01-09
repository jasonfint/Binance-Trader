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
using BinanceAPI.Objects;
using BinanceAPI.Objects.Spot.SpotData;
using BTNET.BV.Base;
using BTNET.BV.Enum;
using BTNET.BVVM.Helpers;
using BTNET.BVVM.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
            if (!TradeVM.UseLimitBuyBool)
            {
                TradeVM.TradeRunner.AddToQueue(Static.SelectedSymbolViewModel.SymbolView.Symbol, TradeVM.OrderQuantity, 0, Static.CurrentTradingMode, BorrowVM.BorrowBuy, OrderSide.Buy, false);
            }
            else
            {
                TradeVM.TradeRunner.AddToQueue(Static.SelectedSymbolViewModel.SymbolView.Symbol, TradeVM.OrderQuantity, TradeVM.SymbolPriceBuy, Static.CurrentTradingMode, BorrowVM.BorrowBuy, OrderSide.Buy, true);
            }
        }

        public static void Sell()
        {
            if (!TradeVM.UseLimitSellBool)
            {
                TradeVM.TradeRunner.AddToQueue(Static.SelectedSymbolViewModel.SymbolView.Symbol, TradeVM.OrderQuantity, 0, Static.CurrentTradingMode, BorrowVM.BorrowSell, OrderSide.Sell, false);
            }
            else
            {
                TradeVM.TradeRunner.AddToQueue(Static.SelectedSymbolViewModel.SymbolView.Symbol, TradeVM.OrderQuantity, TradeVM.SymbolPriceSell, Static.CurrentTradingMode, BorrowVM.BorrowSell, OrderSide.Sell, true);
            }
        }

        public static bool SellAllFreeBase()
        {
            try
            {
                if (ScraperVM.Started)
                {
                    ScraperVM.ScraperStopped?.Invoke(null, "Selling all Free Base and Stopping");
                }

                var fb = BorrowVM.FreeBase;
                var sym = Static.SelectedSymbolViewModel.SymbolView.Symbol;
                var tm = Static.CurrentTradingMode;

                if (fb > 0)
                {
                    byte count = new DecimalHelper(fb.Normalize()).Scale;
                    decimal count2 = QuoteVM.QuantityTickSizeScale;
                    var floor = General.RoundDown(fb, (double)count2);
                    WriteLog.Info("Attempting to Sell All Free Base Asset for: " + sym + "| Amount: " + fb + "| Floored: " + floor);

                    if (count <= count2)
                    {
                        TradeVM.TradeRunner.AddToQueue(sym, fb, 0, tm, false, OrderSide.Sell, false);
                    }
                    else if (count2 > 0 && count > 0)
                    {
                        TradeVM.TradeRunner.AddToQueue(sym, floor, 0, tm, false, OrderSide.Sell, false);
                    }
                    else
                    {
                        WriteLog.Error(FAILED_SELL);
                        _ = Prompt.ShowBox(FAILED_SELL, FAILED);
                        NotifyVM.Notification(FAILED_SELL, Static.Red);
                        return false;
                    }
                }

                _ = Task.Run(() =>
                {
                    if (MainVM.DangerView != null)
                    {
                        InvokeUI.CheckAccess(() =>
                        {
                            MainVM.ToggleDangerView();
                        });
                    }
                }).ConfigureAwait(false);
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
                if (ScraperVM.Started)
                {
                    ScraperVM.ScraperStopped?.Invoke(null, "Selling all Free Base and Stopping");
                }

                if (MainVM.DangerView != null)
                {
                    MainVM.ToggleDangerView();
                }

                if (SellAllFreeBase())
                {
                    WriteLog.Info("Clearing Order List");

                    List<OrderBase> orderBases = new();

                    lock (MainOrders.OrderUpdateLock)
                    {
                        orderBases = Orders.Current.ToList();

                        if (orderBases.Count > 0)
                        {
                            if (SettingsVM.KeepFirstOrderIsChecked == true)
                            {
                                orderBases.RemoveAt(0);
                            }

                            Hidden.HideOrderBulk(orderBases);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.Error(ex);
            }
        }

        public static async Task<WebCallResult<BinancePlacedOrder>> PlaceOrderMarketAsync(string symbol, decimal quantity, TradingMode tradingmode, bool borrow, OrderSide side)
        {
#if DEBUG || DEBUG_SLOW

            await Task.Delay(1);
            WriteLog.Info(
                "TEST ORDER MARKET: Symbol :" + symbol +
                " | OrderQuantity :" + quantity +
                " | BorrowBuy :" + borrow +
                " | Side: " + side +
                " | SelectedTab :" + (Static.CurrentlySelectedSymbolTab == SelectedTab.Buy ? "Buy"
                : Static.CurrentlySelectedSymbolTab == SelectedTab.Sell ? "Sell"
                : Static.CurrentlySelectedSymbolTab == SelectedTab.Settle ? "Settle"
                : Static.CurrentlySelectedSymbolTab == SelectedTab.Error ? "Mode" : "Error") +
                " | SelectedTradingMode :" + (Static.CurrentTradingMode == TradingMode.Spot ? " Spot"
                : Static.CurrentTradingMode == TradingMode.Margin ? " Margin"
                : Static.CurrentTradingMode == TradingMode.Isolated ? " Isolated"
                : " Error")
            );

            NotifyVM.Notification("Order Placed:" + symbol + " | Quantity:" + quantity, Static.Green);
            return new WebCallResult<BinancePlacedOrder>(HttpStatusCode.NotAcceptable, null, null, new ArgumentError("null"));

#else
            var result = tradingmode switch
            {
                TradingMode.Spot =>
                await Client.Local.Spot.Order.PlaceOrderMarketAsync(symbol, side, OrderType.Market, quantity, receiveWindow: 1000),

                TradingMode.Margin =>
                !borrow ? await Client.Local.Margin.Order.PlaceMarginOrderMarketAsync(symbol, side, OrderType.Market, quantity, receiveWindow: 1000)
                : await Client.Local.Margin.Order.PlaceMarginOrderMarketAsync(symbol, side, OrderType.Market, quantity, sideEffectType: SideEffectType.MarginBuy, receiveWindow: 1000),

                TradingMode.Isolated =>
                !borrow ? await Client.Local.Margin.Order.PlaceMarginOrderMarketAsync(symbol, side, OrderType.Market, quantity, isIsolated: true, receiveWindow: 1000)
                : await Client.Local.Margin.Order.PlaceMarginOrderMarketAsync(symbol, side, OrderType.Market, quantity, isIsolated: true, sideEffectType: SideEffectType.MarginBuy, receiveWindow: 1000),

                _ => new WebCallResult<BinancePlacedOrder>(HttpStatusCode.NotAcceptable, null, null, new ArgumentError("null")),
            };

            return result;
#endif
        }

        public static async Task<WebCallResult<BinancePlacedOrder>> PlaceOrderLimitFoKAsync(string symbol, decimal quantity, TradingMode tradingmode, bool borrow, OrderSide side, decimal? price = null)
        {
#if DEBUG || DEBUG_SLOW

            await Task.Delay(1);
            WriteLog.Info(
                "TEST ORDER LIMIT: Symbol :" + symbol +
                " | OrderQuantity :" + quantity +
                " | SymbolPrice :" + price +
                " | BorrowBuy :" + borrow +
                " | Side: " + side +
                " | SelectedTab :" + (Static.CurrentlySelectedSymbolTab == SelectedTab.Buy ? "Buy"
                : Static.CurrentlySelectedSymbolTab == SelectedTab.Sell ? "Sell"
                : Static.CurrentlySelectedSymbolTab == SelectedTab.Settle ? "Settle"
                : Static.CurrentlySelectedSymbolTab == SelectedTab.Error ? "Mode" : "Error") +
                " | SelectedTradingMode :" + (Static.CurrentTradingMode == TradingMode.Spot ? " Spot"
                : Static.CurrentTradingMode == TradingMode.Margin ? " Margin"
                : Static.CurrentTradingMode == TradingMode.Isolated ? " Isolated"
                : " Error")
            );



            NotifyVM.Notification("Order Placed:" + symbol + " | Quantity:" + quantity, Static.Green);
            return new WebCallResult<BinancePlacedOrder>(System.Net.HttpStatusCode.NotAcceptable, null, null, new ArgumentError("null"));
#else
            var result = tradingmode switch
            {
                TradingMode.Spot =>
                await Client.Local.Spot.Order.PlaceOrderLimitAsync(symbol, side, OrderType.Limit, quantity, price, TimeInForce.FillOrKill, receiveWindow: 1000),

                TradingMode.Margin =>
                !borrow ? await Client.Local.Margin.Order.PlaceMarginOrderLimitAsync(symbol, side, OrderType.Limit, quantity, price, TimeInForce.FillOrKill, receiveWindow: 1000)
                : await Client.Local.Margin.Order.PlaceMarginOrderLimitAsync(symbol, side, OrderType.Limit, quantity, price, TimeInForce.FillOrKill, sideEffectType: SideEffectType.MarginBuy, receiveWindow: 1000),

                TradingMode.Isolated =>
                !borrow ? await Client.Local.Margin.Order.PlaceMarginOrderLimitAsync(symbol, side, OrderType.Limit, quantity, price, TimeInForce.FillOrKill, isIsolated: true, receiveWindow: 1000)
                : await Client.Local.Margin.Order.PlaceMarginOrderLimitAsync(symbol, side, OrderType.Limit, quantity, price, TimeInForce.FillOrKill, isIsolated: true, sideEffectType: SideEffectType.MarginBuy, receiveWindow: 1000),

                _ => new WebCallResult<BinancePlacedOrder>(System.Net.HttpStatusCode.NotAcceptable, null, null, new ArgumentError("null")),
            };

            return result;
#endif
        }

        public static async void PlaceOrderMarketNotifyAsync(string symbol, decimal quantity, TradingMode tradingmode, bool borrow, OrderSide side)
        {
#if DEBUG || DEBUG_SLOW

            await Task.Delay(1);
            WriteLog.Info(
                "TEST ORDER MARKET: Symbol :" + symbol +
                " | OrderQuantity :" + quantity +
                " | BorrowBuy :" + borrow +
                " | Side: " + side +
                " | SelectedTab :" + (Static.CurrentlySelectedSymbolTab == SelectedTab.Buy ? "Buy"
                : Static.CurrentlySelectedSymbolTab == SelectedTab.Sell ? "Sell"
                : Static.CurrentlySelectedSymbolTab == SelectedTab.Settle ? "Settle"
                : Static.CurrentlySelectedSymbolTab == SelectedTab.Error ? "Mode" : "Error") +
                " | SelectedTradingMode :" + (Static.CurrentTradingMode == TradingMode.Spot ? " Spot"
                : Static.CurrentTradingMode == TradingMode.Margin ? " Margin"
                : Static.CurrentTradingMode == TradingMode.Isolated ? " Isolated"
                : " Error")
            );

            NotifyVM.Notification("Order Placed:" + symbol + " | Quantity:" + quantity, Static.Green);     
#else
            var result = tradingmode switch
            {
                TradingMode.Spot =>
                await Client.Local.Spot.Order.PlaceOrderMarketAsync(symbol, side, OrderType.Market, quantity, receiveWindow: 1000),

                TradingMode.Margin =>
                !borrow ? await Client.Local.Margin.Order.PlaceMarginOrderMarketAsync(symbol, side, OrderType.Market, quantity, receiveWindow: 1000)
                : await Client.Local.Margin.Order.PlaceMarginOrderMarketAsync(symbol, side, OrderType.Market, quantity, sideEffectType: SideEffectType.MarginBuy, receiveWindow: 1000),

                TradingMode.Isolated =>
                !borrow ? await Client.Local.Margin.Order.PlaceMarginOrderMarketAsync(symbol, side, OrderType.Market, quantity, isIsolated: true, receiveWindow: 1000)
                : await Client.Local.Margin.Order.PlaceMarginOrderMarketAsync(symbol, side, OrderType.Market, quantity, isIsolated: true, sideEffectType: SideEffectType.MarginBuy, receiveWindow: 1000),

                _ => new WebCallResult<BinancePlacedOrder>(HttpStatusCode.NotAcceptable, null, null, new ArgumentError("null")),
            };

            OrderResult(result);
#endif
        }

        public static async void PlaceOrderLimitNotifyAsync(string symbol, decimal quantity, TradingMode tradingmode, bool borrow, OrderSide side, decimal? price = null)
        {
#if DEBUG || DEBUG_SLOW

            await Task.Delay(1);
            WriteLog.Info(
                "TEST ORDER LIMIT: Symbol :" + symbol +
                " | OrderQuantity :" + quantity +
                " | SymbolPrice :" + price +
                " | BorrowBuy :" + borrow +
                " | Side: " + side +
                " | SelectedTab :" + (Static.CurrentlySelectedSymbolTab == SelectedTab.Buy ? "Buy"
                : Static.CurrentlySelectedSymbolTab == SelectedTab.Sell ? "Sell"
                : Static.CurrentlySelectedSymbolTab == SelectedTab.Settle ? "Settle"
                : Static.CurrentlySelectedSymbolTab == SelectedTab.Error ? "Mode" : "Error") +
                " | SelectedTradingMode :" + (Static.CurrentTradingMode == TradingMode.Spot ? " Spot"
                : Static.CurrentTradingMode == TradingMode.Margin ? " Margin"
                : Static.CurrentTradingMode == TradingMode.Isolated ? " Isolated"
                : " Error")
            );

            NotifyVM.Notification("Order Placed:" + symbol + " | Quantity:" + quantity, Static.Green);
#else
            var result = tradingmode switch
            {
                TradingMode.Spot =>
                await Client.Local.Spot.Order.PlaceOrderLimitAsync(symbol, side, OrderType.Limit, quantity, price, TimeInForce.GoodTillCancel, receiveWindow: 1000),

                TradingMode.Margin =>
                !borrow ? await Client.Local.Margin.Order.PlaceMarginOrderLimitAsync(symbol, side, OrderType.Limit, quantity, price, TimeInForce.GoodTillCancel, receiveWindow: 1000)
                : await Client.Local.Margin.Order.PlaceMarginOrderLimitAsync(symbol, side, OrderType.Limit, quantity, price, TimeInForce.GoodTillCancel, sideEffectType: SideEffectType.MarginBuy, receiveWindow: 1000),

                TradingMode.Isolated =>
                !borrow ? await Client.Local.Margin.Order.PlaceMarginOrderLimitAsync(symbol, side, OrderType.Limit, quantity, price, TimeInForce.GoodTillCancel, isIsolated: true, receiveWindow: 1000)
                : await Client.Local.Margin.Order.PlaceMarginOrderLimitAsync(symbol, side, OrderType.Limit, quantity, price, TimeInForce.GoodTillCancel, isIsolated: true, sideEffectType: SideEffectType.MarginBuy, receiveWindow: 1000),

                _ => new WebCallResult<BinancePlacedOrder>(System.Net.HttpStatusCode.NotAcceptable, null, null, new ArgumentError("null")),
            };


            OrderResult(result);
#endif
        }

        public static WebCallResult<BinancePlacedOrder> OrderResult(WebCallResult<BinancePlacedOrder> result)
        {
            if (result.Success)
            {
                NotifyVM.Notification("Order Placed:" + result.Data.Symbol + " | Quantity:" + result.Data.Quantity, Static.Gold);
                OrderManager.LastTotal = 0;
                return result;
            }

            if (result.Error != null)
            {
                WriteLog.Error($"Order placing failed: {result.Error.Code}|{result.Error.Message}");
                NotifyVM.Notification("Order Placing Failed, Try again", Static.Red);
            }

            return result;
        }
    }
}
