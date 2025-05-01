using HarmonyLib;
using Miniscript;
using System.Reflection;

[HarmonyPatch]
public class GreyInterpreterPatch
{
    [HarmonyPatch(typeof(GreyInterpreter), "CryptoLibType")]
    class CryptoLibTypePatch
    {
        static void Postfix(GreyInterpreter __instance, ref GreyMap __result)
        {
            FieldInfo fieldInfo = AccessTools.Field(typeof(GreyInterpreter), "_cryptoLibType");
            GreyMap _cryptoLibType = fieldInfo.GetValue(__instance) as GreyMap;
            if (!_cryptoLibType.ContainsKey("aes"))
            {
                _cryptoLibType["aes"] = Intrinsic.GetByName("aes").GetFunc();
            }
            fieldInfo.SetValue(__instance, _cryptoLibType);
            __result = _cryptoLibType;
        }
    }
}