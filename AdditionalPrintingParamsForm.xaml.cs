using System.Collections.Generic;
using System.Windows;

namespace MachineCheck
{
    public partial class AdditionalPrintingParamsForm : Window
    {
        // Text fields
        public string SerialNumber => SerialNumberTextBox.Text;

        public string CheckPersons => CheckPersonsTextBox.Text;
        public string GeneralConditionNotes => GeneralConditionNotesTextBox.Text;
        public string ElectricalNotes => ElectricalNotesTextBox.Text;
        public string BHPNotes => BHPNotesTextBox.Text;
        public string AdditionalMeasurements => AdditionalMeasurmentsTextBox.Text;
        public string PrecisionNotes => PrecisionTextBox.Text;
        public string FinalSuggestions => FinalSuggestionsTextBox.Text;
        public string ResponsiblePerson => ResponsiblePersonTextBox.Text;

        // Checkboxes
        public bool GeneralConditionChecked => GeneralConditionCheckBox.IsChecked == true;

        public bool ElectricalChecked => ElectricalCheckBox.IsChecked == true;
        public bool BHPChecked => BHPCheckBox.IsChecked == true;
        public bool PrecisionChecked => PrecisionCheckBox.IsChecked == true;
        public bool PrecisionMeasurementsChecked => PrecisionMeasurementsCheckBox.IsChecked == true;
        public bool DialIndicatorChecked => DialIndicatorCheckBox.IsChecked == true;
        public bool CaliperChecked => CaliperCheckBox.IsChecked == true;
        public bool MicrometerChecked => MicrometerCheckBox.IsChecked == true;
        public bool GaugeChecked => GaugeCheckBox.IsChecked == true;
        public bool ProtractorChecked => ProtractorCheckBox.IsChecked == true;
        public bool CurrentMeterChecked => CurrentMeterCheckBox.IsChecked == true;

        public AdditionalPrintingParamsForm(string serialNumber = null)
        {
            InitializeComponent();

            SerialNumberTextBox.Text = serialNumber ?? string.Empty;
            GeneralConditionNotesTextBox.Text = "Sprawdzone";
            ElectricalNotesTextBox.Text = "Sprawdzone";
            BHPNotesTextBox.Text = "Sprawdzone";
            PrecisionTextBox.Text = "Sprawdzone";
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        /// <summary>
        /// Convert all params to a dictionary suitable for Crystal Reports.
        /// </summary>
        public Dictionary<string, string> ToParameterDictionary()
        {
            return new Dictionary<string, string>
            {
                { "SerialNumber", SerialNumber },
                { "CheckPersons", CheckPersons },
                { "GeneralConditionChecked", GeneralConditionChecked.ToString() },
                { "GeneralConditionNotes", GeneralConditionNotes },
                { "ElectricalChecked", ElectricalChecked.ToString() },
                { "ElectricalNotes", ElectricalNotes },
                { "BHPChecked", BHPChecked.ToString() },
                { "BHPNotes", BHPNotes },
                { "PrecisionChecked", PrecisionChecked.ToString() },
                { "PrecisionMeasurementsChecked", PrecisionMeasurementsChecked.ToString() },
                { "DialIndicatorChecked", DialIndicatorChecked.ToString() },
                { "CaliperChecked", CaliperChecked.ToString() },
                { "MicrometerChecked", MicrometerChecked.ToString() },
                { "GaugeChecked", GaugeChecked.ToString() },
                { "ProtractorChecked", ProtractorChecked.ToString() },
                { "CurrentMeterChecked", CurrentMeterChecked.ToString() },
                { "AdditionalMeasurements", AdditionalMeasurements },
                { "PrecisionNotes", PrecisionNotes },
                { "FinalSuggestions", FinalSuggestions },
                { "ResponsiblePerson", ResponsiblePerson }
            };
        }
    }
}