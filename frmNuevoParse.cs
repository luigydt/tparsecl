using MaterialSkin;
using MaterialSkin.Controls;
using ParseClientes.Clases;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ParseClientes
{
    public partial class frmNuevoParse : MaterialForm
    {
        BackDB backDB = new BackDB();
        DataSet dataSet = new DataSet();
        TextBox lastFocused = null;
        string txtFocused;

        //Colecciones de strings del data set para los eventos de Autocompletar
        AutoCompleteStringCollection nombresClientes = new AutoCompleteStringCollection();
        AutoCompleteStringCollection nombresPaises = new AutoCompleteStringCollection();
        AutoCompleteStringCollection nombreDivisiones = new AutoCompleteStringCollection();

        public frmNuevoParse()
        {
            InitializeComponent();

            MaterialSkinManager msm = MaterialSkinManager.Instance;
            msm.Theme = MaterialSkinManager.Themes.LIGHT;
            msm.ColorScheme = new ColorScheme(
                Primary.Blue600, Primary.Blue700,
                Primary.Blue800, Accent.LightBlue400,
                TextShade.WHITE
                );

            txtPais.KeyDown += TxtPais_KeyDown; //evento para el txtPais al darle al Enter escribe el código de pais en la caja
        }
        public static List<T> GetControls<T>(Control container) where T : Control //metodo genérico que devuelve una lista de controles de un control contenedor, en nuestro caso lo utilizaremos para que nos devuelva una lista de TextBoxes en el Control Form
        {
            List<T> controls = new List<T>();
            foreach (Control c in container.Controls)
            {
                if (c is T)
                    controls.Add((T)c);
                controls.AddRange(GetControls<T>(c));
            }
            return controls;
        }
        private void llenarGridCol3()
        {
            GetControls<TextBox>(this).ForEach(p => llenarCelda(p.Text, p));
        }

        private void llenarCelda(string s, TextBox t)// metodo privado que utiliza la funcion llenarGridCol3 para recorrer todos los controles TextBox y dependiendo de si lo que esta escrito representa una columna(c;) escribe el tipo de campo en la columna 3 del grid
        {
            if (s.Contains("c;"))
            {
                string[] split = s.Split(';');
                int row = Int32.Parse(split[1]);
                if (row < dataGridView1.Rows.Count)
                    dataGridView1.Rows[row].Cells[2].Value = separarEnEspacios(t.Name.Substring(3));
            }
        }

        private void TxtPais_KeyDown(object sender, KeyEventArgs e)//Evento que controla un tooltip en la caja de texto de CodPais a partir del nombre del pais, ya que si no has asignado un pais al darle al enter si el pais aparecera un mensaje
        {
            NuevoParseCliente n;
            if (txtPais.Text.Length > 1)
            {
                ttPais.Hide(txtCodPais);
                if (e.KeyCode == Keys.Enter)
                {
                    txtCodPais.Text = "";
                    devolverCodPais(txtPais.Text);
                    n = backDB.PlantillaNuevoCliente(txtCliente.Text, txtPais.Text);
                    if (n.idParse != 0)
                    {
                        llenarTextBoxes(n);
                        llenarGridCol3();
                    }
                    else
                    {
                        groupBox0.Text = "ID:XX";
                    }
                }
            }
            else
            {
                if (e.KeyCode == Keys.Enter)
                {
                    txtCodPais.Text = "";
                    ttPais.Show("Escribe un Pais", txtCodPais);
                    ttPais.UseFading = true;
                }
            }
        }

        private void frmNuevoParse_Load(object sender, EventArgs e)//al cargar el form asigna las listas de texto para los autocompletar
        {
            ttFichero.SetToolTip(btnAbrir, "Abrir Fichero (F2)");
            ttAceptar.SetToolTip(btnConfirmar, "Aceptar/Actualizar");
            ttConfig.SetToolTip(btnConfiguracion, "Configuración (F1)");

            DataGridViewCell dgvc = new DataGridViewTextBoxCell();
            dgvc.Style.ForeColor = ColorTranslator.FromHtml("#1976D2");
            dgvc.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataCellCampo.CellTemplate = dgvc;
            dataCellCampo.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

        private void devolverCodPais(string s)//"linq que devuelve una fila del datatable paises a partir del nombre del pais y escribe en la caja de texto CodPais el codigo de ese datarow  y si no lo encuentra o es nuevo crea uno con la ? para que se vea en rojo 
        {
            DataRow r = dataSet.Tables[1].Select().Where(x => x.ItemArray[1].ToString().ToLower().CompareTo(s.ToLower()) == 0).FirstOrDefault();
            if (r != null)
                txtCodPais.Text = r[0].ToString();
            else
                txtCodPais.Text = s.Substring(0, 2).ToUpper() + "?";
        }

        private void rellenarAutoCompletarTexto()//creamos una list de autocompletar para las texbox nombre,pais,division
        {
            foreach (DataRow dr in dataSet.Tables["Clientes"].Rows)
            {
                nombresClientes.Add(dr.ItemArray[0].ToString());
            }

            txtCliente.AutoCompleteCustomSource = nombresClientes;
            txtCliente.AutoCompleteMode = AutoCompleteMode.Suggest;
            txtCliente.AutoCompleteSource = AutoCompleteSource.CustomSource;

            foreach (DataRow dr in dataSet.Tables["Paises"].Rows)
            {
                nombresPaises.Add(dr.ItemArray[1].ToString());
            }

            txtPais.AutoCompleteCustomSource = nombresPaises;
            txtPais.AutoCompleteMode = AutoCompleteMode.Suggest;
            txtPais.AutoCompleteSource = AutoCompleteSource.CustomSource;

            foreach (DataRow dr in dataSet.Tables["Divisiones"].Rows)
            {
                nombreDivisiones.Add(dr.ItemArray[0].ToString());
            }

            txtDivision.AutoCompleteCustomSource = nombreDivisiones;
            txtDivision.AutoCompleteMode = AutoCompleteMode.Suggest;
            txtDivision.AutoCompleteSource = AutoCompleteSource.CustomSource;
        }

        private void groupBox6_Enter(object sender, EventArgs e)
        {
            ///// Autocompletar + event ok -- txt cambiar nombre, lbl revisar, 
        }

        private void btnAbrir_Click(object sender, EventArgs e)//evnto click del boton abrir, que utiliza la clase Excel y el metodo ExceltoDataset para crear el dataset que sirve como Datasource del Grid
        {
            Excel excel = new Excel();
            RellenarGrid(excel.excelToDataset(AbrirFichero()));
            llenarGridCol3();//y que al abrirse escribe en la columna 3 del grid 
        }
        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)//evento key que nos permite borrar una celda al pulsar suprimir
        {

            if (e.KeyCode == Keys.Delete)
            {
                dataGridView1.SelectedCells[0].Value = "";
            }
        }
        private void borrarCeldasIguales(DataGridViewRow s)//borra las celdas que existen con el mismo nombre cada vez que introducimos una referencia nueva
        {
            foreach (DataGridViewRow dr in dataGridView1.Rows)
            {
                if (dr != s)
                {
                    if (dr.Cells[2].Value != null && dr.Cells[2].Value.ToString() == s.Cells[2].Value.ToString())
                    {
                        dr.Cells[2].Value = "";
                    }
                }
            }
        }
        private string AbrirFichero()//abre el fichero y devuelve un string del Path para el metodo ExceltoDataset de la clase ExcelDataReader
        {
            string path = null;
            try
            {
                OpenFileDialog openFile = new OpenFileDialog();
                openFile.Multiselect = false;        //not allow multiline selection at the file selection level
                openFile.Title = "Open File";

                if (openFile.ShowDialog() == DialogResult.OK)        //executing when file open
                {
                    path = openFile.FileName;
                }
            }
            catch { }
            return path;
        }
        private void RellenarGrid(DataSet dataSet)//metodo para rellenar el grid apartir de un dataset
        {
            if (dataSet.Tables.Count != 0)
            {
                for (int i = 0; i < dataSet.Tables[0].Columns.Count; i++)
                {
                    DataGridViewRow viewRow = new DataGridViewRow();
                    viewRow.CreateCells(dataGridView1);
                    viewRow.Cells[0].Value = dataSet.Tables[0].Columns[i].Ordinal.ToString();
                    viewRow.Cells[1].Value = dataSet.Tables[0].Rows[0].ItemArray[i].ToString();
                    dataGridView1.Rows.Add(viewRow);
                }
            }
        }

        private void llenarTextBoxes(NuevoParseCliente cliente)//Llena los textboxes con el clienteparse de plantilla a partir del nombre client elegido
        {
            txtAdrClase.Text = cliente.adrClase;
            txtAdrEmbalaje.Text = cliente.adrEmbalaje;
            txtAdrOnu.Text = cliente.adrOnu;
            txtBultos.Text = cliente.bultos;
            txtCamposAgrupar.Text = cliente.camposAgrupar;
            //txtCliente.Text = "";
            txtCodBulto.Text = cliente.codBultos;
            //txtCodPais.Text = "";
            txtCondiciones.Text = cliente.condiciones;
            txtCP.Text = cliente.cp;
            txtDelimitador.Text = cliente.delimitador;
            txtDesRem.Text = cliente.nombreDesRem;
            txtDireccion.Text = cliente.direccion;
            txtDivisaValor.Text = cliente.divisaValor;
            txtDivisaVinculacion.Text = cliente.divisaVinculacion;
            txtDivision.Text = cliente.division;
            txtEmail.Text = cliente.email;
            txtEntAlmFis.Text = cliente.entAlmFis;
            txtEntAlmLog.Text = cliente.entAlmLog;
            txtEntEmbalaje.Text = cliente.entEmbalaje;
            txtEntPalet.Text = cliente.entPalet;
            txtEntRef.Text = cliente.entReferencia;
            txtEntSituacion.Text = cliente.entSituacion;
            txtEntUbicacion.Text = cliente.entUbicacion;
            txtFechaRVouz.Text = cliente.fechaRVouz;
            txtFechRecogida.Text = cliente.fechaRecogida;
            txtFiltro.Text = cliente.filtro;
            txtFlete.Text = cliente.flete;
            txtHorario.Text = cliente.horario;
            txtIdCorresponsal.Text = cliente.idCorresponsal;
            txtIdDireccion.Text = cliente.idDireccion;
            txtIdFacturacion.Text = cliente.idFacturacion;
            txtIdOrdenante.Text = cliente.idOrdenante;
            txtKg.Text = cliente.kilos;
            txtKgCliente.Text = cliente.kilosCliente;
            txtKgCovia.Text = cliente.kilosCovia;
            txtKgDecimales.Text = cliente.kilosDecimales;
            txtLInicio.Text = cliente.linealInicio;
            txtM3.Text = cliente.m3;
            txtM3Covia.Text = cliente.m3Covia;
            txtMarcas.Text = cliente.marcas;
            txtMercancia.Text = cliente.mercancia;
            txtObservaciones.Text = cliente.observaciones;
            //txtPais.Text = cliente.pai;
            txtPoblacion.Text = cliente.poblacion;
            txtRefCliente.Text = cliente.refCliente;
            txtServicio.Text = cliente.servicio;
            txtTelefono.Text = cliente.telefono;
            txtTipoBulto.Text = cliente.tipoBultos;
            txtXTipoDir.Text = cliente.xTipoDir;
            txtTrafico.Text = cliente.trafico;
            txtUnidades.Text = cliente.unidades;
            txtValor.Text = cliente.valor;
            txtValorVinculacion.Text = cliente.valorVinculacion;
            txtVinculacion.Text = cliente.vinculacion;
            txtXBuscar.Text = cliente.xBuscar;
            txtXCent.Text = cliente.xCent;
            txtXCp.Text = cliente.xCp;
            txtXDireccion.Text = cliente.xDir;
            txtXId.Text = cliente.xId;
            txtXIdir.Text = cliente.xIDir;
            txtXIent.Text = cliente.xIent;
            txtXNom.Text = cliente.xNom;
            txtXPda.Text = cliente.xPda;
            txtXPoblacion.Text = cliente.xPob;
            txtXTipo.Text = cliente.xTipo;

            if (cliente.idParse != 0)
                groupBox0.Text = "ID:" + cliente.idParse.ToString();
            else
                groupBox0.Text = "ID:XX";

        }

        private void txtCliente_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                txtPais.Focus();
            }
        }

        private void txtPais_TextChanged(object sender, EventArgs e)//Al introducir mas 2 caracterres en el Codigo de Pais cambiara a rojo para ver que esta mal
        {
            if (txtCodPais.Text.Length > 2)
            {
                txtCodPais.BackColor = Color.Red;
                txtCodPais.ForeColor = Color.White;
            }
            else
            {
                txtCodPais.BackColor = Color.White;
                txtCodPais.ForeColor = Color.Black;
            }
        }

        private string separarEnEspacios(String s)//Separa en espacios un string para verlo en la columna 2 
        {
            StringBuilder sb = new StringBuilder();
            foreach (Char c in s.ToCharArray())
            {
                if (char.IsLower(c))
                    sb.Append(c);
                else if (char.IsUpper(c))
                {
                    sb.Append(" ");
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)//al hacer dobleClick en las cells del grid apunta a la variable lastFocused(textbox) y escribe en la propiedad text la columna que es y hace foco en ella.
        {
            if (dataGridView1.Rows.Count != 0)
            {
                int col = dataGridView1.SelectedCells[0].RowIndex;
                if (lastFocused != null)
                {
                    lastFocused.Text = "c;" + col.ToString();
                    dataGridView1.Rows[col].Cells[2].Value = separarEnEspacios(lastFocused.Name.Substring(3));
                    borrarCeldasIguales(dataGridView1.Rows[col]);
                    lastFocused.Focus();
                }
            }
        }

        private void txtDelimitador_Enter(object sender, EventArgs e)//Evento que nos permite introducir el texto guardado de la caja de texto al hacer foco 
        {
            TextBox tx = (TextBox)sender;
            txtFocused = tx.Text;
        }

        private void txtDelimitador_Leave(object sender, EventArgs e)//Evento que nos permite guardar la caja de texto en la que estabamos en foco en una variable lastFOcused y nos permite añadir el tipo de dato de la caja de texto en la columna 2 del grid
        {
            lastFocused = (TextBox)sender;
        }

        private void btnConfirmar_Click(object sender, EventArgs e)//Dar de alta un ParseNuevoCliente
        {
            if (groupBox0.Text.Contains("XX"))
                altaNuevoCliente();

            else
                actualizarCliente();
        }

        private NuevoParseCliente clienteNuevo()//Crea una nueva clase NuevoParseCliente para actualizar o dar alta
        {
            NuevoParseCliente cliente = new NuevoParseCliente();
            cliente.adrClase = checkClienteNuevo(txtAdrClase.Text);
            cliente.adrEmbalaje = checkClienteNuevo(txtAdrEmbalaje.Text);
            cliente.adrOnu = checkClienteNuevo(txtAdrOnu.Text);
            cliente.bultos = checkClienteNuevo(txtBultos.Text);
            cliente.camposAgrupar = checkClienteNuevo(txtCamposAgrupar.Text);
            cliente.codBultos = checkClienteNuevo(txtCodBulto.Text);
            cliente.condiciones = checkClienteNuevo(txtCondiciones.Text);
            cliente.cp = checkClienteNuevo(txtCP.Text);
            cliente.delimitador = checkClienteNuevo(txtDelimitador.Text);
            cliente.direccion = checkClienteNuevo(txtDireccion.Text);
            cliente.divisaValor = checkClienteNuevo(txtDivisaValor.Text);
            cliente.divisaVinculacion = checkClienteNuevo(txtDivisaVinculacion.Text);
            cliente.division = checkClienteNuevo(txtDivision.Text);
            cliente.email = checkClienteNuevo(txtEmail.Text);
            cliente.entAlmFis = checkClienteNuevo(txtEntAlmFis.Text);
            cliente.entAlmLog = checkClienteNuevo(txtEntAlmLog.Text);
            cliente.entEmbalaje = checkClienteNuevo(txtEntEmbalaje.Text);
            cliente.entPalet = checkClienteNuevo(txtEntPalet.Text);
            cliente.entReferencia = checkClienteNuevo(txtEntRef.Text);
            cliente.entSituacion = checkClienteNuevo(txtEntSituacion.Text);
            cliente.entUbicacion = checkClienteNuevo(txtEntUbicacion.Text);
            cliente.fechaRecogida = checkClienteNuevo(txtFechRecogida.Text);
            cliente.fechaRVouz = checkClienteNuevo(txtFechaRVouz.Text);
            cliente.filtro = checkClienteNuevo(txtFiltro.Text);
            cliente.flete = checkClienteNuevo(txtFlete.Text);
            cliente.horario = checkClienteNuevo(txtHorario.Text);
            cliente.idCorresponsal = checkClienteNuevo(txtIdCorresponsal.Text);
            cliente.idDireccion = checkClienteNuevo(txtIdDireccion.Text);
            cliente.idFacturacion = checkClienteNuevo(txtIdFacturacion.Text);
            cliente.idOrdenante = checkClienteNuevo(txtIdOrdenante.Text);
            cliente.kilos = checkClienteNuevo(txtKg.Text);
            cliente.kilosCliente = checkClienteNuevo(txtKgCliente.Text);
            cliente.kilosCovia = checkClienteNuevo(txtKgCovia.Text);
            cliente.kilosDecimales = checkClienteNuevo(txtKgDecimales.Text);
            cliente.linealInicio = checkClienteNuevo(txtLInicio.Text);
            cliente.m3 = checkClienteNuevo(txtM3.Text);
            cliente.m3Covia = checkClienteNuevo(txtM3Covia.Text);
            cliente.marcas = checkClienteNuevo(txtMarcas.Text);
            cliente.mercancia = checkClienteNuevo(txtMercancia.Text);
            cliente.nombre = checkClienteNuevo(txtCliente.Text);
            cliente.nombreDesRem = checkClienteNuevo(txtDesRem.Text);
            cliente.nombrePais = checkClienteNuevo(txtPais.Text);
            cliente.observaciones = checkClienteNuevo(txtObservaciones.Text);
            cliente.pais = checkClienteNuevo(txtCodPais.Text);
            cliente.poblacion = checkClienteNuevo(txtPoblacion.Text);
            cliente.refCliente = checkClienteNuevo(txtRefCliente.Text);
            cliente.servicio = checkClienteNuevo(txtServicio.Text);
            cliente.telefono = checkClienteNuevo(txtTelefono.Text);
            cliente.tipoBultos = checkClienteNuevo(txtTipoBulto.Text);
            cliente.trafico = checkClienteNuevo(txtTrafico.Text);
            cliente.unidades = checkClienteNuevo(txtUnidades.Text);
            cliente.valor = checkClienteNuevo(txtValor.Text);
            cliente.valorVinculacion = checkClienteNuevo(txtValorVinculacion.Text);
            cliente.vinculacion = checkClienteNuevo(txtVinculacion.Text);
            cliente.xBuscar = checkClienteNuevo(txtXBuscar.Text);
            cliente.xCent = checkClienteNuevo(txtXCent.Text);
            cliente.xCp = checkClienteNuevo(txtXCp.Text);
            cliente.xDir = checkClienteNuevo(txtXDireccion.Text);
            cliente.xId = checkClienteNuevo(txtXId.Text);
            cliente.xIDir = checkClienteNuevo(txtXIdir.Text);
            cliente.xIent = checkClienteNuevo(txtXIent.Text);
            cliente.xNom = checkClienteNuevo(txtXNom.Text);
            cliente.xPda = checkClienteNuevo(txtXPda.Text);
            cliente.xPob = checkClienteNuevo(txtXPoblacion.Text);
            cliente.xTipo = checkClienteNuevo(txtXTipo.Text);
            cliente.xTipoDir = checkClienteNuevo(txtXTipoDir.Text);

            return cliente;
        }

        private string checkClienteNuevo(string s)//filtra el texto para devolver null para la insert
        {
            if (s.Length != 0)
                return s;
            return null;

        }

        private void altaNuevoCliente()//da de alta un nuevo cliente con id autoincrement en la BD
        {
            NuevoParseCliente altaCliente = clienteNuevo();
            try
            {
                backDB.AltaNuevoParseCliente(altaCliente);
                MessageBox.Show("Alta");
                this.Close();
            }
            catch (MySql.Data.MySqlClient.MySqlException e)
            {
                MessageBox.Show(e.ToString());
                txtCliente.Focus();
            }


        }

        private void actualizarCliente()//actualiza los campos en la BD con el id concreto
        {
            NuevoParseCliente actualizarCliente = clienteNuevo();
            string[] id = groupBox0.Text.Split(':');
            try
            {
                actualizarCliente.idParse = Int32.Parse(id[1]);
                backDB.ActualizarNuevoParseCliente(actualizarCliente);
                MaterialMessageBox.Show("Actualizado!");
                this.Close();
            }
            catch (MySql.Data.MySqlClient.MySqlException e)
            {
                MaterialMessageBox.Show(e.ToString());
            }


        }

        private void btnConfiguracion_Click(object sender, EventArgs e)//evento click del botonConfiguracion que abre un frmConfiguración
        {
            frmConfiguracion frmConfiguracion = new frmConfiguracion();
            frmConfiguracion.ShowDialog();
        }

        private void frmNuevoParse_Activated(object sender, EventArgs e) //Evento del Form que realiza una accion cuando el form pasa a ser Activo
        {
            backDB = new BackDB();//realiza una nueva instancia de la clase backDB para actualizar los datos de configuracion, de esta manera cuando configuramos estos datos en el frmConfig se actualiza esta información en el frmNuevoParse
            if (!BackDB.existeDatosConexion())//metodo estatico de la clase backDB que nos devuelve true si existe una conexion con datos null,
            {
                try
                {
                    dataSet = backDB.NuevoParseClienteDataSet();//metodo que llena la variable privada dataSet del form para los form
                    rellenarAutoCompletarTexto();//

                }
                catch (Exception ex)//si existe algun tipo de fallo en la configuracion de la conexion nos mostrara este mensaje con el tipo de error y al aceptar el Dialog muestra el frmConfiguracion
                {
                    string message = "Fallo de datos de configuracion: " + ex.Message;
                    string caption = "No se pudo conectar";
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
            else//si encuentra un null en BackDB.existeDatos  aparece un mensaje que abrira automaticamente el frmConfiguracion al darle a Aceptar
            {
                string message = "Aceptar para configurar";
                string caption = "No existe Configuracion de Conexion";
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

        private void frmNuevoParse_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
            {
                this.btnAbrir_Click(this, new EventArgs());
            }
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
