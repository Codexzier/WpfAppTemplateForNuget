﻿using System.Collections.Generic;
using Codexzier.Wpf.ApplicationFramework.Controls.Diagram;
using Codexzier.Wpf.ApplicationFramework.Views.Base;
using WpfAppTemplateForNuget.Views.Data;

namespace WpfAppTemplateForNuget.Views.RenderPicture
{
    internal class RenderPicturePrintViewModel : BaseViewModel
    {
        private List<DiagramLevelItem> _countyResults;
        private DistrictItem _districtData;

        public DistrictItem DistrictData
        {
            get => this._districtData;
            set
            {
                this._districtData = value;
                this.OnNotifyPropertyChanged(nameof(this.DistrictData));
            }
        }

        public List<DiagramLevelItem> CountyResults
        {
            get => this._countyResults;
            set
            {
                this._countyResults = value;
                this.OnNotifyPropertyChanged(nameof(this.CountyResults));
            }
        }
    }
}