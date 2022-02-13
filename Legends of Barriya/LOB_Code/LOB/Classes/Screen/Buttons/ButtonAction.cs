namespace LOB.Classes.Screen.Buttons
{
    /// <summary>
    /// Possible Actions. Used by all <see cref="IButtonEffect"/>s.
    /// </summary>
    internal enum ButtonAction
    {
        AddScreen, AddPopup, OpenPopupMenu, RemoveScreen, LoadMap, BackToMainMenu, None,
        BorderSwitch,
        PlaySoundEffect,
        ChangeVolume,
        SetPlayerRace,
        SetEnemyRace,
        ChangeLevel,
        BuildingMode,
        SaveGame,
        LoadSavedGame,
        GatherResource,
        OpenPortal,
        BuildUnit,
        ChooseName,
        SaveSettings,
        StopActions,
        SpecialAction,
        PotionAction,
        PhalanxAction,
        GodCreation,
        ReplaceScreen,
        RemoveBuilding
    }
}