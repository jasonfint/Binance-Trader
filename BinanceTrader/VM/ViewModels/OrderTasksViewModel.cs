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

using BinanceAPI.Enums;
using BTNET.BVVM;
using BTNET.BVVM.BT.Orders;
using BTNET.BVVM.Helpers;
using System.Windows.Input;

namespace BTNET.VM.ViewModels
{
    public class OrderTasksViewModel : Core
    {
        public ICommand? BuyCommand { get; set; }
        public ICommand? BuyAndSettleCommand { get; set; }
        public ICommand? BuyBorrowAndSettleCommand { get; set; }

        public ICommand? SellCommand { get; set; }
        public ICommand? SellAndSettleCommand { get; set; }
        public ICommand? SellBorrowAndSettleCommand { get; set; }

        public void InitializeCommands()
        {
            BuyCommand = new DelegateCommand(Buy);
            BuyAndSettleCommand = new DelegateCommand(BuyAndSettle);
            BuyBorrowAndSettleCommand = new DelegateCommand(BuyBorrowAndSettle);

            SellCommand = new DelegateCommand(Sell);
            SellAndSettleCommand = new DelegateCommand(SellAndSettle);
            SellBorrowAndSettleCommand = new DelegateCommand(SellBorrowAndSettle);
        }

        private void Buy(object o)
        {
            OrderViewModel order = (OrderViewModel)o;
            InternalOrderTasks.ProcessOrder(order, OrderSide.Buy, false, false);
        }

        private void BuyAndSettle(object o)
        {
            OrderViewModel order = (OrderViewModel)o;
            InternalOrderTasks.ProcessOrder(order, OrderSide.Buy, false);
        }

        private void BuyBorrowAndSettle(object o)
        {
            OrderViewModel order = (OrderViewModel)o;
            InternalOrderTasks.ProcessOrder(order, OrderSide.Buy, true);
        }

        private void Sell(object o)
        {
            OrderViewModel order = (OrderViewModel)o;
            InternalOrderTasks.ProcessOrder(order, OrderSide.Sell, false, false);
        }

        private void SellAndSettle(object o)
        {
            OrderViewModel order = (OrderViewModel)o;
            InternalOrderTasks.ProcessOrder(order, OrderSide.Sell, false);
        }

        private void SellBorrowAndSettle(object o)
        {
            OrderViewModel order = (OrderViewModel)o;
            InternalOrderTasks.ProcessOrder(order, OrderSide.Sell, true);
        }
    }
}
