using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace WPath
{
    public class DirectoryScanner
    {
        public DirectoryScanner(List<string> paths)
        {
            Paths = paths.Select(x => new PathComponent(x)).ToList();
            Task.Factory.StartNew(() => ExistenceChecker(), TaskCreationOptions.LongRunning);
        }

        private void ExistenceChecker()
        {
            while(true)
            {
                foreach (var entry in Paths)
                {
                    entry.Exists = Directory.Exists(entry.Path);
                    entry.FileExists = File.Exists(entry.Path);
                    Debug.WriteLine($"Entry {entry.Path} - exists: {entry.Exists}");
                }
                System.Threading.Thread.Sleep(200);
            }
        }

        public List<PathComponent> Paths { get; private set; }
    }
    public class PathComponent : INotifyPropertyChanged
    {
        private bool _exists;
        private bool _fileExists;
        public PathComponent(string path)
        {
            Path = path ?? throw new ArgumentNullException("path");
            Exists = false;
        }
        public string Path { get; private set; }
        public bool Exists
        {
            get => _exists;
            set 
            { 
                _exists = value;
                NotifyPropertyChanged();
            }
        }

        public bool FileExists
        {
            get => _fileExists;
            set
            {
                _fileExists = value;
                NotifyPropertyChanged();
            }
        }


        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
    public class ViewModel : INotifyPropertyChanged
    {
        public DirectoryScanner DirScannerUser { get; set; }
        public DirectoryScanner DirScannerSystem { get; set; }
        public ViewModel()
        {
            var userVariable = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
            var machineVariable = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);

            var userPaths = userVariable.Split(new[] { ';' }).ToList();
            var systemPaths = machineVariable.Split(new[] { ';' }).ToList();

            DirScannerUser = new DirectoryScanner(userPaths);
            DirScannerSystem = new DirectoryScanner(systemPaths);
        }
        public Thickness MyMargin { get; set; } = new Thickness(50, 50, 50, 50);

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
