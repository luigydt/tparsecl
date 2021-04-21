using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using ParseClientes.Clases;

namespace ParseClientes
{
    public partial class FrmPrincipal : MaterialForm
    {
        Excel excel; //clase Excel que utiliza la libreria ExcelDataReader
        int countC; //variable int = cboxClientes.Count

        int c = 0; // variable que utilizamos como contador de las veces que esta activa la sesion.


        BackDB backDB = new BackDB(); //clase BackDB para las conexiones a Datos
        ParseCliente parseCliente; //Clase parseCliente que cambia y rellena los controles dpendiendo del cboxCliente seleccionado
        TextBox lastFocused = null; // clase TextBox que apunta a la ultima cajadetexto seleccionado
        DataTable Clientes, Paises; //tables para rellenar los combos // cambiar a dataset si se puede en vez de 2 table?

        public FrmPrincipal()
        {
            InitializeComponent();

            MaterialSkinManager msm = MaterialSkinManager.Instance;
            msm.Theme = MaterialSkinManager.Themes.LIGHT;

            msm.ColorScheme = new ColorScheme(
                Primary.Blue600, Primary.Blue700,
                Primary.Blue800, Accent.LightBlue400,
                TextShade.WHITE
                );
        }

        string AbrirFichero()//abre el fichero y devuelve un string del Path para el metodo ExceltoDataset de la clase ExcelDataReader
        {
            string path = null;
            try
            {
                OpenFileDialog openFile = new OpenFileDialog();
                openFile.Multiselect = false;        //no permite selección múltiple de ficheros
                openFile.Title = "Open File";

                if (openFile.ShowDialog() == DialogResult.OK)
                {
                    path = openFile.FileName;
                }
            }
            catch { }
            return path;
        }

        private void RellenarGrid(DataSet dataSet)//metodo para rellenar el grid apartir de un dataset
        {
            dataGridView1.Rows.Clear();
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

        private void btnAbrir_Click(object sender, EventArgs e)//Abre un FileDialog para seleccionar el EXCEL que queremos abrir para la plantilla del GRID
        {
            excel = new Excel();
            RellenarGrid(excel.excelToDataset(AbrirFichero()));
            llenarGridCol3();
        }

        private void btnNuevo_Click(object sender, EventArgs e)//boton para dar de insertar una nueva fila en la tabla Parseclients
        {
            try
            {
                if (backDB.ExisteParseCliente(txtIDCli.Text, txtCodPais.Text, cboxClientes.SelectedValue.ToString(), txtDelegacion.Text))//llamada al metodo ExisteParseCliente que nos devuelve un true el cual determina si se trata de una actualización o de un nuevo usuario
                {
                    string message = "¿Desea Actualizar?";
                    string caption = "Cliente Existente";
                    MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                    DialogResult result;
                    result = MessageBox.Show(this, message, caption, buttons);
                    if (result == DialogResult.Yes)
                    {
                        try
                        {
                            backDB.ActualizarParseCliente(nuevoParseCliente());
                            this.Close();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message.ToString());
                        }
                    }
                    else
                    {
                        txtIDCli.Focus();
                    }
                }
                else
                {
                    string message = "¿Desa Añadir?";
                    string caption = "Nuevo Cliente";
                    MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                    DialogResult result;
                    result = MessageBox.Show(this, message, caption, buttons);
                    if (result == DialogResult.Yes)
                    {
                        try
                        {
                            backDB.AltaParseCliente(nuevoParseCliente());//metodo de la clase BackDB para dar de alta un nuevo cliente y se cierra el Form
                            this.Close();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message.ToString());
                        }
                    }
                    else
                        txtIDCli.Focus();
                }
            }
            catch { }

        }

