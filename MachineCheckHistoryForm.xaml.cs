using MachineCheck.Models;
using MachineCheck.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MachineCheck
{
    public partial class MachineCheckHistoryForm : Window
    {
        private readonly long _machineId;
        private readonly DatabaseService _dbService;
        public ObservableCollection<MachineCheckRecord> MachineChecks { get; set; } = new ObservableCollection<MachineCheckRecord>();

        public MachineCheckHistoryForm(long machineId, DatabaseService dbService)
        {
            InitializeComponent();
            _machineId = machineId;
            _dbService = dbService;

            dgHistory.ItemsSource = MachineChecks;

            _ = LoadHistoryAsync();
        }

        private async Task LoadHistoryAsync()
        {
            try
            {
                var history = await _dbService.GetMachineCheckHistoryAsync(_machineId);

                MachineChecks.Clear();
                foreach (var check in history)
                {
                    MachineChecks.Add(check);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd ładowania historii: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteCheckButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is MachineCheckRecord check)
            {
                var result = MessageBox.Show("Czy na pewno chcesz usunąć ten przegląd?", "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _dbService.DeleteMachineCheckAsync(check.CheckId);
                        var itemToRemove = MachineChecks.FirstOrDefault(c => c.CheckId == check.CheckId);
                        if (itemToRemove != null)
                        {
                            MachineChecks.Remove(itemToRemove);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Nie można usunąć przeglądu. {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void OpenPdfButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is MachineCheckRecord check)
            {
                try
                {
                    string tempPath = Path.Combine(Path.GetTempPath(), $"Protocol_{check.CheckId}.pdf");
                    File.WriteAllBytes(tempPath, check.ProtocolPdf);
                    Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Nie można otworzyć protokołu PDF. {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}