﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MPhys.Devices;
using System.Configuration;
using System.Diagnostics;
using System.Timers;

namespace MPhys.GUI
{

    public partial class SpecForm : Form
    {
        HR550 MonoSpec;
        private Boolean m_bSyncAcq;
        private Boolean m_bStopAcq;


        public SpecForm()
        {
            InitializeComponent();
            this.Location = new Point(60, 26);
            this.ControlBox = false;
            this.TopLevel = false;
            this.TopMost = true;

            try
            {
                MonoSpec = new HR550();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        // Function to activate all buttons
        private void Activate_buttonsSCD()
        {
            //StatusLabel.ForeColor = Color.Green;
            if (StatusLabel.ForeColor == Color.Green && button_shutter.Enabled == false)
            {
                button_shutter.Enabled = true;
                groupboxWlCtrl.Enabled = true;
            }
            else
            {
                button_shutter.Enabled = false;
                groupboxWlCtrl.Enabled = false;
            }
        }
        private void Activate_buttonsCCD()
        {
            //StatusLabelCCD.ForeColor = Color.Green;
            if (StatusLabelCCD.ForeColor == Color.Green && GroupBoxStreamAcq.Enabled == false)
            {
                GroupBoxStreamAcq.Enabled = true;
                GroupBoxAcquire.Enabled = true;
                groupBoxCollect.Enabled = true;
            }
            else
            {
                GroupBoxStreamAcq.Enabled = false;
                GroupBoxAcquire.Enabled = false;
                groupBoxCollect.Enabled = false;
            }
        }

        private void Is_initialized()
        {
            if (MonoSpec.mMono != null && StatusLabel.Text != "Connected")
            {
                StatusLabel.Text = "Connected";
                StatusLabel.ForeColor = Color.Green;
            }
            else
            {
                StatusLabel.Text = "Disconnected";
                StatusLabel.ForeColor = Color.Red;
            }

        }

        static void UpdateAppSettings(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error writing app settings");
            }
        }

        private void buttonComSet_Click(object sender, EventArgs e)
        {
            SCDid SCD = new SCDid();
            string sName = ConfigurationManager.AppSettings.Get("iHR550_sName");
            string sID = ConfigurationManager.AppSettings.Get("iHR550_sID");

            if(comboBoxSCDs.Enabled == true && comboBoxSCDs.Text != null && comboBoxSCDs.Text != "")
            {
                SCD = (SCDid)comboBoxSCDs.SelectedItem;
                UpdateAppSettings("iHR550_sName", SCD.sName);
                UpdateAppSettings("iHR550_sID", SCD.sID);
            }

            SCD.sID = sID;
            SCD.sName = sName;

            if (sName != "")
            {
                try
                {
                    Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                    MonoSpec.InitializeMono(SCD);
                    StatusLabel.Text = "Connected";
                    StatusLabel.ForeColor = Color.Green;
                    textboxPosition.Text = MonoSpec.Text_CurrentWavelength;
                }
                catch
                {
                    StatusLabel.Text = "Disconnected";
                    StatusLabel.ForeColor = Color.Red;
                }
            }
            else
            {
                StatusLabel.Text = "Disconnected";
                StatusLabel.ForeColor = Color.Red;
            }

            Activate_buttonsSCD();

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            comboBoxSCDs.Items.Clear();
            if (checkBox1.Checked)
            {
                comboBoxSCDs.Enabled = true;
                foreach(SCDid s in MonoSpec.combobox_Mono)
                {
                    comboBoxSCDs.Items.Add(s);
                }
            }
            else
            {
                comboBoxSCDs.Enabled = false;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(comboBoxSCDs.Items.Count < 1 && checkBox1.Checked)
            {
                MonoSpec = new HR550();
                foreach (SCDid s in MonoSpec.combobox_Mono)
                {
                    comboBoxSCDs.Items.Add(s);
                }
            }
        }

        private void button_shutter_Click(object sender, EventArgs e)
        {
            string ltext = ShutterStateLabel.Text;
            if(ltext =="Open")
            {
                MonoSpec.Close_Shutter();
                ShutterStateLabel.Text = "Close";
            }
            else
            {
                MonoSpec.Open_Shutter();
                ShutterStateLabel.Text = "Open";
            }
        }

        private void comboboxGrating_SelectedIndexChanged(object sender, EventArgs e)
        {
            Boolean bBusy;

            try
            {
                bBusy = true;
                while (bBusy == true)
                {
                    Application.DoEvents();
                    bBusy = MonoSpec.IsBusy();
                }
                MonoSpec.GetPosition();
                textboxPosition.Text = MonoSpec.Text_CurrentWavelength;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ********* ACQUIRE
        private void buttonCDDInit_Click(object sender, EventArgs e)
        {
            SCDid SCD = new SCDid();
            string sName = ConfigurationManager.AppSettings.Get("iCCD_sName");
            string sID = ConfigurationManager.AppSettings.Get("iCCD_sID");

            if (comboBoxCCDs.Enabled == true && comboBoxCCDs.Text != null && comboBoxCCDs.Text != "")
            {
                SCD = (SCDid)comboBoxCCDs.SelectedItem;
                UpdateAppSettings("iCCD_sName", SCD.sName);
                UpdateAppSettings("iCCD_sID", SCD.sID);
            }

            SCD.sID = sID;
            SCD.sName = sName;

            if (sName != "")
            {
                try
                {
                    Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                    MonoSpec.InitializeCCD(SCD);
                    StatusLabelCCD.Text = "Connected";
                    StatusLabelCCD.ForeColor = Color.Green;
                }
                catch
                {
                    StatusLabelCCD.Text = "Disconnected";
                    StatusLabelCCD.ForeColor = Color.Red;
                }
            }
            else
            {
                StatusLabelCCD.Text = "Disconnected";
                StatusLabelCCD.ForeColor = Color.Red;
            }

            Activate_buttonsCCD();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            comboBoxCCDs.Items.Clear();
            if (checkBox2.Checked)
            {
                comboBoxCCDs.Enabled = true;
                foreach (SCDid s in MonoSpec.combobox_CCD)
                {
                    comboBoxCCDs.Items.Add(s);
                }
            }
            else
            {
                comboBoxCCDs.Enabled = false;
            }
        }

        //=============================================
        private void buttonGo_Click(System.Object sender, System.EventArgs e)
        {
            Double dValue;
            String sDisplay, sValue;
            Boolean isSCDBusy, bContinuous;
            object oData;
            long m_loopCount;

            oData = new object();

            // clear the list box
            listBoxData.Items.Clear();
            Application.DoEvents();

            m_bSyncAcq = checkBoxSyncAcq.Checked;
            m_loopCount = 0;
            m_bStopAcq = false;

            bContinuous = checkBoxContinuous.Checked;

            buttonAbort.Enabled = true;
            buttonGo.Enabled = false;

            if (m_bSyncAcq == true)
            {
                // A Synchronous (or Serial) acquisition method will take one point of data and stop unless continuous
                // is checked.  NOTE: this method will not relinquish control to the 
                // UI thread (i.e. - the UI will appear hung) until it has completed.  If this operation is 
                // already taking place on a non-ui thread, then this is not an issue.  If your scan is happening 
                // in the main UI thread, then you'd want to consider using the Asynchronous method of acquisition...

                do
                {
                    // Start the acquisition
                    isSCDBusy = true;
                    MonoSpec.mCCD.StartAcquisition(true);
                    while ((isSCDBusy == true) && (m_bStopAcq == false))
                    {
                        // Poll Busy
                        isSCDBusy = MonoSpec.mCCD.AcquisitionBusy();
                    }

                    // Retrieve the data
                    MonoSpec.mCCD.GetData(ref oData);
                    IConvertible convert = oData as IConvertible;
                    if (convert != null)
                        dValue = convert.ToDouble(null);
                    else
                        dValue = 0d;

                    Application.DoEvents();
                    if (m_bStopAcq == true)
                        break;

                    sDisplay = String.Format("{0:0.00}, {1:0.000000}", m_loopCount, dValue);
                    listBoxData.Items.Add(sDisplay);
                    listBoxData.TopIndex = listBoxData.Items.Count - 1;

                    m_loopCount = m_loopCount + 1;

                } while (bContinuous == true);

                buttonAbort.Enabled = false;
                buttonGo.Enabled = true;

            }
            else
            {
                // Asynchronous acquisition uses a psuedo state machine to have the event handler start the next 
                // acquire until the entire scan is complete.  The benefit of this approach is that there
                // is no need to poll the detector for AcqBusy.  This polling would hang the UI thread.  With this 
                // approach, the UI should remain responsive for longer integration times. (Shorter integration
                // times will still show unresponsive behavior, because a lot of processing is taking place in 
                // a short period of time.

                if (m_bStopAcq == false)
                {
                    // Start the Acquisition (Data will be extracted in the event handler
                    // and the next Move and acquire will be started )
                    MonoSpec.mCCD.DoAcquisition(true);
                }

            }
        }


        //=============================================
        private void buttonAbort_Click(System.Object sender, System.EventArgs e)
        {
            m_bStopAcq = true;
        }


        //================= ACQUIRE ===================
        private void SaveBtn_Click(object sender, EventArgs e)
        {
            String saveFileName;

            saveFileName = FileName.Text;
            try
            {
                MonoSpec.ccdData.FileType = JYSYSTEMLIBLib.jySupportedFileType.jySPC;
                MonoSpec.ccdData.Save(saveFileName);
            }
            catch
            {
                MessageBox.Show("Couldn't open data file, make sure folder exists", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void StartBtn_Click(object sender, EventArgs e)
        {
            MonoSpec.Start_acquisition();
        }
        //=============================================
        //==================== STREAM ACQ =====================
        private void GoBtn_Click(object sender, EventArgs e)
        {
            int streaming = 2;
            int local_count;

            try 
            {
                local_count = Int32.Parse(Count.Text);
            } 
            catch 
            {
                local_count = 1;
                Console.WriteLine("Unable to convert to (int), count = 1");
            }

            if (Option1.Checked)
            {
                // Don't use yet
            }
            else if (Option2.Checked)
            {
                streaming = 2;
            }
            else if (Option3.Checked)
            {
                streaming = 3;
            }
            string path = FileName.Text;
            MonoSpec.GoStream(path, local_count, streaming);
        }

        private void OpenPathDialog_Click(object sender, EventArgs e)
        {
            var dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                string fileName;
                fileName = dlg.SelectedPath;
                FileName.Text = fileName;
            }
        }
        //=============================================
    }
}