        private ParseCliente nuevoParseCliente()//crea un ParseCLiente nuevo para la Insert del BackDB desde los controloes del form y comprobando con los metodos para cada tipo de control
        {
            ParseCliente nuevo = new ParseCliente();

            nuevo.adr = checkClienteNuevo(txtADR.Text);
            nuevo.bultos = checkClienteNuevo(txtBultos.Text);
            nuevo.calculoRendezVouz = clienteEntradaCalculo(chboxCalculo);
            nuevo.certificados = checkClienteNuevo(txtCertificados.Text);
            nuevo.clase = checkClienteNuevo(txtClase.Text);
            nuevo.cmr = checkClienteNuevo(txtCmr.Text);
            nuevo.codAg = checkClienteNuevo(txtCodAg.Text);
            nuevo.codBultos = checkClienteNuevo(txtCodBulto.Text);
            nuevo.codDesRem = checkClienteNuevo(txtCodDesRem.Text);
            nuevo.codigoCliente = txtIDCli.Text;
            nuevo.condiciones = checkClienteNuevo(txtCondiciones.Text);
            nuevo.contenido = checkClienteNuevo(txtContenido.Text);
            nuevo.covia = checkClienteNuevo(txtCovia.Text);
            nuevo.cp = checkClienteNuevo(txtCP.Text);
            nuevo.delegacion = txtDelegacion.Text;
            nuevo.delimitador = checkClienteNuevo(txtDelimitador.Text);
            nuevo.destinatario = checkClienteNuevo(txtDestinatario.Text);
            nuevo.direccion = checkClienteNuevo(txtDireccion.Text);
            nuevo.entradaTransit = clienteEntradaCalculo(chboxEntrada);
            nuevo.expedicion = checkClienteNuevo(txtEpedicion.Text);
            nuevo.facturar = clienteFacturar(chboxFacturar);
            nuevo.fechaRendezVouz = checkClienteNuevo(txtFecha.Text);
            nuevo.impCod = checkClienteNuevo(txtImpCod.Text);
            nuevo.kilos = checkClienteNuevo(txtKg.Text);
            nuevo.kilosCovia = checkClienteNuevo(txtKgCovia.Text);
            nuevo.lineaInicio = checkClienteNuevo(txtLinicio.Text);
            nuevo.marcas = checkClienteNuevo(txtMarcas.Text);
            nuevo.metrosCubicos = checkClienteNuevo(txtM3.Text);
            nuevo.metrosCubicosCovia = checkClienteNuevo(txtM3Covia.Text);
            nuevo.nomCliente = checkClienteNuevo(cboxClientes.SelectedValue.ToString());
            nuevo.nomPais = nomPais();
            nuevo.observaciones = checkClienteNuevo(txtObserv.Text);
            nuevo.pais = txtCodPais.Text;
            nuevo.paisCod = checkClienteNuevo("d;" + txtCodPais.Text);
            nuevo.pares = checkClienteNuevo(txtPares.Text);
            nuevo.personal = txtPersonal.Text;
            nuevo.poblacion = checkClienteNuevo(txtPoblacion.Text);
            nuevo.refAg = checkClienteNuevo(txtRefAg.Text);
            nuevo.refCliente = checkClienteNuevo(txtRefCliente.Text);
            nuevo.tipoServicio = checkClienteNuevo(txtTipoServ.Text);
            nuevo.trafico = checkClienteNuevo(txtTrafico.Text);
            nuevo.valor = checkClienteNuevo(txtValor.Text);

            return nuevo;
        }
        private string nomPais() //devuelve el nombre del pais seleccionado en el combo en referencia a la table Paises
        {
            return Paises.Rows[cboxPaises.SelectedIndex].ItemArray[1].ToString();
        }

        private string checkClienteNuevo(string s)//filtra el texto para devolver null para la insert
        {
            if (s.Length != 0)
                return s;
            return null;

        }
        private string clienteFacturar(CheckBox checkBox)//comprueba el CheckBox y devuelve un si o no dependiendo del estado de la propiedad Checked.
        {
            if (checkBox.Checked)
                return "Si";
            return "No";

        }
        private int clienteEntradaCalculo(CheckBox checkBox)//Comprueba el estado del CheckBox y devuelve un 1 o 0
        {
            if (checkBox.Checked)
                return 1;
            return 0;
        }
        private void vaciarCajas()//metodo privado para vaciar los TextBox del form y poner en Unchecked los CheckBox
        {
            GetControls<TextBox>(this).ForEach(p => p.Text = "");
            chboxCalculo.Checked = false;
            chboxEntrada.Checked = false;
            chboxFacturar.Checked = false;
        }

