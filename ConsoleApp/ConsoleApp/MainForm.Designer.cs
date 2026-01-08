using System.ComponentModel;
using System.Windows.Forms;
using ScottPlot.WinForms;

namespace ConsoleApp
{
    partial class MainForm
    {
        private IContainer components = null;
        private TableLayoutPanel layoutRoot;
        private Label labelTitle;
        private ComboBox comboYear;
        private Label labelStatus;
        private FormsPlot formsPlot1;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            layoutRoot = new TableLayoutPanel();
            buttonSave = new Button();
            labelTitle = new Label();
            formsPlot1 = new FormsPlot();
            labelStatus = new Label();
            comboYear = new ComboBox();
            layoutRoot.SuspendLayout();
            SuspendLayout();
            // 
            // layoutRoot
            // 
            layoutRoot.ColumnCount = 2;
            layoutRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80F));
            layoutRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            layoutRoot.Controls.Add(buttonSave, 1, 1);
            layoutRoot.Controls.Add(labelTitle, 0, 0);
            layoutRoot.Controls.Add(formsPlot1, 0, 2);
            layoutRoot.Controls.Add(labelStatus, 0, 3);
            layoutRoot.Controls.Add(comboYear, 0, 1);
            layoutRoot.Dock = DockStyle.Fill;
            layoutRoot.Location = new System.Drawing.Point(0, 0);
            layoutRoot.Margin = new Padding(4);
            layoutRoot.Name = "layoutRoot";
            layoutRoot.RowCount = 4;
            layoutRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 63F));
            layoutRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 51F));
            layoutRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layoutRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            layoutRoot.Size = new System.Drawing.Size(1029, 760);
            layoutRoot.TabIndex = 0;
            // 
            // buttonSave
            // 
            buttonSave.Dock = DockStyle.Right;
            buttonSave.Location = new System.Drawing.Point(827, 67);
            buttonSave.Margin = new Padding(4);
            buttonSave.Name = "buttonSave";
            buttonSave.Size = new System.Drawing.Size(198, 43);
            buttonSave.TabIndex = 5;
            buttonSave.Text = "儲存圖檔";
            // 
            // labelTitle
            // 
            layoutRoot.SetColumnSpan(labelTitle, 2);
            labelTitle.Dock = DockStyle.Fill;
            labelTitle.Font = new System.Drawing.Font("微軟正黑體", 16F, System.Drawing.FontStyle.Bold);
            labelTitle.Location = new System.Drawing.Point(4, 0);
            labelTitle.Margin = new Padding(4, 0, 4, 0);
            labelTitle.Name = "labelTitle";
            labelTitle.Size = new System.Drawing.Size(1021, 63);
            labelTitle.TabIndex = 0;
            labelTitle.Text = "高雄市觀光景點到訪人數";
            labelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // formsPlot1
            // 
            layoutRoot.SetColumnSpan(formsPlot1, 2);
            formsPlot1.DisplayScale = 1.25F;
            formsPlot1.Dock = DockStyle.Fill;
            formsPlot1.Location = new System.Drawing.Point(4, 118);
            formsPlot1.Margin = new Padding(4);
            formsPlot1.Name = "formsPlot1";
            formsPlot1.Size = new System.Drawing.Size(1021, 600);
            formsPlot1.TabIndex = 3;
            // 
            // labelStatus
            // 
            layoutRoot.SetColumnSpan(labelStatus, 2);
            labelStatus.Dock = DockStyle.Fill;
            labelStatus.Location = new System.Drawing.Point(4, 722);
            labelStatus.Margin = new Padding(4, 0, 4, 0);
            labelStatus.Name = "labelStatus";
            labelStatus.Size = new System.Drawing.Size(1021, 38);
            labelStatus.TabIndex = 4;
            labelStatus.Text = "狀態：";
            labelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // comboYear
            // 
            comboYear.Anchor = AnchorStyles.Right;
            comboYear.DropDownStyle = ComboBoxStyle.DropDownList;
            comboYear.Location = new System.Drawing.Point(542, 75);
            comboYear.Margin = new Padding(4);
            comboYear.Name = "comboYear";
            comboYear.Size = new System.Drawing.Size(277, 27);
            comboYear.TabIndex = 1;
            comboYear.SelectedIndexChanged += comboYear_SelectedIndexChanged;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1029, 760);
            Controls.Add(layoutRoot);
            Margin = new Padding(4);
            Name = "MainForm";
            Text = "高雄市觀光景點到訪人數";
            Load += MainForm_Load;
            layoutRoot.ResumeLayout(false);
            ResumeLayout(false);
        }
        private Button buttonSave;
    }
}