using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Core.SDKs.HotKey;
using Core.SDKs.Services.Config;
using FluentAvalonia.UI.Controls;
using KitopiaAvalonia.Controls;
using PluginCore;
using PluginCore.Attribute;

namespace KitopiaAvalonia.Pages;

public partial class SettingPage : UserControl
{
    private CompositeDisposable disposables = new CompositeDisposable();
    private ConfigBase? _configBase;
    public SettingPage(ConfigBase configBase)
    {
        _configBase = configBase;
        InitializeComponent();
    }
    private StackPanel nowControl;
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        disposables.Dispose();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        nowControl = StackPanel;
        Application.Current.TryGetResource("FluentFont",null,out var font);
        if (_configBase is not null)
        {
            foreach (var fieldInfo in _configBase.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public  ))
            {
                var configFieldCategory = fieldInfo.GetCustomAttribute<ConfigFieldCategory>();
                if (configFieldCategory is not null)
                {
                    var category = new Expander()
                    {
                        Header = new TextBlock() {Text = configFieldCategory.Category, FontSize = 16},
                        IsExpanded = true,
                    };
                    var stackPanel = new StackPanel();
                    category.Content = stackPanel;
                    nowControl = stackPanel;
                    StackPanel.Children.Add(category);
                }
                if (fieldInfo.GetCustomAttribute<ConfigField>() is { } configField)
                {
                    var SettingsExpander= new SettingsExpander()
                    {
                        Header = configField.Tittle,
                        Description = configField.Description,
                        IconSource = new FontIconSource(){FontFamily = (FontFamily)font,Glyph = System.Convert.ToChar(configField.Symbol).ToString()},
                    };

                    var selectedValue = fieldInfo.GetValue(_configBase);
                    switch (configField.FieldType)
                    {
                        case ConfigFieldType.字符串:
                        {
                            var textBox= new TextBox()
                            {
                                Text = selectedValue?.ToString(),
                            };
                            disposables.Add(textBox.GetObservable(TextBox.TextProperty).Subscribe( (d) =>
                            {
                                _configBase.OnConfigChanged(fieldInfo.Name,d);
                                fieldInfo.SetValue(_configBase, d);
                                ConfigManger.Save(_configBase.Name);
                            }));
                            SettingsExpander.Footer = textBox;
                            break;
                            
                            
                        }
                        case ConfigFieldType.整数:
                        {
                            var textBox= new NumberBox()
                            {
                                Value = (double)selectedValue,
                                Maximum = configField.MaxValue,
                                Minimum = configField.MinValue,
                            };
                            
                            disposables.Add(
                                textBox.GetObservable(NumberBox.ValueProperty).Subscribe( (d) =>
                            {
                                _configBase.OnConfigChanged(fieldInfo.Name,d);
                                fieldInfo.SetValue(_configBase, d);
                                ConfigManger.Save(_configBase.Name);
                                
                            }));
                            
                            SettingsExpander.Footer = textBox;
                            break;
                        }
                        case ConfigFieldType.整数列表:
                        {
                            var comboBox = new ComboBox()
                            {
                                ItemsSource = Enumerable.Range(configField.MinValue,configField.MaxValue).Select( x=>(int)x%configField.Step==0?x:0).Where(x=>x!=0).ToList(),
                                SelectedValue = selectedValue
                            };
                            disposables.Add(
                                comboBox.GetObservable(ComboBox.SelectedValueProperty).Subscribe( (d) =>
                                {
                                    _configBase.OnConfigChanged(fieldInfo.Name,d);
                                    fieldInfo.SetValue(_configBase, d);
                                    ConfigManger.Save(_configBase.Name);
                                        
                                }));
                            SettingsExpander.Footer = comboBox;
                            break;
                        }
                        case ConfigFieldType.整数滑块:
                        {
                            var stackPanel = new StackPanel();
                            stackPanel.Orientation = Orientation.Horizontal;
                            stackPanel.VerticalAlignment = VerticalAlignment.Center;
                            var slider = new Slider()
                            {
                                Maximum = configField.MaxValue,
                                Minimum = configField.MinValue,
                                Value = (int)selectedValue,
                                TickFrequency = configField.Step,
                                IsSnapToTickEnabled = true,
                                Width = 160,
                                VerticalAlignment = VerticalAlignment.Center,
                            };
                            var textBox= new TextBlock()
                            {
                                FontSize = 14,
                                Margin = new Thickness(10,0,0,0),
                                VerticalAlignment = VerticalAlignment.Center,
                            };
                            
                            
                            var binding = new Binding("Value")
                            {
                                Source = slider,
                                Mode = BindingMode.OneWay,
                            };
                            textBox.SetValue(ToolTip.TipProperty,binding);
                            textBox.SetValue(ToolTip.PlacementProperty,PlacementMode.Center);
                            textBox.Bind(TextBlock.TextProperty,binding, BindingPriority.StyleTrigger);
                            
                            disposables.Add(
                                slider.GetObservable(Slider.ValueProperty).Subscribe( (d) =>
                                {
                                    _configBase.OnConfigChanged(fieldInfo.Name,d);
                                    fieldInfo.SetValue(_configBase, (int)d);
                                    ConfigManger.Save(_configBase.Name);
                                    
                                }));
                            stackPanel.Children.Add(textBox);
                            stackPanel.Children.Add(slider);
                            
                            SettingsExpander.Footer = stackPanel;
                            break;
                        }
                            
                        case ConfigFieldType.浮点数:
                            break;
                        case ConfigFieldType.布尔:
                        {
                            var toggleSwitch = new ToggleSwitch()
                            {
                                IsChecked =(bool)selectedValue,
                                FlowDirection = FlowDirection.RightToLeft,
                                OnContent = "开",
                                OffContent = "关",
                            };
                            disposables.Add(
                                toggleSwitch.GetObservable(ToggleSwitch.IsCheckedProperty).Subscribe( (d) =>
                                {
                                    _configBase.OnConfigChanged(fieldInfo.Name,d);
                                    fieldInfo.SetValue(_configBase, d);
                                    ConfigManger.Save(_configBase.Name);
                                    
                                }));
                            SettingsExpander.Footer = toggleSwitch;
                            break;
                        }
                        case ConfigFieldType.快捷键:
                        {
                            var hotKeyModel = (HotKeyModel)selectedValue;
                            var hotKeyControl = new HotKeyShow();
                            hotKeyControl.HotKeyModel = hotKeyModel;
                            disposables.Add(
                                hotKeyControl.GetObservable(HotKeyShow.HotKeyModelProperty).Subscribe( (d) =>
                                {
                                    _configBase.OnConfigChanged(fieldInfo.Name,d);
                                    fieldInfo.SetValue(_configBase, d);
                                    ConfigManger.Save(_configBase.Name);
                                    
                                }));
                            SettingsExpander.Footer = hotKeyControl;
                            break;
                        }
                        case ConfigFieldType.自定义选项:
                        {
                            if (configField.GetType().IsGenericType)
                            {
                                Type[] typeArguments = configField.GetType().GetGenericArguments();
                                
                                var comboBox = new ComboBox()
                                {
                                    ItemsSource = typeArguments[0].GetEnumValues(),
                                    SelectedValue = selectedValue
                                };
                                disposables.Add(
                                    comboBox.GetObservable(ComboBox.SelectedValueProperty).Subscribe( (d) =>
                                    {
                                        _configBase.OnConfigChanged(fieldInfo.Name,d);
                                        fieldInfo.SetValue(_configBase, d);
                                        ConfigManger.Save(_configBase.Name);
                                        
                                    }));
                                SettingsExpander.Footer = comboBox;
                                
                            }

                            break;
                        }
                        case ConfigFieldType.字符串列表:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    
                    nowControl.Children.Add(SettingsExpander);
                }
            }
        }
        
    }
}