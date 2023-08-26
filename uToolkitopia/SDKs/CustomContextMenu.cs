#region

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Vanara.PInvoke;

#endregion

namespace Kitopia.SDKs;

[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
[Guid("32f2578e-55af-4e1c-87c7-2770891ad500")] // 使用唯一的GUID替换
public class CustomContextMenu : Shell32.IExplorerCommand, Ole32.IObjectWithSite
{
    public HRESULT GetTitle(Shell32.IShellItemArray psiItemArray, out string ppszName) =>
        throw new NotImplementedException();

    public HRESULT GetIcon(Shell32.IShellItemArray psiItemArray, out string ppszIcon) =>
        throw new NotImplementedException();

    public HRESULT GetToolTip(Shell32.IShellItemArray psiItemArray, out string ppszInfotip) =>
        throw new NotImplementedException();

    public HRESULT GetCanonicalName(out Guid pguidCommandName) => throw new NotImplementedException();

    public HRESULT
        GetState(Shell32.IShellItemArray psiItemArray, bool fOkToBeSlow, out Shell32.EXPCMDSTATE pCmdState) =>
        throw new NotImplementedException();

    public HRESULT Invoke(Shell32.IShellItemArray psiItemArray, IBindCtx pbc) => throw new NotImplementedException();

    public HRESULT GetFlags(out Shell32.EXPCMDFLAGS pFlags) => throw new NotImplementedException();

    public HRESULT EnumSubCommands(out Shell32.IEnumExplorerCommand ppEnum) => throw new NotImplementedException();

    public HRESULT SetSite(object pUnkSite) => throw new NotImplementedException();

    public HRESULT GetSite(in Guid riid, out object ppvSite) => throw new NotImplementedException();
}