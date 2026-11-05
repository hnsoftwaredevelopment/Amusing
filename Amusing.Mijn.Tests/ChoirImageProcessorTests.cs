using Amusing.Mijn.Services;
using SkiaSharp;
using Xunit;
namespace Amusing.Mijn.Tests;
public class ChoirImageProcessorTests
{
    public static byte[] Png(int width=40,int height=20)
    {
        using var bitmap=new SKBitmap(width,height); using var canvas=new SKCanvas(bitmap); canvas.Clear(SKColors.Red);
        using var image=SKImage.FromBitmap(bitmap); using var data=image.Encode(SKEncodedImageFormat.Png,100); return data.ToArray();
    }
    [Fact] public void PhotosAreLargerThanLegacyButBoundedAndDoNotStretch()
    {
        var result=ChoirImageProcessor.Prepare(Png(3000,1500),"foto");
        using var bitmap=SKBitmap.Decode(result.Bytes);
        Assert.Equal(2560,bitmap.Width); Assert.Equal(1280,bitmap.Height); Assert.Equal("image/jpeg",result.ContentType);
    }
    [Fact] public void LogosPreserveTransparencyAndSmallImagesAreNotEnlarged()
    {
        using var bitmap=new SKBitmap(20,10);bitmap.Erase(SKColors.Transparent);
        using var image=SKImage.FromBitmap(bitmap);using var encoded=image.Encode(SKEncodedImageFormat.Png,100);
        var result=ChoirImageProcessor.Prepare(encoded.ToArray(),"logo");
        using var decoded=SKBitmap.Decode(result.Bytes);
        Assert.Equal(20,decoded.Width); Assert.Equal(0,decoded.GetPixel(0,0).Alpha); Assert.Equal("image/png",result.ContentType);
    }
    [Fact] public void InvalidOrExcessiveInputIsRejected()
    {
        Assert.Throws<InvalidDataException>(()=>ChoirImageProcessor.Prepare("<svg/>"u8.ToArray(),"foto"));
        Assert.Throws<InvalidDataException>(()=>ChoirImageProcessor.Prepare(new byte[8*1024*1024+1],"logo"));
        Assert.Throws<InvalidDataException>(()=>ChoirImageProcessor.Prepare(Png(6000,5000),"foto"));
    }
    [Fact] public void SizeAndPixelErrorsExplainTheActualLimit()
    {
        var size=Assert.Throws<InvalidDataException>(()=>ChoirImageProcessor.Prepare(new byte[8*1024*1024+1],"logo"));
        Assert.Contains("MiB",size.Message);
        Assert.Contains("8.388.608",size.Message);
        var pixels=Assert.Throws<InvalidDataException>(()=>ChoirImageProcessor.Prepare(Png(6000,5000),"foto"));
        Assert.Contains("6000 × 5000",pixels.Message);
        Assert.Contains("30",pixels.Message);
        Assert.Contains("25 miljoen",pixels.Message);
        Assert.Equal(pixels.Message,pixels.Data["UserMessage"]);
    }
    [Fact] public void LegacyBinaryBase64AndDataUrlsRemainReadable()
    {
        var png=Png();
        Assert.Equal(png,ChoirImageProcessor.ReadLegacy(png)!.Bytes);
        Assert.Equal(png,ChoirImageProcessor.ReadLegacy(System.Text.Encoding.UTF8.GetBytes(Convert.ToBase64String(png)))!.Bytes);
        Assert.Equal(png,ChoirImageProcessor.ReadLegacy(System.Text.Encoding.UTF8.GetBytes("data:image/png;base64,"+Convert.ToBase64String(png)))!.Bytes);
        Assert.Null(ChoirImageProcessor.ReadLegacy("<svg/>"u8.ToArray()));
    }
    [Fact] public void JpegOrientationIsAppliedAndMetadataIsRemoved()
    {
        using var bitmap=new SKBitmap(8,4);bitmap.Erase(SKColors.Red);
        using var image=SKImage.FromBitmap(bitmap);using var jpeg=image.Encode(SKEncodedImageFormat.Jpeg,90);
        var bytes=jpeg.ToArray();
        var exif=Convert.FromHexString("FFE100224578696600004D4D002A00000008000101120003000000010006000000000000");
        var fixture=bytes[..2].Concat(exif).Concat(bytes[2..]).ToArray();
        var result=ChoirImageProcessor.Prepare(fixture,"foto");
        using var decoded=SKBitmap.Decode(result.Bytes);
        Assert.Equal(4,decoded.Width);Assert.Equal(8,decoded.Height);
        using var data=SKData.CreateCopy(result.Bytes);using var codec=SKCodec.Create(data);
        Assert.Equal(SKEncodedOrigin.TopLeft,codec.EncodedOrigin);
    }
}
