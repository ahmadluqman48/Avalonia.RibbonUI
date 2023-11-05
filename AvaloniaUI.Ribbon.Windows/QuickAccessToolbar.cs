using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Styling;

using AvaloniaUI.Ribbon.Contracts;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;

namespace AvaloniaUI.Ribbon.Windows
{
    [TemplatePart("PART_MoreButton", typeof(ToggleButton))]
    public class QuickAccessToolbar : ItemsControl//, IKeyTipHandler
    {
        public static readonly StyledProperty<DesktopRibbon> RibbonProperty = AvaloniaProperty.Register<QuickAccessToolbar, DesktopRibbon>("Ribbon");

        public DesktopRibbon Ribbon
        {
            get => GetValue(RibbonProperty);
            set => SetValue(RibbonProperty, value);
        }

        public static readonly DirectProperty<QuickAccessToolbar, ObservableCollection<QuickAccessRecommendation>> RecommendedItemsProperty = AvaloniaProperty.RegisterDirect<QuickAccessToolbar, ObservableCollection<QuickAccessRecommendation>>(nameof(RecommendedItems), o => o.RecommendedItems, (o, v) => o.RecommendedItems = v);
        private ObservableCollection<QuickAccessRecommendation> _recommendedItems = new ObservableCollection<QuickAccessRecommendation>();

        public ObservableCollection<QuickAccessRecommendation> RecommendedItems
        {
            get => _recommendedItems;
            set => SetAndRaise(RecommendedItemsProperty, ref _recommendedItems, value);
        }

        public static readonly AttachedProperty<bool> IsCheckedProperty = AvaloniaProperty.RegisterAttached<QuickAccessToolbar, MenuItem, bool>("IsChecked");

        public static bool GetIsChecked(MenuItem element)
        {
            return element.GetValue(IsCheckedProperty);
        }

        public static void SetIsChecked(MenuItem element, bool value)
        {
            element.SetValue(IsCheckedProperty, value);
        }

        private static readonly string FIXED_ITEM_CLASS = "quickAccessFixedItem";

        static QuickAccessToolbar()
        {
            RibbonProperty.Changed.AddClassHandler<QuickAccessToolbar>((sender, e) =>
            {
                if (sender.Ribbon != null)
                    sender._collapseRibbonItem[!IsCheckedProperty] = sender.Ribbon[!DesktopRibbon.IsCollapsedProperty];
                else
                    SetIsChecked(sender._collapseRibbonItem, false);
            });
        }

        private MenuItem _collapseRibbonItem = new();

        public QuickAccessToolbar() : base()
        {
            _collapseRibbonItem.Classes.Add(FIXED_ITEM_CLASS);
            //_collapseRibbonItem.Header = new DynamicResourceExtension("AvaloniaRibbon.MinimizeRibbon"); // "Minimize the Ribbon";
            _collapseRibbonItem[!HeaderedSelectingItemsControl.HeaderProperty] = _collapseRibbonItem.GetResourceObservable("AvaloniaRibbon.MinimizeRibbon").ToBinding();
            _collapseRibbonItem[!IsEnabledProperty] = this.GetObservable(RibbonProperty).Select(x => x != null).ToBinding();
            _collapseRibbonItem.Click += (_, _) =>
            {
                if (Ribbon != null)
                    Ribbon.IsCollapsed = !Ribbon.IsCollapsed;
            };
        }

        protected override Type StyleKeyOverride => typeof(QuickAccessToolbar);

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            var more = e.NameScope.Find<ToggleButton>("PART_MoreButton");
            if (more is not { })
                return;
            var morCtx = more.ContextMenu;

            MenuItem moreCmdItem = new MenuItem()
            {
                //Header =  new DynamicResourceExtension()., //"More commands...",
                IsEnabled = false, //[!IsEnabledProperty] = this.GetObservable(RibbonProperty).Select(x => x != null).ToBinding(),
            };
            moreCmdItem.Classes.Add(FIXED_ITEM_CLASS);
            moreCmdItem[!HeaderedSelectingItemsControl.HeaderProperty] = moreCmdItem.GetResourceObservable("AvaloniaRibbon.MoreQATCommands").ToBinding();

            if (morCtx is not { })
                return;
            morCtx.Opened += (sneder, a) =>
            {
                if (more.IsChecked != true)
                    more.IsChecked = true;

                ObservableCollection<object> morCtxItems = new ObservableCollection<object>();
                foreach (QuickAccessRecommendation rcm in RecommendedItems)
                {
                    rcm.IsChecked = ContainsItem(rcm.Item);
                    morCtxItems.Add(rcm);
                }

                morCtxItems.Add(new Separator());
                morCtxItems.Add(moreCmdItem);
                morCtxItems.Add(_collapseRibbonItem);
                morCtx.ItemsSource = morCtxItems;
            };

            morCtx.Closed += (sender, a) =>
            {
                if (more.IsChecked == true)
                    more.IsChecked = false;
            };
            more.IsCheckedChanged += delegate (object sender, RoutedEventArgs args)
            {
                if (more.IsChecked == true)
                {
                    morCtx.Open(more);
                }
                else if (more.IsChecked == false)
                {
                    morCtx.Close();
                }
            };
        }

        /*protected override void ItemsChanged(AvaloniaPropertyChangedEventArgs e)
        {
            base.ItemsChanged(e);
            RefreshItems();
        }

        protected override void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base.ItemsCollectionChanged(sender, e);
            RefreshItems();
        }

        void RefreshItems()
        {
            panel.Children.Clear();

            foreach (Control itm in ((AvaloniaList<object>)Items).OfType<Control>())
                panel.Children.Add(itm);
        }*/

        /*private protected override ItemContainerGenerator CreateItemContainerGenerator()
        {
            return new ItemContainerGenerator<QuickAccessItem>(this, QuickAccessItem.ItemProperty, QuickAccessItem.ContentTemplateProperty);
        }*/

        public bool ContainsItem(ICanAddToQuickAccess item) => ContainsItem(item, out object result);

        public bool ContainsItem(ICanAddToQuickAccess item, out object result)
        {
            if (Items.OfType<ICanAddToQuickAccess>().Contains(item))
            {
                result = Items.OfType<ICanAddToQuickAccess>().First();
                return true;
            }
            else if (Items.OfType<QuickAccessItem>().Any(x => x.Item == item))
            {
                result = Items.OfType<QuickAccessItem>().First(x => x.Item == item);
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        public bool AddItem(ICanAddToQuickAccess item)
        {
            bool contains = ContainsItem(item, out object obj);
            if (item == null || contains)
                return false;
            else
            {
                ICanAddToQuickAccess itm = item;
                if (obj is QuickAccessItem qai)
                    itm = qai.Item;

                if (itm.CanAddToQuickAccess)
                {
                    Items.Add(item);
                    //ItemsSource = Items.Append(item);
                    return true;
                }
            }

            return false;
        }

        public bool RemoveItem(ICanAddToQuickAccess item)
        {
            bool contains = ContainsItem(item, out object obj);
            if (item == null || !contains)
                return false;
            else
            {
                var items = Items.ToList();
                Items.Remove(items.First(x =>
                {
                    if (x == item)
                        return true;
                    else if (x is QuickAccessItem itm && itm.Item == item)
                        return true;

                    return false;
                }));
                //ItemsSource = items;
                return true;
            }
        }

        public void MoreFlyoutMenuItemCommand(object parameter)
        {
            if (parameter is ICanAddToQuickAccess item)
            {
                if (!AddItem(item))
                    RemoveItem(item);
            }
            else if (parameter is Action cmd)
                cmd();
        }
    }
}