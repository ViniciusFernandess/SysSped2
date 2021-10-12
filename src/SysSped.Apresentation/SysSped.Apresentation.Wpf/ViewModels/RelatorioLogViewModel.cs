using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Prism.Commands;
using Prism.Mvvm;
using SysSped.Apresentation.Wpf.Interface.ViewModels;
using SysSped.Apresentation.Wpf.Models;
using SysSped.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace SysSped.Apresentation.Wpf.ViewModels
{
    public class RelatorioLogViewModel : BindableBase, IRelatorioLogViewModel
    {
        public MetroWindow _MetroWindow { get; set; }
        public readonly ILogSpedService _logSpedService;

        private readonly ILogRepository _logRepository;
        public ProgressDialogController Progresso { get; set; }

        public RelatorioLogViewModel(MetroWindow metroWindow, ILogRepository logRepository, ILogSpedService logSpedService)
        {
            _MetroWindow = metroWindow;
            _logRepository = logRepository;
            _logSpedService = logSpedService;

            Blocos0000 = ObterBlocos0000Mapeado();

            OpcoesLog = new ObservableCollection<Bloco0000Model>(Blocos0000);
        }

        private List<Bloco0000Model> Blocos0000 { get; set; }

        private ObservableCollection<Bloco0000Model> _opcoesLog;
        public ObservableCollection<Bloco0000Model> OpcoesLog
        {
            get { return _opcoesLog; }
            set
            {
                _opcoesLog = value;
                RaisePropertyChanged("OpcoesLog");
            }
        }

        private DateTime? _dt_ini;
        public DateTime? Dt_ini
        {
            get { return _dt_ini; }
            set { _dt_ini = value; }
        }

        private DateTime? _dt_fim;
        public DateTime? Dt_fim
        {
            get { return _dt_fim; }
            set { _dt_fim = value; }
        }

        private string _txtCaminhoDestino;
        public string TxtCaminhoDestino
        {
            get { return _txtCaminhoDestino; }
            set { SetProperty(ref _txtCaminhoDestino, value); }
        }

        private DelegateCommand _extrairRelatorioCommand;
        public DelegateCommand ExtrairRelatorioCommand => _extrairRelatorioCommand ?? (_extrairRelatorioCommand = new DelegateCommand(ExtrairRelatorio));

        private DelegateCommand _filtrarDataCommand;
        public DelegateCommand FiltrarDataCommand => _filtrarDataCommand ?? (_filtrarDataCommand = new DelegateCommand(FiltrarData));

        private DelegateCommand _selecionarPastaDestinoCommand;
        public DelegateCommand SelecionarPastaDestinoCommand => _selecionarPastaDestinoCommand ?? (_selecionarPastaDestinoCommand = new DelegateCommand(SelecionarPastaDestino));

        private void FiltrarData()
        {
            if (!Dt_ini.HasValue || !Dt_fim.HasValue)
            {
                _MetroWindow.ShowMessageAsync("Erro ao filtrar", "Favor insira um período válido!");
                return;
            }

            var filtados = Blocos0000.Where(x => x.DataCadastro.Date >= Dt_ini.Value.Date && x.DataCadastro.Date <= Dt_fim.Value.Date);
            OpcoesLog = new ObservableCollection<Bloco0000Model>(filtados);
        }

        private void SelecionarPastaDestino()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            TxtCaminhoDestino = dialog.SelectedPath;
        }

        private async void ExtrairRelatorio()
        {
            SelecionarPastaDestino();

            if (string.IsNullOrEmpty(TxtCaminhoDestino))
            {
                await _MetroWindow.ShowMessageAsync("Extração de Relatório.", "Favor Escolher uma pasta destino para o relatório.");
                return;
            }

            var idsSelecionados = OpcoesLog.Where(o => o.IsSelecionado).Select(x => x.Id);

            if (!idsSelecionados.Any())
            {
                await _MetroWindow.ShowMessageAsync("Extração de Relatório.", "Favor Escolher um relatório.");
                return;
            }

            var blocosSelecionados = _logRepository.ObterBloco0000Ativos(idsSelecionados);

            var caminhoDest = _logSpedService.ExtrairRelatorioAlteracoesSped(blocosSelecionados, TxtCaminhoDestino);

            await _MetroWindow.ShowMessageAsync("Gerar Relatório", "Processo finalizado com sucesso!");

            if (!string.IsNullOrEmpty(caminhoDest) && File.Exists(caminhoDest))
                System.Diagnostics.Process.Start(caminhoDest);
        }

        private List<Bloco0000Model> ObterBlocos0000Mapeado()
        {
            var models = _logRepository.ObterBloco0000Ativos();

            var listModel = new List<Bloco0000Model>();
            foreach (var c in models)
            {
                var model = new Bloco0000Model
                {
                    Id = c.Id,
                    CNPJ = c.CNPJ,
                    NOME = c.NOME,
                    DataCadastro = c.DataCadastro,
                    DT_INI = c.DT_INI,
                    DT_FIN = c.DT_FIN
                };

                listModel.Add(model);
            }

            return listModel;
        }
    }
}
