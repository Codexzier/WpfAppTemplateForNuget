﻿using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Animation;
using Codexzier.Wpf.ApplicationFramework.Components.Ui.Anims;

namespace Codexzier.Wpf.ApplicationFramework.Controls.Diagram
{
    public partial class DiagramControl
    {
        public double Scale
        {
            get => (double)this.GetValue(ScaleProperty);
            set => this.SetValue(ScaleProperty, value);
        }

        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.RegisterAttached(
                "Scale",
                typeof(double),
                typeof(DiagramControl),
                new PropertyMetadata(2.5, UpdateDiagram));


        public static readonly DependencyProperty AnimationOnProperty = DependencyProperty.Register(
            "AnimationOn", typeof(bool), typeof(DiagramControl), new PropertyMetadata(true));

        public bool AnimationOn
        {
            get => (bool)this.GetValue(AnimationOnProperty);
            set => this.SetValue(AnimationOnProperty, value);
        }

        public List<DiagramLevelItem> DiagramLevelItemsSource
        {
            get => (List<DiagramLevelItem>)this.GetValue(DiagramLevelItemsSourceProperty);
            set => this.SetValue(DiagramLevelItemsSourceProperty, value);
        }

        public static readonly DependencyProperty DiagramLevelItemsSourceProperty =
            DependencyProperty.RegisterAttached("DiagramLevelItemsSource",
                typeof(List<DiagramLevelItem>),
                typeof(DiagramControl),
                new PropertyMetadata(new List<DiagramLevelItem>(), UpdateDiagram));

        private static void UpdateDiagram(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DiagramControl control)
            {
                SetValueToRects(control);
            }
        }

        // TODO: Not sure, but I have an idea.
        private readonly IList<BarItem> _barItems = new List<BarItem>();

        private static void SetValueToRects(DiagramControl control)
        {
            if (control.DiagramLevelItemsSource == null)
            {
                return;
            }

            if (control.ActualWidth == 0d || control.ActualHeight == 0d)
            {
                control.RenderSize = new Size(control.Width, control.Height);
            }

            control._barItems.Clear();
            control.SimpleDiagram.Children.Clear();

            var heightScale = control.ActualHeight / 200d;

            control.OneHundred.Margin = new Thickness(0, 0, 0, 100 / control.Scale * heightScale);
            control.OneHundredText.Margin = new Thickness(0, 0, 0, 100 / control.Scale * heightScale);

            var widthPerResult = (control.ActualWidth - 20) / control.DiagramLevelItemsSource.Count;

            var delay = 1;
            foreach (var item in control.DiagramLevelItemsSource)
            {
                var heightValue = item.Value / control.Scale * heightScale;

                var barItem = new BarItem(widthPerResult, heightValue, item.ToolTipText, item.Value, item.SetHighlightMark, control.AnimationOn);

                control.SimpleDiagram.Children.Add(barItem.Bar);
                control._barItems.Add(barItem);

                if (!control.AnimationOn)
                {
                    continue;
                }

                var storyboard = new Storyboard();
                Animations.SetMove(barItem.Bar,
                    storyboard,
                    new Thickness(0, 0, 0, barItem.Bar.Height * -1),
                    new Thickness(0),
                    delay * 20);

                storyboard.Begin();
                delay++;

            }
        }

        public DiagramControl() => this.InitializeComponent();

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e) => SetValueToRects(this);
    }
}
