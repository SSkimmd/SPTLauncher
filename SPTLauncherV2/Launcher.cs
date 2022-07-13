using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SPTLauncherV2 {
    public class Launcher : ApplicationContext {
        public Profile CurrentProfile;
        public Config CurrentConfig;
        public bool IsDeveloper;

        public Launcher() {
            OpenSelectProfileForm();
        }

        public void ToggleDev(bool enabled) { IsDeveloper = enabled; }

        public void OpenSelectProfileForm() {
            Form ProfileSelectForm = new Form1(this);
            ProfileSelectForm.Show();
            ProfileSelectForm.FormClosed += delegate {
                ClosedSelectProfileForm();
            };
        }

        public void ClosedSelectProfileForm() {
            Environment.Exit(0);
        }

        public void OpenMainForm() {
            Form MainForm = new MainForm(this);
            MainForm.Show();
            MainForm.FormClosing += OnMainFormClosing;
        }
        public void OnMainFormClosing(object sender, FormClosingEventArgs e) {
            DialogResult result = MessageBox.Show("Are you sure you want to close the application?", "Close Application",
                             MessageBoxButtons.YesNo,
                             MessageBoxIcon.Question);

            if (result == DialogResult.Yes) {
                e.Cancel = false;
                SaveProfile(CurrentProfile);
                Environment.Exit(0);
            } else {
                e.Cancel = true;
            }
        }

        public void OpenConfigEditorForm(string config) {
            Form ConfigEditor = new ConfigEditor(this, config);
            ConfigEditor.Show();
        }

        public void OpenSettingsForm() {
            Form Settings = new Settings(this);
            Settings.Show();
        }

        public void UpdateConfig(Config config) {
            CurrentConfig = config;
        }
        public void UpdateProfile(Profile profile) {
            CurrentProfile = profile;
            UpdateConfig(CurrentProfile.ProfileConfig);
        }

        public bool SaveProfile(Profile profile) {
            string file = Directory.GetCurrentDirectory() + "/" + profile.ProfileName + ".json";

            if(File.Exists(file)) {
                File.WriteAllText(file, JsonConvert.SerializeObject(profile, Formatting.Indented));
                return true;
            }

            return false;
        }

        public void CreateProfile(Profile profile) {
            string file = Directory.GetCurrentDirectory() + "/" + profile.ProfileName + ".json";
            string disabledDir = profile.ProfileConfig.BaseLocation + "/user/mods-disabled/";

            if(!Directory.Exists(disabledDir)) {
                Directory.CreateDirectory(profile.ProfileConfig.BaseLocation + "/user/mods-disabled/");
            }


            if (!File.Exists(file)) {
                File.Create(file).Dispose();
                File.WriteAllText(file, JsonConvert.SerializeObject(profile, Formatting.Indented));
            }

            UpdateProfile(profile);
        }


        public void CompileExistingModList(Profile profile) {
            List<string> modFolders = Directory.GetDirectories(profile.ProfileConfig.BaseLocation + "/user/mods/").ToList();
            List<string> modsDisabled = Directory.GetDirectories(profile.ProfileConfig.BaseLocation + "/user/mods-disabled/").ToList();
            modFolders.AddRange(modsDisabled);

            List<Mod> modList = profile.ProfileConfig.ModList;

            //CHECK MODS DONT ALREADY EXIST THEN ADD THEM
            foreach(string path in modFolders) {
                if (File.Exists(path + "/package.json")) {
                    var data = (JObject)JsonConvert.DeserializeObject(File.ReadAllText(path + "/package.json"));

                    Mod existingMod = modList.Find(mod => mod.ModLocation == path);

                    if(existingMod == null) {
                        bool isModEnabled = !path.Contains("/mods-disabled/");

                        modList.Add(new Mod(data["name"].Value<string>(), data["author"].Value<string>(), path,
                            data["version"].Value<string>(), data["akiVersion"].Value<string>(), data["main"].Value<string>(), isModEnabled));
                    }
                }
            }

            //REMOVE MODS THAT DONT EXIST
            for(int i = 0; i < modList.Count; i++) {
                if(!Directory.Exists(modList[i].ModLocation)) {
                    modList.RemoveAt(i);
                }
            }


            profile.ProfileConfig.ModList = modList;
            UpdateProfile(profile);
        }




        public List<string> GetFileLocations(int index = 0) {
            List<string> locations = new();


            if(index == 1 || index == 0) {
                OpenFileDialog launcherLocation = new();
                launcherLocation.Title = "Launcher File Location";
                launcherLocation.Filter = "Exe Files (.exe)|*.exe";
                if (launcherLocation.ShowDialog() == DialogResult.OK) {
                    locations.Add(launcherLocation.FileName);
                }
            }

            if(index == 2 || index == 0) {
                OpenFileDialog serverLocation = new();
                serverLocation.Title = "Server File Location";
                serverLocation.Filter = "Exe Files (.exe)|*.exe";
                if (serverLocation.ShowDialog() == DialogResult.OK) {
                    locations.Add(serverLocation.FileName);
                }
            }

            if (index == 3 || index == 0) {
                CommonOpenFileDialog baseLocation = new();
                baseLocation.IsFolderPicker = true;
                if (baseLocation.ShowDialog() == CommonFileDialogResult.Ok) {
                    locations.Add(baseLocation.FileName);
                }
            }
            
            foreach(string location in locations) {
                location.Replace("\\", "/");
            }

            return locations;
        }
    }
}
