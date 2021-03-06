﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Codexzier.Wpf.ApplicationFramework.Commands;
using Codexzier.Wpf.ApplicationFramework.Components.Ui.Anims;
using Codexzier.Wpf.ApplicationFramework.Components.Ui.EventBus;
using Codexzier.Wpf.ApplicationFramework.Controls.Diagram;
using WpfAppTemplateForNuget.Components.Data;
using WpfAppTemplateForNuget.Views.Data;
using WpfAppTemplateForNuget.Views.RenderPicture;

namespace WpfAppTemplateForNuget.Views.County
{
    /// <summary>
    ///     Interaction logic for CountyView.xaml
    /// </summary>
    public partial class CountyView : UserControl
    {
        private readonly IList<(TextBlock, Storyboard)> _fadeIn = new List<(TextBlock, Storyboard)>();
        private readonly IList<(TextBlock, Storyboard)> _fadeOut = new List<(TextBlock, Storyboard)>();
        private readonly CountyViewModel _viewModel;

        public CountyView()
        {
            this.InitializeComponent();

            this._viewModel = (CountyViewModel) this.DataContext;

            EventBusManager.Register<CountyView, BaseMessage>(this.CountyMessageEvent);
            this.InitializeAnimation();

            this._viewModel.CommandCreatePicture =
                new ButtonCommandCreatePicture(this._viewModel, this.RenderPicturePrint);
        }

        private void InitializeAnimation()
        {
            this._fadeOut.Add((this.tbDescription_Name, new Storyboard()));
            this._fadeOut.Add((this.tbValue_Name, new Storyboard()));

            this._fadeOut.Add((this.tbDescription_Deaths, new Storyboard()));
            this._fadeOut.Add((this.tbValue_Deaths, new Storyboard()));

            this._fadeOut.Add((this.tbDescription_WeekIncidence, new Storyboard()));
            this._fadeOut.Add((this.tbValue_WeekIncidence, new Storyboard()));

            for (var index = 0; index < this._fadeOut.Count; index++)
                Animations.SetFade(this._fadeOut[index].Item1, this._fadeOut[index].Item2, 1d, 0d, 0, 0);

            this._fadeIn.Add((this.tbDescription_Name, new Storyboard()));
            this._fadeIn.Add((this.tbValue_Name, new Storyboard()));

            this._fadeIn.Add((this.tbDescription_Deaths, new Storyboard()));
            this._fadeIn.Add((this.tbValue_Deaths, new Storyboard()));

            this._fadeIn.Add((this.tbDescription_WeekIncidence, new Storyboard()));
            this._fadeIn.Add((this.tbValue_WeekIncidence, new Storyboard()));

            for (var index = 0; index < this._fadeIn.Count; index++)
                Animations.SetFade(this._fadeIn[index].Item1, this._fadeIn[index].Item2, 0d, 1d, index * 100 + 10,
                    1000);
        }

        private async void CountyMessageEvent(IMessageContainer obj)
        {
            foreach (var item in this._fadeOut) item.Item2.Begin();

            if (obj.Content is DistrictItem districtItem)
            {
                foreach (var item in this._fadeIn) item.Item2.Begin();

                this._viewModel.DistrictData = districtItem;

                await Task.Run(() =>
                {
                    var result = HelperExtension.GetCountyResults(districtItem.Name);

                    var enumerable = result as Landkreis[] ?? result.ToArray();

                    var lastDay = 1;
                    this._viewModel.CountyResults = enumerable.Select(s =>
                    {
                        var toolTip = $"{s.Date:d} | {s.WeekIncidence:N1} | {s.Deaths}";

                        var temp = new DiagramLevelItem
                        {
                            Value = s.WeekIncidence,
                            ToolTipText = toolTip,
                            SetHighlightMark = s.Date.Day == 1 || s.Date.Day < lastDay
                        };

                        lastDay = s.Date.Day;

                        return temp;
                    }).ToList();

                    this._viewModel.CountyDeathResults = enumerable.Select(s =>
                    {
                        var toolTip = $"{s.Date:d} | {s.Deaths:N1} | {s.Deaths}";
                        return new DiagramLevelItem {Value = s.Deaths, ToolTipText = toolTip};
                    }).ToList();

                    this.Dispatcher.Invoke(delegate
                    {
                        this.RenderPicturePrint.DataContext = new RenderPicturePrintViewModel
                        {
                            CountyResults = this._viewModel.CountyResults,
                            DistrictData = this._viewModel.DistrictData
                        };
                    });
                });
            }
        }
    }
}