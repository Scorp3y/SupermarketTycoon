using UnityEngine;
using UnityEngine.Localization.Settings;
using TMPro;

public class LanguageSelector : MonoBehaviour
{
    [Header("Font Settings")]
    public TMP_FontAsset englishFont;
    public TMP_FontAsset russianFont;

    [Header("Font Sizes")]
    public float englishFontSize = 80f;
    public float russianFontSize = 50f;

    [Header("Target Texts")]
    public TMP_Text[] localizedTexts;

    public void SetLanguageByCode(string code)
    {
        var locale = LocalizationSettings.AvailableLocales.Locales
            .Find(x => x.Identifier.Code == code);

        if (locale != null)
        {
            LocalizationSettings.SelectedLocale = locale;
            PlayerPrefs.SetString("lang", code);
            ApplyFontSettings(code);
        }
    }

    void Start()
    {
        string savedLang = PlayerPrefs.GetString("lang", "");
        if (string.IsNullOrEmpty(savedLang))
        {
            string systemLang = Application.systemLanguage.ToString().ToLower();
            savedLang = systemLang.StartsWith("ru") ? "ru" : "en";
            PlayerPrefs.SetString("lang", savedLang);
        }

        SetLanguageByCode(savedLang);
    }

    private void ApplyFontSettings(string code)
    {
        TMP_FontAsset font = code == "ru" ? russianFont : englishFont;
        float fontSize = code == "ru" ? russianFontSize : englishFontSize;

        foreach (var text in localizedTexts)
        {
            if (text != null)
            {
                text.font = font;
                text.fontSize = fontSize;
            }
        }
    }
}
