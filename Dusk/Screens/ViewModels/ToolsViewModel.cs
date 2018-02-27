using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Dusk.Models;
using MahApps.Metro.Controls;
using Xceed.Words.NET;
using VerticalAlignment = Xceed.Words.NET.VerticalAlignment;

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

        private bool _FilterResult;

        public bool FilterResult
        {
            get => _FilterResult;
            set
            {
                if(value == _FilterResult)
                    return;
                _FilterResult = value;
                OnPropertyChanged(nameof(FilterResult));
            }
        }

        private bool _FilterAge;

        public bool FilterAge
        {
            get => _FilterAge;
            set
            {
                if(value == _FilterAge)
                    return;
                _FilterAge = value;
                OnPropertyChanged(nameof(FilterAge));
            }
        }

        private int _AgeFrom = 60;

        public int AgeFrom
        {
            get => _AgeFrom;
            set
            {
                if(value == _AgeFrom)
                    return;
                _AgeFrom = value;
                OnPropertyChanged(nameof(AgeFrom));
            }
        }

        private int _AgeTo = 0;

        public int AgeTo
        {
            get => _AgeTo;
            set
            {
                if(value == _AgeTo)
                    return;
                _AgeTo = value;
                OnPropertyChanged(nameof(AgeTo));
            }
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

        private ListCollectionView _barangays;
        public ListCollectionView Barangays => _barangays ?? (_barangays = new ListCollectionView(Barangay.Cache));

        private bool _FilterDisability;

        public bool FilterDisability
        {
            get => _FilterDisability;
            set
            {
                if (value == _FilterDisability) return;
                _FilterDisability = value;
                OnPropertyChanged(nameof(FilterDisability));
            }
        }

        private string _Disability = "";

        public string Disability
        {
            get => _Disability;
            set
            {
                if (value == _Disability) return;
                _Disability = value;
                OnPropertyChanged(nameof(Disability));
            }
        }

        private bool _FilterLivelihood;

        public bool FilterLivelihood
        {
            get => _FilterLivelihood;
            set
            {
                if (value == _FilterLivelihood)
                    return;
                _FilterLivelihood = value;
                OnPropertyChanged(nameof(FilterLivelihood));
            }
        }

        private string _Livelihood = "";

        public string Livelihood
        {
            get => _Livelihood;
            set
            {
                if (value == _Livelihood)
                    return;
                _Livelihood = value;
                OnPropertyChanged(nameof(Livelihood));
            }
        }


        private bool _PrintAll = true;

        public bool PrintAll
        {
            get => _PrintAll;
            set
            {
                if(value == _PrintAll)
                    return;
                _PrintAll = value;
                OnPropertyChanged(nameof(PrintAll));
            }
        }

        private ICommand _printCommand;

        public ICommand PrintCommand => _printCommand ?? (_printCommand = new DelegateCommand(d =>
        {
            PrintList();
        }));

        private void PrintList()
        {
            if (!Directory.Exists("Temp"))
                Directory.CreateDirectory("Temp");

            var items = MainViewModel.Instance
                .SearchResult
                .Cast<Person>()
                .Where(x=>x.IsSelected || PrintAll)
                .ToList();
            
            foreach (var barangay in Models.Barangay.Cache)
            {
                if(items.All(x=>x.BarangayId!=barangay.Id)) continue;
                
                var temp = Path.Combine("Temp", $"{barangay.Name} {DateTime.Now:d-MMM-yyyy}.docx");
                
                using (var doc = DocX.Load(@"BarangayList.docx"))
                {
                    doc.ReplaceText("[BARANGAY]",barangay.Name);
                    doc.ReplaceText("[DATE]",DateTime.Now.ToString("MMM d, yyyy"));
                    
                    var tbl = doc.Tables.First(); // doc.InsertTable(1, 6);
                    var persons = items.Where(x => x.BarangayId == barangay.Id).ToList();
                    var row = tbl.Rows[1];
                    for (var i = 0; i < persons.Count; i++)
                    {
                        var r = i==0 ? tbl.Rows[1] : tbl.InsertRow();
                        var item = persons[i];

                        foreach (var rCell in r.Cells)
                        {
                            rCell.VerticalAlignment = VerticalAlignment.Center;
                            rCell.MarginBottom = 2;
                            rCell.MarginLeft = 4;
                            rCell.MarginRight = 4;
                            rCell.MarginTop = 2;
                        }
                        
                        r.Cells[0].Paragraphs.First().Append($"{i + 1}").Alignment = Alignment.center;
                        
                        r.Cells[1].Paragraphs.First().Append(item.Fullname);
                        
                        r.Cells[2].Paragraphs.First().Append(item.Sitio);
                        r.Cells[3].Paragraphs.First().Append(item.OscaId).Alignment = Alignment.center;
                        if(item.DateIssued.HasValue && item.DateIssued.Value>DateTime.MinValue)
                        r.Cells[4].Paragraphs.First().Append(item.DateIssued?.ToString("M/d/yyyy")??"")
                            .Alignment =Alignment.center;
                        r.Cells[5].Paragraphs.First().Append(item.BirthDate?.ToString("M/d/yyyy") ?? "")
                            .Alignment = Alignment.center;
                        r.Cells[6].Paragraphs.First().Append(item.Age?.ToString()??"")
                            .Alignment = Alignment.center;

                        if(item.Sex==Sexes.Female)
                            r.Cells[7].Paragraphs.First().Append("F").Alignment = Alignment.center;
                        else if(item.Sex==Sexes.Male)
                            r.Cells[7].Paragraphs.First().Append("M").Alignment = Alignment.center;
                        
                        r.Cells[8].Paragraphs.First().Append(item.KaubanSaBalay);
                        r.Cells[8].Paragraphs.First().Append(item.Disability);
                        r.Cells[10].Paragraphs.First().Append(item.Livelihood);
                        r.Cells[11].Paragraphs.First().Append(item.Mobile?"YES":"NO").Alignment = Alignment.center;
                        r.Cells[12].Paragraphs.First().Append(item.IsSupported?"NAA":"WALA")
                                .Alignment = Alignment.center;
                        r.Cells[13].Paragraphs.First().Append(item.IsPensioner?"YES":"NO")
                            .Alignment = Alignment.center;
                        r.Cells[14].Paragraphs.First().Append(item.Remarks);

                    }
                    //var border = new Xceed.Words.NET.Border(BorderStyle.Tcbs_single, BorderSize.one, 0,
                    //    System.Drawing.Color.Black);
                    //tbl.SetBorder(TableBorderType.Bottom, border);
                    //tbl.SetBorder(TableBorderType.Left, border);
                    //tbl.SetBorder(TableBorderType.Right, border);
                    //tbl.SetBorder(TableBorderType.Top, border);
                    //tbl.SetBorder(TableBorderType.InsideV, border);
                    //tbl.SetBorder(TableBorderType.InsideH, border);
                    try
                    {
                        File.Delete(temp);
                    }
                    catch (Exception e)
                    {
                        //
                    }
                    
                    doc.SaveAs(temp);
                }

                var info = new ProcessStartInfo(temp);
                info.Arguments = "\"" + MainViewModel.Instance.Printers.CurrentItem + "\"";
                info.CreateNoWindow = true;
                info.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                info.UseShellExecute = true;
                info.Verb = "PrintTo";
                Process.Start(info);
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
