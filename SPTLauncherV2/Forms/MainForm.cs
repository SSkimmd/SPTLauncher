using System.Diagnostics;
using System.Text;
using Aspose.Zip;
using Aspose.Zip.Rar;

namespace SPTLauncherV2
{
    public partial class MainForm : Form {
        public Launcher launcher;
        public Mod SelectedMod;
        public string SelectedConfig;
        bool IgnoreOnModChecked = false;
        bool IsDeveloper = false;

        public MainForm(Launcher launcher) {
            this.launcher = launcher;
            this.IsDeveloper = launcher.IsDeveloper;
            InitializeComponent();
            OnLauncherOpen();
        }

        public void OnLauncherOpen() {
            checkedListBox1.SelectedIndexChanged += OnSelectedMod;
            checkedListBox1.SelectedIndexChanged += GetConfigLocation;

            PopulateModList();
            PopulateLaunchOptions();

            checkedListBox1.ItemCheck += OnModChecked;
            button3.Click += delegate { launcher.OpenSettingsForm(); };
            button4.Click += OpenModLocation;
            button5.Click += SelectedLaunchOption;
            button6.Click += (sender, args) => launcher.OpenConfigEditorForm(SelectedConfig);

            button1.Click += delegate { AddMod(); };
            button2.Click += delegate { RemoveMod(); };

            button9.Hide();
            button8.Hide();

            textBox1.TextAlign = HorizontalAlignment.Center;
            textBox2.TextAlign = HorizontalAlignment.Center;
            textBox4.TextAlign = HorizontalAlignment.Center;
            textBox5.TextAlign = HorizontalAlignment.Center;
        }

        private void OpenModLocation(object sender, EventArgs e) {
            Process.Start("explorer.exe", SelectedMod.ModLocation + "/");
        }

        private void GetConfigLocation(object sender, EventArgs e) {      
            List<string> files = Directory.GetFiles(SelectedMod.ModLocation, "*.json", SearchOption.AllDirectories).ToList();

            foreach(string file in files) {
                if(Path.GetFileName(file).ToLower() == "config.json") {
                    SelectedConfig = file;
                    textBox6.Text = file.Replace("\\", "/");
                    return;
                }
            }

            SelectedConfig = "";
            textBox6.Text = "No Config File";
        }

        private void SelectedLaunchOption(object sender, EventArgs e) {
            if(comboBox2.Text == "Both") {
                //launch both
                Directory.SetCurrentDirectory(launcher.CurrentConfig.BaseLocation);
                Process.Start(launcher.CurrentConfig.ServerLocation);
                Process.Start(launcher.CurrentConfig.LauncherLocation);

                if(checkBox1.Checked) {
                    Environment.Exit(0);
                }
            }
            if(comboBox2.Text == "Server") {
                Directory.SetCurrentDirectory(launcher.CurrentConfig.BaseLocation);
                Process.Start(launcher.CurrentConfig.ServerLocation);

                if(checkBox1.Checked) {
                    Environment.Exit(0);
                }
            }
            if(comboBox2.Text == "Launcher") {
                Directory.SetCurrentDirectory(launcher.CurrentConfig.BaseLocation);
                Process.Start(launcher.CurrentConfig.LauncherLocation);

                if(checkBox1.Checked) {
                    Environment.Exit(0);
                }
            }
        }

        private void PopulateLaunchOptions() {
            if(File.Exists(launcher.CurrentConfig.ServerLocation)) {
                comboBox2.Items.Add("Server");
            }
            if(File.Exists(launcher.CurrentConfig.LauncherLocation)) {
                comboBox2.Items.Add("Launcher");
            }
            if(File.Exists(launcher.CurrentConfig.ServerLocation) && File.Exists(launcher.CurrentConfig.LauncherLocation)) {
                comboBox2.Items.Add("Both");
            }

            if (comboBox2.Items.Count > 0) { comboBox2.SelectedIndex = 0; }
        }

        private void PopulateModList() {
            IgnoreOnModChecked = true;

            foreach (Mod mod in launcher.CurrentConfig.ModList) {
                checkedListBox1.Items.Add(mod.ModName);
                checkedListBox1.SetItemChecked(checkedListBox1.Items.Count - 1, mod.ModEnabled);
                MoveMod(mod);
            }

            IgnoreOnModChecked = false;

            if(checkedListBox1.Items.Count > 0) {
                checkedListBox1.SetSelected(0, true);
            }
        }


