using MachineCheck.Models;
using MachineCheck.Services;
using MachineCheck.ViewModels;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace MachineCheck
{
    public partial class MachineCheckForm : Window
    {
        private readonly long _machineId;
        private readonly int _opeId;
        private readonly DatabaseService _dbService;
        private readonly MachineViewModel _machineVM;

        public MachineCheckForm(long machineId, int opeId, DatabaseService dbService)
        {
            InitializeComponent();

            var culture = new CultureInfo("en-CA");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            dpFirstCheck.Language = XmlLanguage.GetLanguage(culture.IetfLanguageTag);
            dpLastCheck.Language = XmlLanguage.GetLanguage(culture.IetfLanguageTag);

            _machineId = machineId;
            _opeId = opeId;
            _dbService = dbService;

            _machineVM = new MachineViewModel();
            this.DataContext = _machineVM;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await LoadMachineDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wystąpił nieznany błąd przy ładowaniu formularza. Poinformuj adminisratora. {Environment.NewLine}{ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadMachineDataAsync()
        {
            try
            {
                var machine = await _dbService.GetMachineCheckDataAsync(_machineId);
                if (machine == null)
                {
                    MessageBox.Show("Nie znaleziono maszyny. Upewnij się, że maszyna jest dodana do środków trwałych.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                }

                _machineVM.MachineId = _machineId;
                _machineVM.MachineName = machine.Name;
                _machineVM.MachineCode = machine.Code;
                _machineVM.MachineInternalNumber = machine.InternalCode;
                _machineVM.MachineNumber = machine.NrEwidencyjny;
                _machineVM.MachineSerialNumber = machine.SerialNumber;
                _machineVM.MachineBuyDate = machine.BuyDate;
                _machineVM.LastCheckDate = machine.LastCheckDate;
                _machineVM.FirstCheckDate = machine.FirstCheckDate;
                _machineVM.CheckInterval = machine.CheckInterval ?? 3;
                _machineVM.DaysBeforeNotif = machine.DaysBeforeNotif ?? 14;
                _machineVM.NotifEmail = machine.NotifEmail ?? "michal.g@gaska.com.pl";
                _machineVM.IsSaved = machine.IsSaved;

                if (_machineVM.LastCheckDate is null)
                {
                    dpLastCheckLabel.Visibility = Visibility.Collapsed;
                    dpLastCheck.Visibility = Visibility.Collapsed;
                    dpFirstCheck.Visibility = Visibility.Visible;
                    dpFirstCheckLabel.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wystąpił nieznany błąd przy ładowaniu formularza. Poinformuj administratora.{Environment.NewLine}{ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateProtocolButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_machineVM.IsSaved)
                {
                    MessageBox.Show("Nie można wygenerować protokołu, ponieważ nie zapisano parametrów przeglądu maszyny. Uzupełnij parametry i kliknij przycisk 'Zapisz'.", "Uwaga", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var additionalParamsForm = new AdditionalPrintingParamsForm(_machineVM.MachineSerialNumber);
                bool? result = additionalParamsForm.ShowDialog();

                if (result != true)
                    return;

                var additionalParams = additionalParamsForm.ToParameterDictionary();

                string exe = @"\\Backup\k\DrukowanieMetekProdukcja\PrintReport\PrintReport.exe";
                string rpt = @"\\Backup\k\DrukowanieMetekProdukcja\PrintReport\MachineProtocol.rpt";

                // Start building args string
                var argsBuilder = new StringBuilder();
                argsBuilder.Append($"--rpt \"{rpt}\" ");
                argsBuilder.Append("--copies 1 ");

                argsBuilder.Append($"--param \"Nr Ewidencyjny={_machineVM.MachineNumber}\" ");
                argsBuilder.Append($"--param \"Nazwa={_machineVM.MachineName}\" ");
                argsBuilder.Append($"--param \"Data zakupu={_machineVM.MachineBuyDate}\" ");
                argsBuilder.Append($"--param \"Numer Wewnętrzny={_machineVM.MachineInternalNumber}\" ");
                argsBuilder.Append($"--param \"NextDate={_machineVM.NextCheckDate}\" ");

                // Add additional params from the form
                foreach (var kvp in additionalParams)
                {
                    // Escape quotes just in case
                    string value = kvp.Value.Replace("\"", "\\\"");
                    argsBuilder.Append($"--param \"{kvp.Key}={value}\" ");
                }

                var psi = new ProcessStartInfo
                {
                    FileName = exe,
                    Arguments = argsBuilder.ToString(),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var proc = Process.Start(psi))
                {
                    string stdout = proc.StandardOutput.ReadToEnd();
                    string stderr = proc.StandardError.ReadToEnd();
                    proc.WaitForExit();

                    int exitCode = proc.ExitCode;

                    if (exitCode == 0) MessageBox.Show("Przekazano wydruk do drukarki.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                    else MessageBox.Show($"Błąd wydruku (kod {exitCode}).\n{stderr}\n{stdout}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wystąpił błąd przy generowaniu formularza. Poinformuj administratora.\n{ex}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_machineVM.IsSaved)
                {
                    MessageBox.Show("Nie można wygenerować protokołu, ponieważ nie zapisano parametrów przeglądu maszyny. Uzupełnij parametry i kliknij przycisk 'Zapisz'.", "Uwaga", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Let user choose file
                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Title = "Wybierz plik PDF do załączenia",
                    Filter = "Pliki PDF (*.pdf)|*.pdf",
                    CheckFileExists = true,
                    CheckPathExists = true
                };

                byte[] fileBytes = null;
                string filePath = string.Empty;

                bool? result = openFileDialog.ShowDialog();

                if (result == true)
                {
                    filePath = openFileDialog.FileName;
                    fileBytes = File.ReadAllBytes(filePath);
                }
                else
                {
                    MessageBox.Show("Nie wybrano pliku. Operacja została anulowana.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Execute machine check with file bytes
                bool success = await _dbService.ExecuteMachineCheckAsync(_machineVM.MachineId, _opeId, Path.GetFileName(filePath), fileBytes, Path.GetExtension(filePath));

                if (success)
                {
                    _machineVM.LastCheckDate = DateTime.Now;

                    dpLastCheck.Visibility = Visibility.Visible;
                    dpLastCheckLabel.Visibility = Visibility.Visible;
                    dpFirstCheck.Visibility = Visibility.Hidden;
                    dpFirstCheckLabel.Visibility = Visibility.Hidden;

                    MessageBox.Show("Pomyślnie wykonano przegląd.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Nie udało się wykonać przeglądu maszyny. Spróbuj ponownie.", "Uwaga", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wystąpił błąd przy wykonywaniu przeglądu danych. Poinformuj administratora.{Environment.NewLine}{ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_machineVM.FirstCheckDate.HasValue)
                {
                    MessageBox.Show("Nie można zapisać danych, ponieważ data pierwszego przeglądu jest pusta.", "Uwaga", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var data = new MachineData
                {
                    Id = _machineVM.MachineId,
                    Code = _machineVM.MachineCode,
                    FirstCheckDate = _machineVM.FirstCheckDate,
                    CheckInterval = _machineVM.CheckInterval,
                    DaysBeforeNotif = _machineVM.DaysBeforeNotif,
                    NotifEmail = _machineVM.NotifEmail,
                    SerialNumber = _machineVM.MachineSerialNumber
                };

                bool success = await _dbService.SaveMachineCheckDataAsync(data);

                if (success)
                {
                    MessageBox.Show("Dane przeglądu maszyny zostały zapisane pomyślnie.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                    _machineVM.IsSaved = true;
                }
                else
                    MessageBox.Show("Nie zapisano żadnych danych. Sprawdź wprowadzone wartości.", "Uwaga", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wystąpił błąd przy zapisie danych. Poinformuj administratora.{Environment.NewLine}{ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            var historyWindow = new MachineCheckHistoryForm(_machineId, _dbService);
            historyWindow.Owner = this;
            historyWindow.ShowDialog();
        }
    }
}