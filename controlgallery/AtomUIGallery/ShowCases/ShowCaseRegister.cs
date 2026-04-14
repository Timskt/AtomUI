using AtomUIGallery.ShowCases.ViewModels;
using AtomUIGallery.ShowCases.Views;
using ReactiveUI;

namespace AtomUIGallery.ShowCases;

/// <summary>
/// AOT 兼容的 ShowCase View 注册模块。
/// 实现 <see cref="IViewModule"/>，通过 <see cref="DefaultViewLocator.Map{TViewModel,TView}"/> 显式绑定
/// 所有 ShowCase View ↔ ViewModel，供 <see cref="ReactiveUI.RoutingState"/> 路由时解析视图。
/// <para>
/// 在 <c>Program.cs</c> 中通过 <c>UseReactiveUI(build => build.ConfigureViewLocator(...))</c> 注册，
/// 无需反射，完全兼容 AOT / NativeAOT 发布。
/// </para>
/// </summary>
public sealed class ShowCaseViewModule : IViewModule
{
    /// <inheritdoc/>
    public void RegisterViews(DefaultViewLocator locator)
    {
        RegisterGeneralCases(locator);
        RegisterLayoutCases(locator);
        RegisterDataDisplayCases(locator);
        RegisterDataEntryCases(locator);
        RegisterFeedbackCases(locator);
        RegisterNavigationCases(locator);
    }

    private static void RegisterGeneralCases(DefaultViewLocator locator)
    {
        locator.Map<AboutUsViewModel, AboutUsPage>(() => new AboutUsPage());
        locator.Map<ButtonViewModel, ButtonShowCase>(() => new ButtonShowCase());
        locator.Map<FloatButtonViewModel, FloatButtonShowCase>(() => new FloatButtonShowCase());
        locator.Map<CustomizeThemeViewModel, CustomizeThemeShowCase>(() => new CustomizeThemeShowCase());
        locator.Map<IconViewModel, IconShowCase>(() => new IconShowCase());
        locator.Map<OsInfoViewModel, OsInfoPage>(() => new OsInfoPage());
        locator.Map<PaletteViewModel, PaletteShowCase>(() => new PaletteShowCase());
        locator.Map<SeparatorViewModel, SeparatorShowCase>(() => new SeparatorShowCase());
        locator.Map<SplitButtonViewModel, SplitButtonShowCase>(() => new SplitButtonShowCase());
    }

    private static void RegisterLayoutCases(DefaultViewLocator locator)
    {
        locator.Map<BoxPanelViewModel, BoxPanelShowCase>(() => new BoxPanelShowCase());
        locator.Map<FlexPanelViewModel, FlexPanelShowCase>(() => new FlexPanelShowCase());
        locator.Map<GridViewModel, GridShowCase>(() => new GridShowCase());
        locator.Map<SpaceViewModel, SpaceShowCase>(() => new SpaceShowCase());
        locator.Map<SplitterViewModel, SplitterShowCase>(() => new SplitterShowCase());
    }

    private static void RegisterDataDisplayCases(DefaultViewLocator locator)
    {
        locator.Map<AvatarViewModel, AvatarShowCase>(() => new AvatarShowCase());
        locator.Map<BadgeViewModel, BadgeShowCase>(() => new BadgeShowCase());
        locator.Map<CalendarViewModel, CalendarShowCase>(() => new CalendarShowCase());
        locator.Map<CollapseViewModel, CollapseShowCase>(() => new CollapseShowCase());
        locator.Map<CardViewModel, CardShowCase>(() => new CardShowCase());
        locator.Map<CarouselViewModel, CarouselShowCase>(() => new CarouselShowCase());
        locator.Map<DataGridViewModel, DataGridShowCase>(() => new DataGridShowCase());
        locator.Map<DescriptionsViewModel, DescriptionsShowCase>(() => new DescriptionsShowCase());
        locator.Map<EmptyViewModel, EmptyShowCase>(() => new EmptyShowCase());
        locator.Map<ImagePreviewerViewModel, ImagePreviewerShowCase>(() => new ImagePreviewerShowCase());
        locator.Map<ExpanderViewModel, ExpanderShowCase>(() => new ExpanderShowCase());
        locator.Map<GroupBoxViewModel, GroupBoxShowCase>(() => new GroupBoxShowCase());
        locator.Map<InfoFlyoutViewModel, InfoFlyoutShowCase>(() => new InfoFlyoutShowCase());
        locator.Map<ListViewModel, ListShowCase>(() => new ListShowCase());
        locator.Map<QRCodeViewModel, QRCodeShowCase>(() => new QRCodeShowCase());
        locator.Map<SegmentedViewModel, SegmentedShowCase>(() => new SegmentedShowCase());
        locator.Map<StatisticViewModel, StatisticShowCase>(() => new StatisticShowCase());
        locator.Map<TagViewModel, TagShowCase>(() => new TagShowCase());
        locator.Map<TimelineViewModel, TimelineShowCase>(() => new TimelineShowCase());
        locator.Map<TooltipViewModel, TooltipShowCase>(() => new TooltipShowCase());
        locator.Map<TourViewModel, TourShowCase>(() => new TourShowCase());
        locator.Map<TreeViewViewModel, TreeViewShowCase>(() => new TreeViewShowCase());
    }

