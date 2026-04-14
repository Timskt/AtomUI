using AtomUIGallery.ShowCases.ViewModels;
using AtomUIGallery.ShowCases.Views;
using ReactiveUI;
using Splat;

namespace AtomUIGallery.ShowCases;

internal static class ShowCaseRegister
{
    /// <summary>
    /// 将所有 ShowCase View 注册到 <see cref="AppLocator.CurrentMutable"/>，
    /// 供 <see cref="ReactiveUI.RoutingState"/> 路由时解析对应的视图。
    /// 应在 <see cref="Avalonia.Application.OnFrameworkInitializationCompleted"/> 中调用，
    /// 确保在 ReactiveUI 通过 UseReactiveUI() 完成初始化之后、主窗口创建之前执行。
    /// </summary>
    public static void Register()
    {
        RegisterGeneralCases();
        RegisterLayoutCases();
        RegisterDataDisplayCases();
        RegisterDataEntryCases();
        RegisterFeedbackCases();
        RegisterNavigationCases();
    }

    private static void RegisterGeneralCases()
    {
        AppLocator.CurrentMutable.Register(() => new AboutUsPage(), typeof(IViewFor<AboutUsViewModel>));
        AppLocator.CurrentMutable.Register(() => new ButtonShowCase(), typeof(IViewFor<ButtonViewModel>));
        AppLocator.CurrentMutable.Register(() => new FloatButtonShowCase(), typeof(IViewFor<FloatButtonViewModel>));
        AppLocator.CurrentMutable.Register(() => new CustomizeThemeShowCase(), typeof(IViewFor<CustomizeThemeViewModel>));
        AppLocator.CurrentMutable.Register(() => new IconShowCase(), typeof(IViewFor<IconViewModel>));
        AppLocator.CurrentMutable.Register(() => new OsInfoPage(), typeof(IViewFor<OsInfoViewModel>));
        AppLocator.CurrentMutable.Register(() => new PaletteShowCase(), typeof(IViewFor<PaletteViewModel>));
        AppLocator.CurrentMutable.Register(() => new SeparatorShowCase(), typeof(IViewFor<SeparatorViewModel>));
        AppLocator.CurrentMutable.Register(() => new SplitButtonShowCase(), typeof(IViewFor<SplitButtonViewModel>));
    }

    private static void RegisterLayoutCases()
    {
        AppLocator.CurrentMutable.Register(() => new BoxPanelShowCase(), typeof(IViewFor<BoxPanelViewModel>));
        AppLocator.CurrentMutable.Register(() => new FlexPanelShowCase(), typeof(IViewFor<FlexPanelViewModel>));
        AppLocator.CurrentMutable.Register(() => new GridShowCase(), typeof(IViewFor<GridViewModel>));
        AppLocator.CurrentMutable.Register(() => new SpaceShowCase(), typeof(IViewFor<SpaceViewModel>));
        AppLocator.CurrentMutable.Register(() => new SplitterShowCase(), typeof(IViewFor<SplitterViewModel>));
    }

    private static void RegisterDataDisplayCases()
    {
        AppLocator.CurrentMutable.Register(() => new AvatarShowCase(), typeof(IViewFor<AvatarViewModel>));
        AppLocator.CurrentMutable.Register(() => new BadgeShowCase(), typeof(IViewFor<BadgeViewModel>));
        AppLocator.CurrentMutable.Register(() => new CalendarShowCase(), typeof(IViewFor<CalendarViewModel>));
        AppLocator.CurrentMutable.Register(() => new CollapseShowCase(), typeof(IViewFor<CollapseViewModel>));
        AppLocator.CurrentMutable.Register(() => new CardShowCase(), typeof(IViewFor<CardViewModel>));
        AppLocator.CurrentMutable.Register(() => new CarouselShowCase(), typeof(IViewFor<CarouselViewModel>));
        AppLocator.CurrentMutable.Register(() => new DataGridShowCase(), typeof(IViewFor<DataGridViewModel>));
        AppLocator.CurrentMutable.Register(() => new DescriptionsShowCase(), typeof(IViewFor<DescriptionsViewModel>));
        AppLocator.CurrentMutable.Register(() => new EmptyShowCase(), typeof(IViewFor<EmptyViewModel>));
        AppLocator.CurrentMutable.Register(() => new ImagePreviewerShowCase(), typeof(IViewFor<ImagePreviewerViewModel>));
        AppLocator.CurrentMutable.Register(() => new ExpanderShowCase(), typeof(IViewFor<ExpanderViewModel>));
        AppLocator.CurrentMutable.Register(() => new GroupBoxShowCase(), typeof(IViewFor<GroupBoxViewModel>));
        AppLocator.CurrentMutable.Register(() => new InfoFlyoutShowCase(), typeof(IViewFor<InfoFlyoutViewModel>));
        AppLocator.CurrentMutable.Register(() => new ListShowCase(), typeof(IViewFor<ListViewModel>));
        AppLocator.CurrentMutable.Register(() => new QRCodeShowCase(), typeof(IViewFor<QRCodeViewModel>));
        AppLocator.CurrentMutable.Register(() => new SegmentedShowCase(), typeof(IViewFor<SegmentedViewModel>));
        AppLocator.CurrentMutable.Register(() => new StatisticShowCase(), typeof(IViewFor<StatisticViewModel>));
        AppLocator.CurrentMutable.Register(() => new TagShowCase(), typeof(IViewFor<TagViewModel>));
        AppLocator.CurrentMutable.Register(() => new TimelineShowCase(), typeof(IViewFor<TimelineViewModel>));
        AppLocator.CurrentMutable.Register(() => new TooltipShowCase(), typeof(IViewFor<TooltipViewModel>));
        AppLocator.CurrentMutable.Register(() => new TourShowCase(), typeof(IViewFor<TourViewModel>));
        AppLocator.CurrentMutable.Register(() => new TreeViewShowCase(), typeof(IViewFor<TreeViewViewModel>));
    }

