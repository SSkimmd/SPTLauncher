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
            launcher.CompileExistingModList(profile);
            launcher.OpenMainForm();
            Hide();
        }

        private void OnNewProfile(object sender, EventArgs e) {
            List<string> fileLocations = launcher.GetFileLocations();
            Profile profile = new(comboBox1.Text, new Config(new List<Mod>(), new List<string>(), 
                fileLocations[0], fileLocations[1], fileLocations[2], ""));
            launcher.CreateProfile(profile);
            CompileExistingModList(profile);
        }
    }
}