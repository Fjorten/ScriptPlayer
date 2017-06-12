﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using ScriptPlayer.Shared;
using ScriptPlayer.VideoSync.Annotations;

namespace ScriptPlayer.VideoSync
{
    /// <summary>
    /// Interaction logic for ConditionEditorDialog.xaml
    /// </summary>
    public partial class ConditionEditorDialog : Window
    {
        public static readonly DependencyProperty ResultProperty = DependencyProperty.Register(
            "Result", typeof(PixelColorSampleCondition), typeof(ConditionEditorDialog), new PropertyMetadata(default(PixelColorSampleCondition)));

        public PixelColorSampleCondition Result
        {
            get { return (PixelColorSampleCondition) GetValue(ResultProperty); }
            set { SetValue(ResultProperty, value); }
        }

        public static readonly DependencyProperty ConditionsProperty = DependencyProperty.Register(
            "Conditions", typeof(List<ConditionViewModel>), typeof(ConditionEditorDialog), new PropertyMetadata(default(List<ConditionViewModel>)));

        public List<ConditionViewModel> Conditions
        {
            get { return (List<ConditionViewModel>) GetValue(ConditionsProperty); }
            set { SetValue(ConditionsProperty, value); }
        }

        private Color _color;
        public ConditionEditorDialog(PixelColorSampleCondition condition)
        {
            Conditions = new List<ConditionViewModel>();
            Conditions.Add(new ConditionViewModel("Red", 0, 255, condition?.Red));
            Conditions.Add(new ConditionViewModel("Green", 0, 255, condition?.Green));
            Conditions.Add(new ConditionViewModel("Blue", 0, 255, condition?.Blue));

            Conditions.Add(new ConditionViewModel("Hue", 0, 100, condition?.Hue));
            Conditions.Add(new ConditionViewModel("Sat", 0, 100, condition?.Saturation));
            Conditions.Add(new ConditionViewModel("Lum", 0, 100, condition?.Luminosity));

            InitializeComponent();

            if (condition != null)
            {
                switch (condition.Source)
                {
                    case ConditionSource.Average:
                        rbAverage.IsChecked = true;
                        break;
                    case ConditionSource.Majority:
                        rbMajority.IsChecked = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            PixelColorSampleCondition result = new PixelColorSampleCondition
            {
                Red = Conditions[0].ToParameter(),
                Green = Conditions[1].ToParameter(),
                Blue = Conditions[2].ToParameter(),
                Hue = Conditions[3].ToParameter(),
                Saturation = Conditions[4].ToParameter(),
                Luminosity = Conditions[5].ToParameter(),
            };

            if (rbMajority.IsChecked == true)
            {
                result.Source = ConditionSource.Majority;
                result.MatchedSamples = new SampleCondtionParameter{MaxValue = 100, MinValue = 50, State = ConditionState.Include};
            }
            else
            {
                result.Source = ConditionSource.Average;
                result.MatchedSamples = new SampleCondtionParameter { MaxValue = 100, MinValue = 50, State = ConditionState.NotUsed };
            }

            Result = result;
            DialogResult = true;
        }
    }

    public class ConditionViewModel : INotifyPropertyChanged
    {
        private ConditionState _state;
        private byte _maximum;
        private byte _minimum;
        private byte _upperValue;
        private byte _lowerValue;
        private string _label;
        private bool _isDoNotUse;
        private bool _isInclude;
        private bool _isExclude;

        public string Label
        {
            get { return _label; }
            set
            {
                if (value == _label) return;
                _label = value;
                OnPropertyChanged();
            }
        }

        public byte LowerValue
        {
            get { return _lowerValue; }
            set
            {
                if (value.Equals(_lowerValue)) return;
                _lowerValue = value;
                OnPropertyChanged();
            }
        }

        public byte UpperValue
        {
            get { return _upperValue; }
            set
            {
                if (value.Equals(_upperValue)) return;
                _upperValue = value;
                OnPropertyChanged();
            }
        }

        public byte Minimum
        {
            get { return _minimum; }
            set
            {
                if (value.Equals(_minimum)) return;
                _minimum = value;
                OnPropertyChanged();
            }
        }

        public byte Maximum
        {
            get { return _maximum; }
            set
            {
                if (value.Equals(_maximum)) return;
                _maximum = value;
                OnPropertyChanged();
            }
        }

        public ConditionState State
        {
            get { return _state; }
            set
            {
                if (value == _state) return;
                _state = value;

                switch (_state)
                {
                    case ConditionState.NotUsed:
                    {
                        IsDoNotUse = true;
                        IsInclude = false;
                        IsExclude = false;
                        break;
                    }
                    case ConditionState.Include:
                    {
                        IsDoNotUse = false;
                        IsInclude = true;
                        IsExclude = false;
                        break;
                    }
                    case ConditionState.Exclude:
                    {
                        IsDoNotUse = false;
                        IsInclude = false;
                        IsExclude = true;
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                OnPropertyChanged();
            }
        }

        public bool IsInclude
        {
            get { return _isInclude; }
            set
            {
                if (value == _isInclude) return;
                _isInclude = value;
                if(_isInclude)
                    State = ConditionState.Include;
                OnPropertyChanged();
            }
        }

        public bool IsExclude
        {
            get { return _isExclude; }
            set
            {
                if (value == _isExclude) return;
                _isExclude = value;
                if (_isExclude)
                    State = ConditionState.Exclude;
                OnPropertyChanged();
            }
        }

        public bool IsDoNotUse
        {
            get { return _isDoNotUse; }
            set
            {
                if (value == _isDoNotUse) return;
                _isDoNotUse = value;
                if (_isDoNotUse)
                    State = ConditionState.NotUsed;
                OnPropertyChanged();
            }
        }

        public ConditionViewModel(string label, byte min, byte max, SampleCondtionParameter parameter)
        {
            Label = label;
            Minimum = min;
            Maximum = max;

            if (parameter == null)
            {
                State = ConditionState.NotUsed;
                LowerValue = min;
                UpperValue = max;
            }
            else
            {
                State = parameter.State;
                LowerValue = parameter.MinValue;
                UpperValue = parameter.MaxValue;
            }
        }

        public SampleCondtionParameter ToParameter()
        {
            return new SampleCondtionParameter
            {
                MaxValue = UpperValue,
                MinValue = LowerValue,
                State = State
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}