        private void cboxPaises_SelectionChangeCommitted(object sender, EventArgs e)//evento que cambia el codigo de pais dependiendo de la seleccion en el combopaises
        {
            try
            {
                parseCliente = backDB.PlantillaCliente(cboxClientes.SelectedValue.ToString(), cboxPaises.SelectedValue.ToString()); // utiliza el metodo Plantilla cliente que nos devuelve los datos de un cliente concreto en un pais
                if (parseCliente.codigoCliente != null)//si el codigo es distinto de null llena los texboxes con la informacion del cliente y vacia los datos del datagridview y utiliza el metodo rellenarGridCol3 a partirde los datos de este cliente
                {
                    LlenarTextBoxes(parseCliente);
                    txtIDCli.Text = parseCliente.codigoCliente.ToString();
                    foreach (DataGridViewRow r in dataGridView1.Rows)
                    {
                        r.Cells[2].Value = "";
                    }
                    llenarGridCol3();
                }
            }
            catch { }
            txtCodPais.Text = cboxPaises.SelectedValue.ToString();//Escribe en la TextBox codPais un codigo que depende de la seleccion del Combobox paises determinado por la propiedad ValueMember del combo.
        }

        private void LlenarTextBoxes(ParseCliente cliente)//metodo para llenar todas las cajas de texto dependiento del cliente seleccionado en el combocliente
        {
            txtCovia.Text = comprobarString(cliente.covia);
            txtDestinatario.Text = comprobarString(cliente.destinatario);
            txtDireccion.Text = comprobarString(cliente.direccion);
            txtPoblacion.Text = comprobarString(cliente.poblacion);
            txtCP.Text = comprobarString(cliente.cp);
            txtBultos.Text = comprobarString(cliente.bultos);
            txtKg.Text = comprobarString(cliente.kilos);
            txtKgCovia.Text = comprobarString(cliente.kilosCovia);
            txtM3.Text = comprobarString(cliente.metrosCubicos);
            txtM3Covia.Text = comprobarString(cliente.metrosCubicosCovia);
            txtPares.Text = comprobarString(cliente.pares);
            txtCondiciones.Text = comprobarString(cliente.condiciones);
            txtValor.Text = comprobarString(cliente.valor);
            txtRefCliente.Text = comprobarString(cliente.refCliente);
            txtRefAg.Text = comprobarString(cliente.refAg);
            txtImpCod.Text = comprobarString(cliente.impCod);
            txtObserv.Text = comprobarString(cliente.observaciones);
            txtEpedicion.Text = comprobarString(cliente.expedicion);
            txtMarcas.Text = comprobarString(cliente.marcas);
            txtClase.Text = comprobarString(cliente.clase);
            txtContenido.Text = comprobarString(cliente.contenido);
            txtCertificados.Text = comprobarString(cliente.certificados);
            txtCodAg.Text = comprobarString(cliente.codAg);
            txtCodDesRem.Text = comprobarString(cliente.codDesRem);
            txtLinicio.Text = comprobarString(cliente.lineaInicio);
            txtCmr.Text = comprobarString(cliente.cmr);
            txtPersonal.Text = comprobarString(cliente.personal);
            txtDelegacion.Text = comprobarString(cliente.delegacion);
            txtCodBulto.Text = comprobarString(cliente.codBultos);
            txtTipoServ.Text = comprobarString(cliente.tipoServicio);
            txtDelimitador.Text = comprobarString(cliente.delimitador);
            txtTrafico.Text = comprobarString(cliente.trafico);
            txtADR.Text = comprobarString(cliente.adr);
            txtFecha.Text = comprobarString(cliente.fechaRendezVouz);
            chboxFacturar.Checked = CheckearString(cliente.facturar);
            chboxEntrada.Checked = CheckearInt(cliente.entradaTransit);
            chboxCalculo.Checked = CheckearInt(cliente.calculoRendezVouz);

        }

        private bool CheckearString(string s)//check/uncheck en el checkbox factura con param string
        {
            if (s.Contains("1") || s.Contains("Si"))
                return true;
            return false;
        }
        private bool CheckearInt(int n)//check/uncheck en el checkbox calculoRV como Entrada Transit con param int
        {
            if (n != 0)
                return true;
            return false;
        }

