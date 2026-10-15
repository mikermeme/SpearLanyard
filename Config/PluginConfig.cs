namespace SpearLanyard;

using BepInEx.Configuration;

using ComfyLib;

public static class PluginConfig {
  public static ConfigEntry<bool> IsModEnabled { get; private set; }
  public static ConfigEntry<int> ReservationDistance { get; private set; }
  public static ConfigEntry<int> ReservationSeconds { get; private set; }

  public static void BindConfig(ConfigFile config) {
    IsModEnabled =
        config.BindInOrder(
            "_Global",
            "isModEnabled",
            true,
            "Globally enable or disable this mod.");

    IsModEnabled.OnSettingChanged(LanyardManager.ClearReservations);

    ReservationDistance =
        config.BindInOrder(
            "Lanyard",
            "reservationDistance",
            30,
            "Spears you throw stay reserved for you until they are more than this many meters away from you; " +
            "if 0, this limit is off. If both this and reservationSeconds are 0, this mod does nothing.",
            new AcceptableValueRange<int>(0, 100));

    ReservationSeconds =
        config.BindInOrder(
            "Lanyard",
            "reservationSeconds",
            0,
            "Spears you throw stay reserved for you until this many seconds have passed since the throw; " +
            "if 0, this limit is off. If both this and reservationDistance are 0, this mod does nothing.",
            new AcceptableValueRange<int>(0, 60));
  }
}
