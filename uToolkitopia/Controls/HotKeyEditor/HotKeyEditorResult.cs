namespace Kitopia.Controls.HotKeyEditor
{
    /// <summary>
    /// Specifies identifiers to indicate the return value of a <see cref="HotKeyEditor"/>.
    /// </summary>
    public enum HotKeyEditorResult
    {
        /// <summary>
        /// No button was tapped.
        /// </summary>
        None,

        /// <summary>
        /// The primary button was tapped by the user.
        /// </summary>
        Primary,

        /// <summary>
        /// The secondary button was tapped by the user.
        /// </summary>
        Secondary
    }
}