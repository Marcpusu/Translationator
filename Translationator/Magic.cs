using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using System.IO;

namespace Translationator
{
    public partial class Magic : Form
    {
        //int iCount = 0;
        //int iInterval = 200;
        List<Traduction> lst = new List<Traduction>();
        public Magic(string[] ResourcesFiles, bool Fast)
        {
            InitializeComponent();

            this.KeyPreview = true;

            DoMagic(ResourcesFiles, Fast);
        }

        private void DoMagic(string[] ResourcesFiles, bool bFast)
        {
            List<Traduction> lst2 = new List<Traduction>();
            dgv.Columns.Add("Id", "Id");
            if (bFast)
            {
                #region Ver la carga rapida con posibles errores en los campos vacíos

                foreach (string sResourcesFile in ResourcesFiles)
                {
                    dgv.Columns.Add(sResourcesFile, Path.GetFileName(sResourcesFile));
                    XDocument xml = XDocument.Load(sResourcesFile);
                    xml.Root.Descendants("data").ToList().ForEach(x =>
                    {
                        if (lst2.Count(y => y.Value[0] == x.Attribute("name").Value.ToString()) > 0)
                            lst2.First(y => y.Value[0] == x.Attribute("name").Value.ToString()).Value.Add((string)x.Element("value"));
                        else
                        {
                            Traduction tr = new Traduction(x.Attribute("name").Value.ToString());
                            tr.Value.Add((string)x.Element("value"));
                            lst2.Add(tr);
                        }
                    });
                }

                #endregion
            }
            else
            {
                #region Ver los idiomas con campos vacíos (Tarda bastante en cargar)
                foreach (string sResourcesFile in ResourcesFiles)
                {
                    XDocument xml = XDocument.Load(sResourcesFile);
                    xml.Root.Descendants("data").ToList().ForEach(x =>
                    {
                        if (lst2.Count(y => y.Value[0] == x.Attribute("name").Value.ToString()) == 0)
                        {
                            Traduction tr = new Traduction(x.Attribute("name").Value.ToString());
                            lst2.Add(tr);
                        }
                    });
                }

                foreach (string sResourcesFile in ResourcesFiles)
                {
                    dgv.Columns.Add(sResourcesFile, Path.GetFileName(sResourcesFile));

                    foreach (Traduction tr in lst2)
                    {
                        XDocument xml = XDocument.Load(sResourcesFile);
                        if (xml.Root.Descendants("data").Count(x => x.Attribute("name").Value.ToString() == tr.Value[0].ToString()) > 0)
                        {
                            tr.Value.Add((string)xml.Root.Descendants("data").First(x => x.Attribute("name").Value.ToString() == tr.Value[0].ToString()).Element("value"));
                        }
                        else
                            tr.Value.Add(string.Empty);

                    }
                }
                #endregion
            }

            lst = lst2.OrderBy(x => x.Value[0]).ToList();

            ChargeGrid();
        }

        private void ChargeGrid()
        {
            //int iItemsSelect = 0;
            //if (lst.Count < iCount + iInterval)
            //    iItemsSelect = (lst.Count - iCount);
            //else
            //    iItemsSelect = iInterval;

            //if (lst.Count < (iCount + iItemsSelect) || iItemsSelect < 0)
            //    return;

            //lst.GetRange(iCount, iItemsSelect).ForEach(x =>
            //{
            //    dgv.Rows.Add(x.Value.ToArray());
            //});
            //iCount += iInterval;

            foreach (Traduction t in lst)
            {
                dgv.Rows.Add(t.Value.ToArray());
            }
        }

        private void dgv_Scroll(object sender, ScrollEventArgs e)
        {
            //if (e.Type == ScrollEventType.SmallIncrement || e.Type == ScrollEventType.LargeIncrement)
            //{
            //    if (e.NewValue >= dgv.Rows.Count - GetDisplayedRowsCount())
            //    {
            //        ChargeGrid();
            //    }
            //}
        }

        private int GetDisplayedRowsCount()
        {
            int count = dgv.Rows[dgv.FirstDisplayedScrollingRowIndex].Height;
            count = dgv.Height / count;
            return count;
        }

        private void dgv_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex != -1 && ((DataGridView)sender).Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString() != string.Empty && e.ColumnIndex > 0)
            {
                ModifyRegister(((DataGridView)sender).Columns[e.ColumnIndex].Name, ((DataGridView)sender).Rows[e.RowIndex].Cells[0].Value.ToString(), ((DataGridView)sender).Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString());
            }
        }

        private void ModifyRegister(string sFile, string sId, string sValue)
        {
            XDocument xml = XDocument.Load(sFile);
            if (xml.Root.Descendants("data").Count(x => x.Attribute("name").Value.ToString() == sId) > 0)
                xml.Root.Descendants("data").First(x => x.Attribute("name").Value.ToString() == sId).Element("value").Value = sValue;
            else
            {
                XElement elem = new XElement("data");
                elem.Add(new XAttribute("name", sId));
                elem.Add(new XElement("value", sValue));
                xml.Root.Add(elem);
            }
            xml.Save(sFile);
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            Buscar();
        }

        private void txtBuscar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Buscar();
            }
        }

        private void Buscar()
        {
            bool bTrobat = false;
            int iIndexRow = dgv.SelectedCells.Count == 0 ? 0 : dgv.SelectedCells[0].RowIndex;
            int iIndexCol = dgv.SelectedCells.Count == 0 ? 0 : dgv.SelectedCells[0].ColumnIndex;

            if (iIndexCol == dgv.ColumnCount - 1)
            {
                iIndexRow++;
                iIndexCol = 0;
            }
            else
                iIndexCol++;

            for (int i = iIndexRow; i < dgv.RowCount; i++)
            {
                for (int j = iIndexCol; j < dgv.ColumnCount; j++)
                {
                    if (dgv.Rows[i].Cells[j].Value != null && dgv.Rows[i].Cells[j].Value.ToString().ToLower().Contains(txtBuscar.Text.ToLower()))
                    {
                        dgv.ClearSelection();
                        dgv.Rows[i].Cells[j].Selected = true;
                        dgv.FirstDisplayedScrollingRowIndex = dgv.Rows[i].Index;
                        bTrobat = true;
                        break;
                    }
                }

                iIndexCol = 0;

                if (bTrobat) break;
            }

            if (!bTrobat)
            {
                for (int i = 0; i < iIndexRow; i++)
                {
                    for (int j = iIndexCol; j < dgv.ColumnCount; j++)
                    {
                        if (dgv.Rows[i].Cells[j].Value != null && dgv.Rows[i].Cells[j].Value.ToString().ToLower().Contains(txtBuscar.Text.ToLower()))
                        {
                            dgv.ClearSelection();
                            dgv.Rows[i].Cells[j].Selected = true;
                            dgv.FirstDisplayedScrollingRowIndex = dgv.Rows[i].Index;
                            bTrobat = true;
                            break;
                        }
                    }

                    iIndexCol = 0;

                    if (bTrobat) break;
                }
            }
        }

        private void Magic_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.F || e.KeyCode == Keys.B) && e.Modifiers == Keys.Control)
            {
                txtBuscar.Focus();
                txtBuscar.SelectAll();
            }
        }

        private void dgv_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            dgv.CurrentCell = ((DataGridView)sender).Rows[e.RowIndex].Cells[e.ColumnIndex];
            dgv.BeginEdit(true);
        }
    }

    public class Traduction
    {
        public Traduction(string sId)
        {
            Value = new List<string>();
            Value.Add(sId);
        }

        public List<string> Value { get; set; }
    }
}
