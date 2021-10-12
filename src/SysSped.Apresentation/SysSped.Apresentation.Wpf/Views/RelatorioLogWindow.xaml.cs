using MahApps.Metro.Controls;
using SysSped.Apresentation.Wpf.ViewModels;
using SysSped.Domain.Interfaces;
using System.Globalization;
using System.Threading;

namespace SysSped.Apresentation.Wpf.Views
{
    /// <summary>
    /// Lógica interna para RelatorioLog.xaml
    /// </summary>
    public partial class RelatorioLogWindow : MetroWindow
    {
        public RelatorioLogWindow(ILogRepository logRepository, ILogSpedService logSpedService)
        {

            Thread.CurrentThread.CurrentCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";

            InitializeComponent();
            this.DataContext = new RelatorioLogViewModel(this, logRepository, logSpedService);
        }
    }
}
