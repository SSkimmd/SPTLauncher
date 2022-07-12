using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;

namespace SPTLauncherV2 {
    public partial class Form1 : Form {
        public Launcher launcher;
        List<Profile> profiles = new List<Profile>();

        public Form1(Launcher launcher) {
            this.launcher = launcher;
            InitializeComponent();
            OnLauncherOpen();
        }

        private void OnLauncherOpen() {
            GetProfiles();
        }

        private void GetProfiles() {
            List<string> files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.json").ToList();

            JSchemaGenerator generator = new JSchemaGenerator();
            JSchema schema = generator.Generate(typeof(Profile));

            button1.Click += OnSelectProfile;
            button2.Click += OnNewProfile;

            foreach(string file in files) {
                try {
                    JObject json = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(file));
                    bool jsonValid = json.IsValid(schema);

                    if(jsonValid) {
                        Profile profile = JsonConvert.DeserializeObject<Profile>(File.ReadAllText(file));
                        profiles.Add(profile);
                        comboBox1.Items.Add(profile.ProfileName);
                    }
                }
                catch {}
            }

            if(profiles.Count > 0) {
                comboBox1.SelectedIndex = 0;
            }
        }

        private void OnSelectProfile(object sender, EventArgs e) {
            if(comboBox1.Text != "" && File.Exists(Directory.GetCurrentDirectory() + "/" + comboBox1.Text + ".json")) {
                Profile profile = profiles.Find(p => p.ProfileName == comboBox1.Text);
                CompileExistingModList(profile);
            }
        }

        private void CompileExistingModList(Profile profile) {
            List<string> modFolders = Directory.GetDirectories(profile.ProfileConfig.BaseLocation + "/user/mods/").ToList();
            List<string> modsDisabled = Directory.GetDirectories(profile.ProfileConfig.BaseLocation + "/user/mods-disabled/").ToList();
            modFolders.AddRange(modsDisabled);
            
            List<Mod> modList = profile.ProfileConfig.ModList;

            foreach(string path in modFolders) {
                if(File.Exists(path + "/package.json")) {
                    var data = (JObject)JsonConvert.DeserializeObject(File.ReadAllText(path + "/package.json"));

                    Mod existingMod = modList.Find(mod => mod.ModLocation == path);

                    if(existingMod == null) {
                        bool isModEnabled = !path.Contains("/mods-disabled/");

                        modList.Add(new Mod(data["name"].Value<string>(), data["author"].Value<string>(), path,
                            data["version"].Value<string>(), data["akiVersion"].Value<string>(), data["main"].Value<string>(), isModEnabled));
                    }
                }
            }
            profile.ProfileConfig.ModList = modList;
            launcher.UpdateProfile(profile);
            launcher.OpenMainForm();
            Hide();
        }

        private void OnNewProfile(object sender, EventArgs e) {
            List<string> fileLocations = GetFileLocations();
            Profile profile = new(comboBox1.Text, new Config(new List<Mod>(), new List<string>(), 
                fileLocations[0], fileLocations[1], fileLocations[2], ""));
            launcher.CreateProfile(profile);
            CompileExistingModList(profile);
        }

        private List<string> GetFileLocations() {
            List<string> locations = new();

            OpenFileDialog launcherLocation = new();
            launcherLocation.Title = "Launcher File Location";
            if(launcherLocation.ShowDialog() == DialogResult.OK) {
                locations.Add(launcherLocation.FileName);
            }

            OpenFileDialog serverLocation = new();
            serverLocation.Title = "Server File Location";
            if(serverLocation.ShowDialog() == DialogResult.OK) {
                locations.Add(serverLocation.FileName);
            }

            CommonOpenFileDialog baseLocation = new();
            baseLocation.IsFolderPicker = true;
            if(baseLocation.ShowDialog() == CommonFileDialogResult.Ok) {
                locations.Add(baseLocation.FileName);
            }
            
            foreach(string location in locations) {
                location.Replace("\\", "/");
            }

            return locations;
        }
    }
}