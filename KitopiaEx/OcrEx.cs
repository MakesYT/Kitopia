using Windows.Media.Ocr;

namespace KitopiaEx;

public class OcrEx
{
    private OcrEngine ocrEngine;

    public void InitOcr()
    {
        ocrEngine = OcrEngine.TryCreateFromUserProfileLanguages();
    }
}