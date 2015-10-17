using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace Microsoft.ShDocVw
{
    [CompilerGenerated, Guid("34A715A0-6587-11D0-924A-0020AFC7AC4D"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch), TypeIdentifier]
    [ComImport]
    public interface DWebBrowserEvents2
    {
        [DispId(102)]
        [PreserveSig]
        void StatusTextChange([MarshalAs(UnmanagedType.BStr)] [In] string Text);
        [DispId(108)]
        [PreserveSig]
        void ProgressChange([In] int Progress, [In] int ProgressMax);
        [DispId(105)]
        [PreserveSig]
        void CommandStateChange([In] int Command, [In] bool Enable);
        [DispId(106)]
        [PreserveSig]
        void DownloadBegin();
        [DispId(104)]
        [PreserveSig]
        void DownloadComplete();
        [DispId(113)]
        [PreserveSig]
        void TitleChange([MarshalAs(UnmanagedType.BStr)] [In] string Text);
        [DispId(112)]
        [PreserveSig]
        void PropertyChange([MarshalAs(UnmanagedType.BStr)] [In] string szProperty);
        [DispId(250)]
        [PreserveSig]
        void BeforeNavigate2([MarshalAs(UnmanagedType.IDispatch)] [In] object pDisp, [MarshalAs(UnmanagedType.Struct)] [In] ref object URL, [MarshalAs(UnmanagedType.Struct)] [In] ref object Flags, [MarshalAs(UnmanagedType.Struct)] [In] ref object TargetFrameName, [MarshalAs(UnmanagedType.Struct)] [In] ref object PostData, [MarshalAs(UnmanagedType.Struct)] [In] ref object Headers, [In] [Out] ref bool Cancel);
        [DispId(251)]
        [PreserveSig]
        void NewWindow2([MarshalAs(UnmanagedType.IDispatch)] [In] [Out] ref object ppDisp, [In] [Out] ref bool Cancel);
        [DispId(252)]
        [PreserveSig]
        void NavigateComplete2([MarshalAs(UnmanagedType.IDispatch)] [In] object pDisp, [MarshalAs(UnmanagedType.Struct)] [In] ref object URL);
        [DispId(259)]
        [PreserveSig]
        void DocumentComplete([MarshalAs(UnmanagedType.IDispatch)] [In] object pDisp, [MarshalAs(UnmanagedType.Struct)] [In] ref object URL);
        [DispId(253)]
        [PreserveSig]
        void OnQuit();
        [DispId(254)]
        [PreserveSig]
        void OnVisible([In] bool Visible);
        [DispId(255)]
        [PreserveSig]
        void OnToolBar([In] bool ToolBar);
        [DispId(256)]
        [PreserveSig]
        void OnMenuBar([In] bool MenuBar);
        [DispId(257)]
        [PreserveSig]
        void OnStatusBar([In] bool StatusBar);
        [DispId(258)]
        [PreserveSig]
        void OnFullScreen([In] bool FullScreen);
        [DispId(260)]
        [PreserveSig]
        void OnTheaterMode([In] bool TheaterMode);
        [DispId(262)]
        [PreserveSig]
        void WindowSetResizable([In] bool Resizable);
        [DispId(264)]
        [PreserveSig]
        void WindowSetLeft([In] int Left);
        [DispId(265)]
        [PreserveSig]
        void WindowSetTop([In] int Top);
        [DispId(266)]
        [PreserveSig]
        void WindowSetWidth([In] int Width);
        [DispId(267)]
        [PreserveSig]
        void WindowSetHeight([In] int Height);
        [DispId(263)]
        [PreserveSig]
        void WindowClosing([In] bool IsChildWindow, [In] [Out] ref bool Cancel);
        [DispId(268)]
        [PreserveSig]
        void ClientToHostWindow([In] [Out] ref int CX, [In] [Out] ref int CY);
        [DispId(269)]
        [PreserveSig]
        void SetSecureLockIcon([In] int SecureLockIcon);
        [DispId(270)]
        [PreserveSig]
        void FileDownload([In] [Out] ref bool Cancel);
        [DispId(271)]
        [PreserveSig]
        void NavigateError([MarshalAs(UnmanagedType.IDispatch)] [In] object pDisp, [MarshalAs(UnmanagedType.Struct)] [In] ref object URL, [MarshalAs(UnmanagedType.Struct)] [In] ref object Frame, [MarshalAs(UnmanagedType.Struct)] [In] ref object StatusCode, [In] [Out] ref bool Cancel);
        [DispId(225)]
        [PreserveSig]
        void PrintTemplateInstantiation([MarshalAs(UnmanagedType.IDispatch)] [In] object pDisp);
        [DispId(226)]
        [PreserveSig]
        void PrintTemplateTeardown([MarshalAs(UnmanagedType.IDispatch)] [In] object pDisp);
        [DispId(227)]
        [PreserveSig]
        void UpdatePageStatus([MarshalAs(UnmanagedType.IDispatch)] [In] object pDisp, [MarshalAs(UnmanagedType.Struct)] [In] ref object nPage, [MarshalAs(UnmanagedType.Struct)] [In] ref object fDone);
        [DispId(272)]
        [PreserveSig]
        void PrivacyImpactedStateChange([In] bool bImpacted);
        [DispId(273)]
        [PreserveSig]
        void NewWindow3([MarshalAs(UnmanagedType.IDispatch)] [In] [Out] ref object ppDisp, [In] [Out] ref bool Cancel, [In] uint dwFlags, [MarshalAs(UnmanagedType.BStr)] [In] string bstrUrlContext, [MarshalAs(UnmanagedType.BStr)] [In] string bstrUrl);
    }
}
