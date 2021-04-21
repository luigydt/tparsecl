using MaterialSkin.Controls;
using System;

namespace ParseClientes
{
    public partial class frmConfiguracion : MaterialForm
    {
        DatosConexion datosConexion;
        public frmConfiguracion()
        {
            InitializeComponent();
            datosConexion = BackDB.cargarDatosConexion();
        }

        private void frmConfig_Load(object sender, EventArgs e) // Evento que se realiza al cargar el Form y asigna a las cajas de texto la informacion de conexion almacenada mediante LiteDB en la lista datosConexion
        {
            txtDataBase.Text = datosConexion.dataBase;
            txtHost.Text = datosConexion.host;
            txtPassword.Text = datosConexion.stringPwd;
            txtPuerto.Text = datosConexion.port;
            txtUsuario.Text = datosConexion.usuario;

        }

        private void btnAceptar_Click(object sender, EventArgs e)//Al aceptar actualiza los datos de conexion  y cierra el form
        {
            datosConexion.host = txtHost.Text;
            datosConexion.dataBase = txtDataBase.Text;
            datosConexion.port = txtPuerto.Text;
            datosConexion.stringPwd = txtPassword.Text;
            datosConexion.usuario = txtUsuario.Text;

            BackDB.guardarDatosConexion(datosConexion);

            this.Close();
        }
    }
}
