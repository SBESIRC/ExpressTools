using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ThColumnInfo.View
{
    public partial class DataResult : UserControl
    {
        public DataResult()
        {
            InitializeComponent();
            Init();
        }
        private void Init()
        {
            this.dataGridView1.Columns.Add("code","编号");
            this.dataGridView1.Columns.Add("bh","bxh");
            this.dataGridView1.Columns.Add("num", "数量");
            this.dataGridView1.Columns.Add("all", "全部纵筋");
            this.dataGridView1.Columns.Add("corner", "角筋");
            this.dataGridView1.Columns.Add("bSide", "b边一侧中部筋");
            this.dataGridView1.Columns.Add("hside", "h边一侧中部筋");
            this.dataGridView1.Columns.Add("hooping", "箍筋");
            this.dataGridView1.Columns.Add("limbNum", "肢数");
            int baseWidth = 80;
            for(int i=0;i<this.dataGridView1.Columns.Count;i++)
            {
                this.dataGridView1.Columns[i].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                this.dataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;

                switch (this.dataGridView1.Columns[i].Name)
                {
                    case "all":
                        this.dataGridView1.Columns[i].Width = (int)(baseWidth*1.4);
                        break;
                    case "bSide":
                    case "hside":
                        this.dataGridView1.Columns[i].Width = (int)(baseWidth * 0.8);
                        break;
                    case "hooping":
                        this.dataGridView1.Columns[i].Width = (int)(baseWidth * 1.4);
                        break;
                    case "limbNum":
                        this.dataGridView1.Columns[i].Width = (int)(baseWidth * 0.9);
                        break;
                    default:
                        this.dataGridView1.Columns[i].Width = baseWidth;
                        break;
                }
            }

            this.dataGridView1.RowHeadersVisible = false;


        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if(e.ColumnIndex<0 || e.RowIndex<0)
            {
                return;
            }
            if(this.dataGridView1.Columns[e.ColumnIndex].Name!= "code")
            {
                return;
            }
            //ColumnInf columnInf = this.dataGridView1.Rows[e.RowIndex].Tag as ColumnInf;
            //if(columnInf==null)
            //{
            //    return;
            //}
            //List<string> handles = columnInf.Handles.Distinct().ToList();
            //if (handles.IndexOf(columnInf.CurrentHandle) < 0)
            //{
            //    handles.Add(columnInf.CurrentHandle);
            //}
            //DrawableOverruleController.RemoveDrawableRule(); //先移除，再附加
            //DrawableOverruleController.ShowHatchForColumn(handles);
        }
    }
}
