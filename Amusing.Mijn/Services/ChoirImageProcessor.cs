using System.Text;
using SkiaSharp;
namespace Amusing.Mijn.Services;

public sealed record ChoirImageData(byte[] Bytes,string ContentType);
public static class ChoirImageProcessor
{
    public const int PhotoLimit=16*1024*1024, LogoLimit=8*1024*1024;
    public static bool IsKind(string kind)=>kind is "foto" or "logo";
    public static int Limit(string kind)=>kind=="logo"?LogoLimit:PhotoLimit;
    public static string SizeMessage(long bytes,string kind)
    {
        var culture=System.Globalization.CultureInfo.GetCultureInfo("nl-NL");
        return string.Format(culture,"Het bestand is {0:N0} bytes ({1:N2} MB; {2:N2} MiB). Voor {3} is maximaal {4:N0} bytes ({5} MiB) toegestaan. Verklein het bestand en probeer opnieuw.",bytes,bytes/1_000_000d,bytes/1048576d,kind=="logo"?"een logo":"een koorfoto",Limit(kind),kind=="logo"?8:16);
    }
    private static InvalidDataException Invalid(string message)
    {
        var error=new InvalidDataException(message);error.Data["UserMessage"]=message;return error;
    }
    public static ChoirImageData Prepare(byte[] bytes,string kind)
    {
        if(!IsKind(kind) || bytes.Length==0) throw Invalid("Kies een niet-leeg afbeeldingsbestand.");
        if(bytes.Length>Limit(kind)) throw Invalid(SizeMessage(bytes.Length,kind));
        using var data=SKData.CreateCopy(bytes);
        using var codec=SKCodec.Create(data);
        if(codec is null || codec.EncodedFormat is not (SKEncodedImageFormat.Png or SKEncodedImageFormat.Jpeg or SKEncodedImageFormat.Webp) || codec.FrameCount>1) throw Invalid("Dit bestand is niet leesbaar als stilstaande PNG-, JPEG- of WebP-afbeelding. Kies een ander bestand of exporteer de afbeelding opnieuw.");
        int w=codec.Info.Width,h=codec.Info.Height;
        if(w<=0 || h<=0) throw Invalid("De afmetingen van de afbeelding zijn ongeldig.");
        if((long)w*h>25_000_000)
        {
            var culture=System.Globalization.CultureInfo.GetCultureInfo("nl-NL");
            double scale=Math.Min(Math.Sqrt(25_000_000d/((long)w*h)),12000d/Math.Max(w,h));
            int suggestedW=Math.Max(1,(int)(w*scale)),suggestedH=Math.Max(1,(int)(h*scale));
            throw Invalid(string.Format(culture,"Deze afbeelding bevat {0:N0} pixels: {1} breed × {2} hoog. Dat is meer dan de toegestane 25.000.000 pixels in totaal. Verklein de foto bijvoorbeeld naar {3} × {4} pixels en probeer opnieuw.",(long)w*h,w,h,suggestedW,suggestedH));
        }
        if(w>12000 || h>12000) throw Invalid($"Een zijde van deze afbeelding is te lang ({w} × {h} pixels). Elke zijde mag maximaal 12000 pixels zijn. Verklein de afmetingen en probeer opnieuw.");
        using var bitmap=new SKBitmap(new SKImageInfo(w,h,SKColorType.Rgba8888,SKAlphaType.Premul));
        if(codec.GetPixels(bitmap.Info,bitmap.GetPixels())!=SKCodecResult.Success) throw Invalid("De afbeelding kon niet volledig worden gelezen en is mogelijk beschadigd. Exporteer haar opnieuw als PNG, JPEG of WebP.");
        bool swapped=codec.EncodedOrigin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;
        int orientedW=swapped?h:w,orientedH=swapped?w:h,max=kind=="logo"?1024:2560;
        double ratio=Math.Min(1,(double)max/Math.Max(orientedW,orientedH));
        int width=Math.Max(1,(int)Math.Round(orientedW*ratio)),height=Math.Max(1,(int)Math.Round(orientedH*ratio));
        using var output=new SKBitmap(width,height,SKColorType.Rgba8888,SKAlphaType.Premul);
        using(var canvas=new SKCanvas(output))
        {
            canvas.Clear(kind=="logo"?SKColors.Transparent:SKColors.White);
            canvas.Scale((float)width/orientedW,(float)height/orientedH);
            switch(codec.EncodedOrigin)
            {
                case SKEncodedOrigin.TopRight: canvas.Translate(w,0);canvas.Scale(-1,1);break;
                case SKEncodedOrigin.BottomRight: canvas.Translate(w,h);canvas.RotateDegrees(180);break;
                case SKEncodedOrigin.BottomLeft: canvas.Translate(0,h);canvas.Scale(1,-1);break;
                case SKEncodedOrigin.LeftTop: canvas.RotateDegrees(90);canvas.Scale(1,-1);break;
                case SKEncodedOrigin.RightTop: canvas.Translate(h,0);canvas.RotateDegrees(90);break;
                case SKEncodedOrigin.RightBottom: canvas.Translate(h,w);canvas.RotateDegrees(90);canvas.Scale(-1,1);break;
                case SKEncodedOrigin.LeftBottom: canvas.Translate(0,w);canvas.RotateDegrees(270);break;
            }
            using var image=SKImage.FromBitmap(bitmap);
            canvas.DrawImage(image,new SKRect(0,0,w,h),new SKSamplingOptions(SKFilterMode.Linear));
        }
        using var normalized=SKImage.FromBitmap(output);
        using var encoded=normalized.Encode(kind=="logo"?SKEncodedImageFormat.Png:SKEncodedImageFormat.Jpeg,90);
        if(encoded is null || encoded.Size>12*1024*1024) throw Invalid("De afbeelding blijft na verwerking te groot (maximaal 12 MiB). Verklein de afbeelding of kies een eenvoudiger bestand.");
        return new(encoded.ToArray(),kind=="logo"?"image/png":"image/jpeg");
    }
    public static ChoirImageData? ReadLegacy(byte[]? stored)
    {
        if(stored is null || stored.Length==0 || stored.Length>PhotoLimit) return null;
        var bytes=stored;
        if(Mime(bytes) is null)
        {
            var text=Encoding.UTF8.GetString(bytes).Trim();
            if(text.StartsWith("data:image/",StringComparison.OrdinalIgnoreCase))
            {
                var comma=text.IndexOf(',');
                if(comma<0 || !text[..comma].EndsWith(";base64",StringComparison.OrdinalIgnoreCase)) return null;
                text=text[(comma+1)..];
            }
            try { bytes=Convert.FromBase64String(text); } catch(FormatException) { return null; }
        }
        var mime=Mime(bytes);
        if(mime is null) return null;
        using var data=SKData.CreateCopy(bytes);using var codec=SKCodec.Create(data);
        if(codec is null || codec.Info.Width<=0 || codec.Info.Height<=0 || (long)codec.Info.Width*codec.Info.Height>25_000_000) return null;
        return new(bytes,mime);
    }
    private static string? Mime(byte[] b)
    {
        if(b.AsSpan().StartsWith(new byte[]{137,80,78,71,13,10,26,10})) return "image/png";
        if(b.Length>=3 && b[0]==255 && b[1]==216 && b[2]==255) return "image/jpeg";
        if(b.AsSpan().StartsWith("GIF87a"u8) || b.AsSpan().StartsWith("GIF89a"u8)) return "image/gif";
        if(b.Length>=12 && b.AsSpan(0,4).SequenceEqual("RIFF"u8) && b.AsSpan(8,4).SequenceEqual("WEBP"u8)) return "image/webp";
        return null;
    }
}
