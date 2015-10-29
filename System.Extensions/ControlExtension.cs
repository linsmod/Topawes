using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Forms
{
    public static class ControlExtension
    {
        /// <summary>
        /// 在创建控件的线程上调用方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="control"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static async Task InvokeTask(this Control control, Func<Task> method)
        {
            if (control.InvokeRequired)
            {
                await (Task)control.Invoke(method);
            }
            else
                await method.Invoke();
        }

        public static async Task InvokeTask<T>(this Control control, Func<T, Task> method, T arg)
        {
            if (control.InvokeRequired)
            {
                await (Task)control.Invoke(method, arg);
            }
            else
                await method.Invoke(arg);
        }

        public static async Task InvokeTask<T>(this Control control, Func<T[], Task> method, params T[] arg)
        {
            if (control.InvokeRequired)
            {
                await (Task)control.Invoke(method, arg);
            }
            else
                await method.Invoke(arg);
        }

        /// <summary>
        /// 在创建控件的线程上调用方法并返回值
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="control"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static async Task<TResult> InvokeTask<TResult>(this Control control, Func<Task<TResult>> method)
        {
            if (control.InvokeRequired)
            {
                return await (Task<TResult>)control.Invoke(method);
            }
            else
                return await method.Invoke();
        }

        /// <summary>
        /// 在创建控件的线程上调用方法并返回值
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="control"></param>
        /// <param name="method"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static async Task<TResult> InvokeTask<T, TResult>(this Control control, Func<T, Task<TResult>> method, T arg)
        {
            if (control.InvokeRequired)
            {
                return await (Task<TResult>)control.Invoke(method, arg);
            }
            else
                return await method.Invoke(arg);
        }

        /// <summary>
        /// 在创建控件的线程上调用方法并返回值
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="control"></param>
        /// <param name="method"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <returns></returns>
        public static async Task<TResult> InvokeTask<T1, T2, TResult>(this Control control, Func<T1, T2, Task<TResult>> method, T1 arg1, T2 arg2)
        {
            if (control.InvokeRequired)
            {
                return await (Task<TResult>)control.Invoke(method, arg1, arg2);
            }
            else
                return await method.Invoke(arg1, arg2);
        }

        /// <summary>
        /// 在创建控件的线程上调用方法并返回值
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="control"></param>
        /// <param name="method"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <returns></returns>
        public static async Task<TResult> InvokeTask<T1, T2, TResult>(this Control control, Func<T1, T2[], Task<TResult>> method, T1 arg1, params T2[] arg2)
        {
            if (control.InvokeRequired)
            {
                return await (Task<TResult>)control.Invoke(method, arg1, arg2);
            }
            else
                return await method.Invoke(arg1, arg2);
        }

        /// <summary>
        /// 在创建控件的线程上调用方法并返回值
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="control"></param>
        /// <param name="method"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <returns></returns>
        public static async Task<TResult> InvokeTask<T1, T2, T3, TResult>(this Control control, Func<T1, T2, T3, Task<TResult>> method, T1 arg1, T2 arg2, T3 arg3)
        {
            if (control.InvokeRequired)
            {
                return await (Task<TResult>)control.Invoke(method, arg1, arg2, arg3);
            }
            else
                return await method.Invoke(arg1, arg2, arg3);
        }

        /// <summary>
        /// 在创建控件的线程上调用方法并返回值
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="control"></param>
        /// <param name="method"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <param name="arg4"></param>
        /// <returns></returns>
        public static async Task<TResult> InvokeTask<T1, T2, T3, T4, TResult>(this Control control, Func<T1, T2, T3, T4, Task<TResult>> method, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            if (control.InvokeRequired)
            {
                return await (Task<TResult>)control.Invoke(method, arg1, arg2, arg3, arg4);
            }
            else
                return await method.Invoke(arg1, arg2, arg3, arg4);
        }

        /// <summary>
        /// 在创建控件的线程上调用方法并返回值
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="control"></param>
        /// <param name="method"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <param name="arg4"></param>
        /// <param name="arg5"></param>
        /// <returns></returns>
        public static async Task<TResult> InvokeTask<T1, T2, T3, T4, T5, TResult>(this Control control, Func<T1, T2, T3, T4, T5, Task<TResult>> method, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            if (control.InvokeRequired)
            {
                return await (Task<TResult>)control.Invoke(method, arg1, arg2, arg3, arg4, arg5);
            }
            else
                return await method.Invoke(arg1, arg2, arg3, arg4, arg5);
        }

        /// <summary>
        /// 在创建控件的线程上调用方法并返回值
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="control"></param>
        /// <param name="method"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <param name="arg4"></param>
        /// <param name="arg5"></param>
        /// <param name="arg6"></param>
        /// <returns></returns>
        public static async Task<TResult> InvokeTask<T1, T2, T3, T4, T5, T6, TResult>(this Control control, Func<T1, T2, T3, T4, T5, T6, Task<TResult>> method, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            if (control.InvokeRequired)
            {
                return await (Task<TResult>)control.Invoke(method, arg1, arg2, arg3, arg4, arg5, arg6);
            }
            else
                return await method.Invoke(arg1, arg2, arg3, arg4, arg5, arg6);
        }

        /// <summary>
        /// 在创建控件的线程上调用方法并返回值
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="control"></param>
        /// <param name="method"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <param name="arg4"></param>
        /// <param name="arg5"></param>
        /// <param name="arg6"></param>
        /// <param name="arg7"></param>
        /// <returns></returns>
        public static async Task<TResult> InvokeTask<T1, T2, T3, T4, T5, T6, T7, TResult>(this Control control, Func<T1, T2, T3, T4, T5, T6, T7, Task<TResult>> method, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            if (control.InvokeRequired)
            {
                return await (Task<TResult>)control.Invoke(method, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
            }
            else
                return await method.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }

        public static void InvokeAction(this Control control, Action method)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(method);
            }
            else
                method();
        }

        public static void InvokeAction<T>(this Control control, Action<T> method, T args)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(method, args);
            }
            else
                method.Invoke(args);
        }

        public static void InvokeActionAsync(this Control control, Action method)
        {
            if (control.InvokeRequired)
            {
                control.BeginInvoke(method);
            }
            else
                method.BeginInvoke(null, null);
        }

        public static void InvokeActionAsync<T>(this Control control, Action<T> method, T args)
        {
            if (control.InvokeRequired)
            {
                control.BeginInvoke(method, args);
            }
            else
                method.BeginInvoke(args, null, null);
        }
    }
}
