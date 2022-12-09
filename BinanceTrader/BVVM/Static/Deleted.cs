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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace BTNET.BVVM
{
    internal class Deleted : Core
    {
        public static bool IsHideTriggered = false;

        public static void HideOrder(OrderBase order)
        {
            order.IsOrderHidden = true;
            Static.ManageStoredOrders.AddSingleOrderToMemoryStorage(order, true);
            IsHideTriggered = true;
        }

        public static void HideOrderBulk(Collection<OrderBase> orders)
        {
            foreach (OrderBase order in orders)
            {
                order.IsOrderHidden = true;
                Static.ManageStoredOrders.AddSingleOrderToMemoryStorage(order, true);
            }

            IsHideTriggered = true;
        }

        /// <summary>
        /// Enumerate the Deleted List
        /// </summary>
        /// <param name="Orders"></param>
        public static Task EnumerateHiddenOrdersAsync(ObservableCollection<OrderBase>? Orders)
        {
            if (Orders == null)
            {
                return Task.CompletedTask;
            }

            foreach (var r in Orders.ToList())
            {
                if (r.IsOrderHidden)
                {
                    InvokeUI.CheckAccess(() =>
                    {
                        if (r.Helper != null)
                        {
                            r.Helper.SettleOrderEnabled = false;
                            r.Helper.SettleControlsEnabled = false;
                        }

                        _ = Orders.Remove(r);
                    });
                }
            }

            return Task.CompletedTask;
        }
    }
}
