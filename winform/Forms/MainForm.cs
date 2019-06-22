﻿using MMRando.Forms;
using MMRando.Forms.Tooltips;
using MMRando.Models;
using MMRando.Utils;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Linq;

namespace MMRando
{
    public partial class MainForm : Form
    {
        private string _seedOld = "";

        private BindingSource settingsDataSource;

        public Settings _settings { get; set; }

        public AboutForm About { get; private set; }
        public ManualForm Manual { get; private set; }
        public LogicEditorForm LogicEditor { get; private set; }
        public ItemEditForm ItemEditor { get; private set; }


        public static string AssemblyVersion
        {
            get
            {
                Version v = Assembly.GetExecutingAssembly().GetName().Version;
                return $"Majora's Mask Randomizer v{v}";
            }
        }

        public MainForm()
        {
            InitializeComponent();
            InitializeSettings();
            InitializeTooltips();
            BindProperties();

            ItemEditor = new ItemEditForm(_settings);
            LogicEditor = new LogicEditorForm();
            Manual = new ManualForm();
            About = new AboutForm();

            Text = AssemblyVersion;
        }

        private void InitializeTooltips()
        {
            // Output Settings
            TooltipBuilder.SetTooltip(cN64, "Output a randomized .z64 ROM that can be loaded into a N64 Emulator.");
            TooltipBuilder.SetTooltip(cVC, "Output a randomized .WAD file that can be loaded into a Wii Virtual Channel.");
            TooltipBuilder.SetTooltip(cSpoiler, "Output a spoiler log.\n\n The spoiler log contains a list over all items, and their shuffled locations.\n In addition, the spoiler log contains version information, seed and settings string used in the randomization.");
            TooltipBuilder.SetTooltip(cHTMLLog, "Output a html spoiler log (Requires spoiler log to be checked).\n\n Similar to the regular spoiler log, but readable in browsers. The locations/items are hidden by default, and hovering over them will make them visible.");
            TooltipBuilder.SetTooltip(cPatch, "Output a patch file that can be applied using the Patch settings tab to reproduce the same ROM.\nPatch file includes all settings except Tunic and Tatl color.");

            // Main Settings
            TooltipBuilder.SetTooltip(cMode, "Select mode of logic:\n - Casual/glitchless: The randomization logic ensures that no glitches are required to beat the game.\n - Using glitches: The randomization logic allows for placement of items that are only obtainable using known glitches.\n - Vanilla Layout: All items are left vanilla.\n - User logic: Upload your own custom logic to be used in the randomization.\n - No logic: Completely random, no guarantee the game is beatable.");

            TooltipBuilder.SetTooltip(cUserItems, "Only randomize a custom list of items.\n\nThe item list can be edited from the menu: Customize -> Item List Editor. When checked, some settings will become disabled.");
            TooltipBuilder.SetTooltip(cMixSongs, "Enable songs being placed among items in the randomization pool.");
            TooltipBuilder.SetTooltip(cDChests, "Enable keys, boss keys, maps and compasses being placed in the randomization pool.");
            TooltipBuilder.SetTooltip(cShop, "Enable shop items being placed in the randomization pool.");
            TooltipBuilder.SetTooltip(cBottled, "Enable captured bottle contents being randomized.");
            TooltipBuilder.SetTooltip(cSoS, "Exclude song of soaring from being placed in the randomization pool.");
            TooltipBuilder.SetTooltip(cGossip, "Enable gossip stones displaying hints on where certain items are located.");
            TooltipBuilder.SetTooltip(cDEnt, "Enable randomization of dungeon entrances. \n\nStone Tower Temple is always vanilla, but Inverted Stone Tower Temple is randomized.");
            TooltipBuilder.SetTooltip(cAdditional, "Enable miscellaneous items being placed in the randomization pool.\n\nAmong the miscellaneous items are:\nFreestanding heartpieces, overworld chests, (hidden) grotto chests, Tingle's maps and bank heartpiece.");
            TooltipBuilder.SetTooltip(cEnemy, "Enable randomization of enemies. May cause softlocks in some circumstances, use at your own risk.");
            TooltipBuilder.SetTooltip(cMoonItems, "Enable moon items being placed in the randomization pool.\n\nIncludes the four Moon Trial Heart Pieces and the Fierce Deity's Mask.");

            // Gimmicks
            TooltipBuilder.SetTooltip(cDMult, "Select a damage mode, affecting how much damage Link takes:\n\n - Default: Link takes normal damage.\n - 2x: Link takes double damage.\n - 4x: Link takes quadruple damage.\n - 1-hit KO: Any damage kills Link.\n - Doom: Hardcore mode. Link's hearts are slowly being drained continuously.");
            TooltipBuilder.SetTooltip(cDType, "Select an effect to occur whenever Link is being damaged:\n\n - Default: Vanilla effects occur.\n - Fire: All damage burns Link.\n - Ice: All damage freezes Link.\n - Shock: All damage shocks link.\n - Knockdown: All damage knocks Link down.\n - Random: Any random effect of the above.");
            TooltipBuilder.SetTooltip(cGravity, "Select a movement modifier:\n\n - Default: No movement modifier.\n - High speed: Link moves at a much higher velocity.\n - Super low gravity: Link can jump very high.\n - Low gravity: Link can jump high.\n - High gravity: Link can barely jump.");
            TooltipBuilder.SetTooltip(cFloors, "Select a floortype for every floor ingame:\n\n - Default: Vanilla floortypes.\n - Sand: Link sinks slowly into every floor, affecting movement speed.\n - Ice: Every floor is slippery.\n - Snow: Similar to sand. \n - Random: Any random floortypes of the above.");
            TooltipBuilder.SetTooltip(cClockSpeed, "Modify the speed of time. \n\nNote: The slowdown effect of playing inverted song of time does not scale with time speed.");
            TooltipBuilder.SetTooltip(cHideClock, "Clock UI will be hidden.");

            // Comforts/cosmetics
            TooltipBuilder.SetTooltip(cCutsc, "Enable shortened cutscenes.\n\nCertain cutscenes are skipped or otherwise shortened.\nDISCLAIMER: This may cause crashing in certain emulators.");
            TooltipBuilder.SetTooltip(cQText, "Enable quick text. Dialogs are fast-forwarded to choices/end of dialog.");
            TooltipBuilder.SetTooltip(cBGM, "Randomize background music sequences that are played throughout the game.");
            TooltipBuilder.SetTooltip(cNoMusic, "Mute background music.");
            TooltipBuilder.SetTooltip(cFreeHints, "Enable reading gossip stone hints without requiring the Mask of Truth.");
            TooltipBuilder.SetTooltip(cClearHints, "Gossip stone hints will give clear item and location names.");
            TooltipBuilder.SetTooltip(cNoDowngrades, "Downgrading items will be prevented.");
            TooltipBuilder.SetTooltip(bTunic, "Select the color of Link's Tunic.");
            TooltipBuilder.SetTooltip(cLink, "Select a character model to replace Link's default model.");
            TooltipBuilder.SetTooltip(cTatl, "Select a color scheme to replace Tatl's default color scheme.");
        }

