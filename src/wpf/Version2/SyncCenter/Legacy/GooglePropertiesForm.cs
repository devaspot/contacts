//-------------------------------------------------------------------------- 
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: PropertiesForm.cs
//			
//  Description: Supports editing of the FileSync properties.
//
//-------------------------------------------------------------------------- 

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Synrc
{
	/// <summary>
	/// Supports editing of the FileSync properties.
	/// </summary>
	public class GooglePropertiesForm : System.Windows.Forms.Form
	{
		public string RegistryComment;
		private System.Windows.Forms.Button okButton;
		public TextBox textBox1;
		public TextBox textBox2;
		private Label label1;
		private Label label2;
		public CheckBox checkBox1;
		private ComboBox comboBox1;
		private Button button1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Allows the user to edit the properties of the file synchronization instance.
		/// </summary>
		public GooglePropertiesForm()
		{
			//
			// Required for Windows Form Designer support.
			//
			InitializeComponent();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.okButton = new System.Windows.Forms.Button();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.textBox2 = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.comboBox1 = new System.Windows.Forms.ComboBox();
			this.button1 = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Location = new System.Drawing.Point(224, 124);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(75, 23);
			this.okButton.TabIndex = 12;
			this.okButton.Text = "OK";
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(133, 15);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(243, 20);
			this.textBox1.TabIndex = 13;
			// 
			// textBox2
			// 
			this.textBox2.Location = new System.Drawing.Point(133, 41);
			this.textBox2.Name = "textBox2";
			this.textBox2.PasswordChar = '*';
			this.textBox2.Size = new System.Drawing.Size(243, 20);
			this.textBox2.TabIndex = 14;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(72, 18);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(55, 13);
			this.label1.TabIndex = 15;
			this.label1.Text = "Username";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(74, 44);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(53, 13);
			this.label2.TabIndex = 16;
			this.label2.Text = "Password";
			// 
			// checkBox1
			// 
			this.checkBox1.AutoSize = true;
			this.checkBox1.Location = new System.Drawing.Point(133, 94);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size(76, 17);
			this.checkBox1.TabIndex = 18;
			this.checkBox1.Text = "Read Only";
			this.checkBox1.UseVisualStyleBackColor = true;
			// 
			// comboBox1
			// 
			this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBox1.FormattingEnabled = true;
			this.comboBox1.Items.AddRange(new object[] {
            "Given Middle Family",
            "Family Given Middle"});
			this.comboBox1.Location = new System.Drawing.Point(133, 67);
			this.comboBox1.Name = "comboBox1";
			this.comboBox1.Size = new System.Drawing.Size(166, 21);
			this.comboBox1.TabIndex = 19;
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(305, 124);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 20;
			this.button1.Text = "Cancel";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// GooglePropertiesForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(388, 159);
			this.ControlBox = false;
			this.Controls.Add(this.button1);
			this.Controls.Add(this.comboBox1);
			this.Controls.Add(this.checkBox1);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.textBox2);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.okButton);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "GooglePropertiesForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Google Contacts Properties";
			this.Load += new System.EventHandler(this.GooglePropertiesForm_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private void GooglePropertiesForm_Load(object sender, EventArgs e)
		{
			comboBox1.SelectedIndex = 0;
		}

		private void okButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}

		private void button1_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}

	}
}