        private void RefreshModList() {
            IgnoreOnModChecked = true;
            checkedListBox1.Items.Clear();
            Debug.WriteLine(checkedListBox1.Items.Count);

            foreach(Mod mod in launcher.CurrentConfig.ModList) {
                checkedListBox1.Items.Add(mod.ModName);
                checkedListBox1.SetItemChecked(checkedListBox1.Items.Count - 1, mod.ModEnabled);
            }

            if(checkedListBox1.Items.Count > 0) {
                checkedListBox1.SetSelected(0, true);
            }

            IgnoreOnModChecked = false;
        }

        private void AddMod() {
            OpenFileDialog fileLocation = new();
            fileLocation.Title = "Select Archive Location";
            fileLocation.Filter = "Zip Files (.zip)|*.zip;*.ZIP|RAR Files (.rar)|*.rar;*.RAR|7Zip Files (.7z)|*.7z";
            if(fileLocation.ShowDialog() == DialogResult.OK) {
                ExtractMod(fileLocation.FileName);
                launcher.CompileExistingModList(launcher.CurrentProfile);
                RefreshModList();
            }
        }

        private void RemoveMod() {
            Mod m = launcher.CurrentConfig.ModList[checkedListBox1.SelectedIndex];

            try {
                Directory.Delete(m.ModLocation, true);

                while(Directory.Exists(m.ModLocation)) {
                    //wait
                }

                launcher.CompileExistingModList(launcher.CurrentProfile);
                RefreshModList();
            } catch(Exception e) {
                MessageBox.Show("Folder Is Open In Explorer, Close To Remove Mod");
            }
        }

        private void ExtractMod(string file) {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string fileExt = (Path.GetExtension(file).ToLower());

            if(fileExt == ".rar") {
                RarArchive archive = new RarArchive(file);
                archive.ExtractToDirectory(launcher.CurrentConfig.BaseLocation + "/user/mods/");
                MessageBox.Show("Mod Added");
            } else if(fileExt == ".zip") {
                Archive archive = new Archive(file);
                archive.ExtractToDirectory(launcher.CurrentConfig.BaseLocation + "/user/mods/");
                MessageBox.Show("Mod Added");
            }
        }

        private void MoveMod(Mod mod) {
            if(!mod.ModEnabled) {
                string from = mod.ModLocation;
                string to = mod.ModLocation.Replace("/mods/", "/mods-disabled/");

                if(from != to) {
                    try {
                        Directory.Move(from, to);
                        mod.ModLocation = to;
                        mod.ModEnabled = false;

                        launcher.SaveProfile(launcher.CurrentProfile);
                    } catch(Exception e) {
                        MessageBox.Show("The Folder May Be Open In Explorer, Close It To Enable/Disable Mod");
                    }
                }
            } else {
                string from = mod.ModLocation;
                string to = mod.ModLocation.Replace("/mods-disabled/", "/mods/");

                if(from != to) {
                    try {
                        Directory.Move(from, to);
                        mod.ModLocation = to;
                        mod.ModEnabled = true;

                        launcher.SaveProfile(launcher.CurrentProfile);
                    } catch(Exception e) {
                        MessageBox.Show("The Folder May Be Open In Explorer, Close It To Enable/Disable Mod");
                    }
                }
            }
        }

        private void OnModChecked(object sender, EventArgs e) {
            if(!IgnoreOnModChecked) {
                bool modChecked = checkedListBox1.GetItemChecked(checkedListBox1.SelectedIndex);
                launcher.CurrentConfig.ModList[checkedListBox1.SelectedIndex].ModEnabled = !modChecked;
                MoveMod(launcher.CurrentConfig.ModList[checkedListBox1.SelectedIndex]);

                textBox3.Text = launcher.CurrentConfig.ModList[checkedListBox1.SelectedIndex].ModLocation;
            }
        }

        private void OnSelectedMod(object sender, EventArgs e) {
            CheckedListBox list = (CheckedListBox)sender;
            Mod mod = SelectedMod = launcher.CurrentConfig.ModList[list.SelectedIndex];

            textBox1.Text = mod.ModName;
            textBox2.Text = mod.ModAuthor;
            textBox3.Text = mod.ModLocation;
            textBox3.Text = textBox3.Text.Replace("\\", "/");
            textBox4.Text = mod.ModVersion;
            textBox5.Text = mod.GameVersion;
        }
    }
}