    private static void RegisterDataEntryCases(DefaultViewLocator locator)
    {
        locator.Map<AutoCompleteViewModel, AutoCompleteShowCase>(() => new AutoCompleteShowCase());
        locator.Map<CascaderViewModel, CascaderShowCase>(() => new CascaderShowCase());
        locator.Map<CheckBoxViewModel, CheckBoxShowCase>(() => new CheckBoxShowCase());
        locator.Map<ColorPickerViewModel, ColorPickerShowCase>(() => new ColorPickerShowCase());
        locator.Map<DatePickerViewModel, DatePickerShowCase>(() => new DatePickerShowCase());
        locator.Map<FormViewModel, FormShowCase>(() => new FormShowCase());
        locator.Map<LineEditViewModel, LineEditShowCase>(() => new LineEditShowCase());
        locator.Map<MentionsViewModel, MentionsShowCase>(() => new MentionsShowCase());
        locator.Map<NumberUpDownViewModel, NumberUpDownShowCase>(() => new NumberUpDownShowCase());
        locator.Map<RadioButtonViewModel, RadioButtonShowCase>(() => new RadioButtonShowCase());
        locator.Map<RateViewModel, RateShowCase>(() => new RateShowCase());
        locator.Map<SelectViewModel, SelectShowCase>(() => new SelectShowCase());
        locator.Map<SliderViewModel, SliderShowCase>(() => new SliderShowCase());
        locator.Map<TimePickerViewModel, TimePickerShowCase>(() => new TimePickerShowCase());
        locator.Map<ToggleSwitchViewModel, ToggleSwitchShowCase>(() => new ToggleSwitchShowCase());
        locator.Map<TransferViewModel, TransferShowCase>(() => new TransferShowCase());
        locator.Map<TreeSelectViewModel, TreeSelectShowCase>(() => new TreeSelectShowCase());
        locator.Map<UploadViewModel, UploadShowCase>(() => new UploadShowCase());
    }

    private static void RegisterFeedbackCases(DefaultViewLocator locator)
    {
        locator.Map<AlertViewModel, AlertShowCase>(() => new AlertShowCase());
        locator.Map<DrawerViewModel, DrawerShowCase>(() => new DrawerShowCase());
        locator.Map<SpinViewModel, SpinShowCase>(() => new SpinShowCase());
        locator.Map<MessageViewModel, MessageShowCase>(() => new MessageShowCase());
        locator.Map<ModalViewModel, ModalShowCase>(() => new ModalShowCase());
        locator.Map<NotificationViewModel, NotificationShowCase>(() => new NotificationShowCase());
        locator.Map<PopupConfirmViewModel, PopupConfirmShowCase>(() => new PopupConfirmShowCase());
        locator.Map<ProgressBarViewModel, ProgressBarShowCase>(() => new ProgressBarShowCase());
        locator.Map<ResultViewModel, ResultShowCase>(() => new ResultShowCase());
        locator.Map<SkeletonViewModel, SkeletonShowCase>(() => new SkeletonShowCase());
        locator.Map<WatermarkViewModel, WatermarkShowCase>(() => new WatermarkShowCase());
    }

    private static void RegisterNavigationCases(DefaultViewLocator locator)
    {
        locator.Map<BreadcrumbViewModel, BreadcrumbShowCase>(() => new BreadcrumbShowCase());
        locator.Map<ButtonSpinnerViewModel, ButtonSpinnerShowCase>(() => new ButtonSpinnerShowCase());
        locator.Map<ComboBoxViewModel, ComboBoxShowCase>(() => new ComboBoxShowCase());
        locator.Map<DropdownButtonViewModel, DropdownButtonShowCase>(() => new DropdownButtonShowCase());
        locator.Map<MenuViewModel, MenuShowCase>(() => new MenuShowCase());
        locator.Map<PaginationViewModel, PaginationShowCase>(() => new PaginationShowCase());
        locator.Map<StepsViewModel, StepsShowCase>(() => new StepsShowCase());
        locator.Map<TabControlViewModel, TabControlShowCase>(() => new TabControlShowCase());
    }
}
