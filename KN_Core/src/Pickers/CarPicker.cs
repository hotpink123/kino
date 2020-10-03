using System.Collections.Generic;
using System.Linq;
using KN_Loader;
using SyncMultiplayer;
using Object = UnityEngine.Object;

namespace KN_Core {
  public class CarPicker {
    public bool IsPicking { get; set; }

    public KnCar PickedCar { get; private set; }
    public KnCar PlayerCar { get; private set; }

    public List<KnCar> Cars { get; }

    public int NwCarsCount => loadingCars_.Count;

    public delegate void CarLoadCallback();
    public event CarLoadCallback OnCarLoaded;

    private float carsListHeight_;

    private readonly List<LoadingCar> loadingCars_;

    public CarPicker() {
      Cars = new List<KnCar>(16);
      loadingCars_ = new List<LoadingCar>(16);
    }

    public void Reset() {
      IsPicking = false;
      PickedCar = null;
    }

    public void Toggle() {
      IsPicking = !IsPicking;
      if (!IsPicking) {
        Reset();
      }
    }

    public void OnGui(Gui gui, ref float x, ref float y) {
      if (!IsPicking) {
        return;
      }

      const float boxWidth = Gui.Width + Gui.OffsetGuiX * 2.0f;
      const float width = Gui.Width;
      const float height = Gui.Height;

      float yBegin = y;

      GuiCars(gui, ref x, ref y, boxWidth, width, height);
      carsListHeight_ = y - yBegin - Gui.Height;

      x += Gui.Width + Gui.OffsetGuiX;
      y = yBegin;
    }

    private void GuiCars(Gui gui, ref float x, ref float y, float boxWidth, float width, float height) {
      gui.Box(x, y, boxWidth, Gui.Height, $"{Locale.Get("cp_cars")} / {loadingCars_.Count}", Skin.MainContainerDark);
      y += Gui.Height;

      gui.Box(x, y, boxWidth, carsListHeight_, Skin.MainContainer);
      y += Gui.OffsetY;
      x += Gui.OffsetGuiX;

      if (gui.Button(ref x, ref y, width, height, Locale.Get("cp_player"), Skin.Button)) {
        PickedCar = PlayerCar;
      }

      if (Cars.Count > 0) {
        gui.Line(x, y, width, 1.0f, Skin.SeparatorColor);
        y += Gui.OffsetY;

        foreach (var c in Cars) {
          if (gui.Button(ref x, ref y, width, height, c.Name, Skin.Button)) {
            PickedCar = c;
          }
        }
      }
    }

    public void Update() {
      if (KnCar.IsNull(PlayerCar)) {
        FindPlayerCar();
      }

      Cars.RemoveAll(KnCar.IsNull);

      loadingCars_.RemoveAll(car => {
        if (car.Loaded && (car.Player == null || car.Player.userCar == null)) {
          Log.Write($"[KN_Core::CarPicker]: Removed car: '{car.Player?.FilteredNickName}' ({car.Player?.NetworkID})");
          return true;
        }
        return false;
      });

      var nwPlayers = NetworkController.InstanceGame?.Players;
      if (nwPlayers != null) {
        if (loadingCars_.Count != nwPlayers.Count) {
          foreach (var player in nwPlayers) {
            if (loadingCars_.All(c => c.Player != player)) {
              loadingCars_.Add(new LoadingCar {Player = player, Loading = true});
              Log.Write($"[KN_Core::CarPicker]: Added car to load: '{player.FilteredNickName}' ({player.NetworkID})");
            }
          }
        }

        foreach (var car in loadingCars_) {
          if (car.Player == null) {
            continue;
          }

          if (car.Player.IsCarLoading()) {
            car.Loading = true;
          }
          else if (car.Loading && !car.Player.ugcCarReadyForConstruction) {
            car.Loaded = true;
            car.Loading = false;
            if (car.Player.userCar == null) {
              continue;
            }

            Cars.Add(new KnCar(car.Player.userCar));
            Log.Write($"[KN_Core::CarPicker]: Car loaded: '{car.Player.FilteredNickName}' ({car.Player.NetworkID})");
            OnCarLoaded?.Invoke();
          }
        }
      }
    }

    private void FindPlayerCar() {
      PlayerCar = null;
      var cars = Object.FindObjectsOfType<RaceCar>();
      if (cars != null && cars.Length > 0) {
        foreach (var c in cars) {
          if (!c.isNetworkCar) {
            PlayerCar = new KnCar(c);
            return;
          }
        }
      }
    }
  }
}