    private static void RegisterDataEntryCases()
    {
        AppLocator.CurrentMutable.Register(() => new AutoCompleteShowCase(), typeof(IViewFor<AutoCompleteViewModel>));
        AppLocator.CurrentMutable.Register(() => new CascaderShowCase(), typeof(IViewFor<CascaderViewModel>));
        AppLocator.CurrentMutable.Register(() => new CheckBoxShowCase(), typeof(IViewFor<CheckBoxViewModel>));
        AppLocator.CurrentMutable.Register(() => new ColorPickerShowCase(), typeof(IViewFor<ColorPickerViewModel>));
        AppLocator.CurrentMutable.Register(() => new DatePickerShowCase(), typeof(IViewFor<DatePickerViewModel>));
        AppLocator.CurrentMutable.Register(() => new FormShowCase(), typeof(IViewFor<FormViewModel>));
        AppLocator.CurrentMutable.Register(() => new LineEditShowCase(), typeof(IViewFor<LineEditViewModel>));
        AppLocator.CurrentMutable.Register(() => new MentionsShowCase(), typeof(IViewFor<MentionsViewModel>));
        AppLocator.CurrentMutable.Register(() => new NumberUpDownShowCase(), typeof(IViewFor<NumberUpDownViewModel>));
        AppLocator.CurrentMutable.Register(() => new RadioButtonShowCase(), typeof(IViewFor<RadioButtonViewModel>));
        AppLocator.CurrentMutable.Register(() => new RateShowCase(), typeof(IViewFor<RateViewModel>));
        AppLocator.CurrentMutable.Register(() => new SelectShowCase(), typeof(IViewFor<SelectViewModel>));
        AppLocator.CurrentMutable.Register(() => new SliderShowCase(), typeof(IViewFor<SliderViewModel>));
        AppLocator.CurrentMutable.Register(() => new TimePickerShowCase(), typeof(IViewFor<TimePickerViewModel>));
        AppLocator.CurrentMutable.Register(() => new ToggleSwitchShowCase(), typeof(IViewFor<ToggleSwitchViewModel>));
        AppLocator.CurrentMutable.Register(() => new TransferShowCase(), typeof(IViewFor<TransferViewModel>));
        AppLocator.CurrentMutable.Register(() => new TreeSelectShowCase(), typeof(IViewFor<TreeSelectViewModel>));
        AppLocator.CurrentMutable.Register(() => new UploadShowCase(), typeof(IViewFor<UploadViewModel>));
    }

    private static void RegisterFeedbackCases()
    {
        AppLocator.CurrentMutable.Register(() => new AlertShowCase(), typeof(IViewFor<AlertViewModel>));
        AppLocator.CurrentMutable.Register(() => new DrawerShowCase(), typeof(IViewFor<DrawerViewModel>));
        AppLocator.CurrentMutable.Register(() => new SpinShowCase(), typeof(IViewFor<SpinViewModel>));
        AppLocator.CurrentMutable.Register(() => new MessageShowCase(), typeof(IViewFor<MessageViewModel>));
        AppLocator.CurrentMutable.Register(() => new ModalShowCase(), typeof(IViewFor<ModalViewModel>));
        AppLocator.CurrentMutable.Register(() => new NotificationShowCase(), typeof(IViewFor<NotificationViewModel>));
        AppLocator.CurrentMutable.Register(() => new PopupConfirmShowCase(), typeof(IViewFor<PopupConfirmViewModel>));
        AppLocator.CurrentMutable.Register(() => new ProgressBarShowCase(), typeof(IViewFor<ProgressBarViewModel>));
        AppLocator.CurrentMutable.Register(() => new ResultShowCase(), typeof(IViewFor<ResultViewModel>));
        AppLocator.CurrentMutable.Register(() => new SkeletonShowCase(), typeof(IViewFor<SkeletonViewModel>));
        AppLocator.CurrentMutable.Register(() => new WatermarkShowCase(), typeof(IViewFor<WatermarkViewModel>));
    }

    private static void RegisterNavigationCases()
    {
        AppLocator.CurrentMutable.Register(() => new BreadcrumbShowCase(), typeof(IViewFor<BreadcrumbViewModel>));
        AppLocator.CurrentMutable.Register(() => new ButtonSpinnerShowCase(), typeof(IViewFor<ButtonSpinnerViewModel>));
        AppLocator.CurrentMutable.Register(() => new ComboBoxShowCase(), typeof(IViewFor<ComboBoxViewModel>));
        AppLocator.CurrentMutable.Register(() => new DropdownButtonShowCase(), typeof(IViewFor<DropdownButtonViewModel>));
        AppLocator.CurrentMutable.Register(() => new MenuShowCase(), typeof(IViewFor<MenuViewModel>));
        AppLocator.CurrentMutable.Register(() => new PaginationShowCase(), typeof(IViewFor<PaginationViewModel>));
        AppLocator.CurrentMutable.Register(() => new StepsShowCase(), typeof(IViewFor<StepsViewModel>));
        AppLocator.CurrentMutable.Register(() => new TabControlShowCase(), typeof(IViewFor<TabControlViewModel>));
    }
}
