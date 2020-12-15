using Newtonsoft.Json;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using WeatherApp.Commands;
using WeatherApp.Models;
using WeatherApp.Services;

namespace WeatherApp.ViewModels
{
    public class ApplicationViewModel : BaseViewModel
    {
        #region Membres

        private BaseViewModel currentViewModel;
        private List<BaseViewModel> viewModels;
        private TemperatureViewModel tvm;
        private OpenWeatherService ows;
        private string filename;

        private VistaSaveFileDialog saveFileDialog;
        private VistaOpenFileDialog openFileDialog;

        #endregion

        #region Propriétés
        /// <summary>
        /// Model actuellement affiché
        /// </summary>
        public BaseViewModel CurrentViewModel
        {
            get { return currentViewModel; }
            set { 
                currentViewModel = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// String contenant le nom du fichier
        /// </summary>
        public string Filename
        {
            get
            {
                return filename;
            }
            set
            {
                filename = value;
            }
        }

        /// <summary>
        /// Commande pour changer la page à afficher
        /// </summary>
        public DelegateCommand<string> ChangePageCommand { get; set; }

   
        public DelegateCommand<string> ImportCommand { get; set; }

        public DelegateCommand<string> ExportCommand { get; set; }
        /// <summary>
        public DelegateCommand<string> ChangeLanguageCommand { get; set; }
        /// </summary>


        public List<BaseViewModel> ViewModels
        {
            get {
                if (viewModels == null)
                    viewModels = new List<BaseViewModel>();
                return viewModels; 
            }
        }
        #endregion

        public ApplicationViewModel()
        {
            ChangePageCommand = new DelegateCommand<string>(ChangePage);
            ImportCommand = new DelegateCommand<string>(Import);
            ExportCommand = new DelegateCommand<string>(Export, CanExport);
            ChangeLanguageCommand = new DelegateCommand<string>(ChangeLanguage);
            /// TODO 13b : Instancier ChangeLanguageCommand qui doit appeler la méthode ChangeLanguage

            initViewModels();          

            CurrentViewModel = ViewModels[0];

        }

        #region Méthodes
        void initViewModels()
        {
            /// TemperatureViewModel setup
            tvm = new TemperatureViewModel();

            string apiKey = "";

            if (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "DEVELOPMENT")
            {
                apiKey = AppConfiguration.GetValue("OWApiKey");
            }

            if (string.IsNullOrEmpty(Properties.Settings.Default.apiKey) && apiKey == "")
            {
                tvm.RawText = "Aucune clé API, veuillez la configurer";
            } else
            {
                if (apiKey == "")
                    apiKey = Properties.Settings.Default.apiKey;

                ows = new OpenWeatherService(apiKey);
            }
                
            tvm.SetTemperatureService(ows);
            ViewModels.Add(tvm);

            var cvm = new ConfigurationViewModel();
            ViewModels.Add(cvm);
        }



        private void ChangePage(string pageName)
        {            
            if (CurrentViewModel is ConfigurationViewModel)
            {
                ows.SetApiKey(Properties.Settings.Default.apiKey);

                var vm = (TemperatureViewModel)ViewModels.FirstOrDefault(x => x.Name == typeof(TemperatureViewModel).Name);
                if (vm.TemperatureService == null)
                    vm.SetTemperatureService(ows);                
            }

            CurrentViewModel = ViewModels.FirstOrDefault(x => x.Name == pageName);  
        }

        /// <summary>
        /// TODO 07 : Méthode CanExport ne retourne vrai que si la collection a du contenu   ????? Not updating ..nevermind
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool CanExport(string obj)
        {
            if (tvm.Temperatures != null)
            {
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Méthode qui exécute l'exportation
        /// </summary>
        /// <param name="obj"></param>
        private void Export(string obj)
        {

            if (saveFileDialog == null)
            {
                saveFileDialog = new VistaSaveFileDialog();
                saveFileDialog.Filter = "Json file|*.json|All files|*.*";
                saveFileDialog.DefaultExt = "json";
            }

            if (saveFileDialog.ShowDialog() == true)
            {
                filename = saveFileDialog.FileName;
                saveToFile();
            }

            /// Voir
            /// Solution : 14_pratique_examen
            /// Projet : demo_openFolderDialog
            /// ---
            /// Algo
            /// Si la réponse de la boîte de dialogue est vrai
            ///   Garder le nom du fichier dans Filename
            ///   Appeler la méthode saveToFile
            ///   

        }

        private void saveToFile()
        {
            var data = tvm.Temperatures;

            var resultat = JsonConvert.SerializeObject(data, Formatting.Indented);
         
                using (var tw = new StreamWriter(filename))
                {
                    tw.WriteLine(resultat);
                    tw.Close();
                }
            }
            /// Voir 
            /// Solution : 14_pratique_examen
            /// Projet : serialization_object
            /// Méthode : serialize_array()
            /// 
            /// ---
            /// Algo
            /// Initilisation du StreamWriter
            /// Sérialiser la collection de températures
            /// Écrire dans le fichier
            /// Fermer le fichier           

        

        private void openFromFile()
        {

            if (!File.Exists(filename))
            {
                Console.WriteLine($"Le fichier {filename} n'existe pas. Veuillez le générer à partir de la sérialisation d'un tableau vers un fichier.");
                Console.ReadKey();
                return;
            }

            List<TemperatureModel> data;
            using (StreamReader sr = File.OpenText(filename))
            {
                var fileContent = sr.ReadToEnd();

                data = JsonConvert.DeserializeObject<List<TemperatureModel>>(fileContent);
                tvm.Temperatures.Clear();
                foreach (TemperatureModel temp in data)
                {
                    tvm.Temperatures.Add(temp);
                }
            }
           

        }

    private void Import(string obj)
    {
        if (openFileDialog == null)
        {
            openFileDialog = new VistaOpenFileDialog();
            openFileDialog.Filter = "Json file|*.json|All files|*.*";
            openFileDialog.DefaultExt = "json";
        }

        if (openFileDialog.ShowDialog() == true)
        {
            filename = openFileDialog.FileName;
            openFromFile();
        }
    }

        public void Restart()
        {

            var filename = Application.ResourceAssembly.Location;
            var newFile = Path.ChangeExtension(filename, ".exe");
            Process.Start(newFile);
            Application.Current.Shutdown();
        }

        private void ChangeLanguage(string Language)
        {
            Properties.Settings.Default.language = Language;
            Properties.Settings.Default.Save();

            if (MessageBox.Show(
                    Properties.Resources.msg_restart,
                    Properties.Resources.wn_warning,
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                Restart();

        }


        #endregion
    }
}
