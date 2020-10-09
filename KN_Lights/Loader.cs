using BepInEx;
using KN_Core;

namespace KN_Lights {
  [BepInPlugin("trbflxr.kn_lights", "KN_Lights", StringVersion)]
  public class Loader : BaseUnityPlugin {
    public const int Version = 129;
    public const int Patch = 0;
    public const int ClientVersion = 273;
    public const string StringVersion = "1.2.9";

    public Loader() {
      Core.CoreInstance.AddMod(new Lights(Core.CoreInstance, Version, Patch, ClientVersion));
    }
  }
}