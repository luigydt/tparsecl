using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace ParseClientes
{
    public partial class frmMenu : MaterialForm
    {
        BackDB backDB = new BackDB();
        public frmMenu()
        {
            InitializeComponent();
            MaterialSkinManager msm = MaterialSkinManager.Instance;
            msm.Theme = MaterialSkinManager.Themes.LIGHT;

            msm.ColorScheme = new ColorScheme(
                Primary.Blue600, Primary.Blue700,
                Primary.Blue800, Accent.LightBlue400,
                TextShade.WHITE
                );
            ttipConfig.SetToolTip(btnConfiguracion, "Configuración (F1)");
        }

        private void btnAntiguo_Click(object sender, EventArgs e)
        {
            if (!BackDB.existeDatosConexion())//el metodo existedDatosConexion nos devuelve true si la configuracion esta vacía, y false si existe una configuracion
            {
                if (backDB.checkConexion())
                {
                    this.Hide();
                    FrmPrincipal frmPrincipal = new FrmPrincipal();
                    frmPrincipal.ShowDialog();
                    this.Close();
                }
                else
                {
                    string message = "Cambie datos Conexion ";
                    string caption = "Fallo conexión";
                    var result = MessageBox.Show(message, caption,
                                                 MessageBoxButtons.YesNo,
                                                 MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        frmConfiguracion nuevo = new frmConfiguracion();
                        nuevo.ShowDialog();
                    }
                }

            }
            else
            {
                string message = "Iniciar";
                string caption = "No existen Datos Configuracion";
                var result = MessageBox.Show(message, caption,
                                             MessageBoxButtons.YesNo,
                                             MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    frmConfiguracion nuevo = new frmConfiguracion();
                    nuevo.ShowDialog();
                }
            }


        }

        private void btnNuevo_Click(object sender, EventArgs e)
        {
            if (!BackDB.existeDatosConexion())//el metodo existedDatosConexion nos devuelve true si la configuracion esta vacía, y false si existe una configuracion
            {
                if (backDB.checkConexion())
                {
                    this.Hide();
                    frmNuevoParse frm = new frmNuevoParse();
                    frm.ShowDialog();
                    this.Close();
                }
                else
                {
                    string message = "Cambie datos Conexion ";
                    string caption = "Fallo conexión";
                    var result = MessageBox.Show(message, caption,
                                                 MessageBoxButtons.YesNo,
                                                 MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        frmConfiguracion nuevo = new frmConfiguracion();
                        nuevo.ShowDialog();
                    }
                }

            }
            else
            {
                string message = "Iniciar";
                string caption = "No existen Datos Configuracion";
                var result = MessageBox.Show(message, caption,
                                             MessageBoxButtons.YesNo,
                                             MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    frmConfiguracion nuevo = new frmConfiguracion();
                    nuevo.ShowDialog();
                }
            }
        }

        private void btnConfiguracion_Click(object sender, EventArgs e)
        {
            frmConfiguracion frmConfiguracion = new frmConfiguracion();
            frmConfiguracion.ShowDialog();
        }

        private void frmMenu_Activated(object sender, EventArgs e)
        {
            backDB = new BackDB();
        }

        private void frmMenu_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                this.btnConfiguracion_Click(this, new EventArgs());
            }
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            Process.Start(@"O:\ProgramasComunes\UsuariosWeb\Documentos\Parse Clientes(Manual).pdf");
        }
    }
}