        #region Forms Code

        private void mmrMain_Load(object sender, EventArgs e)
        {
            // initialise some stuff
            InitializeBackgroundWorker();
        }

        private void InitializeBackgroundWorker()
        {
            bgWorker.DoWork += new DoWorkEventHandler(bgWorker_DoWork);
            bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgWorker_WorkerCompleted);
            bgWorker.ProgressChanged += new ProgressChangedEventHandler(bgWorker_ProgressChanged);
        }

        private void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pProgress.Value = e.ProgressPercentage;
            var message = (string)e.UserState;
            lStatus.Text = message;
        }

        private void bgWorker_WorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            pProgress.Value = 0;
            lStatus.Text = "Ready...";
            EnableAllControls(true);
            ToggleCheckBoxes();
        }

        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;

            RandomizedResult randomized;

            if (!_settings.ApplyPatch)
            {
                if (!TryRandomize(worker, out randomized))
                {
                    return;
                }
            }
            else
            {
                randomized = new RandomizedResult(_settings, null);
            }

            if (_settings.OutputSpoiler
                && _settings.LogicMode != LogicMode.Vanilla)
            {
                SpoilerUtils.CreateSpoilerLog(randomized, _settings);
            }


            if (_settings.ApplyPatch || _settings.OutputGame || _settings.OutputROMPatch)
            {
                try
                {
                    var _builder = new Builder(randomized);
                    _builder.MakeROM(_settings.InputROMFilename, _settings.OutputROMFilename, worker);
                }
                catch (Exception ex)
                {
                    string nl = Environment.NewLine;
                    MessageBox.Show($"Error building ROM: {ex.Message}{nl}{nl}Please contact the development team and provide them more information",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            MessageBox.Show("Generation complete!",
                "Success", MessageBoxButtons.OK, MessageBoxIcon.None);
        }

        private void bTunic_Click(object sender, EventArgs e)
        {
            cTunic.ShowDialog();
            _settings.TunicColor = cTunic.Color;
        }

        private void bopen_Click(object sender, EventArgs e)
        {
            var result = openROM.ShowDialog();

            if (result == DialogResult.OK)
            {
                var validate = RomUtils.ValidateROM(openROM.FileName);

                if (validate == ValidateRomResult.InvalidFile)
                {
                    MessageBox.Show("Input file is not a clean Majora's Mask (U).",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _settings.InputROMFilename = openROM.FileName;
            }
        }


        private void bRandomise_Click(object sender, EventArgs e)
        {
            _settings.OutputROMFilename = Path.Combine("Output", _settings.DefaultOutputROMFilename);
            _settings.ApplyPatch = false;

            GenerateOutput();
        }

        private void bApplyPatch_Click(object sender, EventArgs e)
        {
            var filename = Path.ChangeExtension(Path.GetFileName(_settings.InputPatchFilename), "z64");

            _settings.OutputROMFilename = Path.Combine("Output", filename);
            _settings.ApplyPatch = true;
            GenerateOutput();
        }

        private void GenerateOutput()
        {
            ValidateRomResult result = ValidateRomResult.NoFile;
            if (_settings.NeedInputROM)
            {
                result = RomUtils.ValidateROM(_settings.InputROMFilename);
                if (result == ValidateRomResult.NoFile)
                {
                    MessageBox.Show("Input ROM not found, cannot generate output.",
                        "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                else if (result == ValidateRomResult.InvalidFile)
                {
                    MessageBox.Show("Input file is not a clean Majora's Mask (U).",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            _settings.InputROMFormat = result;

            if (_settings.ApplyPatch && string.IsNullOrWhiteSpace(_settings.InputPatchFilename))
            {
                MessageBox.Show("No patch file selected.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!(_settings.ApplyPatch || _settings.OutputGame || _settings.OutputROMPatch || _settings.OutputSpoiler))
            {
                MessageBox.Show("No output selected.",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            EnableAllControls(false);
            bgWorker.RunWorkerAsync();
        }


        private void tSString_Enter(object sender, EventArgs e)
        {
            //_oldSettingsString = tSString.Text;
            //_isUpdating = true;
        }

        private void tSString_Leave(object sender, EventArgs e)
        {
            //try
            //{
            //    _settings.Update(tSString.Text);
            //    UpdateCheckboxes();
            //    ToggleCheckBoxes();
            //}
            //catch
            //{
            //    tSString.Text = _oldSettingsString;
            //    _settings.Update(_oldSettingsString);
            //    MessageBox.Show("Settings string is invalid; reverted to previous settings.",
            //        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}

            //_isUpdating = false;
        }

        private void tSString_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                cDummy.Select();
            };
        }

        private void tSeed_Enter(object sender, EventArgs e)
        {
            _seedOld = _settings.Seed;
        }

        private void tSeed_Leave(object sender, EventArgs e)
        {
            try
            {
                int seed = Convert.ToInt32(tSeed.Text);
                if (seed < 0)
                {
                    seed = Math.Abs(seed);
                    tSeed.Text = seed.ToString();
                    MessageBox.Show("Seed must be positive",
                        "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    _settings.Seed = tSeed.Text;
                }
            }
            catch
            {
                tSeed.Text = _seedOld;
                MessageBox.Show("Invalid seed: must be a positive integer.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
        }

        private void tSeed_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                cDummy.Select();
            }
        }

        private void cMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            //
            //if (_settings.LogicMode == LogicMode.UserLogic
            //    && openLogic.ShowDialog() != DialogResult.OK)
        }

        private void mExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void mAbout_Click(object sender, EventArgs e)
        {
            About.ShowDialog();
        }

        private void mManual_Click(object sender, EventArgs e)
        {
            Manual.Show();
        }

        private void mLogicEdit_Click(object sender, EventArgs e)
        {
            LogicEditor.Show();
        }

        private void mItemIncl_Click(object sender, EventArgs e)
        {
            ItemEditor.Show();
        }



        /// <summary>
        /// Checks for settings that invalidate others, and disable the checkboxes for them.
        /// </summary>
        private void ToggleCheckBoxes()
        {
            var onMainTab = ttOutput.SelectedTab.TabIndex == 0;

            if (_settings.LogicMode == LogicMode.Vanilla)
            {
                cMixSongs.Enabled = false;
                cSoS.Enabled = false;
                cDChests.Enabled = false;
                cDEnt.Enabled = false;
                cBottled.Enabled = false;
                cShop.Enabled = false;
                cSpoiler.Enabled = false;
                cGossip.Enabled = false;
                cAdditional.Enabled = false;
                cUserItems.Enabled = false;
                cMoonItems.Enabled = false;
            }
            else
            {
                cMixSongs.Enabled = onMainTab;
                cSoS.Enabled = onMainTab;
                cDChests.Enabled = onMainTab;
                cDEnt.Enabled = onMainTab;
                cBottled.Enabled = onMainTab;
                cShop.Enabled = onMainTab;
                cSpoiler.Enabled = onMainTab;
                cGossip.Enabled = onMainTab;
                cAdditional.Enabled = onMainTab;
                cUserItems.Enabled = onMainTab;
                cMoonItems.Enabled = onMainTab;
            }

            cHTMLLog.Enabled = onMainTab;

            if (_settings.UseCustomItemList)
            {
                cSoS.Enabled = false;
                cDChests.Enabled = false;
                cBottled.Enabled = false;
                cShop.Enabled = false;
                cAdditional.Enabled = false;
                cMoonItems.Enabled = false;
            }
            else
            {
                if (_settings.LogicMode != LogicMode.Vanilla)
                {
                    cSoS.Enabled = onMainTab;
                    cDChests.Enabled = onMainTab;
                    cBottled.Enabled = onMainTab;
                    cShop.Enabled = onMainTab;
                    cAdditional.Enabled = onMainTab;
                    cMoonItems.Enabled = onMainTab;
                }
            }
        }

        private void EnableAllControls(bool v)
        {
            cAdditional.Enabled = v;
            cBGM.Enabled = v;
            cNoMusic.Enabled = v;
            cBottled.Enabled = v;
            cCutsc.Enabled = v;
            cDChests.Enabled = v;
            cDEnt.Enabled = v;
            cMode.Enabled = v;
            cDMult.Enabled = v;
            cDType.Enabled = v;
            cDummy.Enabled = v;
            cEnemy.Enabled = v;
            cFloors.Enabled = v;
            cClockSpeed.Enabled = v;
            cHideClock.Enabled = v;
            cGossip.Enabled = v;
            cGravity.Enabled = v;
            cLink.Enabled = v;
            cMixSongs.Enabled = v;
            cSoS.Enabled = v;
            cShop.Enabled = v;
            cUserItems.Enabled = v;
            cVC.Enabled = v;
            cQText.Enabled = v;
            cSpoiler.Enabled = v;
            cTatl.Enabled = v;
            cFreeHints.Enabled = v;
            cClearHints.Enabled = v;
            cNoDowngrades.Enabled = v;
            cHTMLLog.Enabled = v;
            cN64.Enabled = v;
            cMoonItems.Enabled = v;
            cPatch.Enabled = v;
            bApplyPatch.Enabled = v;

            bopen.Enabled = v;
            bRandomise.Enabled = v;
            bTunic.Enabled = v;

            tSeed.Enabled = v;
            tSString.Enabled = v;
        }

        #endregion

        #region Settings

        public void InitializeSettings()
        {
            _settings = new Settings();

            bTunic.BackColor = _settings.TunicColor;
            _settings.Seed = Math.Abs(Environment.TickCount).ToString();


            tSeed.Text = _settings.Seed.ToString();
        }

        private void BindProperties()
        {
            settingsDataSource = new BindingSource { DataSource = typeof(Settings) };
            BindTextBoxToSettings(tROMName, nameof(_settings.InputROMFilename));

            // Output Settings
            BindCheckboxToSettings(cN64, nameof(_settings.OutputN64ROM));
            BindCheckboxToSettings(cVC, nameof(_settings.OutputVC));
            BindCheckboxToSettings(cSpoiler, nameof(_settings.OutputTextSpoiler));
            BindCheckboxToSettings(cHTMLLog, nameof(_settings.OutputHTMLSpoiler));
            BindCheckboxToSettings(cPatch, nameof(_settings.OutputROMPatch));

            // Main generation settings
            BindCheckboxToSettings(cUserItems, nameof(_settings.UseCustomItemList));
            BindCheckboxToSettings(cMixSongs, nameof(_settings.AddSongs));
            BindCheckboxToSettings(cDChests, nameof(_settings.AddDungeonItems));
            BindCheckboxToSettings(cShop, nameof(_settings.AddShopItems));
            BindCheckboxToSettings(cBottled, nameof(_settings.RandomizeBottleCatchContents));
            BindCheckboxToSettings(cSoS, nameof(_settings.ExcludeSongOfSoaring));
            BindCheckboxToSettings(cGossip, nameof(_settings.EnableGossipHints));
            BindCheckboxToSettings(cDEnt, nameof(_settings.RandomizeDungeonEntrances));
            BindCheckboxToSettings(cAdditional, nameof(_settings.AddOtherItems));
            BindCheckboxToSettings(cEnemy, nameof(_settings.RandomizeEnemies));
            BindCheckboxToSettings(cMoonItems, nameof(_settings.AddMoonItems));

            // Gimmicks
            BindCheckboxToSettings(cHideClock, nameof(_settings.HideClock));

            // Comforts/cosmetics
            BindCheckboxToSettings(cCutsc, nameof(_settings.ShortenCutscenes));
            BindCheckboxToSettings(cQText, nameof(_settings.QuickTextEnabled));
            BindCheckboxToSettings(cBGM, nameof(_settings.RandomizeBGM));
            BindCheckboxToSettings(cNoMusic, nameof(_settings.NoBGM));
            BindCheckboxToSettings(cFreeHints, nameof(_settings.FreeHints));
            BindCheckboxToSettings(cClearHints, nameof(_settings.ClearHints));
            BindCheckboxToSettings(cNoDowngrades, nameof(_settings.PreventDowngrades));

            BindComboBoxToEnumSetting(cDMult, nameof(_settings.DamageMode), DamageMode.Default);
            BindComboBoxToEnumSetting(cDType, nameof(_settings.DamageEffect), DamageEffect.Default);
            BindComboBoxToEnumSetting(cMode, nameof(_settings.LogicMode), LogicMode.Casual);
            BindComboBoxToEnumSetting(cLink, nameof(_settings.Character), Character.LinkMM);
            BindComboBoxToEnumSetting(cTatl, nameof(_settings.TatlColorSchema), TatlColorSchema.Default);
            BindComboBoxToEnumSetting(cGravity, nameof(_settings.MovementMode), MovementMode.Default);
            BindComboBoxToEnumSetting(cFloors, nameof(_settings.FloorType), FloorType.Default);
            BindComboBoxToEnumSetting(cClockSpeed, nameof(_settings.ClockSpeed), ClockSpeed.Default);

            bTunic.DataBindings.Add("BackColor", settingsDataSource, nameof(_settings.TunicColor), true, DataSourceUpdateMode.OnPropertyChanged);
            settingsDataSource.DataSource = _settings;
        }

        private void BindCheckboxToSettings(CheckBox checkbox, string property)
        {
            checkbox.DataBindings.Add("Checked", settingsDataSource, property, true, DataSourceUpdateMode.OnPropertyChanged);
        }
        
        private void BindTextBoxToSettings(TextBox textBox, string property)
        {
            textBox.DataBindings.Add("Text", settingsDataSource, property);
        }

        private void BindComboBoxToEnumSetting<T>(ComboBox comboBox, string property, T enumDefault)
        {
            comboBox.BindEnumToCombobox(enumDefault);
            comboBox.DataBindings.Add("SelectedValue", settingsDataSource, property, true, DataSourceUpdateMode.OnPropertyChanged);
        }

        #endregion

        #region Randomization

        /// <summary>
        /// Try to create a RandomizedResult
        /// </summary>
        private bool TryRandomize(BackgroundWorker worker, out RandomizedResult randomized)
        {
            randomized = null;
            try
            {
                Randomizer randomizer = new Randomizer(_settings);
                randomized = randomizer.Randomize(worker);
                return true;
            }
            catch (InvalidDataException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;

            }
            catch (Exception ex)
            {
                string nl = Environment.NewLine;
                MessageBox.Show($"Error randomizing logic: {ex.Message}{nl}{nl}Please try a different seed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        #endregion

        private void BLoadPatch_Click(object sender, EventArgs e)
        {
            openPatch.ShowDialog();
            _settings.InputPatchFilename = openPatch.FileName;
            tPatch.Text = _settings.InputPatchFilename;
        }

        private void ttOutput_Changed(object sender, EventArgs e)
        {
            ToggleCheckBoxes();

            TogglePatchSettings(ttOutput.SelectedTab.TabIndex == 0);
        }


        private void TogglePatchSettings(bool v)
        {
            // ROM Settings
            cPatch.Enabled = v;

            // Main Settings
            cMode.Enabled = v;
            cEnemy.Enabled = v;

            //Gimmicks
            cDMult.Enabled = v;
            cDType.Enabled = v;
            cGravity.Enabled = v;
            cFloors.Enabled = v;
            cClockSpeed.Enabled = v;
            cHideClock.Enabled = v;


            // Comfort/Cosmetics
            cCutsc.Enabled = v;
            cQText.Enabled = v;
            cBGM.Enabled = v;
            cFreeHints.Enabled = v;
            cClearHints.Enabled = v;
            cNoDowngrades.Enabled = v;

            cLink.Enabled = v;

            // Other..?
            cDummy.Enabled = v;

            if (!v)
            {
                _settings.InputPatchFilename = null;
                tPatch.Text = null;
            }
        }
    }
}
