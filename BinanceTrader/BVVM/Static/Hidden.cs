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

using BTNET.BV.Base;
using BTNET.BVVM.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BTNET.BVVM
{
    internal class Hidden : Core
    {
        public static bool IsHideTriggered = false;

        public static bool IsStatusTriggered = false;

        public static void HideOrder(OrderBase order)
        {
            InvokeUI.CheckAccess(() =>
            {
                order.IsOrderHidden = true;
            });

            Static.ManageStoredOrders.AddSingleOrderToMemoryStorage(order, true);
            IsHideTriggered = true;
        }

        public static void HideOrderBulk(IEnumerable<OrderBase> orders)
        {
            foreach (OrderBase order in orders)
            {
                InvokeUI.CheckAccess(() =>
                {
                    order.IsOrderHidden = true;
                });

                Static.ManageStoredOrders.AddSingleOrderToMemoryStorage(order, true);
            }

            IsHideTriggered = true;
        }

        /// <summary>
        /// Enumerate the Deleted List
        /// </summary>
        /// <param name="Orders"></param>
        public static Task EnumerateHiddenOrdersAsync()
        {
            if (Orders == null)
            {
                return Task.CompletedTask;
            }

            lock (MainOrders.OrderUpdateLock)
            {
                List<OrderBase> copy = Orders.Current.ToList();

                foreach (var r in copy.Where(r => r.IsOrderHidden))
                {
                    InvokeUI.CheckAccess(() =>
                    {
                        r.SettleOrderEnabled = false;
                        r.SettleControlsEnabled = false;
                        Orders.Current.Remove(r);
                    });
                }
            }

            return Task.CompletedTask;
        }
    }
}
