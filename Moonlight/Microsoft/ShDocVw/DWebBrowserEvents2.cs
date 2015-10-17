using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace Microsoft.ShDocVw
{
    [Guid("34A715A0-6587-11D0-924A-0020AFC7AC4D"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch), TypeLibType(TypeLibTypeFlags.FHidden)]
    [ComImport]
    public interface DWebBrowserEvents2
    {
        [DispId(102)]
        void StatusTextChange([In] string text);
        [DispId(108)]
        void ProgressChange([In] int progress, [In] int progressMax);
        [DispId(105)]
        void CommandStateChange([In] long command, [In] bool enable);
        [DispId(106)]
        void DownloadBegin();
        [DispId(104)]
        void DownloadComplete();
        [DispId(113)]
        void TitleChange([In] string text);
        [DispId(112)]
        void PropertyChange([In] string szProperty);
        [DispId(250)]
        void BeforeNavigate2([MarshalAs(UnmanagedType.IDispatch)] [In] object pDisp, [In] ref object URL, [In] ref object flags, [In] ref object targetFrameName, [In] ref object postData, [In] ref object headers, [In] [Out] ref bool cancel);
        [DispId(251)]
        void NewWindow2([MarshalAs(UnmanagedType.IDispatch)] [In] [Out] ref object pDisp, [In] [Out] ref bool cancel);
        [DispId(252)]
        void NavigateComplete2([MarshalAs(UnmanagedType.IDispatch)] [In] object pDisp, [In] ref object URL);
        [DispId(259)]
        void DocumentComplete([MarshalAs(UnmanagedType.IDispatch)] [In] object pDisp, [In] ref object URL);
        [DispId(253)]
        void OnQuit();
        [DispId(254)]
        void OnVisible([In] bool visible);
        [DispId(255)]
        void OnToolBar([In] bool toolBar);
        [DispId(256)]
        void OnMenuBar([In] bool menuBar);
        [DispId(257)]
        void OnStatusBar([In] bool statusBar);
        [DispId(258)]
        void OnFullScreen([In] bool fullScreen);
        [DispId(260)]
        void OnTheaterMode([In] bool theaterMode);
        [DispId(262)]
        void WindowSetResizable([In] bool resizable);
        [DispId(264)]
        void WindowSetLeft([In] int left);
        [DispId(265)]
        void WindowSetTop([In] int top);
        [DispId(266)]
        void WindowSetWidth([In] int width);
        [DispId(267)]
        void WindowSetHeight([In] int height);
        [DispId(263)]
        void WindowClosing([In] bool isChildWindow, [In] [Out] ref bool cancel);
        [DispId(268)]
        void ClientToHostWindow([In] [Out] ref long cx, [In] [Out] ref long cy);
        [DispId(269)]
        void SetSecureLockIcon([In] int secureLockIcon);
        [DispId(270)]
        void FileDownload([In] [Out] ref bool cancel);
        [DispId(271)]
        void NavigateError([MarshalAs(UnmanagedType.IDispatch)] [In] object pDisp, [In] ref object URL, [In] ref object frame, [In] ref object statusCode, [In] [Out] ref bool cancel);
        [DispId(225)]
        void PrintTemplateInstantiation([MarshalAs(UnmanagedType.IDispatch)] [In] object pDisp);
        [DispId(226)]
        void PrintTemplateTeardown([MarshalAs(UnmanagedType.IDispatch)] [In] object pDisp);
        [DispId(227)]
        void UpdatePageStatus([MarshalAs(UnmanagedType.IDispatch)] [In] object pDisp, [In] ref object nPage, [In] ref object fDone);
        [DispId(272)]
        void PrivacyImpactedStateChange([In] bool bImpacted);
    }
}