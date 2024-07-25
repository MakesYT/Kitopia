#nullable disable
using System;
using Avalonia;
using Avalonia.Data;
using Avalonia.Input;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Input;

namespace KitopiaAvalonia.Controls
{
    /// <summary>
    /// Derives from XamlUICommand, adding a set of standard platform commands with pre-defined properties.
    /// </summary>
    public class StandardUICommand : XamlUICommand
    {
        /// <summary>
        /// Defines the <see cref="P:KitopiaAvalonia.Controls.StandardUICommand.Kind" /> property
        /// </summary>
        public static readonly StyledProperty<StandardUICommandKind> KindProperty =
            AvaloniaProperty.Register<StandardUICommand, StandardUICommandKind>(nameof(Kind),
                StandardUICommandKind.None, false, BindingMode.OneWay, (Func<StandardUICommandKind, bool>)null,
                (Func<AvaloniaObject, StandardUICommandKind, StandardUICommandKind>)null, false);

        public StandardUICommand()
        {
        }

        public StandardUICommand(StandardUICommandKind kind)
        {
            this.Kind = kind;
            this.SetupCommand();
        }

        /// <summary>
        /// Gets the platform command (with pre-defined properties such as icon, keyboard accelerator,
        /// and description) that can be used with a StandardUICommand.
        /// </summary>
        public StandardUICommandKind Kind
        {
            get => this.GetValue<StandardUICommandKind>(StandardUICommand.KindProperty);
            set
            {
                this.SetValue<StandardUICommandKind>(StandardUICommand.KindProperty, value, BindingPriority.LocalValue);
            }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (!(change.Property == (AvaloniaProperty)StandardUICommand.KindProperty))
                return;
            this.SetupCommand();
        }

        private void SetupCommand()
        {
            switch (this.Kind)
            {
                case StandardUICommandKind.None:
                    this.Label = string.Empty;
                    this.IconSource = (IconSource)null;
                    this.Description = string.Empty;
                    this.HotKey = (KeyGesture)null;
                    break;
                case StandardUICommandKind.Cut:
                    this.Label = "剪贴";
                    this.IconSource = (IconSource)new SymbolIconSource()
                    {
                        Symbol = Symbol.Cut
                    };
                    this.Description = "Remove the selected content and put it on the clipboard";
                    this.HotKey = new KeyGesture(Key.X, KeyModifiers.Control);
                    break;
                case StandardUICommandKind.Copy:
                    this.Label = "Copy";
                    this.IconSource = (IconSource)new SymbolIconSource()
                    {
                        Symbol = Symbol.Copy
                    };
                    this.Description = "Copy the selected content to the clipboard";
                    this.HotKey = new KeyGesture(Key.C, KeyModifiers.Control);
                    break;
                case StandardUICommandKind.Paste:
                    this.Label = "Paste";
                    this.IconSource = (IconSource)new SymbolIconSource()
                    {
                        Symbol = Symbol.Paste
                    };
                    this.Description = "Insert the contents of the clipboard at the current location";
                    this.HotKey = new KeyGesture(Key.V, KeyModifiers.Control);
                    break;
                case StandardUICommandKind.SelectAll:
                    this.Label = "Select All";
                    this.IconSource = (IconSource)new SymbolIconSource()
                    {
                        Symbol = Symbol.SelectAll
                    };
                    this.Description = "Select all content";
                    this.HotKey = new KeyGesture(Key.A, KeyModifiers.Control);
                    break;
                case StandardUICommandKind.Delete:
                    this.Label = "Delete";
                    this.IconSource = (IconSource)new SymbolIconSource()
                    {
                        Symbol = Symbol.Delete
                    };
                    this.Description = "Delete the selected content";
                    this.HotKey = new KeyGesture(Key.Delete);
                    break;
                case StandardUICommandKind.Share:
                    this.Label = "Share";
                    this.IconSource = (IconSource)new SymbolIconSource()
                    {
                        Symbol = Symbol.Share
                    };
                    this.Description = "Share the selected content";
                    break;
                case StandardUICommandKind.Save:
                    this.Label = "Save";
                    this.IconSource = (IconSource)new SymbolIconSource()
                    {
                        Symbol = Symbol.Save
                    };
                    this.Description = "Save your changes";
                    this.HotKey = new KeyGesture(Key.S, KeyModifiers.Control);
                    break;
                case StandardUICommandKind.Open:
                    this.Label = this.Description = "Open";
                    this.IconSource = (IconSource)new SymbolIconSource()
                    {
                        Symbol = Symbol.Open
                    };
                    this.HotKey = new KeyGesture(Key.O, KeyModifiers.Control);
                    break;
                case StandardUICommandKind.Close:
                    this.Label = this.Description = "Close";
                    this.IconSource = (IconSource)new SymbolIconSource()
                    {
                        Symbol = Symbol.Dismiss
                    };
                    this.HotKey = new KeyGesture(Key.W, KeyModifiers.Control);
                    break;
                case StandardUICommandKind.Pause:
                    this.Label = this.Description = "Pause";
                    this.IconSource = (IconSource)new SymbolIconSource()
                    {
                        Symbol = Symbol.Pause
                    };
                    break;
                case StandardUICommandKind.Play:
                    this.Label = this.Description = "Play";
                    this.IconSource = (IconSource)new SymbolIconSource()
                    {
                        Symbol = Symbol.Play
                    };
                    break;
                case StandardUICommandKind.Stop:
                    this.Label = this.Description = "Stop";
                    this.IconSource = (IconSource)new SymbolIconSource()
                    {
                        Symbol = Symbol.Stop
                    };
                    break;
                case StandardUICommandKind.Forward:
                    this.Label = "Forward";
                    this.IconSource = (IconSource)new SymbolIconSource()
                    {
                        Symbol = Symbol.Forward
                    };
                    this.Description = "Go to the next item";
                    break;
                case StandardUICommandKind.Backward:
                    this.Label = "Backward";
                    this.IconSource = (IconSource)new SymbolIconSource()
                    {
                        Symbol = Symbol.Back
                    };
                    this.Description = "Back";
                    break;
                case StandardUICommandKind.Undo:
                    this.Label = "Undo";
                    this.IconSource = (IconSource)new SymbolIconSource()
                    {
                        Symbol = Symbol.Undo
                    };
                    this.Description = "Reverse the most recent action";
                    this.HotKey = new KeyGesture(Key.Z, KeyModifiers.Control);
                    break;
                case StandardUICommandKind.Redo:
                    this.Label = "Redo";
                    this.IconSource = (IconSource)new SymbolIconSource()
                    {
                        Symbol = Symbol.Redo
                    };
                    this.Description = "Repeat the most recently undone action";
                    this.HotKey = new KeyGesture(Key.Y, KeyModifiers.Control);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}