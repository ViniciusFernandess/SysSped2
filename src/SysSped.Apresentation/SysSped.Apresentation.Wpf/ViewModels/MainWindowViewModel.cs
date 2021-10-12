using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using OfficeOpenXml;
using Prism.Commands;
using Prism.Mvvm;
using SysSped.Apresentation.Wpf.Interface.ViewModels;
using SysSped.Apresentation.Wpf.Views;
using SysSped.Domain.Core;
using SysSped.Domain.Interfaces;
using SysSped.Domain.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SysSped.Apresentation.Wpf.ViewModels
{
    public class MainWindowViewModel : BindableBase, IMainWindowViewModel
    {
        private readonly IImportacaoRepository _repoImportacao;
        private readonly ILogRepository _logRepo;
        private readonly IExcelService _servExcel;
        private readonly ITxtService _servSped;
        private readonly ILogSpedService _servLogSped;

        public MetroWindow _MetroWindow { get; set; }
        public ProgressDialogController Progresso { get; set; }


        public MainWindowViewModel(MetroWindow metroWindow, IImportacaoRepository repoImportacao, ILogRepository logRepo, IExcelService servExcel, ITxtService servSped, ILogSpedService servLogSped)
        {
            _repoImportacao = repoImportacao;
            _logRepo = logRepo;
            _servExcel = servExcel;
            _servSped = servSped;
            _servLogSped = servLogSped;
            _MetroWindow = metroWindow;

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            BtnAtualizarVisibility = Visibility.Hidden;
            BtnCorrigirSpedVisibility = Visibility.Hidden;
            BtnReImportarVisibility = Visibility.Hidden;
        }

        #region Props

        private string _modo;
        public string Modo
        {
            get { return _modo; }
            set { SetProperty(ref _modo, value); }
        }

        private string _txtFile;
        public string TxtFile
        {
            get { return _txtFile; }
            set { SetProperty(ref _txtFile, value); }
        }

        private string _txtEditor;
        public string TxtEditor
        {
            get { return _txtEditor; }
            set { SetProperty(ref _txtEditor, value); }
        }

        private Visibility _btnAtualizarVisibility;
        public Visibility BtnAtualizarVisibility
        {
            get { return _btnAtualizarVisibility; }
            set { SetProperty(ref _btnAtualizarVisibility, value); }
        }

        private Visibility _btnReImportarVisibility;
        public Visibility BtnReImportarVisibility
        {
            get { return _btnReImportarVisibility; }
            set { SetProperty(ref _btnReImportarVisibility, value); }
        }

        private Visibility _btnCorrigirSpedVisibility;
        public Visibility BtnCorrigirSpedVisibility
        {
            get { return _btnCorrigirSpedVisibility; }
            set { SetProperty(ref _btnCorrigirSpedVisibility, value); }
        }
        #endregion

        #region Commands
        private DelegateCommand _exibirTelaRelatorioLog;
        public DelegateCommand ExibirTelaRelatorioLogCommand => _exibirTelaRelatorioLog ?? (_exibirTelaRelatorioLog = new DelegateCommand(ExibirTelaRelatorioLog));


        private DelegateCommand _aplicarModoImportarPlanilhaCommand;
        public DelegateCommand AplicarModoImportarPlanilhaCommand => _aplicarModoImportarPlanilhaCommand ?? (_aplicarModoImportarPlanilhaCommand = new DelegateCommand(AplicarModoImportarPlanilha));


        private DelegateCommand _escolherArquivoCommand;
        public DelegateCommand EscolherArquivoCommand => _escolherArquivoCommand ?? (_escolherArquivoCommand = new DelegateCommand(EscolherArquivo));


        private DelegateCommand _aplicarModoCorrigirSpedCommand;
        public DelegateCommand AplicarModoCorrigirSpedCommand => _aplicarModoCorrigirSpedCommand ?? (_aplicarModoCorrigirSpedCommand = new DelegateCommand(AplicarModoCorrigirSped));


        private DelegateCommand _atualizarCommand;
        public DelegateCommand AtualizarCommand => _atualizarCommand ?? (_atualizarCommand = new DelegateCommand(Atualizar));


        private DelegateCommand _reImportarCommand;
        public DelegateCommand ReimportarCommand => _reImportarCommand ?? (_reImportarCommand = new DelegateCommand(ReImportar));


        private DelegateCommand _corrigirSpedCommand;
        public DelegateCommand CorrigirSpedCommand => _corrigirSpedCommand ?? (_corrigirSpedCommand = new DelegateCommand(CorrigirSped));

        public RelatorioLogWindow RelatorioLog { get; }
        #endregion

        private void ExibirTelaRelatorioLog()
        {
            _MetroWindow.Visibility = Visibility.Collapsed;

            var relatorioLogWindow = new RelatorioLogWindow(_logRepo, _servLogSped);
            relatorioLogWindow.ShowDialog();

            _MetroWindow.Visibility = Visibility.Visible;
        }

        private void AplicarModoImportarPlanilha()
        {
            DesabilitarTodosBotoes();

            Modo = "Planilha";
            BtnAtualizarVisibility = Visibility.Visible;
            BtnReImportarVisibility = Visibility.Visible;

            EscolherArquivo();
        }

        private void AplicarModoCorrigirSped()
        {
            DesabilitarTodosBotoes();

            Modo = "Sped";
            BtnCorrigirSpedVisibility = Visibility.Visible;

            EscolherArquivo();
        }

        private void DesabilitarTodosBotoes()
        {
            BtnAtualizarVisibility = Visibility.Hidden;
            BtnReImportarVisibility = Visibility.Hidden;
            BtnCorrigirSpedVisibility = Visibility.Hidden;
            TxtFile = "...";
        }

        private void EscolherArquivo()
        {

            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (Modo == "Sped")
                openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.txt)|*.txt";
            else
                openFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.xlsx)|*.xlsx";

            if (openFileDialog.ShowDialog() == true)
                TxtFile = openFileDialog.FileName;
        }

        private async void Atualizar()
        {
            bool teveErro = false;
            var mensagemErro = "";

            var arquivo = new FileInfo(TxtFile);
            if (string.IsNullOrEmpty(TxtFile) || !arquivo.Exists || (arquivo.Extension.ToLower() != ".xlsx" && arquivo.Extension.ToLower() != ".xls"))
            {
                await _MetroWindow.ShowMessageAsync("Correção de Sped", "Planilha não encontrada.");
                return;
            }


            Progresso = await _MetroWindow.ShowProgressAsync("Por favor, aguarde.", "Processando...");
            Progresso.SetIndeterminate();

            var serv = new ImportacaoService(_repoImportacao, _servExcel);
            var pkg = new ExcelPackage(new FileInfo(TxtFile));

            // marcar tempo
            var timer = new Stopwatch();
            timer.Start();

            try
            {
                await Task.Factory.StartNew(() => serv.AtualizarBase(pkg));
            }
            catch (Exception ex)
            {
                teveErro = true;
                mensagemErro = ex.Message + "\n" + ex.InnerException?.Message ?? "";
            }

            if (serv.IsValid())
                TxtEditor = "Base atualizada com sucesso.";
            else
                TxtEditor = string.Join(@"\m", serv.Erros.Select(x => x.Mensagem));

            timer.Stop();
            TimeSpan timeTaken = timer.Elapsed;
            //TxtEditor += "\nTime taken: " + timeTaken.ToString(@"m\:ss\.fff");

            await Progresso?.CloseAsync();


            if (teveErro)
                await _MetroWindow.ShowMessageAsync("Ocorreu um erro inesperado ao tentar atualizar a base.", mensagemErro);
            else if (serv.IsValid())
                await _MetroWindow.ShowMessageAsync("Importação da base", "Finalizado com sucesso!");
            else
                await _MetroWindow.ShowMessageAsync("Erro ao importar planilha.", TxtEditor);

        }

        private async void ReImportar()
        {
            var arquivo = new FileInfo(TxtFile);
            bool sucesso = false;
            bool teveErro = false;
            var mensagemErro = "";

            if (string.IsNullOrEmpty(TxtFile) || !arquivo.Exists || (arquivo.Extension.ToLower() != ".xlsx" && arquivo.Extension.ToLower() != ".xls"))
            {
                await _MetroWindow.ShowMessageAsync("Correção de Sped", "Planilha não encontrada.");
                return;
            }

            Progresso = await _MetroWindow.ShowProgressAsync("Por favor, aguarde.", "Processando...");
            Progresso.SetIndeterminate();

            // marcar tempo
            var timer = new Stopwatch();
            timer.Start();

            await Task.Factory.StartNew(() =>
            {
                try
                {
                    var serv = new ImportacaoService(_repoImportacao, _servExcel);

                    var pkg = new ExcelPackage(new FileInfo(TxtFile));
                    serv.RenovarBase(pkg);

                    if (serv.IsValid())
                        TxtEditor = "Importado com sucesso.";
                    else
                        TxtEditor = string.Join(@"\n", serv.Erros.Select(x => x.Mensagem));

                    sucesso = serv.IsValid();
                }
                catch (Exception ex)
                {
                    teveErro = true;
                    mensagemErro = ex.Message + "\n" + ex.InnerException?.Message ?? "";
                }

            });

            timer.Stop();
            TimeSpan timeTaken = timer.Elapsed;
            //TxtEditor += "\nTempo de processo: " + timeTaken.ToString(@"hh\:mm\:ss\.fff");

            await Progresso?.CloseAsync();

            if (teveErro)
                await _MetroWindow.ShowMessageAsync("Ocorreu um erro inexperado ao tentar importar a base.", mensagemErro);
            else if (sucesso)
                await _MetroWindow.ShowMessageAsync("Importação da base", "Finalizado com sucesso!");
            else
                await _MetroWindow.ShowMessageAsync("Erro ao importar planilha.", TxtEditor);
        }

        private async void CorrigirSped()
        {
            bool teveerro = false;
            var mensagemErro = "";
            var arquivo = new FileInfo(TxtFile);
            if (string.IsNullOrEmpty(TxtFile) || !arquivo.Exists || arquivo.Extension.ToLower() != ".txt")
            {
                await _MetroWindow.ShowMessageAsync("Correção de Sped", "Sped não encontrado.");
                return;
            }

            var _base = _repoImportacao.ObterImportacaoAtiva();

            var baseTemEanRepetido = _base.Any() && _base.Count() != _base.Select(x => x.ean.Trim().ToLower()).Distinct().Count();
            if (baseTemEanRepetido)
            {
                await _MetroWindow.ShowMessageAsync("Contém EAN repetido na base.", "Favor reimporte a base.");
                return;
            }

            var baseTemCodigoRepetido = _base.Any() && _base.Count() != _base.Select(x => x.codigointerno.Trim().ToLower()).Distinct().Count();
            if (baseTemCodigoRepetido)
            {
                await _MetroWindow.ShowMessageAsync("Contém Código de Produto repetido na  base.", "Favor reimporte a base.");
                return;
            }


            var dialog = new MetroDialogSettings();
            dialog.AffirmativeButtonText = "EAN + Código Interno";
            dialog.NegativeButtonText = "     EAN   +   NCM     ";

            if (Progresso?.IsOpen ?? false)
                await Progresso?.CloseAsync();
            var result = await _MetroWindow.ShowMessageAsync("Modo de busca de produto.", "Favor escolha uma opção para buscar o produto do SPED na base de dados.", MessageDialogStyle.AffirmativeAndNegative, dialog);

            var regraPesquisaProd = result == MessageDialogResult.Affirmative ? EnumRegraPesquisaProd.EAN_MAIS_CODINTERNO : EnumRegraPesquisaProd.EAN_MAIS_NCM;


            Progresso = await _MetroWindow.ShowProgressAsync("Por favor, aguarde.", "Processando...");
            Progresso.SetIndeterminate();

            // marcar tempo
            var timer = new Stopwatch();
            timer.Start();

            bool spedteveAlteracao = false;
            string caminhoLogDest = "";
            string caminhoLogC170NaoAlteradosDest = "";
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    var txtArquivo = ObtemTextoLidoArquivo(TxtFile);

                    var sped = _servSped.ExecutaLeitura(txtArquivo);
                 
                    var serv = new CorrecaoSpedService(_repoImportacao, _logRepo, regraPesquisaProd);
                    spedteveAlteracao = serv.TratarSped(sped, txtArquivo);

                    var arquivoIrginal = new FileInfo(TxtFile);

                    if (spedteveAlteracao)
                    {
                        caminhoLogDest = arquivoIrginal.FullName.Replace(arquivoIrginal.Extension, $@"_Corrigido_{DateTime.Now.ToString("dd-MM-yyyy HHmmss")}{arquivoIrginal.Extension}");

                        File.WriteAllLines(caminhoLogDest, txtArquivo);
                    }

                    caminhoLogC170NaoAlteradosDest = _servLogSped.ExtrairRelatorioC100NaoTratado(sped, arquivoIrginal.DirectoryName);
                }
                catch (Exception ex)
                {
                    teveerro = true;
                    mensagemErro = ex.Message.ToString() + "\n" + ex.InnerException?.Message ?? "";
                }

            });

            timer.Stop();
            TimeSpan timeTaken = timer.Elapsed;
            TxtEditor += "\nTime taken: " + timeTaken.ToString(@"hh\:mm\:ss\.fff");

            if (teveerro)
            {
                await Progresso?.CloseAsync();
                await _MetroWindow.ShowMessageAsync("Ocorreu um erro inexperado ao tentar corrigir o SPED.", mensagemErro);
            }
            else if (spedteveAlteracao)
            {
                await Progresso?.CloseAsync();
                await _MetroWindow.ShowMessageAsync("Correção de Sped", "Finalizado com sucesso!");

                if (!string.IsNullOrEmpty(caminhoLogDest) && File.Exists(caminhoLogDest))
                    System.Diagnostics.Process.Start(caminhoLogDest);

                if (!string.IsNullOrEmpty(caminhoLogC170NaoAlteradosDest) && File.Exists(caminhoLogC170NaoAlteradosDest))
                    System.Diagnostics.Process.Start(caminhoLogC170NaoAlteradosDest);
            }
            else
            {
                if (Progresso?.IsOpen ?? false)
                    await Progresso?.CloseAsync();
                await _MetroWindow.ShowMessageAsync("Correção de Sped", "Não foram encontrados produtos em sua base de dados compativel com esse arquivo SPED! O sistema não realizou nenhuma alteração.");


                if (!string.IsNullOrEmpty(caminhoLogC170NaoAlteradosDest) && File.Exists(caminhoLogC170NaoAlteradosDest))
                    System.Diagnostics.Process.Start(caminhoLogC170NaoAlteradosDest);
            }
        }


        private string[] ObtemTextoLidoArquivo(string caminhoArquivo)
        {
            return File.ReadAllLines(caminhoArquivo, Encoding.Default);
        }
    }
}
