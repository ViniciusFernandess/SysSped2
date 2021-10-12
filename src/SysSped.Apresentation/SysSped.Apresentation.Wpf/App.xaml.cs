using Ninject;
using SysSped.Apresentation.Wpf.Views;
using SysSped.Domain.Interfaces;
using SysSped.Infra.CrossCutting.Excel;
using SysSped.Infra.CrossCutting.Txt;
using SysSped.Infra.Data;
using System.Windows;

namespace SysSped.Apresentation.Wpf
{
    /// <summary>
    /// Interação lógica para App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IKernel container;


        private void ConfigureContainer()
        {
            this.container = new StandardKernel();
            container.Bind<IImportacaoRepository>().To<ImportacaoRepository>().InThreadScope();
            container.Bind<ILogRepository>().To<LogRepository>();
            container.Bind<IExcelService>().To<ArquivoImportacaoService>().InThreadScope();
            container.Bind<ITxtService>().To<TxtService>();
            container.Bind<ILogSpedService>().To<LogSpedService>();

        }

        private void ComposeObjects()
        {
            Current.MainWindow = this.container.Get<MainWindow>();
            Current.MainWindow.Title = "Conversor SPED";
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {

                var _mySqlServicebase = new MySQLDatabaseRepository(true);

                var userJaExiste = _mySqlServicebase.VerificaUserExiste();

                if (!userJaExiste)
                    _mySqlServicebase.CriaUser();

                var bdJaExiste = _mySqlServicebase.VerificaBDExiste();

                if (!bdJaExiste)
                    _mySqlServicebase.CriaBD();


                var _mySqlService = new MySQLDatabaseRepository();

                if (!_mySqlService.VerificarTableExiste("tiposped"))
                    _mySqlService.CriarTableTipoSped();

                if (!_mySqlService.VerificarTableExiste("c170"))
                    _mySqlService.CriarTableC170();

                if (!_mySqlService.VerificarTableExiste("bloco0000"))
                    _mySqlService.CriarTableBloco0000();

                if (!_mySqlService.VerificarTableExiste("rowimportacao"))
                    _mySqlService.CriarTableRowImportacao();

                if (!_mySqlService.VerificarTableExiste("logalteracaosped"))
                    _mySqlService.CriarTableLogAlteracaoSped();

                // Composite setting


                base.OnStartup(e);
                ConfigureContainer();
                ComposeObjects();

                Current.MainWindow.Show();

            }
            catch (System.Exception ex)
            {
                base.OnStartup(e);
                ConfigureContainer();
                Current.MainWindow = this.container.Get<ErrorWindow>();


                Current.MainWindow.Show();
            }
        }


        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("An unhandled exception just occurred: " + e.Exception.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            e.Handled = true;


        }
    }
}
