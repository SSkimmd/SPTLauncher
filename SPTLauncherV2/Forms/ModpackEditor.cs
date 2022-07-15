using Aspose.Zip;
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
    public partial class ModpackEditor : Form {
        public Launcher launcher;
        private List<Mod> exportModList = new();
        private List<Mod> modpackModList = new();

        public ModpackEditor(Launcher launcher) {
            this.launcher = launcher;
            InitializeComponent();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            button3.Click += delegate { GetCurrentModList(); };
            button2.Click += delegate { ExportModpack(); };
        }

        private void GetCurrentModList() {
            foreach(Mod mod in launcher.CurrentConfig.ModList) {
                checkedListBox1.Items.Add(mod.ModName);
                modpackModList.Add(mod);
            }
        }

        private void OpenModpack() {
            OpenFileDialog modpackLocation = new();
            modpackLocation.Title = "Launcher File Location";
            modpackLocation.Filter = "Zip Files|*.zip";
            if(modpackLocation.ShowDialog() == DialogResult.OK) {
                Archive modpack = new Archive(modpackLocation.FileName);
                modpack.ExtractToDirectory(modpackLocation.FileName + "/");

                while(!File.Exists(modpackLocation.FileName + "/modpack.json")) { /*wait*/ }

                var data = (JObject)JsonConvert.DeserializeObject(File.ReadAllText(modpackLocation.FileName + "/modpack.json"));
                textBox1.Text = data["Name"].ToString();
                textBox2.Text = data["Author"].ToString();
                textBox3.Text = data["TargetVersion"].ToString();
                modpackModList = data["ModList"].ToObject<List<Mod>>();
            }
        }

        private void GetModList(Modpack modpack) {
            foreach(Mod mod in modpack.ModList) {
                modpackModList.Add(mod);
            }
        }

        private void ExportModpack() {
            for(int i = 0; i < checkedListBox1.Items.Count; i++) {
                if(checkedListBox1.GetItemChecked(i)) {
                    exportModList.Add(modpackModList[i]);
                    Debug.WriteLine(modpackModList[i].ModName);
                }
            }

            bool valid = textBox1.Text.Length > 0 && textBox2.Text.Length > 0 && textBox3.Text.Length > 0;

            if(valid) {
                SaveFileDialog saveModpack = new SaveFileDialog();
                saveModpack.Filter = "Zip Files|*.zip";

                if(saveModpack.ShowDialog() == DialogResult.OK) {
                    using(FileStream modpack = File.Open(saveModpack.FileName, FileMode.Create)) {
                        using(Archive archive = new Archive()) {
                            foreach(Mod mod in exportModList) {
                                archive.CreateEntries(mod.ModLocation);
                            }

                            archive.Save(modpack);
                        }
                    }
                }
            }
        }
    }
}
