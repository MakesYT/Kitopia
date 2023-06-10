// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows;

namespace Kitopia.Controls.HotKeyEditor;

public class HotKeyEditorClosingEventArgs : RoutedEventArgs
{
    public HotKeyEditorClosingEventArgs(RoutedEvent routedEvent, object source) : base(routedEvent, source)
    {
    }

    public required HotKeyEditorResult Result
    {
        get;
        init;
    }

    public bool Cancel
    {
        get;
        set;
    }
}

public class HotKeyEditorClosedEventArgs : RoutedEventArgs
{
    public HotKeyEditorClosedEventArgs(RoutedEvent routedEvent, object source) : base(routedEvent, source)
    {
    }

    public required HotKeyEditorResult Result
    {
        get;
        init;
    }
}

public class HotKeyEditorButtonClickEventArgs : RoutedEventArgs
{
    public HotKeyEditorButtonClickEventArgs(RoutedEvent routedEvent, object source) : base(routedEvent, source)
    {
    }

    public required HotKeyEditorButton Button
    {
        get;
        init;
    }
}