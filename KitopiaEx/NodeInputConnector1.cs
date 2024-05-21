using System;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Styling;
using PluginCore;

namespace KitopiaEx;

public class NodeInputConnector1 : INodeInputConnector
{
    public StyleInclude Style =>
        new(new Uri("avares://KitopiaEx"))
            { Source = new Uri("NodeInputConnector1Style.axaml", UriKind.Relative) };

    public IDataTemplate IDataTemplate =>
        new ResourceInclude(new Uri("avares://KitopiaEx"))
                { Source = new Uri("NodeInputConnector1DataTemple.axaml", UriKind.Relative) }
           .TryGetResource("Template", null, out var variant)
            ? (IDataTemplate)variant
            : null;

    public ObservableValue Value { get; } = new ObservableValue();
}