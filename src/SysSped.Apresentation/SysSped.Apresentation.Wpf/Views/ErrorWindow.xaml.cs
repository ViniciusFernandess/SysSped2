using SysSped.Apresentation.Wpf.ViewModels;
using SysSped.Domain.Interfaces;
using MahApps.Metro.Controls;
using Prism.Mvvm;
using SysSped.Apresentation.Wpf.Interface.ViewModels;
using MahApps.Metro.Controls.Dialogs;

namespace SysSped.Apresentation.Wpf.Views
{
    /// <summary>
    /// Lógica interna para ErrorWindow.xaml
    /// </summary>
    public partial class ErrorWindow : MetroWindow
    {
        public ErrorWindow()
        {
            InitializeComponent();
            Erro();
        }

        public async void Erro()
        {
            await this.ShowMessageAsync("Ocorreu um erro com o MySql.", "Um erro ocorreu ao tentar se conectar com o banco de dados.");
            return;
        }
    }
}
