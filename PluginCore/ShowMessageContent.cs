using System;

namespace PluginCore;

public class ShowMessageContent
{
    public ShowMessageContent(string? closeButtonText, string? secondaryButtonText, string? primaryButtonText,
        Action? yes, Action? no, Action? cancel)
    {
        CloseButtonText = closeButtonText;
        SecondaryButtonText = secondaryButtonText;
        PrimaryButtonText = primaryButtonText;
        Yes = yes;
        No = no;
        Cancel = cancel;
    }

    public string? CloseButtonText
    {
        get;
        private set;
    }

    public string? SecondaryButtonText
    {
        get;
        private set;
    }

    public string? PrimaryButtonText
    {
        get;
        private set;
    }

    public Action? Yes
    {
        get;
        private set;
    }

    public Action? No
    {
        get;
        private set;
    }

    public Action? Cancel
    {
        get;
        private set;
    }
}