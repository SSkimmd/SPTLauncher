using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SPTLauncherV2 {
    public partial class Settings : Form {
        Launcher launcher;
        public Settings(Launcher launcher) {
            this.launcher = launcher;

            InitializeComponent();
            OnSettingsOpen();
        }

        private void OnSettingsOpen() {
            button2.Click += delegate { OnChangeFileLocations(1); };
            button3.Click += delegate { OnChangeFileLocations(2); };
            button4.Click += delegate { OnChangeFileLocations(3); };
            button1.Click += delegate { launcher.UpdateProfile(launcher.CurrentProfile); };
            checkBox1.CheckedChanged += delegate { launcher.ToggleDev(checkBox1.Checked); };

            UpdateSettings();
        }

        private void UpdateSettings() {
            textBox1.Text = launcher.CurrentProfile.ProfileConfig.LauncherLocation;
            textBox2.Text = launcher.CurrentProfile.ProfileConfig.ServerLocation;
            textBox3.Text = launcher.CurrentProfile.ProfileConfig.BaseLocation;
        }

        private void OnChangeFileLocations(int index) {
            if(index == 1) { launcher.CurrentConfig.LauncherLocation = launcher.GetFileLocations(index)[0]; }
            if(index == 2) { launcher.CurrentConfig.ServerLocation = launcher.GetFileLocations(index)[0]; }
            if(index == 3) { launcher.CurrentConfig.BaseLocation = launcher.GetFileLocations(index)[0]; }

            UpdateSettings();
        }
    }
}