        private string comprobarString(string s)//comprueba si la palabra que recibe de la base de datos empieza por d; (dato a pelo) y lo quita para printarlo en la caja de texto
        {
            return s;
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)//al hacer click en una fila del grid printa el nº de fila (c;X) y hace focus en la caja de texto para tabular a la siguiente 
        {
            if (dataGridView1.Rows.Count != 0)// si el Grid tiene filas 
            {
                int col = dataGridView1.SelectedCells[0].RowIndex;//indice de la fila seleccionada 

                if (lastFocused != null || string.IsNullOrWhiteSpace(lastFocused.Text))
                {
                    lastFocused.Text = "c;" + col.ToString();
                    dataGridView1.Rows[col].Cells[2].Value = lastFocused.Name.Substring(3);//utilizamos el metodo SubString que nos devuelve el name del Txt apartir del 3er index, el cual nos quita la palabra txt del Name.
                    borrarCeldasIguales(dataGridView1.Rows[col]);
                    lastFocused.Focus();
                }
            }
        }


        private void txtPersonal_Leave(object sender, EventArgs e)//cambia la variable lastFocus para que al dejar la textbox actual se actualize 
        {
            lastFocused = (TextBox)sender;
        }

        private void borrarCeldasIguales(DataGridViewRow s)//metodo privado que al darle una fila del grid busca en las otras filas del mismo grid la palabra y la borra para seleccionarse en la fila que hacemos doble click
        {
            foreach (DataGridViewRow dr in dataGridView1.Rows)
            {
                if (dr != s)
                {
                    if (dr.Cells[2].Value != null && s.Cells[2].Value != null)
                    {
                        if (dr.Cells[2].Value.ToString() == s.Cells[2].Value.ToString())
                            dr.Cells[2].Value = "";
                    }
                }
            }
        }

        private void txtCodPais_TextChanged(object sender, EventArgs e)//si el color de la caja de texto codpais tiene mas de 2 letras cambiara a rojo para indicar que deben ser 2
        {
            if (txtCodPais.TextLength > 2)
                txtCodPais.ForeColor = Color.Red;
            else
                txtCodPais.ForeColor = Color.Black;
        }

        private void btnClienteNuevo_Click(object sender, EventArgs e)//Abre una ventana input para escribir el nombre del nuevo Cliente, limpia todas las cajas de texto y deja seleccionado el nuevo cliente
        {
            string cliente = Microsoft.VisualBasic.Interaction.InputBox("Nuevo Cliente", "Nombre Cliente", "Default", 400, 400);
            if (cliente.Length != 0)
            {
                cboxClientes.DataSource = null;
                DataRow nuevo = Clientes.NewRow();
                nuevo[0] = cliente;
                Clientes.Rows.Add(nuevo);
                cboxClientes.DataSource = Clientes;
                cboxClientes.DisplayMember = "nomcli";
                cboxClientes.ValueMember = "nomcli";
                cboxClientes.SelectedValue = cliente;
                vaciarCajas();
            }
        }

        private void frmPrincipal_Load(object sender, EventArgs e)//Al cargar el Form crea un style para un datagridviewCell , añade los tooltips y crea la funcion del evento
        {
            DataGridViewCell dgvc = new DataGridViewTextBoxCell();
            dgvc.Style.ForeColor = ColorTranslator.FromHtml("#1976D2");
            dgvc.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataCellCampo.CellTemplate = dgvc;
            dataCellCampo.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;

            ttipFile.SetToolTip(btnAbrir, "Abrir Fichero (F2)");
            ttipAceptar.SetToolTip(btnNuevo, "Aceptar/Actualizar");
            ttipConfig.SetToolTip(btnConfiguracion, "Config Conexion (F1)");
            txtDireccion.Focus();
        }

        private void llenarGridCol3()//Metodo privado que utiliza el metodo static GetControls para utilizar la funcion determinada en las TextBox como por ejemplo vaciar las cajas
        {

            GetControls<TextBox>(this).ForEach(p => llenarCelda(p.Text, p));//utiliza la funciono llenarCelda a partir de un string y una caja de texto

        }

