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

namespace MPhys.GUI
{
    public partial class AutoForm : Form
    {
        private System.Data.DataTable dataTable;

        public AutoForm()
        {
            InitializeComponent();
            this.Location = new Point(60, 26);
            this.ControlBox = false;
            this.TopLevel = false;
            this.TopMost = true;

            comboNDF1pos.SelectedIndex = 0;
            comboNDF2pos.SelectedIndex = 0;
            //comboNDF1pos.Text = "Select";
            //comboNDF2pos.Text = "Select";
            textBoxTemp.Text = "295.55";
            textBoxExp.Text = "0.1";

            dataTable = new DataTable();
            dataTable.TableName = "PendingTask";
            Create_DataSet();
        }

        private bool Check_unique_rows()
        {
            DataView view = new DataView(dataTable);
            DataTable distinctValues = view.ToTable(true, "temperature", "NDF1pos", "NDF2pos", "ExpTime");
            if(distinctValues.Rows.Count == dataTable.Rows.Count)
            {
                Console.WriteLine("Fine");
                return true;
            }
            else
            {
                dataTable = distinctValues;
                MessageBox.Show("This value was already entered");
                return false;
            }
        }

        private void Create_DataSet()
        {
            DataColumn column;
            
            // TEMP
            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Double");
            column.ColumnName = "temperature";
            column.ReadOnly = false;
            column.Unique = false;
            dataTable.Columns.Add(column);

            //NDF1
            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Int32");
            column.ColumnName = "NDF1pos";
            column.ReadOnly = false;
            column.Unique = false;
            dataTable.Columns.Add(column);

            //NDF2
            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Int32");
            column.ColumnName = "NDF2pos";
            column.ReadOnly = false;
            column.Unique = false;
            dataTable.Columns.Add(column);

            //Exposure Time
            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Double");
            column.ColumnName = "ExpTime";
            column.ReadOnly = false;
            column.Unique = false;
            dataTable.Columns.Add(column);
        }

        private void Update_TextBox()
        {
            string textrow;
            listBoxTasks.Items.Clear();
            for (int i =0; i < dataTable.Rows.Count; i++)
            {
                DataRow lastRow = dataTable.Rows[i];

                string id = i.ToString();
                string temp = lastRow["temperature"].ToString();
                string pos1 = lastRow["NDF1pos"].ToString();
                string pos2 = lastRow["NDF2pos"].ToString();
                string expt = lastRow["ExpTime"].ToString();

                textrow = id + "  |  " + temp + "  |  " + pos1 + "  |  " + pos2 + "  |  " + expt + "  ";
                listBoxTasks.Items.Add(textrow);
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            DataRow row;

            try
            {
                double expt = double.Parse(textBoxExp.Text);
                double temp = double.Parse(textBoxTemp.Text);
                int pos1 = int.Parse(comboNDF1pos.SelectedItem.ToString());
                int pos2 = int.Parse(comboNDF2pos.SelectedItem.ToString());
                int id;

                if (temp < 5.0 || temp > 400)
                {
                    MessageBox.Show("Temperature is too small or too big");
                    return;
                }
                if (expt < 0.01)
                {
                    MessageBox.Show("Exposure time is too small");
                    return;
                }

                row = dataTable.NewRow();

                id = dataTable.Rows.Count;
                row["temperature"] = temp;
                row["NDF1pos"] = pos1;
                row["NDF2pos"] = pos2; 
                row["ExpTime"] = expt;
                dataTable.Rows.Add(row);

                string text = id + "  |  " + temp + "  |  " + pos1 + "  |  " + pos2 + "  |  " + expt + "  ";
                if (Check_unique_rows())
                {
                    listBoxTasks.Items.Add(text);
                }
                    

            }
            catch
            {
                MessageBox.Show("Incorrect inputs, cannot add");
            }
        }

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            string CombinedPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Profiles");
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = System.IO.Path.GetFullPath(CombinedPath);
            dlg.Filter = "xml files (*.xml)|*.xml";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                dataTable.Clear();
                string fileName;
                fileName = dlg.FileName;
                dataTable.ReadXml(fileName);
            }
            Update_TextBox();
        }

        private void buttonSaveProfile_Click(object sender, EventArgs e)
        {
            string path = ".\\Profiles";
            System.IO.Directory.CreateDirectory(path);

            string filename = "ProfileName";

            if (InputBox("Save the profile", "Enter the name of the profile (.xml)", ref filename) == DialogResult.OK)
            {
                dataTable.WriteXml(path + "\\" + filename + ".xml");
            }
            
        }

        public static DialogResult InputBox(string title, string promptText, ref string value)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }

    }
}
