using System;
using System.ComponentModel;
using Dusk.Models;

namespace Dusk.Screens.ViewModels
{
    class ToolsViewModel : ViewModelBase
    {
        private static ToolsViewModel _instance;
        public static ToolsViewModel Instance => _instance ?? (_instance = new ToolsViewModel());

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Instance;
        }


        private ShowPeople _ShowPeople = ShowPeople.ShowAll;

        public ShowPeople ShowPeople
        {
            get => _ShowPeople;
            set
            {
                if (value == _ShowPeople) return;
                _ShowPeople = value;
                OnPropertyChanged(nameof(ShowPeople));
            }
        }

        private bool _FilterGender;

        public bool FilterGender
        {
            get => _FilterGender;
            set
            {
                if (value == _FilterGender) return;
                _FilterGender = value;
                OnPropertyChanged(nameof(FilterGender));
            }
        }

        private Sexes _Gender;

        public Sexes Gender
        {
            get => _Gender;
            set
            {
                if (value == _Gender) return;
                _Gender = value;
                OnPropertyChanged(nameof(Gender));
            }
        }

        private bool _FilterCivilStatus;

        public bool FilterCivilStatus
        {
            get => _FilterCivilStatus;
            set
            {
                if (value == _FilterCivilStatus) return;
                _FilterCivilStatus = value;
                OnPropertyChanged(nameof(FilterCivilStatus));
            }
        }

        private CivilStatuses _CivilStatus;

        public CivilStatuses CivilStatus
        {
            get => _CivilStatus;
            set
            {
                if (value == _CivilStatus) return;
                _CivilStatus = value;
                OnPropertyChanged(nameof(CivilStatus));
            }
        }


        private bool _FilterBarangay;

        public bool FilterBarangay
        {
            get => _FilterBarangay;
            set
            {
                if (value == _FilterBarangay) return;
                _FilterBarangay = value;
                OnPropertyChanged(nameof(FilterBarangay));
            }
        }

        private Barangay _Barangay;

        public Barangay Barangay
        {
            get => _Barangay;
            set
            {
                if (value == _Barangay) return;
                _Barangay = value;
                OnPropertyChanged(nameof(Barangay));
            }
        }



    }

    public enum ShowPeople
    {
        [Description("Show All the People")] ShowAll,
        [Description("Show the Living Only")] Alive,
        [Description("Show Only the Dead")] Dead
    }
}
