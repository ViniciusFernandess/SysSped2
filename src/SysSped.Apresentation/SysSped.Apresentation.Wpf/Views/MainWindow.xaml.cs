using MahApps.Metro.Controls;
using SysSped.Apresentation.Wpf.ViewModels;
using SysSped.Domain.Interfaces;

namespace SysSped.Apresentation.Wpf.Views
{
    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    public partial class MainWindow : MetroWindow
    {

        public MainWindow(IImportacaoRepository repoImportacao, ILogRepository logRepo, IExcelService servExcel, ITxtService servSped, ILogSpedService servLogSped)
        {
            InitializeComponent();
            this.DataContext = new MainWindowViewModel(this, repoImportacao, logRepo, servExcel, servSped, servLogSped);
        }
    }
}
