using SyncMultiplayer;
using UnityEngine;

namespace KN_Core.Submodule {
  public class Settings : BaseMod {
    private bool rPoints_;
    public bool RPoints {
      get => rPoints_;
      set {
        rPoints_ = value;
        Core.ModConfig.Set("r_points", value);
        GameConsole.Bool["r_points"] = value;
        GameConsole.UpdatePoints();
      }
    }

    private bool hideNames_;
    public bool HideNames {
      get => hideNames_;
      set {
        hideNames_ = value;
        Core.ModConfig.Set("hide_names", value);
      }
    }

    private int prevPlayersCount_;
    private readonly Exhaust exhaust_;

    public Settings(Core core) : base(core, "SETTINGS", int.MaxValue - 1) {
      exhaust_ = new Exhaust(core);
    }

    public void Awake() {
      RPoints = Core.ModConfig.Get<bool>("r_points");
      HideNames = Core.ModConfig.Get<bool>("hide_names");
    }

    public override void OnStop() {
      Core.ModConfig.Set("r_points", RPoints);
      Core.ModConfig.Set("hide_names", HideNames);
    }

    public override void Update(int id) {
      int players = NetworkController.InstanceGame.Players.Count;
      // if ((prevPlayersCount_ != players && !Core.IsInGarage) || !exhaust_.IsInitialized) {
      // exhaust_.Initialize();
      // }

      if (Input.GetKeyDown(KeyCode.Delete)) {
        exhaust_.Initialize();
      }

      prevPlayersCount_ = players;
      if (!Core.IsInGarage) {
        exhaust_.Update();
      }
    }

    public override void OnGUI(int id, Gui gui, ref float x, ref float y) {
      const float width = Gui.Width * 1.4f;
      const float height = Gui.Height;

      x += Gui.OffsetSmall;

      if (gui.Button(ref x, ref y, width, height, "HIDE POINTS", RPoints ? Skin.Button : Skin.ButtonActive)) {
        RPoints = !RPoints;
      }

      if (gui.Button(ref x, ref y, width, height, "HIDE NAMES", HideNames ? Skin.ButtonActive : Skin.Button)) {
        HideNames = !HideNames;
      }

      gui.Box(x, y, width, height, "There will be more buttons...", Skin.MainContainerLeft);
      y += Gui.Height;

      gui.Box(x, y, width, height, "In the future", Skin.MainContainerLeft);
      y += Gui.Height;

      y += Gui.OffsetY;
      exhaust_.OnGUI(gui, ref x, ref y);
    }
  }
}