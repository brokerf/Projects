namespace LOB.Classes.Screen.Buttons
{
    /// <summary>
    /// An effect that can be activated with <see cref="Use"/>, returns an <see cref="ButtonAction"/> and an <see cref="object"/> filled with parameters.
    /// </summary>
    internal interface IButtonEffect
    {
        public (ButtonAction, object) Use();
    }
}