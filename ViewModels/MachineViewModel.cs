using System;
using System.ComponentModel;

namespace MachineCheck.ViewModels
{
    public class MachineViewModel : INotifyPropertyChanged
    {
        private long _machineId;
        private string _machineName;
        private string _machineCode;
        private string _machineNumber;
        private string _machineSerialNumber;
        private string _machineInternalNumber;
        private DateTime _machineBuyDate;
        private int? _checkInterval;
        private DateTime? _firstCheckDate;
        private DateTime? _lastCheckDate;
        private string _nextCheckDate;
        private int? _daysBeforeNotif;
        private string _notifEmail;
        private bool _isSaved;

        public bool CanShowHistory => LastCheckDate.HasValue;

        public long MachineId
        {
            get => _machineId;
            set { _machineId = value; OnPropertyChanged(nameof(MachineId)); }
        }

        public string MachineName
        {
            get => _machineName;
            set { _machineName = value; OnPropertyChanged(nameof(MachineName)); }
        }

        public string MachineCode
        {
            get => _machineCode;
            set { _machineCode = value; OnPropertyChanged(nameof(MachineCode)); }
        }

        public string MachineNumber
        {
            get => _machineNumber;
            set { _machineNumber = value; OnPropertyChanged(nameof(MachineNumber)); }
        }

        public string MachineSerialNumber
        {
            get => _machineSerialNumber;
            set { _machineSerialNumber = value; OnPropertyChanged(nameof(MachineSerialNumber)); }
        }

        public string MachineInternalNumber
        {
            get => _machineInternalNumber;
            set { _machineInternalNumber = value; OnPropertyChanged(nameof(MachineInternalNumber)); }
        }

        public DateTime MachineBuyDate
        {
            get => _machineBuyDate;
            set { _machineBuyDate = value; OnPropertyChanged(nameof(MachineBuyDate)); }
        }

        public int? CheckInterval
        {
            get => _checkInterval;
            set
            {
                _checkInterval = value;
                OnPropertyChanged(nameof(CheckInterval));
                UpdateNextCheck();
            }
        }

        public DateTime? FirstCheckDate
        {
            get => _firstCheckDate;
            set
            {
                _firstCheckDate = value;
                OnPropertyChanged(nameof(FirstCheckDate));
                UpdateNextCheck();
            }
        }

        public DateTime? LastCheckDate
        {
            get => _lastCheckDate;
            set
            {
                _lastCheckDate = value;
                OnPropertyChanged(nameof(LastCheckDate));
                OnPropertyChanged(nameof(CanShowHistory));
                UpdateNextCheck();
            }
        }

        public string NextCheckDate
        {
            get => _nextCheckDate;
            private set { _nextCheckDate = value; OnPropertyChanged(nameof(NextCheckDate)); }
        }

        public int? DaysBeforeNotif
        {
            get => _daysBeforeNotif;
            set { _daysBeforeNotif = value; OnPropertyChanged(nameof(DaysBeforeNotif)); }
        }

        public string NotifEmail
        {
            get => _notifEmail;
            set { _notifEmail = value; OnPropertyChanged(nameof(NotifEmail)); }
        }

        private void UpdateNextCheck()
        {
            if (LastCheckDate.HasValue && CheckInterval.HasValue)
            {
                NextCheckDate = LastCheckDate.Value.AddMonths(CheckInterval.Value).ToShortDateString();
            }
            else if (FirstCheckDate.HasValue && CheckInterval.HasValue)
            {
                NextCheckDate = FirstCheckDate.Value.AddMonths(CheckInterval.Value).ToShortDateString();
            }
            else
            {
                NextCheckDate = string.Empty;
            }
        }

        public bool IsSaved
        {
            get => _isSaved;
            set { _isSaved = value; OnPropertyChanged(nameof(IsSaved)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}