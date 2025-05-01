using HarmonyLib;
using System.Collections.Generic;

[HarmonyPatch]
public class PlayerUtilsPatch
{
    [HarmonyPatch(typeof(PlayerUtils), "ConfigLanguage")]
    class ConfigLanguagePatch
    {
        private static Dictionary<string, string> contents = new Dictionary<string, string>()
        {
            { "DOC_CRYPTO_AES", "<mark=#00D0124D><b>[Parameters]</b></mark>\n[1] <color=#33cccc>string</color> data\n[2] <color=#33cccc>string</color> password\n[3] <color=#33cccc>bool</color> decrypt = <color=orange>false</color>\n\n<mark=#00D0124D><b>[Description]</b></mark>\nAES256 cryptography system.\nBased on direction used, returns a <color=#33cccc>string</color> containing the encrypted (0) or decrypted (1) data.\nIn case of an error returns a <color=#33cccc>string</color> containing the error." }
        };

        static void Postfix()
        {
            foreach (var content in contents)
            {
                TranslationSystem.Singleton.rootTexts["English"].content.Add(content.Key, content.Value);
            }
        }
    }
}