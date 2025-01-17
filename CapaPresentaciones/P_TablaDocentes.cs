using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using CapaEntidades;
using CapaNegocios;
using ImageMagick;

namespace CapaPresentaciones
{
    public partial class P_TablaDocentes : Form
    {
        readonly E_Docente ObjEntidad = new E_Docente();
        readonly N_Docente ObjNegocio = new N_Docente();

        public P_TablaDocentes()
        {
            InitializeComponent();
        }

        private void MensajeConfirmacion(string Mensaje)
        {
            MessageBox.Show(Mensaje, "Sistema de Tutoría", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void MensajeError(string Mensaje)
        {
            MessageBox.Show(Mensaje, "Sistema de Tutoría", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void AccionesTabla()
        {
            dgvTabla.Columns[0].Visible = false;
            dgvTabla.Columns[3].Visible = false;
            dgvTabla.Columns[4].Visible = false;
            dgvTabla.Columns[5].Visible = false;
            dgvTabla.Columns[13].Visible = false;

            dgvTabla.Columns[1].HeaderText = "";
            dgvTabla.Columns[2].HeaderText = "Cod. Docente";
            dgvTabla.Columns[6].HeaderText = "Docente";
            dgvTabla.Columns[7].HeaderText = "Email";
            dgvTabla.Columns[8].HeaderText = "Dirección";
            dgvTabla.Columns[9].HeaderText = "Teléfono";
            dgvTabla.Columns[10].HeaderText = "Categoría";
            dgvTabla.Columns[11].HeaderText = "Subcategoría";
            dgvTabla.Columns[12].HeaderText = "Régimen";
            dgvTabla.Columns[14].HeaderText = "Escuela Profesional";
            dgvTabla.Columns[15].HeaderText = "Horario";
        }

        public void MostrarRegistros()
        {
            dgvTabla.DataSource = N_Docente.MostrarRegistros(E_InicioSesion.Usuario);
            AccionesTabla();
        }

        public void BuscarRegistros()
        {
            dgvTabla.DataSource = N_Docente.BuscarRegistros(E_InicioSesion.Usuario, txtBuscar.Text);
        }

        private void ActualizarDatos(object sender, FormClosedEventArgs e)
        {
            MostrarRegistros();
        }

        private void ExportarDatos(DataGridView Datos)
        {
            Microsoft.Office.Interop.Excel.Application ArchivoExcel = new Microsoft.Office.Interop.Excel.Application();
            ArchivoExcel.Application.Workbooks.Add(true);
            int IndiceColumna = 0;

            foreach (DataGridViewColumn Columna in Datos.Columns)
            {
                IndiceColumna++;
                ArchivoExcel.Cells[1, IndiceColumna] = Columna.Name;
            }

            int IndiceFila = 0;

            foreach (DataGridViewRow Fila in Datos.Rows)
            {
                IndiceFila++;
                IndiceColumna = 0;
                foreach (DataGridViewColumn Columna in Datos.Columns)
                {
                    IndiceColumna++;
                    ArchivoExcel.Cells[IndiceFila + 1, IndiceColumna] = Fila.Cells[Columna.Name].Value;
                }
            }

            ArchivoExcel.Visible = true;
        }

        public Image HacerImagenCircular(Image img)
        {
            int x = img.Width / 2;
            int y = img.Height / 2;
            int r = Math.Min(x, y);

            Bitmap tmp = null;
            tmp = new Bitmap(2 * r, 2 * r);
            using (Graphics g = Graphics.FromImage(tmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TranslateTransform(tmp.Width / 2, tmp.Height / 2);
                GraphicsPath gp = new GraphicsPath();
                gp.AddEllipse(0 - r, 0 - r, 2 * r, 2 * r);
                Region rg = new Region(gp);
                g.SetClip(rg, CombineMode.Replace);
                Bitmap bmp = new Bitmap(img);
                g.DrawImage(bmp, new Rectangle(-r, -r, 2 * r, 2 * r), new Rectangle(x - r, y - r, 2 * r, 2 * r), GraphicsUnit.Pixel);

            }

            return tmp;
        }

        private void P_TablaDocentes_Load(object sender, EventArgs e)
        {
            MostrarRegistros();
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            P_DatosDocente NuevoRegistro = new P_DatosDocente();
            NuevoRegistro.FormClosed += new FormClosedEventHandler(ActualizarDatos);
            NuevoRegistro.ShowDialog();
            NuevoRegistro.Dispose();
        }

        private void btnModificar_Click(object sender, EventArgs e)
        {
            P_DatosDocente EditarRegistro = new P_DatosDocente();
            EditarRegistro.FormClosed += new FormClosedEventHandler(ActualizarDatos);

            if (dgvTabla.SelectedRows.Count > 0)
            {
                Program.Evento = 1;

                if (dgvTabla.CurrentRow.Cells[0].Value.GetType() == Type.GetType("System.DBNull"))
                {
                    string fullImagePath = System.IO.Path.Combine(Application.StartupPath, @"../../Iconos/Perfil Docente.png");
                    EditarRegistro.imgPerfil.Image = Image.FromFile(fullImagePath);
                }
                else
                {
                    byte[] Perfil = new byte[0];
                    Perfil = (byte[])dgvTabla.CurrentRow.Cells[0].Value;
                    MemoryStream MemoriaPerfil = new MemoryStream(Perfil);
                    EditarRegistro.imgPerfil.Image = HacerImagenCircular(Bitmap.FromStream(MemoriaPerfil));
                    MemoriaPerfil = null;
                    MemoriaPerfil = null;
                }

                EditarRegistro.txtCodigo.Text = dgvTabla.CurrentRow.Cells[2].Value.ToString();
                EditarRegistro.txtAPaterno.Text = dgvTabla.CurrentRow.Cells[3].Value.ToString();
                EditarRegistro.txtAMaterno.Text = dgvTabla.CurrentRow.Cells[4].Value.ToString();
                EditarRegistro.txtNombre.Text = dgvTabla.CurrentRow.Cells[5].Value.ToString();
                EditarRegistro.txtEmail.Text = dgvTabla.CurrentRow.Cells[7].Value.ToString().Substring(0, dgvTabla.CurrentRow.Cells[7].Value.ToString().Length - 14);
                EditarRegistro.txtDireccion.Text = dgvTabla.CurrentRow.Cells[8].Value.ToString();
                EditarRegistro.txtTelefono.Text = dgvTabla.CurrentRow.Cells[9].Value.ToString();

                EditarRegistro.cxtSubcategoria.Items.Clear();
                EditarRegistro.cxtRegimen.Items.Clear();
                EditarRegistro.cxtRegimen.Items.Add("TIEMPO COMPLETO");
                EditarRegistro.cxtRegimen.Items.Add("TIEMPO PARCIAL");

                if (dgvTabla.CurrentRow.Cells[10].Value.ToString() == "NOMBRADO")
                {

                    EditarRegistro.cxtSubcategoria.Items.Add("PRINCIPAL");
                    EditarRegistro.cxtSubcategoria.Items.Add("ASOCIADO");
                    EditarRegistro.cxtSubcategoria.Items.Add("AUXILIAR");

                    EditarRegistro.cxtRegimen.Enabled = true;
                    EditarRegistro.cxtRegimen.Items.Insert(1, "DEDICACIÓN EXCLUSIVA");
                }
                else
                {
                    EditarRegistro.cxtSubcategoria.Items.Add("A1");
                    EditarRegistro.cxtSubcategoria.Items.Add("A2");
                    EditarRegistro.cxtSubcategoria.Items.Add("A3");
                    EditarRegistro.cxtSubcategoria.Items.Add("B1");
                    EditarRegistro.cxtSubcategoria.Items.Add("B2");
                    EditarRegistro.cxtSubcategoria.Items.Add("B3");

                    EditarRegistro.cxtRegimen.Enabled = false;
                }

                EditarRegistro.cxtCategoria.SelectedItem = dgvTabla.CurrentRow.Cells[10].Value.ToString();
                EditarRegistro.cxtSubcategoria.SelectedItem = dgvTabla.CurrentRow.Cells[11].Value.ToString();
                EditarRegistro.cxtRegimen.SelectedItem = dgvTabla.CurrentRow.Cells[12].Value.ToString();

                EditarRegistro.cxtEscuela.SelectedValue = dgvTabla.CurrentRow.Cells[13].Value.ToString();
                EditarRegistro.txtHorario.Text = dgvTabla.CurrentRow.Cells[15].Value.ToString();
                

                EditarRegistro.ShowDialog();
            }
            else
            {
                MensajeError("Debe seleccionar una fila");
            }
            EditarRegistro.Dispose();
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (dgvTabla.SelectedRows.Count > 0)
            {
                DialogResult Opcion;
                Opcion = MessageBox.Show("¿Realmente desea eliminar el registro?", "Sistema de Tutoría", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (Opcion == DialogResult.OK)
                {
                    ObjEntidad.CodDocente = dgvTabla.CurrentRow.Cells[0].Value.ToString();
                    ObjNegocio.EliminarRegistros(ObjEntidad);
                    MensajeConfirmacion("Registro eliminado exitosamente");
                    MostrarRegistros();
                }
            }
            else
            {
                MensajeError("Debe seleccionar una fila");
            }
        }

        private void btnExportar_Click(object sender, EventArgs e)
        {
            ExportarDatos(dgvTabla);
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            BuscarRegistros();
        }

        private void dgvTabla_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvTabla.Columns[e.ColumnIndex].HeaderText == "")
            {
                byte[] bits = new byte[0];
                bits = (byte[])e.Value;
                MemoryStream ms = new MemoryStream(bits);
                Image imgSave = Image.FromStream(ms);
                e.Value = HacerImagenCircular(imgSave);
            }
        }
    }
}
