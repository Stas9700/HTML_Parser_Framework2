using HTML_Parser_Framework.MODEL;
using HTML_Parser_Framework.MVVM_Classes;
using Microsoft.Win32;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace HTML_Parser_Framework.VIEWMODEL
{
    public class LinkCounterWindowViewModel: Model
    {
        public LinkCounterWindowViewModel()
        {
            _this_dispatcher = Dispatcher.CurrentDispatcher;
        }

        #region Variables
        private ObservableCollection<PageInfo> _pages;
        private PageInfo _selectedPageInfo;
        private int _url_Counter;
        private Dispatcher _this_dispatcher;
        private CancellationTokenSource token;
        private CancellationTokenSource Token {
            get => token;
            set 
            {
                token = value;
                if(value == null) ButtonText = "Посчитать";
                else ButtonText = "Остановить";
            } 
        }
        private string _buttonText = "Посчитать";
        private bool _simpleFind;
        private bool _regexFind;
        #endregion

        #region Properties
        public ObservableCollection<PageInfo> Pages
        {
            get => _pages;
            set
            {
                _pages = value;
                OnPropertyChanged();
            }
        }
        public PageInfo SelectedPage
        {
            get => _selectedPageInfo;
            set
            {
                _selectedPageInfo = value;
                OnPropertyChanged();
            }
        }
        public int URL_Counter
        {
            get => _url_Counter;
            set
            {
                _url_Counter = value;
                OnPropertyChanged();
            }
        }
        public int URL_All
        {
            get => Pages != null ? Pages.Count : 0;
            set
            {
                OnPropertyChanged();
            }
        }
        public string ButtonText
        {
            get => _buttonText;
            set
            {
                _buttonText = value;
                OnPropertyChanged();
            }
        }
        public bool SimpleFind
        {
            get => _simpleFind;
            set
            {
                _simpleFind = value;
                OnPropertyChanged();
            }
        }
        public bool RegexFind
        {
            get => _regexFind;
            set
            {
                _regexFind = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region methods
        private void _chooseFile()
        {
            var dialog = new OpenFileDialog();
            dialog.ShowDialog();
            if (dialog.FileName == "" || dialog.FileName == null) return;
            string file = dialog.FileName;
            string FileType = Path.GetExtension(dialog.FileName);
            MessageBox.Show(file);
            switch (FileType)
            {
                case ".txt":
                    _fromTXT(file);
                    break;
                case ".xls":
                case ".xlsx":
                    _fromExcel(file);
                    break;
            }
        }

        private async void _fromTXT(string filepath)
        {
            Pages = new ObservableCollection<PageInfo>();

            await Task.Run(() =>
            {
                StreamReader reader = new StreamReader(filepath);
                using (reader)
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var page = new PageInfo(line);
                        _this_dispatcher.Invoke(new Action(() =>
                        {
                            Pages.Add(page);
                        }));
                    }
                }
            });
            
        }
        private async void _fromExcel(string filepath)
        {
            Pages = new ObservableCollection<PageInfo>();

            await Task.Run(() =>
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage(new FileInfo(filepath)))
                {
                    var firstSheet = package.Workbook.Worksheets["Лист1"];
                    int LastRow = firstSheet.Dimension.End.Row;
                    for (int i = 1; i <= LastRow; i++)
                    {
                        var page = new PageInfo(firstSheet.Cells[i, 1].Text);
                         _this_dispatcher.Invoke(new Action(() =>
                        {
                            Pages.Add(page);
                        }));
                    }
                }
            });
   
        }
        private void _count()
        {
            Token = new CancellationTokenSource();
                try
                {
                if (SimpleFind) _сountSimpleAsync(Token.Token);
                else if (RegexFind) _countRegexAsync(Token.Token);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
        }
        private async void _сountSimpleAsync(CancellationToken ct)
        {
            URL_All = Pages.Count;
            URL_Counter = 0;
                await Task.Run(() =>
                {
                    foreach (var item in Pages)
                    {
                        if (ct.IsCancellationRequested)
                        { Token = null; break; }
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(item.URL);
                        var text = request.GetResponse();
                        string s = new StreamReader(text.GetResponseStream(), Encoding.UTF8).ReadToEnd();
                        int kolvo = s.Split(new string[] { "<a" }, StringSplitOptions.None).Length - 1; 
                        _this_dispatcher.Invoke(new Action(() =>
                        {
                            URL_Counter++;
                            item.Count = kolvo;
                        }));
                    }
                });
            Token = null;
        }

        private async void _countRegexAsync(CancellationToken ct)
        {
            URL_All = Pages.Count;
            URL_Counter = 0;
            await Task.Run(() =>
            {
                foreach (var item in Pages)
                {
                    if (ct.IsCancellationRequested)
                    { Token = null; break; }
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(item.URL);
                    var text = request.GetResponse();
                    string s = new StreamReader(text.GetResponseStream(), Encoding.UTF8).ReadToEnd();
                    //Regex regex = new Regex("(<a\\shref=\"([^\"]+)\"([^<>]*)>(<\\w>)*([^<>]*)<)");
                    //Regex regex = new Regex("(<a\\shref=\"\\w+\".+</a>)");
                    //Regex regex = new Regex("<a\\s.*?>");
                    //Regex regex = new Regex("<a\\s");
                    Regex regex = new Regex("<a\\s.*?href=\".*?\".*?>");
                    var matches = regex.Matches(s).Count;
                    _this_dispatcher.Invoke(new Action(() =>
                    {
                        URL_Counter++;
                        item.Count = matches;
                    }));
                }
            });
            Token = null;
        }
        #endregion

        #region Commands
        private RelayCommand _chooseFileCommand;
        public RelayCommand ChooseFileCommand
        {
            get
            {
                return _chooseFileCommand ?? (_chooseFileCommand = new RelayCommand(p =>
                {
                    _chooseFile(); 
                }));
            }
        }

        private RelayCommand _countCommand;
        public RelayCommand CountCommand
        {
            get
            {
                return _countCommand ?? (_countCommand = new RelayCommand(p =>
                {
                    if(Token == null) 
                        _count();  
                    else
                        token.Cancel();
                }));
            }
        }

        private RelayCommand _cancelCommand;
        public RelayCommand CancelCommand
        {
            get
            {
                return _cancelCommand ?? (_cancelCommand = new RelayCommand(p =>
                {
                    token.Cancel();
                }));
            }
        }
        #endregion
    }
}