        private void llenarCelda(string s, TextBox t)//metodo privado void que determina si el texto de la celta contiene una c; si la contiene separa mediante el metodo Split el numero de la Fila y escribe en el campo de esa fila el tipo de Dato que se almacena en esa Caja de Texto.
        {
            if (s.Contains("c;"))
            {
                string[] split = s.Split(';');
                int row = Int32.Parse(split[1]);
                if (row < dataGridView1.Rows.Count)
                    dataGridView1.Rows[row].Cells[2].Value = t.Name.Substring(3);
            }
        }
        public static List<T> GetControls<T>(Control container) where T : Control //metodo que devuelve una lista de controles (genericos) dentro de un Control determinado (Usando Generics)
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

        private void txtPoblacion_Enter(object sender, EventArgs e)//Evento que asigna el texto de la Textbox focuseada a la variable privada txtFocused.
        {

        }

        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)//Evento que nos permite mediante la tecla Suprimir borrar el texto de una celta determinada//normalmente se borrara la columna 2
        {

            if (e.KeyCode == Keys.Delete)
                dataGridView1.SelectedCells[0].Value = "";

        }

        private void FrmPrincipal_Activated(object sender, EventArgs e)//Evento que se carga al estar el Form en estado Activo y reinstancia la variable BackDB para actualizar los datos de configuracion
        {

            if (!BackDB.existeDatosConexion())//el metodo existedDatosConexion nos devuelve true si la configuracion esta vacía, y false si existe una configuracion
            {
                try
                {
                    if (c == 0)
                    {
                        backDB = new BackDB();
                        parseCliente = new ParseCliente();
                        Clientes = backDB.NombresClientes().Tables["Clientes"];
                        cboxClientes.DataSource = Clientes;
                        cboxClientes.DisplayMember = "nomcli";
                        cboxClientes.ValueMember = "nomcli";
                        countC = Clientes.Rows.Count;

                        //combo para Paises
                        Paises = backDB.NombresClientes().Tables["Paises"];
                        cboxPaises.DataSource = Paises;
                        cboxPaises.DisplayMember = "nompais";
                        cboxPaises.ValueMember = "pais";

                        c++; // si es la primera vez que se ha activado el form con datos de conexion, aumentamos en 1 el contador para que no vuelva a cargar todo otra vez 
                    }

                }
                catch (Exception ex)//si falla la conexion nos dara un mensaje con error de conexion para configurar otra vez la configuracion de conexion
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
            else//Si no existe una configuracion correta de conexion nos abrira un mensaje que al aceptar nos abrira automaticamente el form de Config
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

                else
                    this.Show();
            }
        }

        private void btnConfiguracion_Click(object sender, EventArgs e)//abre el form de configuracion al hacer click
        {
            frmConfiguracion frmConfiguracion = new frmConfiguracion();
            frmConfiguracion.ShowDialog();
        }

        private void FrmPrincipal_KeyDown_1(object sender, KeyEventArgs e)
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

        private void btnPaisNuevo_Click(object sender, EventArgs e)//Abre una ventana input para escribir el nombre del nuevo Pais, deja seleccionado el nuevo pais y añade al textboxCodPais una sugerencia
                                                                   //de codigo con las 2 primeras letras del pais nuevo y el signo ? y al ser mas de 2 letras cambia de color
        {
            string pais = Microsoft.VisualBasic.Interaction.InputBox("Nuevo País", "País", "Default", 400, 400);//utilizamos la libreria VisualBasic para crear un InputBox
            if (pais.Length > 0)
            {
                cboxPaises.DataSource = null; //vacia el combo de paises 
                DataRow nuevo = Paises.NewRow();//crea un datarow nuevo para la tabla paises con el nuevo Pais
                nuevo[1] = pais;
                nuevo[0] = pais.Substring(0, 2).ToUpper() + "?";// crea una nueva sugerencia de codigo de pais para el pais nuevo, aunq aqui lo recomendable seria utilizar los codigos ISO
                Paises.Rows.Add(nuevo);// añade a la tabla Paises el nuevo pais  y vuelve a enlazarse conel combo por la propiedad DataSource con el nuevo pais que hemos añadido
                cboxPaises.DataSource = Paises;
                cboxPaises.DisplayMember = "nompais";
                cboxPaises.ValueMember = "pais";
                cboxPaises.SelectedIndex = cboxPaises.Items.Count - 1;// hace focus en el pais nuevo.
                txtCodPais.Text = nuevo[0].ToString();
            }
        }



    }


}

