using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Styling;

using AvaloniaUI.Ribbon.Contracts;

using DynamicData;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace AvaloniaUI.Ribbon.Windows
{
    [TemplatePart("PART_MoreButton", typeof(ToggleButton))]
    public class QuickAccessToolbar : ItemsControl, INotifyPropertyChanged//, IKeyTipHandler
    {
        #region Fields

        public static readonly AttachedProperty<bool> IsCheckedProperty = AvaloniaProperty.RegisterAttached<QuickAccessToolbar, MenuItem, bool>("IsChecked");
        public static readonly DirectProperty<QuickAccessToolbar, ObservableCollection<QuickAccessRecommendation>> RecommendedItemsProperty = AvaloniaProperty.RegisterDirect<QuickAccessToolbar, ObservableCollection<QuickAccessRecommendation>>(nameof(RecommendedItems), o => o.RecommendedItems, (o, v) => o.RecommendedItems = v);
        public static readonly StyledProperty<DesktopRibbon> RibbonProperty = AvaloniaProperty.Register<QuickAccessToolbar, DesktopRibbon>("Ribbon");

        private static readonly string FIXED_ITEM_CLASS = "quickAccessFixedItem";

        private MenuItem _collapseRibbonItem = new();

        private ObservableCollection<QuickAccessRecommendation> _recommendedItems = new ObservableCollection<QuickAccessRecommendation>();

        #endregion Fields

        #region Constructors

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
            ItemsSource = new ObservableCollection<QuickAccessItem>();
        }

        #endregion Constructors

        #region Properties

        public ObservableCollection<QuickAccessRecommendation> RecommendedItems
        {
            get => _recommendedItems;
            set => SetAndRaise(RecommendedItemsProperty, ref _recommendedItems, value);
        }

        public DesktopRibbon Ribbon
        {
            get => GetValue(RibbonProperty);
            set => SetValue(RibbonProperty, value);
        }

        protected override Type StyleKeyOverride => typeof(QuickAccessToolbar);

        #endregion Properties

        public static bool GetIsChecked(MenuItem element)
        {
            return element.GetValue(IsCheckedProperty);
        }

        public static void SetIsChecked(MenuItem element, bool value)
        {
            element.SetValue(IsCheckedProperty, value);
        }

        public bool AddItem(ICanAddToQuickAccess item)
        {
            bool contains = ContainsItem(item, out object obj);
            if (item == null || contains)
                return false;
            else
            {
                if (item.CanAddToQuickAccess)
                {
                    (ItemsSource as ObservableCollection<QuickAccessItem>).Add(new QuickAccessItem() { Item = item });
                    return true;
                }
            }

            return false;
        }

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

        public bool RemoveItem(ICanAddToQuickAccess item)
        {
            bool contains = ContainsItem(item, out object obj);
            if (item == null || !contains)
                return false;
            else
            {
                var items = (ItemsSource as ObservableCollection<QuickAccessItem>).ToList();
                return (ItemsSource as ObservableCollection<QuickAccessItem>).Remove(items.First(x => x.Item == item));
                /*
                Items.Remove(items.First(x =>
                {
                    if (x == item)
                        return true;
                    else if (x is QuickAccessItem itm && itm.Item == item)
                        return true;

                    return false;
                }));
                */
            }
        }

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
    }
}