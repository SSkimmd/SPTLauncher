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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SPTLauncherV2 {
    public partial class ConfigEditor : Form {
        public Launcher launcher;
        public string config;
        public JToken root;
        bool IsDeveloper;

        Label profileLabel;
        ComboBox profileDropdown;
        ComboBox inventoryDropdown;

        JToken itemListRoot;

        public ConfigEditor(Launcher launcher, string config) {
            this.launcher = launcher;
            this.config = config;
            this.IsDeveloper = launcher.IsDeveloper;
            this.DoubleBuffered = true;
            string itemDatabasePath = Directory.GetCurrentDirectory() + "/itemsout.json";

            if(File.Exists(itemDatabasePath)) {
                using (var sreader = new StreamReader(itemDatabasePath))
                using (var jreader = new JsonTextReader(sreader)) {
                    itemListRoot = JToken.Load(jreader);
                }
            }

            InitializeComponent();

            button1.Click += delegate { SaveConfigFile(); };
        }

        private string GetItemNameFromID(string id) {
            foreach(var obj in itemListRoot) {
                if(obj["ID"].ToString() == id) {
                    return obj["Name"].ToString();
                }
            }

            return "Error";
        }

        public void OpenProfile(string profile) {
            if(File.Exists(profile)) {
                using (var sreader = new StreamReader(profile))
                using (var jreader = new JsonTextReader(sreader)) {
                    try {
                        root = JToken.Load(jreader);
                    } catch(Exception e) {
                        Debug.WriteLine(e.Message);
                    }


                    if(!root["characters"]["pmc"].HasValues) { MessageBox.Show("Profile is not valid"); Close(); return; }

                    string profileName = root["characters"]["pmc"]["Info"]["Nickname"].ToString();
                    profileLabel = new();
                    profileLabel.Width = 400;
                    profileLabel.Text = "Select Object To Edit, Profile Name: " + profileName;


                    profileDropdown = new();
                    profileDropdown.Width = 300;
                    profileDropdown.SelectedIndexChanged += delegate
                    {
                        ClearProfileEditor();

                        if(profileDropdown.SelectedItem.ToString() != "Inventory") {
                            DisplayJsonTree(root["characters"]["pmc"][profileDropdown.SelectedItem]);
                        } else {
                            DisplayInventory();
                        }
                    };

                    var obj = (JObject)root["characters"]["pmc"];

                    foreach(var property in obj.Properties()) {
                        profileDropdown.Items.Add(property.Name);
                    }

                    panel1.Controls.Add(profileLabel);
                    panel1.Controls.Add(profileDropdown);
                }
            }
        }

        private void DisplayInventory() {
            inventoryDropdown = new();
            inventoryDropdown.Width = 300;

            JArray inventory = root["characters"]["pmc"]["Inventory"]["items"] as JArray;

            foreach(var item in inventory) {
                string name = GetItemNameFromID(item["_tpl"].ToString());
                inventoryDropdown.Items.Add(name);
            }

            inventoryDropdown.SelectedIndexChanged += delegate {
                ClearInventoryEditor();
                int index = inventoryDropdown.SelectedIndex;
                var item = root["characters"]["pmc"]["Inventory"]["items"][index];
                DisplayJsonTree(item); 
            };

            panel1.Controls.Add(inventoryDropdown);
        }

        private void ClearInventoryEditor() {
            panel1.Controls.Clear();
            panel1.Controls.Add(profileLabel);
            panel1.Controls.Add(profileDropdown);
            panel1.Controls.Add(inventoryDropdown);
        }

        private void ClearProfileEditor() {
            panel1.Controls.Clear();
            panel1.Controls.Add(profileLabel);
            panel1.Controls.Add(profileDropdown);
        }

        public void OpenConfigFile(string config) {
            if(File.Exists(config)) {
                using (var sreader = new StreamReader(config))
                using (var jreader = new JsonTextReader(sreader)) {
                    root = JToken.Load(jreader);
                    DisplayJsonTree(root);
                }      
            }
        }

        private void SaveConfigFile() {
            using(var swriter = new StreamWriter(config)) 
            using(var jwriter = new JsonTextWriter(swriter)) {
                jwriter.Formatting = Formatting.Indented;
                root.WriteTo(jwriter);
            }

            MessageBox.Show("Config File Has Been Saved");
        }
        
        public void DisplayJsonTree(JToken root) {
            DisplayNode(root);
        }

        private void ClearTree() {
            panel1.Controls.Clear();
            DisplayJsonTree(root);
        }

        public void DisplayNode(JToken token) {
            if(token is JObject) {
                var obj = (JObject)token;

                foreach(var property in obj.Properties()) {
                    Label label = new Label();
                    label.Text = property.Name;
                    label.Font = new Font("Arial", 10);
                    label.Margin = new Padding(0, 20, 0, 0);
                    label.Height = 20;
                    label.Width = 400;
                    panel1.Controls.Add(label);

                    DisplayNode(property.Value);
                }
            } else if(token is JValue) {
                if(token.Type == JTokenType.String) {
                    TextBox textbox = new();
                    textbox.Text = token.ToString();
                    textbox.Width = 300;
                    textbox.Height = 20;

                    textbox.TextChanged += delegate {
                        JValue? p = token as JValue;
                        p.Value = textbox.Text;
                    };

                    panel1.Controls.Add(textbox);

                } else if(token.Type == JTokenType.Integer || token.Type == JTokenType.Float) {
                    NumericUpDown numselect = new();
                    numselect.Maximum = Int64.MaxValue;
                    numselect.Minimum = Int64.MinValue;
                    numselect.Value = (decimal)token.ToObject<float>();
                    numselect.Width = 300;
                    numselect.Height = 20;

                    numselect.ValueChanged += delegate {
                        JValue? p = token as JValue;
                        p.Value = (int)numselect.Value;
                    };

                    panel1.Controls.Add(numselect);
                    
                } else if(token.Type == JTokenType.Boolean) {
                    CheckBox checkbox = new();
                    checkbox.Checked = token.ToObject<bool>();
                    checkbox.Height = 20;

                    checkbox.CheckedChanged += delegate {
                        JValue? p = token as JValue;
                        p.Value = checkbox.Checked;
                    };

                    panel1.Controls.Add(checkbox);
                }
            } else if(token is JArray) {
                var array = (JArray)token;

                Button button = new();
                button.Text = "New Item";
                button.Width = 110;
                button.Height = 30;
                button.Click += delegate { array.Add(array[0].DeepClone()); ClearTree(); };
                panel1.Controls.Add(button);

                ComboBox combobox = new();
                combobox.Width = 300;
                foreach (var item in array) {
                    if(item is JObject) {
                        combobox.Items.Add(item.First());
                    } else {
                        combobox.Items.Add(item);
                    }
                }

                if(combobox.Items.Count > 0) {
                    combobox.SelectedIndex = 0;
                    panel1.Controls.Add(combobox);
                }

                Button button2 = new();
                button2.Text = "Remove Item";
                button2.Width = 110;
                button2.Height = 30;
                button2.Click += delegate { array.RemoveAt(combobox.SelectedIndex); ClearTree(); };
                panel1.Controls.Add(button2);

                for(int i = 0; i < array.Count; i++) {
                    DisplayNode(array[i]);
                }
            }
        }
    }
}
