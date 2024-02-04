﻿using System.Collections.ObjectModel;
using Core.SDKs.HotKey;
using Core.SDKs.Services.Config;

namespace Core.ViewModel.Pages;

public class HotKeyManagerPageViewModel
{
    public ObservableCollection<HotKeyModel> HotKeys {
    get
    {
        return ConfigManger.Config.hotKeys;
    }}
}