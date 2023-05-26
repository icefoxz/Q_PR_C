using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting;

[Preserve]
public class SkipUnityLogo
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    private static void BeforeSplashScreen() => Task.Run(AsyncSkip);
    private static void AsyncSkip() => SplashScreen.Stop(SplashScreen.StopBehavior.StopImmediate);
}