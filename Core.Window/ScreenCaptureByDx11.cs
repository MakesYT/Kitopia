using System.Buffers;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Core.SDKs.Services;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using PluginCore;
using Silk.NET.Core.Contexts;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Pbm;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Qoi;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using IScreenCapture = PluginCore.IScreenCapture;

namespace Core.SDKs.Tools;

public class ScreenCaptureByDx11 : IScreenCapture
{
    private static readonly Lazy<Configuration> Lazy = new(CreateDefaultInstance);
    public static Configuration Configuration => Lazy.Value;
    private static readonly ILog log = LogManager.GetLogger(nameof(ScreenCaptureByDx11));

    private static Configuration CreateDefaultInstance()
    {
        return new Configuration(
            new PngConfigurationModule(),
            new JpegConfigurationModule(),
            new GifConfigurationModule(),
            new BmpConfigurationModule(),
            new PbmConfigurationModule(),
            new TgaConfigurationModule(),
            new TiffConfigurationModule(),
            new WebpConfigurationModule(),
            new QoiConfigurationModule())
        {
            PreferContiguousImageBuffers = true
        };
    }

    public class DisposableTool(Action busySetter) : IDisposable
    {
        public void Dispose() => busySetter.Invoke();
    }

    public (Queue<Bitmap>, Queue<Bitmap>) CaptureAllScreen()
    {
        Queue<Bitmap> bitmaps = new();
        Queue<Bitmap> mosaics = new();
        unsafe
        {
            DXGI dxgi = new DXGI(new DefaultNativeContext("dxgi"));
            ComPtr<IDXGIFactory1> factory = null;
            IDXGIAdapter1* adapter1 = null;
            ID3D11Device* device = null;
            ID3D11DeviceContext* context = null;
            ID3D11DeviceContext* immediateContext = null;
            D3D11 d3D11 = new D3D11(new DefaultNativeContext("d3d11"));
            try
            {
                if (dxgi.CreateDXGIFactory1<IDXGIFactory1>(out factory) != 0)
                {
                    throw new Exception("Failed to create DXGI factory");
                }

                if (factory.EnumAdapters1(0, ref adapter1) != 0)
                {
                    throw new Exception("Failed to create DXGI adapter");
                }

                D3DFeatureLevel featureLevel = D3DFeatureLevel.Level101;
                D3DFeatureLevel[] featureLevels =
                [
                    D3DFeatureLevel.Level111, D3DFeatureLevel.Level110, D3DFeatureLevel.Level101,
                    D3DFeatureLevel.Level100
                ];

                fixed (D3DFeatureLevel* pFeatureLevels = &featureLevels[0])
                {
                    if (d3D11.CreateDevice((IDXGIAdapter*)adapter1, D3DDriverType.Unknown, IntPtr.Zero,
                            (uint)CreateDeviceFlag.None, pFeatureLevels, (uint)featureLevels.Length, D3D11.SdkVersion,
                            ref device,
                            &featureLevel, ref context) != 0)
                    {
                        throw new Exception("Failed to create D3D11 device");
                    }
                }

                device->GetImmediateContext(ref immediateContext);

                uint i = 0;
                IDXGIOutput* output = null;
                while (adapter1->EnumOutputs(i, ref output) == 0)
                {
                    i++;
                    IDXGIOutputDuplication* outputDuplication = null;
                    IDXGIResource* desktopResource = null;
                    ID3D11Texture2D* stagingTexture = null;
                    ComPtr<IDXGIOutput5> output5 = null;
                    ComPtr<ID3D11Resource> desktopTexture = null;
                    ComPtr<ID3D11Resource> stagingResource = null;
                    try
                    {
                        OutputDesc desc = new OutputDesc(null);
                        if (output->GetDesc(ref desc) != 0)
                        {
                            throw new Exception("Failed to get output description");
                        }

                        if (output->QueryInterface<IDXGIOutput5>(out output5) != 0)
                        {
                            throw new Exception("Failed to get IDXGIOutput5");
                        }


                        if (output5.DuplicateOutput((IUnknown*)device, ref outputDuplication) != 0)
                        {
                            throw new Exception("Failed to get output duplication");
                        }

                        OutduplFrameInfo outduplFrameInfo = new OutduplFrameInfo();
                        Thread.Sleep(10);
                        if (outputDuplication->AcquireNextFrame(2000, &outduplFrameInfo, &desktopResource) != 0 ||
                            outduplFrameInfo.LastPresentTime == 0)
                        {
                            throw new Exception("Failed to acquire next frame");
                        }

                        if (desktopResource->QueryInterface<ID3D11Resource>(out desktopTexture) != 0)
                        {
                            throw new Exception("Failed to get desktop texture");
                        }

                        Texture2DDesc stagingTextureDesc = new()
                        {
                            CPUAccessFlags = (uint)CpuAccessFlag.Read,
                            BindFlags = (uint)(BindFlag.None),
                            Format = Format.FormatB8G8R8A8Unorm,
                            Width = (uint)desc.DesktopCoordinates.Size.X,
                            Height = (uint)desc.DesktopCoordinates.Size.Y,
                            MiscFlags = (uint)ResourceMiscFlag.None,
                            MipLevels = 1,
                            ArraySize = 1,
                            SampleDesc = { Count = 1, Quality = 0 },
                            Usage = Usage.Staging
                        };

                        if (device->CreateTexture2D(&stagingTextureDesc, null, ref stagingTexture) != 0)
                        {
                            throw new Exception("Failed to create staging texture");
                        }

                        stagingTexture->QueryInterface<ID3D11Resource>(out stagingResource);
                        immediateContext->CopyResource(stagingResource, desktopTexture);

                        MappedSubresource mappedSubresource = new MappedSubresource();
                        if (immediateContext->Map(stagingResource, 0, Map.Read, 0, &mappedSubresource) != 0)
                        {
                            throw new Exception("Failed to map staging texture");
                        }

                        var span = new ReadOnlySpan<byte>(mappedSubresource.PData,
                            (int)mappedSubresource.DepthPitch);
                        immediateContext->Unmap(stagingResource, 0);
                        outputDuplication->ReleaseFrame();
                        var loadPixelData = Image.LoadPixelData<Bgra32>(Configuration, span,
                            desc.DesktopCoordinates.Size.X, desc.DesktopCoordinates.Size.Y);
                        span = null;

                        if (!loadPixelData.DangerousTryGetSinglePixelMemory(out Memory<Bgra32> memory))
                        {
                            throw new Exception(
                                "This can only happen with multi-GB images or when PreferContiguousImageBuffers is not set to true.");
                        }

                        using (MemoryHandle pinHandle = memory.Pin())
                        {
                            var bitmap1 = new Bitmap(PixelFormat.Bgra8888, AlphaFormat.Unpremul,
                                (IntPtr)pinHandle.Pointer,
                                new PixelSize(desc.DesktopCoordinates.Size.X, desc.DesktopCoordinates.Size.Y),
                                new Vector(96, 96),
                                (desc.DesktopCoordinates.Size.Y * PixelFormat.Bgra8888.BitsPerPixel + 7) / 8);
                            bitmaps.Enqueue(bitmap1);
                        }

                        loadPixelData.Mutate(x => x.BoxBlur(10));

                        if (!loadPixelData.DangerousTryGetSinglePixelMemory(out Memory<Bgra32> memory1))
                        {
                            throw new Exception(
                                "This can only happen with multi-GB images or when PreferContiguousImageBuffers is not set to true.");
                        }

                        using (MemoryHandle pinHandle = memory1.Pin())
                        {
                            var bitmap1 = new Bitmap(PixelFormat.Bgra8888, AlphaFormat.Unpremul,
                                (IntPtr)pinHandle.Pointer,
                                new PixelSize(desc.DesktopCoordinates.Size.X, desc.DesktopCoordinates.Size.Y),
                                new Vector(96, 96),
                                (desc.DesktopCoordinates.Size.Y * PixelFormat.Bgra8888.BitsPerPixel + 7) / 8);
                            mosaics.Enqueue(bitmap1);
                        }

                        loadPixelData.Dispose();
                    }
                    catch (Exception e)
                    {
                        log.Error(e);
                    }
                    finally
                    {
                        output->Release();
                        outputDuplication->Release();
                        desktopResource->Release();
                        stagingTexture->Release();
                        output5.Release();
                        desktopTexture.Release();
                        stagingResource.Release();
                        output = null;
                        outputDuplication = null;
                        desktopResource = null;
                        stagingTexture = null;
                        output5 = null;
                        desktopTexture = null;
                        stagingResource = null;
                    }
                }
            }
            finally
            {
                dxgi.Dispose();
                d3D11.Dispose();
                factory.Dispose();
                adapter1->Release();
                device->Release();
                context->Release();
                immediateContext->Release();
                dxgi = null;
                d3D11 = null;
                factory = null;
                adapter1 = null;
                device = null;
                context = null;
                immediateContext = null;
            }
        }


        return (bitmaps, mosaics);
    }

    public (Bitmap?, Bitmap?)? CaptureScreen(ScreenCaptureInfo screenCaptureInfo, bool withMosaic = false)
    {
        return (null, null);
    }

    public ScreenCaptureInfo GetScreenCaptureInfoByUserManual()
    {
        return ServiceManager.Services.GetService<IScreenCaptureWindow>()!.GetScreenCaptureInfo()
                             .Result;
    }
}