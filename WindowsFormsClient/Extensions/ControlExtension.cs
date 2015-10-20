using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsClient.Extensions
{
    public static class ControlExtension
    {
        /// <summary>
        /// 如果InvokeRequired，自动调用Control.Invoke执行delegate
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="control"></param>
        /// <param name="d"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static TResult SmartInvoke<TResult>(this Control control, Delegate d, params object[] args)
        {
            if (control.InvokeRequired)
            {
                return (TResult)control.Invoke(d, args);
            }
            return (TResult)d.DynamicInvoke(args);
        }

        public static object SmartInvoke(this Control control, Action d)
        {
            if (control.InvokeRequired)
            {
                return control.Invoke(d);
            }
            else
                d.Invoke(); return null;
        }

        public static void SmartInvokeAsync(this Control control, Action d)
        {
            if (control.InvokeRequired)
            {
                control.BeginInvoke(new MethodInvoker(() => { d(); }));
            }
            else
                d();
        }
    }
}
