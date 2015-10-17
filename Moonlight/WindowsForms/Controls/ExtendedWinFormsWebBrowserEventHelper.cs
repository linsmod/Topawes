using Microsoft.ShDocVw;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System;

namespace Moonlight.WindowsForms.Controls
{
    [DebuggerStepThrough]
    [DebuggerNonUserCode]
    internal sealed class ExtendedWinFormsWebBrowserEventHelper : StandardOleMarshalObject, DWebBrowserEvents2
    {
        private ExtendedWinFormsWebBrowser parent;
        [DebuggerHidden]
        public ExtendedWinFormsWebBrowserEventHelper(ExtendedWinFormsWebBrowser extendedWinFormsWebBrowser)
        {
            this.parent = extendedWinFormsWebBrowser;
        }
        [DebuggerHidden]
        public void BeforeNavigate2(object pDisp, ref object URL, ref object Flags, ref object TargetFrameName, ref object PostData, ref object Headers, ref bool Cancel)
        {
        }

        public void ClientToHostWindow([In, Out] ref long cx, [In, Out] ref long cy)
        {
        }

        [DebuggerHidden]
        public void ClientToHostWindow(ref int CX, ref int CY)
        {
        }

        public void CommandStateChange([In] long command, [In] bool enable)
        {
        }

        [DebuggerHidden]
        public void CommandStateChange(int Command, bool Enable)
        {
        }
        [DebuggerHidden]
        public void DocumentComplete(object pDisp, ref object URL)
        {
        }
        [DebuggerHidden]
        public void DownloadBegin()
        {
        }
        [DebuggerHidden]
        public void DownloadComplete()
        {
        }
        [DebuggerHidden]
        public void FileDownload(ref bool Cancel)
        {
        }
        [DebuggerHidden]
        public void NavigateComplete2(object pDisp, ref object URL)
        {
        }
        [DebuggerHidden]
        public void NavigateError(object pDisp, ref object URL, ref object Frame, ref object StatusCode, ref bool Cancel)
        {
            this.parent.NavigateError(URL as string, (int)StatusCode);
        }
        [DebuggerHidden]
        public void NewWindow2(ref object ppDisp, ref bool Cancel)
        {
        }
        [DebuggerHidden]
        public void NewWindow3(ref object ppDisp, ref bool Cancel, uint dwFlags, string bstrUrlContext, string bstrUrl)
        {
        }
        [DebuggerHidden]
        public void OnFullScreen(bool FullScreen)
        {
        }
        [DebuggerHidden]
        public void OnMenuBar(bool MenuBar)
        {
        }
        [DebuggerHidden]
        public void OnQuit()
        {
        }
        [DebuggerHidden]
        public void OnStatusBar(bool StatusBar)
        {
        }
        [DebuggerHidden]
        public void OnTheaterMode(bool TheaterMode)
        {
        }
        [DebuggerHidden]
        public void OnToolBar(bool ToolBar)
        {
        }
        [DebuggerHidden]
        public void OnVisible(bool Visible)
        {
        }
        [DebuggerHidden]
        public void PrintTemplateInstantiation(object pDisp)
        {
        }
        [DebuggerHidden]
        public void PrintTemplateTeardown(object pDisp)
        {
        }
        [DebuggerHidden]
        public void PrivacyImpactedStateChange(bool bImpacted)
        {
        }
        [DebuggerHidden]
        public void ProgressChange(int Progress, int ProgressMax)
        {
        }
        [DebuggerHidden]
        public void PropertyChange(string szProperty)
        {
        }
        [DebuggerHidden]
        public void SetSecureLockIcon(int SecureLockIcon)
        {
        }
        [DebuggerHidden]
        public void StatusTextChange(string Text)
        {
        }
        [DebuggerHidden]
        public void TitleChange(string Text)
        {
        }
        [DebuggerHidden]
        public void UpdatePageStatus(object pDisp, ref object nPage, ref object fDone)
        {
        }
        [DebuggerHidden]
        public void WindowClosing(bool IsChildWindow, ref bool Cancel)
        {
        }
        [DebuggerHidden]
        public void WindowSetHeight(int Height)
        {
        }
        [DebuggerHidden]
        public void WindowSetLeft(int Left)
        {
        }
        [DebuggerHidden]
        public void WindowSetResizable(bool Resizable)
        {
        }
        [DebuggerHidden]
        public void WindowSetTop(int Top)
        {
        }
        [DebuggerHidden]
        public void WindowSetWidth(int Width)
        {
        }
    }
}
