using HarmonyLib;
using FoxCrypto;
using Miniscript;

[HarmonyPatch]
class PlayerIntrinsicsPatch
{
    private static bool intrinsicsAdded;
    [HarmonyPatch(typeof(PlayerIntrinsics), "AddInstrinsics")]
    static void Postfix()
    {
        if (intrinsicsAdded == true)
            return;
        intrinsicsAdded = true;
        Intrinsic intrinsic = Intrinsic.Create("aes");
        intrinsic.AddParam("data");
        intrinsic.AddParam("password");
        intrinsic.AddParam("decrypt", 0.0);
        intrinsic.code = delegate (TAC.Context context, Intrinsic.Result partialResult)
        {
            ValString valString = context.GetVar("data") as ValString;
            if (valString == null || string.IsNullOrEmpty(valString.value))
            {
                return new Intrinsic.Result("aes: Invalid data");
            }
            ValString valString1 = context.GetVar("password") as ValString;
            if (valString1 == null || string.IsNullOrEmpty(valString1.value) || valString1.value.Length < 3 || valString1.value.Length > 32)
            {
                return new Intrinsic.Result("aes: Password must be 3-32 characters");
            }
            ValNumber valNumber = context.GetVar("decrypt") as ValNumber;
            var output = CryptoInterface.Run(valString.value, valString1.value, valNumber.BoolValue() ? "dec" : "enc");
            if (output == null)
            {
                return new Intrinsic.Result("aes: Incorrect password or malformed data");
            }
            return new Intrinsic.Result(output);
        };
    }
}