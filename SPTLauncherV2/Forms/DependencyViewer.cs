using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SPTLauncherV2 {
    public partial class DependencyViewer : Form {
        public Launcher launcher;
        public bool modDependencyType = false;
        public DependencyViewer(Launcher launcher) {
            this.launcher = launcher;
            InitializeComponent();
            
            foreach(Mod m in launcher.CurrentConfig.ModList) {
                listBox1.Items.Add(m.ModName);
            }

            listBox1.SelectedIndexChanged += delegate { GetModDependencies(launcher.CurrentConfig.ModList[listBox1.SelectedIndex]); };
            button1.Click += delegate { ToggleDependencyType(); };
        }

        private void ToggleDependencyType() {
            modDependencyType = !modDependencyType;
            listBox1.SetSelected(0, true);

            if (!modDependencyType) {
                label2.Text = "Dev Dependencies";
                button1.Text = "Mod";
            } else {
                label2.Text = "Mod Dependencies";
                button1.Text = "Dev";
            }
        }

        private void GetModDependencies(Mod mod) {
            listBox2.Items.Clear();
            var data = (JObject)JsonConvert.DeserializeObject(File.ReadAllText(mod.ModLocation + "/package.json"));
            JToken root;

            if (!modDependencyType) {
                root = data["devDependencies"];
            } else {
                root = data["dependencies"];
            }

            if (root != null) {
                var obj = (JObject)root;

                foreach (var property in obj.Properties()) {
                    listBox2.Items.Add(property.Name);
                }
            }
        }
    }
}
