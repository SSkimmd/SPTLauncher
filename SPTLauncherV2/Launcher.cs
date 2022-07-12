using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SPTLauncherV2 {
    public class Launcher : ApplicationContext {
        public Profile CurrentProfile;
        public Config CurrentConfig;
        public bool IsDeveloper;

        public Launcher() {
            OpenSelectProfileForm();
        }


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

            if (!File.Exists(file)) {
                File.Create(file).Dispose();
                File.WriteAllText(file, JsonConvert.SerializeObject(profile, Formatting.Indented));
            }

            UpdateProfile(profile);
        }
    }
}
