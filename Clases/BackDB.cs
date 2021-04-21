using LiteDB;
using MySql.Data.MySqlClient;
using ParseClientes.Clases;
using System;
using System.Data;

namespace ParseClientes
{
    class BackDB
    {
        String strConexion;
        static DatosConexion datosConexion;
        MySqlConnection mySqlConnection;
        private const string PATH = @"datosConexiones.db";

        public BackDB()
        {
            datosConexion = cargarDatosConexion();
            strConexion = datosConexion.stringConexion();
            mySqlConnection = new MySqlConnection(strConexion);
        }
        public static DatosConexion cargarDatosConexion() // Metodo statico que busca en la base de datos local un DatosConexion y el cual si no existe ninguno crea uno vacio y lo devuelve, si no, devolvera el que existe en ese momento.Normalmente usaremos solo 1 que iremos actualizando
        {
            using (var db = new LiteDatabase(PATH))
            {
                var datosConexion = db.GetCollection<DatosConexion>("datosConexion");
                var results = datosConexion.Query().Where(x => x.Id == 1).FirstOrDefault();
                if (results == null)
                {
                    results = new DatosConexion { host = "", port = "", stringPwd = "", Id = 1, dataBase = "", usuario = "" };
                    datosConexion.Insert(results);
                }
                return results;
            }
        }

        public static bool existeDatosConexion()//devuelve true si existe alguna DatosConexion con un usuario real, si no false;
        {
            using (var db = new LiteDatabase(PATH))
            {
                var config = db.GetCollection<DatosConexion>("datosConexion");
                var res = config.Query().Where(x => x.usuario == null).FirstOrDefault();//linq que determina si existe un usuario valido, es decir != null y devuelve true si no lo encuentra, y false si encuentra un usuario con el campo usuario=null 
                if (res == null)
                {
                    return false;
                }
                else return true;
            }
        }

        public bool checkConexion()
        {
            using (mySqlConnection)
            {
                try
                {
                    mySqlConnection.Open();
                    if (mySqlConnection.State == ConnectionState.Open) return true;
                }
                catch { }
            }
            return false;
        }
        public static void guardarDatosConexion(DatosConexion DatosConexion)//Actualiza una DatosConexion en la DB para los datos 
        {
            using (var db = new LiteDatabase(PATH))
            {
                var config = db.GetCollection<DatosConexion>("datosConexion");
                config.Update(DatosConexion);
            }
        }

        public DataSet NombresClientes()//Devuelve un dataset con 2 tablas (nombreClientes, Paises)
        {
            DataSet dataSetClientes = new DataSet();


            MySqlCommand cmdNomClientes = new MySqlCommand();//command para tabla Nombres
            cmdNomClientes.CommandType = CommandType.Text;
            cmdNomClientes.CommandText = "SELECT DISTINCT nomcli FROM parsecliente WHERE nomcli != ' ' ORDER BY nomcli ";
            cmdNomClientes.Connection = mySqlConnection;

            MySqlCommand cmdPaises = new MySqlCommand();//command para tabla Paises
            cmdPaises.CommandType = CommandType.Text;
            cmdPaises.CommandText = "SELECT DISTINCT pais , nompais FROM parsecliente WHERE nompais != ' ' ORDER BY nompais ";
            cmdPaises.Connection = mySqlConnection;

            using (mySqlConnection)
            {
                mySqlConnection.Open();
                MySqlDataAdapter adapterClientes = new MySqlDataAdapter(cmdNomClientes);
                adapterClientes.Fill(dataSetClientes, "Clientes");

                MySqlDataAdapter adapterPaises = new MySqlDataAdapter(cmdPaises);
                adapterPaises.Fill(dataSetClientes, "Paises");
            }
            return dataSetClientes;
        }
        public ParseCliente PlantillaCliente(string nombreCliente, string pais)//Devuelve los datos de un cliente independientemente del pais para crear una plantilla en el form
        {
            ParseCliente parseCliente = new ParseCliente();

            MySqlCommand cmdCliente = new MySqlCommand();//cmd devuelve datos de un nomcliente
            cmdCliente.CommandType = CommandType.Text;
            cmdCliente.CommandText = "SELECT * FROM parsecliente p WHERE nomcli = @p_nomcli AND pais= @p_pais";
            cmdCliente.Connection = mySqlConnection;

            MySqlParameter p_nomcli = new MySqlParameter();
            p_nomcli.Direction = ParameterDirection.Input;
            p_nomcli.ParameterName = "@p_nomcli";
            p_nomcli.MySqlDbType = MySqlDbType.VarChar;
            p_nomcli.Size = 25;

            MySqlParameter p_pais = new MySqlParameter();
            p_pais.Direction = ParameterDirection.Input;
            p_pais.ParameterName = "@p_pais";
            p_pais.MySqlDbType = MySqlDbType.VarChar;
            p_pais.Size = 2;

            cmdCliente.Parameters.Add(p_nomcli);
            cmdCliente.Parameters.Add(p_pais);
            cmdCliente.Parameters[0].Value = nombreCliente;
            cmdCliente.Parameters[1].Value = pais;
            using (mySqlConnection)
            {
                mySqlConnection.Open();
                MySqlDataReader lectorCliente = cmdCliente.ExecuteReader();

                while (lectorCliente.Read()) // leemos el lector el cual nos devuelve una UNICA fila en el que cada dato corresponde con cada variable de la clase Cliente
                {
                    parseCliente.codigoCliente = comprobarLector(lectorCliente.GetValue(0));
                    parseCliente.pais = comprobarLector(lectorCliente.GetValue(1));
                    parseCliente.lineaInicio = comprobarLector(lectorCliente.GetValue(2));
                    parseCliente.covia = comprobarLector(lectorCliente.GetValue(3));
                    parseCliente.codDesRem = comprobarLector(lectorCliente.GetValue(4));
                    parseCliente.destinatario = comprobarLector(lectorCliente.GetValue(5));
                    parseCliente.direccion = comprobarLector(lectorCliente.GetValue(6));
                    parseCliente.poblacion = comprobarLector(lectorCliente.GetValue(7));
                    parseCliente.cp = comprobarLector(lectorCliente.GetValue(8));
                    parseCliente.bultos = comprobarLector(lectorCliente.GetValue(9));
                    parseCliente.kilos = comprobarLector(lectorCliente.GetValue(10));
                    parseCliente.kilosCovia = comprobarLector(lectorCliente.GetValue(11));
                    parseCliente.metrosCubicos = comprobarLector(lectorCliente.GetValue(12));
                    parseCliente.metrosCubicosCovia = comprobarLector(lectorCliente.GetValue(13));
                    parseCliente.pares = comprobarLector(lectorCliente.GetValue(14));
                    parseCliente.condiciones = comprobarLector(lectorCliente.GetValue(15));
                    parseCliente.valor = comprobarLector(lectorCliente.GetValue(16));
                    parseCliente.refCliente = comprobarLector(lectorCliente.GetValue(17));
                    parseCliente.refAg = comprobarLector(lectorCliente.GetValue(18));
                    parseCliente.impCod = comprobarLector(lectorCliente.GetValue(19));
                    parseCliente.paisCod = comprobarLector(lectorCliente.GetValue(20));
                    parseCliente.observaciones = comprobarLector(lectorCliente.GetValue(21));
                    parseCliente.expedicion = comprobarLector(lectorCliente.GetValue(22));
                    parseCliente.marcas = comprobarLector(lectorCliente.GetValue(23));
                    parseCliente.clase = comprobarLector(lectorCliente.GetValue(24));
                    parseCliente.contenido = comprobarLector(lectorCliente.GetValue(25));
                    parseCliente.certificados = comprobarLector(lectorCliente.GetValue(26));
                    parseCliente.codAg = comprobarLector(lectorCliente.GetValue(27));
                    parseCliente.cmr = comprobarLector(lectorCliente.GetValue(28));
                    parseCliente.personal = comprobarLector(lectorCliente.GetValue(29));
                    parseCliente.nomCliente = comprobarLector(lectorCliente.GetValue(30));
                    parseCliente.facturar = comprobarLector(lectorCliente.GetValue(31));
                    parseCliente.delegacion = comprobarLector(lectorCliente.GetValue(32));
                    parseCliente.nomPais = comprobarLector(lectorCliente.GetValue(33));
                    parseCliente.codBultos = comprobarLector(lectorCliente.GetValue(34));
                    parseCliente.tipoServicio = comprobarLector(lectorCliente.GetValue(35));
                    parseCliente.delimitador = comprobarLector(lectorCliente.GetValue(36));
                    parseCliente.trafico = comprobarLector(lectorCliente.GetValue(37));
                    parseCliente.entradaTransit = lectorCliente.GetInt32(38);
                    parseCliente.calculoRendezVouz = lectorCliente.GetInt32(39);
                    parseCliente.adr = comprobarLector(lectorCliente.GetValue(40));
                    parseCliente.fechaRendezVouz = comprobarLector(lectorCliente.GetValue(41));

                }
            }
            return parseCliente;
        }
        private string comprobarLector(object s)//comprueba los valores null para devolver ""
        {
            if (s != null)
                return s.ToString();
            return "";
        }
        public void AltaParseCliente(ParseCliente parseCliente)//insert en la BD usando como parametro un ParseCliente que nos envia el Form
        {
            MySqlCommand cmdAlta = new MySqlCommand();
            cmdAlta.CommandType = CommandType.Text;
            cmdAlta.CommandText = "INSERT INTO parsecliente (codcli,pais,lineaInicio,covia,cod_desrem,Destinatario,Direccion,Poblacion,cp,bultos,kilos,kilos_covia,m3,m3_covia,pares,condiciones,valor,refcli,refag,impcod,paisCod,observaciones,expedicion,marcas,clase,contenido,certificados,codag,cmr,personal,nomcli,facturar,delegacion,nompais,codBultos,tipoServicio,delimitador,trafico,entrada_transit_time,calculo_rendezvous,adr,fecha_rendezvous)" +
                                  " VALUES(@p_codCli, @p_pais, @p_linealInicio, @p_covia, @p_codDesRem, @p_destinatario, @p_direccion, @p_poblacion, @p_cp, @p_bultos, @p_kilos, @p_kilosCovia, @p_m3, @p_m3Covia, @p_pares, @p_condiciones, @p_valor, @p_refCli, @p_refAg, @p_impCod, @p_paisCod, @p_observaciones, @p_expedicion, @p_marcas, @p_clase, @p_contenido, @p_certificados, @p_codAg, @p_cmr, @p_personal, @p_nomCli, @p_facturar, @p_delegacion, @p_nomPais, @p_codBultos, @p_tipoServicio, @p_delimitador, @p_trafico, @p_entradaTransit, @p_calculoRV, @p_adr, @p_fechaRV) ";
            cmdAlta.Connection = mySqlConnection;

            MySqlParameter p_codCli = new MySqlParameter();
            p_codCli.MySqlDbType = MySqlDbType.VarChar;
            p_codCli.Size = 6;
            p_codCli.ParameterName = "@p_codCli";
            p_codCli.Direction = ParameterDirection.Input;

            MySqlParameter p_pais = new MySqlParameter();
            p_pais.MySqlDbType = MySqlDbType.VarChar;
            p_pais.Size = 2;
            p_pais.ParameterName = "@p_pais";
            p_pais.Direction = ParameterDirection.Input;

            MySqlParameter p_linealInicio = new MySqlParameter();
            p_linealInicio.MySqlDbType = MySqlDbType.VarChar;
            p_linealInicio.Size = 6;
            p_linealInicio.ParameterName = "@p_linealInicio";
            p_linealInicio.Direction = ParameterDirection.Input;

            MySqlParameter p_covia = new MySqlParameter();
            p_covia.MySqlDbType = MySqlDbType.VarChar;
            p_covia.Size = 25;
            p_covia.ParameterName = "@p_covia";
            p_covia.Direction = ParameterDirection.Input;

            MySqlParameter p_codDesRem = new MySqlParameter();
            p_codDesRem.MySqlDbType = MySqlDbType.VarChar;
            p_codDesRem.Size = 6;
            p_codDesRem.ParameterName = "@p_codDesRem";
            p_codDesRem.Direction = ParameterDirection.Input;

            MySqlParameter p_destinatario = new MySqlParameter();
            p_destinatario.MySqlDbType = MySqlDbType.VarChar;
            p_destinatario.Size = 25;
            p_destinatario.ParameterName = "@p_destinatario";
            p_destinatario.Direction = ParameterDirection.Input;

            MySqlParameter p_direccion = new MySqlParameter();
            p_direccion.MySqlDbType = MySqlDbType.VarChar;
            p_direccion.Size = 25;
            p_direccion.ParameterName = "@p_direccion";
            p_direccion.Direction = ParameterDirection.Input;

            MySqlParameter p_poblacion = new MySqlParameter();
            p_poblacion.MySqlDbType = MySqlDbType.VarChar;
            p_poblacion.Size = 25;
            p_poblacion.ParameterName = "@p_poblacion";
            p_poblacion.Direction = ParameterDirection.Input;

            MySqlParameter p_cp = new MySqlParameter();
            p_cp.MySqlDbType = MySqlDbType.VarChar;
            p_cp.Size = 25;
            p_cp.ParameterName = "@p_cp";
            p_cp.Direction = ParameterDirection.Input;

            MySqlParameter p_bultos = new MySqlParameter();
            p_bultos.MySqlDbType = MySqlDbType.VarChar;
            p_bultos.Size = 25;
            p_bultos.ParameterName = "@p_bultos";
            p_bultos.Direction = ParameterDirection.Input;

            MySqlParameter p_kilos = new MySqlParameter();
            p_kilos.MySqlDbType = MySqlDbType.VarChar;
            p_kilos.Size = 25;
            p_kilos.ParameterName = "@p_kilos";
            p_kilos.Direction = ParameterDirection.Input;

            MySqlParameter p_kilosCovia = new MySqlParameter();
            p_kilosCovia.MySqlDbType = MySqlDbType.VarChar;
            p_kilosCovia.Size = 25;
            p_kilosCovia.ParameterName = "@p_kilosCovia";
            p_kilosCovia.Direction = ParameterDirection.Input;

            MySqlParameter p_m3 = new MySqlParameter();
            p_m3.MySqlDbType = MySqlDbType.VarChar;
            p_m3.Size = 25;
            p_m3.ParameterName = "@p_m3";
            p_m3.Direction = ParameterDirection.Input;

            MySqlParameter p_m3Covia = new MySqlParameter();
            p_m3Covia.MySqlDbType = MySqlDbType.VarChar;
            p_m3Covia.Size = 25;
            p_m3Covia.ParameterName = "@p_m3Covia";
            p_m3Covia.Direction = ParameterDirection.Input;

            MySqlParameter p_pares = new MySqlParameter();
            p_pares.MySqlDbType = MySqlDbType.VarChar;
            p_pares.Size = 25;
            p_pares.ParameterName = "@p_pares";
            p_pares.Direction = ParameterDirection.Input;

            MySqlParameter p_condiciones = new MySqlParameter();
            p_condiciones.MySqlDbType = MySqlDbType.VarChar;
            p_condiciones.Size = 25;
            p_condiciones.ParameterName = "@p_condiciones";
            p_condiciones.Direction = ParameterDirection.Input;

            MySqlParameter p_valor = new MySqlParameter();
            p_valor.MySqlDbType = MySqlDbType.VarChar;
            p_valor.Size = 25;
            p_valor.ParameterName = "@p_valor";
            p_valor.Direction = ParameterDirection.Input;

            MySqlParameter p_refCli = new MySqlParameter();
            p_refCli.MySqlDbType = MySqlDbType.VarChar;
            p_refCli.Size = 25;
            p_refCli.ParameterName = "@p_refCli";
            p_refCli.Direction = ParameterDirection.Input;

            MySqlParameter p_refAg = new MySqlParameter();
            p_refAg.MySqlDbType = MySqlDbType.VarChar;
            p_refAg.Size = 25;
            p_refAg.ParameterName = "@p_refAg";
            p_refAg.Direction = ParameterDirection.Input;

            MySqlParameter p_impCod = new MySqlParameter();
            p_impCod.MySqlDbType = MySqlDbType.VarChar;
            p_impCod.Size = 25;
            p_impCod.ParameterName = "@p_impCod";
            p_impCod.Direction = ParameterDirection.Input;

            MySqlParameter p_paisCod = new MySqlParameter();
            p_paisCod.MySqlDbType = MySqlDbType.VarChar;
            p_paisCod.Size = 25;
            p_paisCod.ParameterName = "@p_paisCod";
            p_paisCod.Direction = ParameterDirection.Input;

            MySqlParameter p_observaciones = new MySqlParameter();
            p_observaciones.MySqlDbType = MySqlDbType.VarChar;
            p_observaciones.Size = 100;
            p_observaciones.ParameterName = "@p_observaciones";
            p_observaciones.Direction = ParameterDirection.Input;

            MySqlParameter p_expedicion = new MySqlParameter();
            p_expedicion.MySqlDbType = MySqlDbType.VarChar;
            p_expedicion.Size = 25;
            p_expedicion.ParameterName = "@p_expedicion";
            p_expedicion.Direction = ParameterDirection.Input;

            MySqlParameter p_marcas = new MySqlParameter();
            p_marcas.MySqlDbType = MySqlDbType.VarChar;
            p_marcas.Size = 25;
            p_marcas.ParameterName = "@p_marcas";
            p_marcas.Direction = ParameterDirection.Input;

            MySqlParameter p_clase = new MySqlParameter();
            p_clase.MySqlDbType = MySqlDbType.VarChar;
            p_clase.Size = 25;
            p_clase.ParameterName = "@p_clase";
            p_clase.Direction = ParameterDirection.Input;

            MySqlParameter p_contenido = new MySqlParameter();
            p_contenido.MySqlDbType = MySqlDbType.VarChar;
            p_contenido.Size = 25;
            p_contenido.ParameterName = "@p_contenido";
            p_contenido.Direction = ParameterDirection.Input;

            MySqlParameter p_certificados = new MySqlParameter();
            p_certificados.MySqlDbType = MySqlDbType.VarChar;
            p_certificados.Size = 25;
            p_certificados.ParameterName = "@p_certificados";
            p_certificados.Direction = ParameterDirection.Input;

            MySqlParameter p_codAg = new MySqlParameter();
            p_codAg.MySqlDbType = MySqlDbType.VarChar;
            p_codAg.Size = 25;
            p_codAg.ParameterName = "@p_codAg";
            p_codAg.Direction = ParameterDirection.Input;

            MySqlParameter p_cmr = new MySqlParameter();
            p_cmr.MySqlDbType = MySqlDbType.VarChar;
            p_cmr.Size = 25;
            p_cmr.ParameterName = "@p_cmr";
            p_cmr.Direction = ParameterDirection.Input;

            MySqlParameter p_personal = new MySqlParameter();
            p_personal.MySqlDbType = MySqlDbType.VarChar;
            p_personal.Size = 25;
            p_personal.ParameterName = "@p_personal";
            p_personal.Direction = ParameterDirection.Input;

            MySqlParameter p_nomCli = new MySqlParameter();
            p_nomCli.MySqlDbType = MySqlDbType.VarChar;
            p_nomCli.Size = 25;
            p_nomCli.ParameterName = "@p_nomCli";
            p_nomCli.Direction = ParameterDirection.Input;

            MySqlParameter p_facturar = new MySqlParameter();
            p_facturar.MySqlDbType = MySqlDbType.VarChar;
            p_facturar.Size = 2;
            p_facturar.ParameterName = "@p_facturar";
            p_facturar.Direction = ParameterDirection.Input;

            MySqlParameter p_delegacion = new MySqlParameter();
            p_delegacion.MySqlDbType = MySqlDbType.VarChar;
            p_delegacion.Size = 4;
            p_delegacion.ParameterName = "@p_delegacion";
            p_delegacion.Direction = ParameterDirection.Input;

            MySqlParameter p_nomPais = new MySqlParameter();
            p_nomPais.MySqlDbType = MySqlDbType.VarChar;
            p_nomPais.Size = 25;
            p_nomPais.ParameterName = "@p_nomPais";
            p_codCli.Direction = ParameterDirection.Input;

            MySqlParameter p_codBultos = new MySqlParameter();
            p_codBultos.MySqlDbType = MySqlDbType.VarChar;
            p_codBultos.Size = 25;
            p_codBultos.ParameterName = "@p_codBultos";
            p_codBultos.Direction = ParameterDirection.Input;

            MySqlParameter p_tipoServicio = new MySqlParameter();
            p_tipoServicio.MySqlDbType = MySqlDbType.VarChar;
            p_tipoServicio.Size = 25;
            p_tipoServicio.ParameterName = "@p_tipoServicio";
            p_tipoServicio.Direction = ParameterDirection.Input;

            MySqlParameter p_delimitador = new MySqlParameter();
            p_delimitador.MySqlDbType = MySqlDbType.VarChar;
            p_delimitador.Size = 3;
            p_delimitador.ParameterName = "@p_delimitador";
            p_delimitador.Direction = ParameterDirection.Input;

            MySqlParameter p_trafico = new MySqlParameter();
            p_trafico.MySqlDbType = MySqlDbType.VarChar;
            p_trafico.Size = 1;
            p_trafico.ParameterName = "@p_trafico";
            p_trafico.Direction = ParameterDirection.Input;

            MySqlParameter p_entradaTransit = new MySqlParameter();
            p_entradaTransit.MySqlDbType = MySqlDbType.Int32;
            p_entradaTransit.ParameterName = "@p_entradaTransit";
            p_entradaTransit.Direction = ParameterDirection.Input;

            MySqlParameter p_calculoRV = new MySqlParameter();
            p_calculoRV.MySqlDbType = MySqlDbType.Int32;
            p_calculoRV.ParameterName = "@p_calculoRV";
            p_calculoRV.Direction = ParameterDirection.Input;

            MySqlParameter p_adr = new MySqlParameter();
            p_adr.MySqlDbType = MySqlDbType.VarChar;
            p_adr.Size = 25;
            p_adr.ParameterName = "@p_adr";
            p_adr.Direction = ParameterDirection.Input;

            MySqlParameter p_fechaRV = new MySqlParameter();
            p_fechaRV.MySqlDbType = MySqlDbType.VarChar;
            p_fechaRV.Size = 25;
            p_fechaRV.ParameterName = "@p_fechaRV";
            p_fechaRV.Direction = ParameterDirection.Input;

            cmdAlta.Parameters.Add(p_codCli);
            cmdAlta.Parameters.Add(p_pais);
            cmdAlta.Parameters.Add(p_linealInicio);
            cmdAlta.Parameters.Add(p_covia);
            cmdAlta.Parameters.Add(p_codDesRem);
            cmdAlta.Parameters.Add(p_destinatario);
            cmdAlta.Parameters.Add(p_direccion);
            cmdAlta.Parameters.Add(p_poblacion);
            cmdAlta.Parameters.Add(p_cp);
            cmdAlta.Parameters.Add(p_bultos);
            cmdAlta.Parameters.Add(p_kilos);
            cmdAlta.Parameters.Add(p_kilosCovia);
            cmdAlta.Parameters.Add(p_m3);
            cmdAlta.Parameters.Add(p_m3Covia);
            cmdAlta.Parameters.Add(p_pares);
            cmdAlta.Parameters.Add(p_condiciones);
            cmdAlta.Parameters.Add(p_valor);
            cmdAlta.Parameters.Add(p_refCli);
            cmdAlta.Parameters.Add(p_refAg);
            cmdAlta.Parameters.Add(p_impCod);
            cmdAlta.Parameters.Add(p_paisCod);
            cmdAlta.Parameters.Add(p_observaciones);
            cmdAlta.Parameters.Add(p_expedicion);
            cmdAlta.Parameters.Add(p_marcas);
            cmdAlta.Parameters.Add(p_clase);
            cmdAlta.Parameters.Add(p_contenido);
            cmdAlta.Parameters.Add(p_certificados);
            cmdAlta.Parameters.Add(p_codAg);
            cmdAlta.Parameters.Add(p_cmr);
            cmdAlta.Parameters.Add(p_personal);
            cmdAlta.Parameters.Add(p_nomCli);
            cmdAlta.Parameters.Add(p_facturar);
            cmdAlta.Parameters.Add(p_delegacion);
            cmdAlta.Parameters.Add(p_nomPais);
            cmdAlta.Parameters.Add(p_codBultos);
            cmdAlta.Parameters.Add(p_tipoServicio);
            cmdAlta.Parameters.Add(p_delimitador);
            cmdAlta.Parameters.Add(p_trafico);
            cmdAlta.Parameters.Add(p_entradaTransit);
            cmdAlta.Parameters.Add(p_calculoRV);
            cmdAlta.Parameters.Add(p_adr);
            cmdAlta.Parameters.Add(p_fechaRV);


            cmdAlta.Parameters[0].Value = parseCliente.codigoCliente;
            cmdAlta.Parameters[1].Value = parseCliente.pais;
            cmdAlta.Parameters[2].Value = parseCliente.lineaInicio;
            cmdAlta.Parameters[3].Value = parseCliente.covia;
            cmdAlta.Parameters[4].Value = parseCliente.codDesRem;
            cmdAlta.Parameters[5].Value = parseCliente.destinatario;
            cmdAlta.Parameters[6].Value = parseCliente.direccion;
            cmdAlta.Parameters[7].Value = parseCliente.poblacion;
            cmdAlta.Parameters[8].Value = parseCliente.cp;
            cmdAlta.Parameters[9].Value = parseCliente.bultos;
            cmdAlta.Parameters[10].Value = parseCliente.kilos;
            cmdAlta.Parameters[11].Value = parseCliente.kilosCovia;
            cmdAlta.Parameters[12].Value = parseCliente.metrosCubicos;
            cmdAlta.Parameters[13].Value = parseCliente.metrosCubicosCovia;
            cmdAlta.Parameters[14].Value = parseCliente.pares;
            cmdAlta.Parameters[15].Value = parseCliente.condiciones;
            cmdAlta.Parameters[16].Value = parseCliente.valor;
            cmdAlta.Parameters[17].Value = parseCliente.refCliente;
            cmdAlta.Parameters[18].Value = parseCliente.refAg;
            cmdAlta.Parameters[19].Value = parseCliente.impCod;
            cmdAlta.Parameters[20].Value = parseCliente.paisCod;
            cmdAlta.Parameters[21].Value = parseCliente.observaciones;
            cmdAlta.Parameters[22].Value = parseCliente.expedicion;
            cmdAlta.Parameters[23].Value = parseCliente.marcas;
            cmdAlta.Parameters[24].Value = parseCliente.clase;
            cmdAlta.Parameters[25].Value = parseCliente.contenido;
            cmdAlta.Parameters[26].Value = parseCliente.certificados;
            cmdAlta.Parameters[27].Value = parseCliente.codAg;
            cmdAlta.Parameters[28].Value = parseCliente.cmr;
            cmdAlta.Parameters[29].Value = parseCliente.personal;
            cmdAlta.Parameters[30].Value = parseCliente.nomCliente;
            cmdAlta.Parameters[31].Value = parseCliente.facturar;
            cmdAlta.Parameters[32].Value = parseCliente.delegacion;
            cmdAlta.Parameters[33].Value = parseCliente.nomPais;
            cmdAlta.Parameters[34].Value = parseCliente.codBultos;
            cmdAlta.Parameters[35].Value = parseCliente.tipoServicio;
            cmdAlta.Parameters[36].Value = parseCliente.delimitador;
            cmdAlta.Parameters[37].Value = parseCliente.trafico;
            cmdAlta.Parameters[38].Value = parseCliente.entradaTransit;
            cmdAlta.Parameters[39].Value = parseCliente.calculoRendezVouz;
            cmdAlta.Parameters[40].Value = parseCliente.adr;
            cmdAlta.Parameters[41].Value = parseCliente.fechaRendezVouz;
            using (mySqlConnection)
            {
                try
                {
                    mySqlConnection.Open();
                    cmdAlta.ExecuteNonQuery();
                }
                catch { };
            }
        }
        public void ActualizarParseCliente(ParseCliente parseCliente)//actualizar un Cliente con un codCli 
        {
            MySqlCommand cmdActualizar = new MySqlCommand();
            cmdActualizar.CommandType = CommandType.Text;
            cmdActualizar.CommandText = "UPDATE parsecliente SET lineaInicio = @p_linealInicio,covia = @p_covia, cod_desrem = @p_codDesRem, Destinatario = @p_destinatario, Direccion = @p_direccion, Poblacion = @p_poblacion, Cp = @p_cp, bultos = @p_bultos, kilos = @p_kilos, kilos_covia = @p_kilosCovia, m3 = @p_m3, m3_covia = @p_m3Covia, pares = @p_pares, condiciones = @p_condiciones, valor = @p_valor, refcli = @p_refCli, refag = @p_refAg, impcod = @p_impCod, paisCod = @p_paisCod, observaciones = @p_observaciones, expedicion = @p_expedicion, marcas = @p_marcas, clase = @p_clase, contenido = @p_contenido, certificados = @p_certificados, codag = @p_codAg, cmr = @p_cmr, personal = @p_personal, facturar = @p_facturar, nompais = @p_nomPais, codBultos = @p_codBultos, tipoServicio = @p_tipoServicio, delimitador = @p_delimitador, trafico = @p_trafico, entrada_transit_time = @p_entradaTransit, calculo_rendezvous = @p_calculoRV, adr = @p_adr, fecha_rendezvous = @p_fechaRV WHERE codcli = @p_codCli AND pais = @p_pais AND nomcli = @p_nomCli AND delegacion = @p_delegacion ";
            cmdActualizar.Connection = mySqlConnection;

            MySqlParameter p_linealInicio = new MySqlParameter();
            p_linealInicio.MySqlDbType = MySqlDbType.VarChar;
            p_linealInicio.Size = 6;
            p_linealInicio.ParameterName = "@p_linealInicio";
            p_linealInicio.Direction = ParameterDirection.Input;

            MySqlParameter p_covia = new MySqlParameter();
            p_covia.MySqlDbType = MySqlDbType.VarChar;
            p_covia.Size = 25;
            p_covia.ParameterName = "@p_covia";
            p_covia.Direction = ParameterDirection.Input;

            MySqlParameter p_codDesRem = new MySqlParameter();
            p_codDesRem.MySqlDbType = MySqlDbType.VarChar;
            p_codDesRem.Size = 6;
            p_codDesRem.ParameterName = "@p_codDesRem";
            p_codDesRem.Direction = ParameterDirection.Input;

            MySqlParameter p_destinatario = new MySqlParameter();
            p_destinatario.MySqlDbType = MySqlDbType.VarChar;
            p_destinatario.Size = 25;
            p_destinatario.ParameterName = "@p_destinatario";
            p_destinatario.Direction = ParameterDirection.Input;

            MySqlParameter p_direccion = new MySqlParameter();
            p_direccion.MySqlDbType = MySqlDbType.VarChar;
            p_direccion.Size = 25;
            p_direccion.ParameterName = "@p_direccion";
            p_direccion.Direction = ParameterDirection.Input;

            MySqlParameter p_poblacion = new MySqlParameter();
            p_poblacion.MySqlDbType = MySqlDbType.VarChar;
            p_poblacion.Size = 25;
            p_poblacion.ParameterName = "@p_poblacion";
            p_poblacion.Direction = ParameterDirection.Input;

            MySqlParameter p_cp = new MySqlParameter();
            p_cp.MySqlDbType = MySqlDbType.VarChar;
            p_cp.Size = 25;
            p_cp.ParameterName = "@p_cp";
            p_cp.Direction = ParameterDirection.Input;

            MySqlParameter p_bultos = new MySqlParameter();
            p_bultos.MySqlDbType = MySqlDbType.VarChar;
            p_bultos.Size = 25;
            p_bultos.ParameterName = "@p_bultos";
            p_bultos.Direction = ParameterDirection.Input;

            MySqlParameter p_kilos = new MySqlParameter();
            p_kilos.MySqlDbType = MySqlDbType.VarChar;
            p_kilos.Size = 25;
            p_kilos.ParameterName = "@p_kilos";
            p_kilos.Direction = ParameterDirection.Input;

            MySqlParameter p_kilosCovia = new MySqlParameter();
            p_kilosCovia.MySqlDbType = MySqlDbType.VarChar;
            p_kilosCovia.Size = 25;
            p_kilosCovia.ParameterName = "@p_kilosCovia";
            p_kilosCovia.Direction = ParameterDirection.Input;

            MySqlParameter p_m3 = new MySqlParameter();
            p_m3.MySqlDbType = MySqlDbType.VarChar;
            p_m3.Size = 25;
            p_m3.ParameterName = "@p_m3";
            p_m3.Direction = ParameterDirection.Input;

            MySqlParameter p_m3Covia = new MySqlParameter();
            p_m3Covia.MySqlDbType = MySqlDbType.VarChar;
            p_m3Covia.Size = 25;
            p_m3Covia.ParameterName = "@p_m3Covia";
            p_m3Covia.Direction = ParameterDirection.Input;

            MySqlParameter p_pares = new MySqlParameter();
            p_pares.MySqlDbType = MySqlDbType.VarChar;
            p_pares.Size = 25;
            p_pares.ParameterName = "@p_pares";
            p_pares.Direction = ParameterDirection.Input;

            MySqlParameter p_condiciones = new MySqlParameter();
            p_condiciones.MySqlDbType = MySqlDbType.VarChar;
            p_condiciones.Size = 25;
            p_condiciones.ParameterName = "@p_condiciones";
            p_condiciones.Direction = ParameterDirection.Input;

            MySqlParameter p_valor = new MySqlParameter();
            p_valor.MySqlDbType = MySqlDbType.VarChar;
            p_valor.Size = 25;
            p_valor.ParameterName = "@p_valor";
            p_valor.Direction = ParameterDirection.Input;

            MySqlParameter p_refCli = new MySqlParameter();
            p_refCli.MySqlDbType = MySqlDbType.VarChar;
            p_refCli.Size = 25;
            p_refCli.ParameterName = "@p_refCli";
            p_refCli.Direction = ParameterDirection.Input;

            MySqlParameter p_refAg = new MySqlParameter();
            p_refAg.MySqlDbType = MySqlDbType.VarChar;
            p_refAg.Size = 25;
            p_refAg.ParameterName = "@p_refAg";
            p_refAg.Direction = ParameterDirection.Input;

            MySqlParameter p_impCod = new MySqlParameter();
            p_impCod.MySqlDbType = MySqlDbType.VarChar;
            p_impCod.Size = 25;
            p_impCod.ParameterName = "@p_impCod";
            p_impCod.Direction = ParameterDirection.Input;

            MySqlParameter p_paisCod = new MySqlParameter();
            p_paisCod.MySqlDbType = MySqlDbType.VarChar;
            p_paisCod.Size = 25;
            p_paisCod.ParameterName = "@p_paisCod";
            p_paisCod.Direction = ParameterDirection.Input;

            MySqlParameter p_observaciones = new MySqlParameter();
            p_observaciones.MySqlDbType = MySqlDbType.VarChar;
            p_observaciones.Size = 100;
            p_observaciones.ParameterName = "@p_observaciones";
            p_observaciones.Direction = ParameterDirection.Input;

            MySqlParameter p_expedicion = new MySqlParameter();
            p_expedicion.MySqlDbType = MySqlDbType.VarChar;
            p_expedicion.Size = 25;
            p_expedicion.ParameterName = "@p_expedicion";
            p_expedicion.Direction = ParameterDirection.Input;

            MySqlParameter p_marcas = new MySqlParameter();
            p_marcas.MySqlDbType = MySqlDbType.VarChar;
            p_marcas.Size = 25;
            p_marcas.ParameterName = "@p_marcas";
            p_marcas.Direction = ParameterDirection.Input;

            MySqlParameter p_clase = new MySqlParameter();
            p_clase.MySqlDbType = MySqlDbType.VarChar;
            p_clase.Size = 25;
            p_clase.ParameterName = "@p_clase";
            p_clase.Direction = ParameterDirection.Input;

            MySqlParameter p_contenido = new MySqlParameter();
            p_contenido.MySqlDbType = MySqlDbType.VarChar;
            p_contenido.Size = 25;
            p_contenido.ParameterName = "@p_contenido";
            p_contenido.Direction = ParameterDirection.Input;

            MySqlParameter p_certificados = new MySqlParameter();
            p_certificados.MySqlDbType = MySqlDbType.VarChar;
            p_certificados.Size = 25;
            p_certificados.ParameterName = "@p_certificados";
            p_certificados.Direction = ParameterDirection.Input;

            MySqlParameter p_codAg = new MySqlParameter();
            p_codAg.MySqlDbType = MySqlDbType.VarChar;
            p_codAg.Size = 25;
            p_codAg.ParameterName = "@p_codAg";
            p_codAg.Direction = ParameterDirection.Input;

            MySqlParameter p_cmr = new MySqlParameter();
            p_cmr.MySqlDbType = MySqlDbType.VarChar;
            p_cmr.Size = 25;
            p_cmr.ParameterName = "@p_cmr";
            p_cmr.Direction = ParameterDirection.Input;

            MySqlParameter p_personal = new MySqlParameter();
            p_personal.MySqlDbType = MySqlDbType.VarChar;
            p_personal.Size = 25;
            p_personal.ParameterName = "@p_personal";
            p_personal.Direction = ParameterDirection.Input;

            MySqlParameter p_facturar = new MySqlParameter();
            p_facturar.MySqlDbType = MySqlDbType.VarChar;
            p_facturar.Size = 2;
            p_facturar.ParameterName = "@p_facturar";
            p_facturar.Direction = ParameterDirection.Input;

            MySqlParameter p_nomPais = new MySqlParameter();
            p_nomPais.MySqlDbType = MySqlDbType.VarChar;
            p_nomPais.Size = 25;
            p_nomPais.ParameterName = "@p_nomPais";
            p_nomPais.Direction = ParameterDirection.Input;

            MySqlParameter p_codBultos = new MySqlParameter();
            p_codBultos.MySqlDbType = MySqlDbType.VarChar;
            p_codBultos.Size = 25;
            p_codBultos.ParameterName = "@p_codBultos";
            p_codBultos.Direction = ParameterDirection.Input;

            MySqlParameter p_tipoServicio = new MySqlParameter();
            p_tipoServicio.MySqlDbType = MySqlDbType.VarChar;
            p_tipoServicio.Size = 25;
            p_tipoServicio.ParameterName = "@p_tipoServicio";
            p_tipoServicio.Direction = ParameterDirection.Input;

            MySqlParameter p_delimitador = new MySqlParameter();
            p_delimitador.MySqlDbType = MySqlDbType.VarChar;
            p_delimitador.Size = 3;
            p_delimitador.ParameterName = "@p_delimitador";
            p_delimitador.Direction = ParameterDirection.Input;

            MySqlParameter p_trafico = new MySqlParameter();
            p_trafico.MySqlDbType = MySqlDbType.VarChar;
            p_trafico.Size = 1;
            p_trafico.ParameterName = "@p_trafico";
            p_trafico.Direction = ParameterDirection.Input;

            MySqlParameter p_entradaTransit = new MySqlParameter();
            p_entradaTransit.MySqlDbType = MySqlDbType.Int32;
            p_entradaTransit.ParameterName = "@p_entradaTransit";
            p_entradaTransit.Direction = ParameterDirection.Input;

            MySqlParameter p_calculoRV = new MySqlParameter();
            p_calculoRV.MySqlDbType = MySqlDbType.Int32;
            p_calculoRV.ParameterName = "@p_calculoRV";
            p_calculoRV.Direction = ParameterDirection.Input;

            MySqlParameter p_adr = new MySqlParameter();
            p_adr.MySqlDbType = MySqlDbType.VarChar;
            p_adr.Size = 25;
            p_adr.ParameterName = "@p_adr";
            p_adr.Direction = ParameterDirection.Input;

            MySqlParameter p_fechaRV = new MySqlParameter();
            p_fechaRV.MySqlDbType = MySqlDbType.VarChar;
            p_fechaRV.Size = 25;
            p_fechaRV.ParameterName = "@p_fechaRV";
            p_fechaRV.Direction = ParameterDirection.Input;

            MySqlParameter p_codCli = new MySqlParameter();
            p_codCli.MySqlDbType = MySqlDbType.VarChar;
            p_codCli.Size = 6;
            p_codCli.ParameterName = "@p_codCli";
            p_codCli.Direction = ParameterDirection.Input;

            MySqlParameter p_pais = new MySqlParameter();
            p_pais.MySqlDbType = MySqlDbType.VarChar;
            p_pais.Size = 2;
            p_pais.ParameterName = "@p_pais";
            p_pais.Direction = ParameterDirection.Input;

            MySqlParameter p_nomCli = new MySqlParameter();
            p_nomCli.MySqlDbType = MySqlDbType.VarChar;
            p_nomCli.Size = 25;
            p_nomCli.ParameterName = "@p_nomCli";
            p_nomCli.Direction = ParameterDirection.Input;

            MySqlParameter p_delegacion = new MySqlParameter();
            p_delegacion.MySqlDbType = MySqlDbType.VarChar;
            p_delegacion.Size = 4;
            p_delegacion.ParameterName = "@p_delegacion";
            p_delegacion.Direction = ParameterDirection.Input;

            cmdActualizar.Parameters.Add(p_linealInicio);
            cmdActualizar.Parameters.Add(p_covia);
            cmdActualizar.Parameters.Add(p_codDesRem);
            cmdActualizar.Parameters.Add(p_destinatario);
            cmdActualizar.Parameters.Add(p_direccion);
            cmdActualizar.Parameters.Add(p_poblacion);
            cmdActualizar.Parameters.Add(p_cp);
            cmdActualizar.Parameters.Add(p_bultos);
            cmdActualizar.Parameters.Add(p_kilos);
            cmdActualizar.Parameters.Add(p_kilosCovia);
            cmdActualizar.Parameters.Add(p_m3);
            cmdActualizar.Parameters.Add(p_m3Covia);
            cmdActualizar.Parameters.Add(p_pares);
            cmdActualizar.Parameters.Add(p_condiciones);
            cmdActualizar.Parameters.Add(p_valor);
            cmdActualizar.Parameters.Add(p_refCli);
            cmdActualizar.Parameters.Add(p_refAg);
            cmdActualizar.Parameters.Add(p_impCod);
            cmdActualizar.Parameters.Add(p_paisCod);
            cmdActualizar.Parameters.Add(p_observaciones);
            cmdActualizar.Parameters.Add(p_expedicion);
            cmdActualizar.Parameters.Add(p_marcas);
            cmdActualizar.Parameters.Add(p_clase);
            cmdActualizar.Parameters.Add(p_contenido);
            cmdActualizar.Parameters.Add(p_certificados);
            cmdActualizar.Parameters.Add(p_codAg);
            cmdActualizar.Parameters.Add(p_cmr);
            cmdActualizar.Parameters.Add(p_personal);
            cmdActualizar.Parameters.Add(p_facturar);
            cmdActualizar.Parameters.Add(p_nomPais);
            cmdActualizar.Parameters.Add(p_codBultos);
            cmdActualizar.Parameters.Add(p_tipoServicio);
            cmdActualizar.Parameters.Add(p_delimitador);
            cmdActualizar.Parameters.Add(p_trafico);
            cmdActualizar.Parameters.Add(p_entradaTransit);
            cmdActualizar.Parameters.Add(p_calculoRV);
            cmdActualizar.Parameters.Add(p_adr);
            cmdActualizar.Parameters.Add(p_fechaRV);
            cmdActualizar.Parameters.Add(p_codCli);
            cmdActualizar.Parameters.Add(p_pais);
            cmdActualizar.Parameters.Add(p_nomCli);
            cmdActualizar.Parameters.Add(p_delegacion);

            cmdActualizar.Parameters[0].Value = parseCliente.lineaInicio;
            cmdActualizar.Parameters[1].Value = parseCliente.covia;
            cmdActualizar.Parameters[2].Value = parseCliente.codDesRem;
            cmdActualizar.Parameters[3].Value = parseCliente.destinatario;
            cmdActualizar.Parameters[4].Value = parseCliente.direccion;
            cmdActualizar.Parameters[5].Value = parseCliente.poblacion;
            cmdActualizar.Parameters[6].Value = parseCliente.cp;
            cmdActualizar.Parameters[7].Value = parseCliente.bultos;
            cmdActualizar.Parameters[8].Value = parseCliente.kilos;
            cmdActualizar.Parameters[9].Value = parseCliente.kilosCovia;
            cmdActualizar.Parameters[10].Value = parseCliente.metrosCubicos;
            cmdActualizar.Parameters[11].Value = parseCliente.metrosCubicosCovia;
            cmdActualizar.Parameters[12].Value = parseCliente.pares;
            cmdActualizar.Parameters[13].Value = parseCliente.condiciones;
            cmdActualizar.Parameters[14].Value = parseCliente.valor;
            cmdActualizar.Parameters[15].Value = parseCliente.refCliente;
            cmdActualizar.Parameters[16].Value = parseCliente.refAg;
            cmdActualizar.Parameters[17].Value = parseCliente.impCod;
            cmdActualizar.Parameters[18].Value = parseCliente.paisCod;
            cmdActualizar.Parameters[19].Value = parseCliente.observaciones;
            cmdActualizar.Parameters[20].Value = parseCliente.expedicion;
            cmdActualizar.Parameters[21].Value = parseCliente.marcas;
            cmdActualizar.Parameters[22].Value = parseCliente.clase;
            cmdActualizar.Parameters[23].Value = parseCliente.contenido;
            cmdActualizar.Parameters[24].Value = parseCliente.certificados;
            cmdActualizar.Parameters[25].Value = parseCliente.codAg;
            cmdActualizar.Parameters[26].Value = parseCliente.cmr;
            cmdActualizar.Parameters[27].Value = parseCliente.personal;
            cmdActualizar.Parameters[28].Value = parseCliente.facturar;
            cmdActualizar.Parameters[29].Value = parseCliente.nomPais;
            cmdActualizar.Parameters[30].Value = parseCliente.codBultos;
            cmdActualizar.Parameters[31].Value = parseCliente.tipoServicio;
            cmdActualizar.Parameters[32].Value = parseCliente.delimitador;
            cmdActualizar.Parameters[33].Value = parseCliente.trafico;
            cmdActualizar.Parameters[34].Value = parseCliente.entradaTransit;
            cmdActualizar.Parameters[35].Value = parseCliente.calculoRendezVouz;
            cmdActualizar.Parameters[36].Value = parseCliente.adr;
            cmdActualizar.Parameters[37].Value = parseCliente.fechaRendezVouz;
            cmdActualizar.Parameters[38].Value = parseCliente.codigoCliente;
            cmdActualizar.Parameters[39].Value = parseCliente.pais;
            cmdActualizar.Parameters[40].Value = parseCliente.nomCliente;
            cmdActualizar.Parameters[41].Value = parseCliente.delegacion;

            using (mySqlConnection)
            {
                try
                {
                    mySqlConnection.Open();
                    cmdActualizar.ExecuteNonQuery();
                }
                catch { }
            }
        }
        public bool ExisteParseCliente(string codCli, string pais, string nomcli, string delegacion)
        {
            MySqlCommand cmdExisteCliente = new MySqlCommand();
            cmdExisteCliente.CommandType = CommandType.Text;
            cmdExisteCliente.CommandText = "SELECT * FROM parsecliente p WHERE codcli = @p_codCli AND pais = @p_pais AND nomcli = @p_nomCli AND delegacion = @p_delegacion ";
            cmdExisteCliente.Connection = mySqlConnection;

            MySqlParameter p_codCli = new MySqlParameter();
            p_codCli.MySqlDbType = MySqlDbType.VarChar;
            p_codCli.Size = 6;
            p_codCli.ParameterName = "@p_codCli";
            p_codCli.Direction = ParameterDirection.Input;
            cmdExisteCliente.Parameters.Add(p_codCli);

            MySqlParameter p_pais = new MySqlParameter();
            p_pais.MySqlDbType = MySqlDbType.VarChar;
            p_pais.Size = 2;
            p_pais.ParameterName = "@p_pais";
            p_pais.Direction = ParameterDirection.Input;
            cmdExisteCliente.Parameters.Add(p_pais);

            MySqlParameter p_nomCli = new MySqlParameter();
            p_nomCli.MySqlDbType = MySqlDbType.VarChar;
            p_nomCli.Size = 25;
            p_nomCli.ParameterName = "@p_nomCli";
            p_nomCli.Direction = ParameterDirection.Input;
            cmdExisteCliente.Parameters.Add(p_nomCli);

            MySqlParameter p_delegacion = new MySqlParameter();
            p_delegacion.MySqlDbType = MySqlDbType.VarChar;
            p_delegacion.Size = 4;
            p_delegacion.ParameterName = "@p_delegacion";
            p_delegacion.Direction = ParameterDirection.Input;
            cmdExisteCliente.Parameters.Add(p_delegacion);

            cmdExisteCliente.Parameters[0].Value = codCli;
            cmdExisteCliente.Parameters[1].Value = pais;
            cmdExisteCliente.Parameters[2].Value = nomcli;
            cmdExisteCliente.Parameters[3].Value = delegacion;

            using (mySqlConnection)
            {
                mySqlConnection.Open();
                if (cmdExisteCliente.ExecuteScalar() != null)
                {
                    return true;
                }
            }
            return false;
        }
        public DataSet NuevoParseClienteDataSet()////Devuelve un dataset con 3 tablas (nombreClientes, Paises,Delegaciones)
        {
            DataSet dataSet = new DataSet();
            MySqlCommand cmdNomClientes = new MySqlCommand();//command para tabla Nombres
            cmdNomClientes.CommandType = CommandType.Text;
            cmdNomClientes.CommandText = "SELECT DISTINCT nombre FROM bfirst_parse WHERE nombre != ' '";
            cmdNomClientes.Connection = mySqlConnection;

            MySqlCommand cmdPaises = new MySqlCommand();//command para tabla Paises
            cmdPaises.CommandType = CommandType.Text;
            cmdPaises.CommandText = "SELECT DISTINCT pais, nombre_pais FROM bfirst_parse WHERE pais != ' '";
            cmdPaises.Connection = mySqlConnection;

            MySqlCommand cmdDiviones = new MySqlCommand();//command para Divisiones
            cmdDiviones.CommandType = CommandType.Text;
            cmdDiviones.CommandText = "SELECT DISTINCT division FROM bfirst_parse WHERE division != ' '";
            cmdDiviones.Connection = mySqlConnection;

            using (mySqlConnection)
            {
                mySqlConnection.Open();
                MySqlDataAdapter adapterClientes = new MySqlDataAdapter(cmdNomClientes);
                adapterClientes.Fill(dataSet, "Clientes");

                MySqlDataAdapter adapterPaises = new MySqlDataAdapter(cmdPaises);
                adapterPaises.Fill(dataSet, "Paises");

                MySqlDataAdapter adapterDivisiones = new MySqlDataAdapter(cmdDiviones);
                adapterDivisiones.Fill(dataSet, "Divisiones");
            }

            return dataSet;
        }
        public NuevoParseCliente PlantillaNuevoCliente(string nombre, string pais)
        {
            NuevoParseCliente cliente = new NuevoParseCliente();

            MySqlCommand cmdParseClienteNuevo = new MySqlCommand();
            cmdParseClienteNuevo.CommandType = CommandType.Text;
            cmdParseClienteNuevo.CommandText = "SELECT * FROM bfirst_parse p WHERE nombre = @p_nomcli AND nombre_pais = @p_pais";
            cmdParseClienteNuevo.Connection = mySqlConnection;

            MySqlParameter p_cliente = new MySqlParameter();
            p_cliente.Direction = ParameterDirection.Input;
            p_cliente.ParameterName = "p_nomcli";
            p_cliente.MySqlDbType = MySqlDbType.VarChar;
            p_cliente.Size = 25;

            MySqlParameter p_pais = new MySqlParameter();
            p_pais.Direction = ParameterDirection.Input;
            p_pais.ParameterName = "p_pais";
            p_pais.MySqlDbType = MySqlDbType.VarChar;
            p_pais.Size = 25;

            cmdParseClienteNuevo.Parameters.Add(p_cliente);
            cmdParseClienteNuevo.Parameters.Add(p_pais);
            cmdParseClienteNuevo.Parameters[0].Value = nombre;
            cmdParseClienteNuevo.Parameters[1].Value = pais;

            using (mySqlConnection)
            {
                mySqlConnection.Open();
                MySqlDataReader lectorCliente = cmdParseClienteNuevo.ExecuteReader();
                while (lectorCliente.Read())
                {
                    cliente.idParse = lectorCliente.GetInt32(0);
                    cliente.division = comprobarLector(lectorCliente.GetValue(1));
                    cliente.nombre = comprobarLector(lectorCliente.GetValue(2));
                    cliente.nombrePais = comprobarLector(lectorCliente.GetValue(3));
                    cliente.pais = comprobarLector(lectorCliente.GetValue(4));
                    cliente.delimitador = comprobarLector(lectorCliente.GetValue(5));
                    cliente.linealInicio = comprobarLector(lectorCliente.GetValue(6));
                    cliente.camposAgrupar = comprobarLector(lectorCliente.GetValue(7));
                    cliente.trafico = comprobarLector(lectorCliente.GetValue(8));
                    cliente.flete = comprobarLector(lectorCliente.GetValue(9));
                    cliente.filtro = comprobarLector(lectorCliente.GetValue(10));
                    cliente.idFacturacion = comprobarLector(lectorCliente.GetValue(11));
                    cliente.idDireccion = comprobarLector(lectorCliente.GetValue(12));
                    cliente.idOrdenante = comprobarLector(lectorCliente.GetValue(13));
                    cliente.idCorresponsal = comprobarLector(lectorCliente.GetValue(14));
                    cliente.nombreDesRem = comprobarLector(lectorCliente.GetValue(15));
                    cliente.direccion = comprobarLector(lectorCliente.GetValue(16));
                    cliente.poblacion = comprobarLector(lectorCliente.GetValue(17));
                    cliente.cp = comprobarLector(lectorCliente.GetValue(18));
                    cliente.telefono = comprobarLector(lectorCliente.GetValue(19));
                    cliente.email = comprobarLector(lectorCliente.GetValue(20));
                    cliente.horario = comprobarLector(lectorCliente.GetValue(21));
                    cliente.bultos = comprobarLector(lectorCliente.GetValue(22));
                    cliente.kilos = comprobarLector(lectorCliente.GetValue(23));
                    cliente.kilosCovia = comprobarLector(lectorCliente.GetValue(24));
                    cliente.kilosCliente = comprobarLector(lectorCliente.GetValue(25));
                    cliente.kilosDecimales = comprobarLector(lectorCliente.GetValue(26));
                    cliente.m3 = comprobarLector(lectorCliente.GetValue(27));
                    cliente.m3Covia = comprobarLector(lectorCliente.GetValue(28));
                    cliente.unidades = comprobarLector(lectorCliente.GetValue(29));
                    cliente.condiciones = comprobarLector(lectorCliente.GetValue(30));
                    cliente.valor = comprobarLector(lectorCliente.GetValue(31));
                    cliente.divisaValor = comprobarLector(lectorCliente.GetValue(32));
                    cliente.refCliente = comprobarLector(lectorCliente.GetValue(33));
                    cliente.vinculacion = comprobarLector(lectorCliente.GetValue(34));
                    cliente.valorVinculacion = comprobarLector(lectorCliente.GetValue(35));
                    cliente.divisaVinculacion = comprobarLector(lectorCliente.GetValue(36));
                    cliente.observaciones = comprobarLector(lectorCliente.GetValue(37));
                    cliente.marcas = comprobarLector(lectorCliente.GetValue(38));
                    cliente.tipoBultos = comprobarLector(lectorCliente.GetValue(39));
                    cliente.mercancia = comprobarLector(lectorCliente.GetValue(40));
                    cliente.codBultos = comprobarLector(lectorCliente.GetValue(41));
                    cliente.servicio = comprobarLector(lectorCliente.GetValue(42));
                    cliente.fechaRecogida = comprobarLector(lectorCliente.GetValue(43));
                    cliente.fechaRVouz = comprobarLector(lectorCliente.GetValue(44));
                    cliente.adrClase = comprobarLector(lectorCliente.GetValue(45));
                    cliente.adrOnu = comprobarLector(lectorCliente.GetValue(46));
                    cliente.adrEmbalaje = comprobarLector(lectorCliente.GetValue(47));
                    cliente.entAlmFis = comprobarLector(lectorCliente.GetValue(48));
                    cliente.entAlmLog = comprobarLector(lectorCliente.GetValue(49));
                    cliente.entReferencia = comprobarLector(lectorCliente.GetValue(50));
                    cliente.entPalet = comprobarLector(lectorCliente.GetValue(51));
                    cliente.entEmbalaje = comprobarLector(lectorCliente.GetValue(52));
                    cliente.entUbicacion = comprobarLector(lectorCliente.GetValue(53));
                    cliente.entSituacion = comprobarLector(lectorCliente.GetValue(54));
                    cliente.xId = comprobarLector(lectorCliente.GetValue(55));
                    cliente.xTipo = comprobarLector(lectorCliente.GetValue(56));
                    cliente.xBuscar = comprobarLector(lectorCliente.GetValue(57));
                    cliente.xIent = comprobarLector(lectorCliente.GetValue(58));
                    cliente.xCent = comprobarLector(lectorCliente.GetValue(59));
                    cliente.xNom = comprobarLector(lectorCliente.GetValue(60));
                    cliente.xCp = comprobarLector(lectorCliente.GetValue(61));
                    cliente.xPob = comprobarLector(lectorCliente.GetValue(62));
                    cliente.xDir = comprobarLector(lectorCliente.GetValue(63));
                    cliente.xPda = comprobarLector(lectorCliente.GetValue(64));
                    cliente.xIDir = comprobarLector(lectorCliente.GetValue(65));
                    cliente.xTipoDir = comprobarLector(lectorCliente.GetValue(66));
                }
            }
            return cliente;
        }
        public void AltaNuevoParseCliente(NuevoParseCliente cliente)//Alta/Insert NuevoParseCliente en BD
        {
            MySqlCommand cmdAltaNuevoParse = new MySqlCommand();
            cmdAltaNuevoParse.CommandType = CommandType.Text;
            cmdAltaNuevoParse.CommandText = "INSERT INTO bfirst_parse (division, nombre, nombre_pais, pais, delimitador, lineaInicio, camposagrupar, trafico, flete, filtro, idfacturacion, iddireccion, idordenante, idcorresponsal, nombredesrem, direccion, poblacion, cp, telefono, email, horario, bultos, kilos, kilos_covia, kilos_cliente, kilos_decimales, m3, m3_covia, unidades, condiciones, valor, divisavalor, refcli, vinculacion, valorvinculacion, divisavinculacion, observaciones, marcas, tipobultos, mercancia, codBultos, servicio, fecha_recogida, fecha_rendezvous, adr_clase, adr_onu, adr_embalaje, ent_almfis, ent_almlog, ent_referencia, ent_palet, ent_embalaje, ent_ubicacion, ent_situacion, xid, xtipo, xbuscar, xient, xcent, xnom, xcp, xpob, xdir, xpda, xidir, xtipodir) VALUES (@p_division, @p_nombre, @p_nombrePais, @p_pais, @p_delimitador, @p_linealInicio, @p_camposAgrupar, @p_trafico, @p_flete, @p_filtro, @p_idFacturacion, @p_idDireccion, @p_idOrdenante, @p_idCorresponsal, @p_desRem, @p_direccion, @p_poblacion, @p_cp, @p_telefono, @p_email, @p_horario, @p_bultos, @p_kilos, @p_kilosCovia, @p_kilosCliente, @p_kilosDec, @p_m3, @p_m3Covia, @p_unidades, @p_condiciones, @p_valor, @p_divisaValor, @p_refCliente, @p_vinculacion, @p_valorVinculacion, @p_divisaVinculacion, @p_observaciones, @p_marcas, @p_tipoBultos, @p_mercancia, @p_codBultos, @p_servicio, @p_fechaRecogida, @p_fechaRVouz, @p_adrClase, @p_adrOnu, @p_adrEmbalaje, @p_entAlmFis, @p_entAlmLog, @p_entRef, @p_entPalet,@p_entEmbalaje, @p_entUbicacion, @p_entSituacion, @p_xId, @p_xTipo, @p_xBuscar, @p_xIent, @p_xCent, @p_xNom, @p_xCp, @p_xPob, @p_xDir, @p_xPda, @p_xIdir, @p_xTipoDir)";
            cmdAltaNuevoParse.Connection = mySqlConnection;

            MySqlParameter p_division = new MySqlParameter();
            p_division.ParameterName = "@p_division";
            p_division.Direction = ParameterDirection.Input;
            p_division.MySqlDbType = MySqlDbType.VarChar;
            p_division.Size = 7;
            cmdAltaNuevoParse.Parameters.Add(p_division);
            p_division.Value = cliente.division;

            MySqlParameter p_nombre = new MySqlParameter();
            p_nombre.ParameterName = "@p_nombre";
            p_nombre.Direction = ParameterDirection.Input;
            p_nombre.MySqlDbType = MySqlDbType.VarChar;
            p_nombre.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_nombre);
            p_nombre.Value = cliente.nombre;

            MySqlParameter p_nombrePais = new MySqlParameter();
            p_nombrePais.ParameterName = "p_nombrePais";
            p_nombrePais.Direction = ParameterDirection.Input;
            p_nombrePais.MySqlDbType = MySqlDbType.VarChar;
            p_nombrePais.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_nombrePais);
            p_nombrePais.Value = cliente.nombrePais;

            MySqlParameter p_pais = new MySqlParameter();
            p_pais.ParameterName = "p_pais";
            p_pais.Direction = ParameterDirection.Input;
            p_pais.MySqlDbType = MySqlDbType.VarChar;
            p_pais.Size = 2;
            cmdAltaNuevoParse.Parameters.Add(p_pais);
            p_pais.Value = cliente.pais;

            MySqlParameter p_delimitador = new MySqlParameter();
            p_delimitador.ParameterName = "p_delimitador";
            p_delimitador.Direction = ParameterDirection.Input;
            p_delimitador.MySqlDbType = MySqlDbType.VarChar;
            p_delimitador.Size = 50;
            cmdAltaNuevoParse.Parameters.Add(p_delimitador);
            p_delimitador.Value = cliente.delimitador;

            MySqlParameter p_linealInicio = new MySqlParameter();
            p_linealInicio.ParameterName = "p_linealInicio";
            p_linealInicio.Direction = ParameterDirection.Input;
            p_linealInicio.MySqlDbType = MySqlDbType.VarChar;
            p_linealInicio.Size = 4;
            cmdAltaNuevoParse.Parameters.Add(p_linealInicio);
            p_linealInicio.Value = cliente.linealInicio;

            MySqlParameter p_camposAgrupar = new MySqlParameter();
            p_camposAgrupar.ParameterName = "p_camposAgrupar";
            p_camposAgrupar.Direction = ParameterDirection.Input;
            p_camposAgrupar.MySqlDbType = MySqlDbType.VarChar;
            p_camposAgrupar.Size = 200;
            cmdAltaNuevoParse.Parameters.Add(p_camposAgrupar);
            p_camposAgrupar.Value = cliente.camposAgrupar;

            MySqlParameter p_trafico = new MySqlParameter();
            p_trafico.ParameterName = "p_trafico";
            p_trafico.Direction = ParameterDirection.Input;
            p_trafico.MySqlDbType = MySqlDbType.VarChar;
            p_trafico.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_trafico);
            p_trafico.Value = cliente.trafico;

            MySqlParameter p_flete = new MySqlParameter();
            p_flete.ParameterName = "p_flete";
            p_flete.Direction = ParameterDirection.Input;
            p_flete.MySqlDbType = MySqlDbType.VarChar;
            p_flete.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_flete);
            p_flete.Value = cliente.flete;

            MySqlParameter p_filtro = new MySqlParameter();
            p_filtro.ParameterName = "p_filtro";
            p_filtro.Direction = ParameterDirection.Input;
            p_filtro.MySqlDbType = MySqlDbType.VarChar;
            p_filtro.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_filtro);
            p_filtro.Value = cliente.filtro;

            MySqlParameter p_idFacturacion = new MySqlParameter();
            p_idFacturacion.ParameterName = "p_idFacturacion";
            p_idFacturacion.Direction = ParameterDirection.Input;
            p_idFacturacion.MySqlDbType = MySqlDbType.VarChar;
            p_idFacturacion.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_idFacturacion);
            p_idFacturacion.Value = cliente.idFacturacion;

            MySqlParameter p_idDireccion = new MySqlParameter();
            p_idDireccion.ParameterName = "p_idDireccion";
            p_idDireccion.Direction = ParameterDirection.Input;
            p_idDireccion.MySqlDbType = MySqlDbType.VarChar;
            p_idDireccion.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_idDireccion);
            p_idDireccion.Value = cliente.idDireccion;

            MySqlParameter p_idOrdenante = new MySqlParameter();
            p_idOrdenante.ParameterName = "p_idOrdenante";
            p_idOrdenante.Direction = ParameterDirection.Input;
            p_idOrdenante.MySqlDbType = MySqlDbType.VarChar;
            p_idOrdenante.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_idOrdenante);
            p_idOrdenante.Value = cliente.idOrdenante;

            MySqlParameter p_idCorresponsal = new MySqlParameter();
            p_idCorresponsal.ParameterName = "p_idCorresponsal";
            p_idCorresponsal.Direction = ParameterDirection.Input;
            p_idCorresponsal.MySqlDbType = MySqlDbType.VarChar;
            p_idCorresponsal.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_idCorresponsal);
            p_idCorresponsal.Value = cliente.idCorresponsal;

            MySqlParameter p_desRem = new MySqlParameter();
            p_desRem.ParameterName = "p_desRem";
            p_desRem.Direction = ParameterDirection.Input;
            p_desRem.MySqlDbType = MySqlDbType.VarChar;
            p_desRem.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_desRem);
            p_desRem.Value = cliente.nombreDesRem;

            MySqlParameter p_direccion = new MySqlParameter();
            p_direccion.ParameterName = "p_direccion";
            p_direccion.Direction = ParameterDirection.Input;
            p_direccion.MySqlDbType = MySqlDbType.VarChar;
            p_direccion.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_direccion);
            p_direccion.Value = cliente.direccion;

            MySqlParameter p_poblacion = new MySqlParameter();
            p_poblacion.ParameterName = "p_poblacion";
            p_poblacion.Direction = ParameterDirection.Input;
            p_poblacion.MySqlDbType = MySqlDbType.VarChar;
            p_poblacion.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_poblacion);
            p_poblacion.Value = cliente.poblacion;

            MySqlParameter p_cp = new MySqlParameter();
            p_cp.ParameterName = "p_cp";
            p_cp.Direction = ParameterDirection.Input;
            p_cp.MySqlDbType = MySqlDbType.VarChar;
            p_cp.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_cp);
            p_cp.Value = cliente.cp;

            MySqlParameter p_telefono = new MySqlParameter();
            p_telefono.ParameterName = "p_telefono";
            p_telefono.Direction = ParameterDirection.Input;
            p_telefono.MySqlDbType = MySqlDbType.VarChar;
            p_telefono.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_telefono);
            p_telefono.Value = cliente.telefono;

            MySqlParameter p_email = new MySqlParameter();
            p_email.ParameterName = "p_email";
            p_email.Direction = ParameterDirection.Input;
            p_email.MySqlDbType = MySqlDbType.VarChar;
            p_email.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_email);
            p_email.Value = cliente.email;

            MySqlParameter p_horario = new MySqlParameter();
            p_horario.ParameterName = "p_horario";
            p_horario.Direction = ParameterDirection.Input;
            p_horario.MySqlDbType = MySqlDbType.VarChar;
            p_horario.Size = 75;
            cmdAltaNuevoParse.Parameters.Add(p_horario);
            p_horario.Value = cliente.horario;

            MySqlParameter p_bultos = new MySqlParameter();
            p_bultos.ParameterName = "p_bultos";
            p_bultos.Direction = ParameterDirection.Input;
            p_bultos.MySqlDbType = MySqlDbType.VarChar;
            p_bultos.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_bultos);
            p_bultos.Value = cliente.bultos;

            MySqlParameter p_kilos = new MySqlParameter();
            p_kilos.ParameterName = "p_kilos";
            p_kilos.Direction = ParameterDirection.Input;
            p_kilos.MySqlDbType = MySqlDbType.VarChar;
            p_kilos.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_kilos);
            p_kilos.Value = cliente.kilos;

            MySqlParameter p_kilosCovia = new MySqlParameter();
            p_kilosCovia.ParameterName = "p_kilosCovia";
            p_kilosCovia.Direction = ParameterDirection.Input;
            p_kilosCovia.MySqlDbType = MySqlDbType.VarChar;
            p_kilosCovia.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_kilosCovia);
            p_kilosCovia.Value = cliente.kilosCovia;

            MySqlParameter p_kilosCliente = new MySqlParameter();
            p_kilosCliente.ParameterName = "p_kilosCliente";
            p_kilosCliente.Direction = ParameterDirection.Input;
            p_kilosCliente.MySqlDbType = MySqlDbType.VarChar;
            p_kilosCliente.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_kilosCliente);
            p_kilosCliente.Value = cliente.kilosCliente;

            MySqlParameter p_kilosDec = new MySqlParameter();
            p_kilosDec.ParameterName = "p_kilosDec";
            p_kilosDec.Direction = ParameterDirection.Input;
            p_kilosDec.MySqlDbType = MySqlDbType.VarChar;
            p_kilosDec.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_kilosDec);
            p_kilosDec.Value = cliente.kilosDecimales;

            MySqlParameter p_m3 = new MySqlParameter();
            p_m3.ParameterName = "p_m3";
            p_m3.Direction = ParameterDirection.Input;
            p_m3.MySqlDbType = MySqlDbType.VarChar;
            p_m3.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_m3);
            p_m3.Value = cliente.m3;

            MySqlParameter p_m3Covia = new MySqlParameter();
            p_m3Covia.ParameterName = "p_m3Covia";
            p_m3Covia.Direction = ParameterDirection.Input;
            p_m3Covia.MySqlDbType = MySqlDbType.VarChar;
            p_m3Covia.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_m3Covia);
            p_m3Covia.Value = cliente.m3Covia;

            MySqlParameter p_unidades = new MySqlParameter();
            p_unidades.ParameterName = "p_unidades";
            p_unidades.Direction = ParameterDirection.Input;
            p_unidades.MySqlDbType = MySqlDbType.VarChar;
            p_unidades.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_unidades);
            p_unidades.Value = cliente.unidades;

            MySqlParameter p_condiciones = new MySqlParameter();
            p_condiciones.ParameterName = "p_condiciones";
            p_condiciones.Direction = ParameterDirection.Input;
            p_condiciones.MySqlDbType = MySqlDbType.VarChar;
            p_condiciones.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_condiciones);
            p_condiciones.Value = cliente.condiciones;

            MySqlParameter p_valor = new MySqlParameter();
            p_valor.ParameterName = "p_valor";
            p_valor.Direction = ParameterDirection.Input;
            p_valor.MySqlDbType = MySqlDbType.VarChar;
            p_valor.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_valor);
            p_valor.Value = cliente.valor;

            MySqlParameter p_divisaValor = new MySqlParameter();
            p_divisaValor.ParameterName = "p_divisaValor";
            p_divisaValor.Direction = ParameterDirection.Input;
            p_divisaValor.MySqlDbType = MySqlDbType.VarChar;
            p_divisaValor.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_divisaValor);
            p_divisaValor.Value = cliente.divisaValor;

            MySqlParameter p_refCliente = new MySqlParameter();
            p_refCliente.ParameterName = "p_refCliente";
            p_refCliente.Direction = ParameterDirection.Input;
            p_refCliente.MySqlDbType = MySqlDbType.VarChar;
            p_refCliente.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_refCliente);
            p_refCliente.Value = cliente.refCliente;

            MySqlParameter p_vinculacion = new MySqlParameter();
            p_vinculacion.ParameterName = "p_vinculacion";
            p_vinculacion.Direction = ParameterDirection.Input;
            p_vinculacion.MySqlDbType = MySqlDbType.VarChar;
            p_vinculacion.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_vinculacion);
            p_vinculacion.Value = cliente.vinculacion;

            MySqlParameter p_valorVinculacion = new MySqlParameter();
            p_valorVinculacion.ParameterName = "p_valorVinculacion";
            p_valorVinculacion.Direction = ParameterDirection.Input;
            p_valorVinculacion.MySqlDbType = MySqlDbType.VarChar;
            p_valorVinculacion.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_valorVinculacion);
            p_valorVinculacion.Value = cliente.valorVinculacion;

            MySqlParameter p_divisaVinculacion = new MySqlParameter();
            p_divisaVinculacion.ParameterName = "p_divisaVinculacion";
            p_divisaVinculacion.Direction = ParameterDirection.Input;
            p_divisaVinculacion.MySqlDbType = MySqlDbType.VarChar;
            p_divisaVinculacion.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_divisaVinculacion);
            p_divisaVinculacion.Value = cliente.divisaVinculacion;

            MySqlParameter p_observaciones = new MySqlParameter();
            p_observaciones.ParameterName = "p_observaciones";
            p_observaciones.Direction = ParameterDirection.Input;
            p_observaciones.MySqlDbType = MySqlDbType.VarChar;
            p_observaciones.Size = 100;
            cmdAltaNuevoParse.Parameters.Add(p_observaciones);
            p_observaciones.Value = cliente.observaciones;

            MySqlParameter p_marcas = new MySqlParameter();
            p_marcas.ParameterName = "p_marcas";
            p_marcas.Direction = ParameterDirection.Input;
            p_marcas.MySqlDbType = MySqlDbType.VarChar;
            p_marcas.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_marcas);
            p_marcas.Value = cliente.marcas;

            MySqlParameter p_tipoBultos = new MySqlParameter();
            p_tipoBultos.ParameterName = "p_tipoBultos";
            p_tipoBultos.Direction = ParameterDirection.Input;
            p_tipoBultos.MySqlDbType = MySqlDbType.VarChar;
            p_tipoBultos.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_tipoBultos);
            p_tipoBultos.Value = cliente.tipoBultos;

            MySqlParameter p_mercancia = new MySqlParameter();
            p_mercancia.ParameterName = "p_mercancia";
            p_mercancia.Direction = ParameterDirection.Input;
            p_mercancia.MySqlDbType = MySqlDbType.VarChar;
            p_mercancia.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_mercancia);
            p_mercancia.Value = cliente.mercancia;

            MySqlParameter p_codBultos = new MySqlParameter();
            p_codBultos.ParameterName = "p_codBultos";
            p_codBultos.Direction = ParameterDirection.Input;
            p_codBultos.MySqlDbType = MySqlDbType.VarChar;
            p_codBultos.Size = 200;
            cmdAltaNuevoParse.Parameters.Add(p_codBultos);
            p_codBultos.Value = cliente.codBultos;

            MySqlParameter p_servicio = new MySqlParameter();
            p_servicio.ParameterName = "p_servicio";
            p_servicio.Direction = ParameterDirection.Input;
            p_servicio.MySqlDbType = MySqlDbType.VarChar;
            p_servicio.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_servicio);
            p_servicio.Value = cliente.servicio;

            MySqlParameter p_fechaRecogida = new MySqlParameter();
            p_fechaRecogida.ParameterName = "p_fechaRecogida";
            p_fechaRecogida.Direction = ParameterDirection.Input;
            p_fechaRecogida.MySqlDbType = MySqlDbType.VarChar;
            p_fechaRecogida.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_fechaRecogida);
            p_fechaRecogida.Value = cliente.fechaRecogida;

            MySqlParameter p_fechaRVouz = new MySqlParameter();
            p_fechaRVouz.ParameterName = "p_fechaRVouz";
            p_fechaRVouz.Direction = ParameterDirection.Input;
            p_fechaRVouz.MySqlDbType = MySqlDbType.VarChar;
            p_fechaRVouz.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_fechaRVouz);
            p_fechaRVouz.Value = cliente.fechaRVouz;

            MySqlParameter p_adrClase = new MySqlParameter();
            p_adrClase.ParameterName = "p_adrClase";
            p_adrClase.Direction = ParameterDirection.Input;
            p_adrClase.MySqlDbType = MySqlDbType.VarChar;
            p_adrClase.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_adrClase);
            p_adrClase.Value = cliente.adrClase;

            MySqlParameter p_adrOnu = new MySqlParameter();
            p_adrOnu.ParameterName = "p_adrOnu";
            p_adrOnu.Direction = ParameterDirection.Input;
            p_adrOnu.MySqlDbType = MySqlDbType.VarChar;
            p_adrOnu.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_adrOnu);
            p_adrOnu.Value = cliente.adrOnu;

            MySqlParameter p_adrEmbalaje = new MySqlParameter();
            p_adrEmbalaje.ParameterName = "p_adrEmbalaje";
            p_adrEmbalaje.Direction = ParameterDirection.Input;
            p_adrEmbalaje.MySqlDbType = MySqlDbType.VarChar;
            p_adrEmbalaje.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_adrEmbalaje);
            p_adrEmbalaje.Value = cliente.adrEmbalaje;

            MySqlParameter p_entAlmFis = new MySqlParameter();
            p_entAlmFis.ParameterName = "p_entAlmFis";
            p_entAlmFis.Direction = ParameterDirection.Input;
            p_entAlmFis.MySqlDbType = MySqlDbType.VarChar;
            p_entAlmFis.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_entAlmFis);
            p_entAlmFis.Value = cliente.entAlmFis;

            MySqlParameter p_entAlmLog = new MySqlParameter();
            p_entAlmLog.ParameterName = "p_entAlmLog";
            p_entAlmLog.Direction = ParameterDirection.Input;
            p_entAlmLog.MySqlDbType = MySqlDbType.VarChar;
            p_entAlmLog.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_entAlmLog);
            p_entAlmLog.Value = cliente.entAlmLog;

            MySqlParameter p_entRef = new MySqlParameter();
            p_entRef.ParameterName = "p_entRef";
            p_entRef.Direction = ParameterDirection.Input;
            p_entRef.MySqlDbType = MySqlDbType.VarChar;
            p_entRef.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_entRef);
            p_entRef.Value = cliente.entReferencia;

            MySqlParameter p_entPalet = new MySqlParameter();
            p_entPalet.ParameterName = "p_entPalet";
            p_entPalet.Direction = ParameterDirection.Input;
            p_entPalet.MySqlDbType = MySqlDbType.VarChar;
            p_entPalet.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_entPalet);
            p_entPalet.Value = cliente.entPalet;

            MySqlParameter p_entEmbalaje = new MySqlParameter();
            p_entEmbalaje.ParameterName = "p_entEmbalaje";
            p_entEmbalaje.Direction = ParameterDirection.Input;
            p_entEmbalaje.MySqlDbType = MySqlDbType.VarChar;
            p_entEmbalaje.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_entEmbalaje);
            p_entEmbalaje.Value = cliente.entEmbalaje;


            MySqlParameter p_entUbicacion = new MySqlParameter();
            p_entUbicacion.ParameterName = "p_entUbicacion";
            p_entUbicacion.Direction = ParameterDirection.Input;
            p_entUbicacion.MySqlDbType = MySqlDbType.VarChar;
            p_entUbicacion.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_entUbicacion);
            p_entUbicacion.Value = cliente.entUbicacion;

            MySqlParameter p_entSituacion = new MySqlParameter();
            p_entSituacion.ParameterName = "p_entSituacion";
            p_entSituacion.Direction = ParameterDirection.Input;
            p_entSituacion.MySqlDbType = MySqlDbType.VarChar;
            p_entSituacion.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_entSituacion);
            p_entSituacion.Value = cliente.entSituacion;

            MySqlParameter p_xId = new MySqlParameter();
            p_xId.ParameterName = "p_xId";
            p_xId.Direction = ParameterDirection.Input;
            p_xId.MySqlDbType = MySqlDbType.VarChar;
            p_xId.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_xId);
            p_xId.Value = cliente.xId;

            MySqlParameter p_xTipo = new MySqlParameter();
            p_xTipo.ParameterName = "p_xTipo";
            p_xTipo.Direction = ParameterDirection.Input;
            p_xTipo.MySqlDbType = MySqlDbType.VarChar;
            p_xTipo.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_xTipo);
            p_xTipo.Value = cliente.xTipo;

            MySqlParameter p_xBuscar = new MySqlParameter();
            p_xBuscar.ParameterName = "p_xBuscar";
            p_xBuscar.Direction = ParameterDirection.Input;
            p_xBuscar.MySqlDbType = MySqlDbType.VarChar;
            p_xBuscar.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_xBuscar);
            p_xBuscar.Value = cliente.xBuscar;

            MySqlParameter p_xIent = new MySqlParameter();
            p_xIent.ParameterName = "p_xIent";
            p_xIent.Direction = ParameterDirection.Input;
            p_xIent.MySqlDbType = MySqlDbType.VarChar;
            p_xIent.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_xIent);
            p_xIent.Value = cliente.xIent;

            MySqlParameter p_xCent = new MySqlParameter();
            p_xCent.ParameterName = "p_xCent";
            p_xCent.Direction = ParameterDirection.Input;
            p_xCent.MySqlDbType = MySqlDbType.VarChar;
            p_xCent.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_xCent);
            p_xCent.Value = cliente.xCent;

            MySqlParameter p_xNom = new MySqlParameter();
            p_xNom.ParameterName = "p_xNom";
            p_xNom.Direction = ParameterDirection.Input;
            p_xNom.MySqlDbType = MySqlDbType.VarChar;
            p_xNom.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_xNom);
            p_xNom.Value = cliente.xNom;

            MySqlParameter p_xCp = new MySqlParameter();
            p_xCp.ParameterName = "p_xCp";
            p_xCp.Direction = ParameterDirection.Input;
            p_xCp.MySqlDbType = MySqlDbType.VarChar;
            p_xCp.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_xCp);
            p_xCp.Value = cliente.xCp;

            MySqlParameter p_xPob = new MySqlParameter();
            p_xPob.ParameterName = "p_xPob";
            p_xPob.Direction = ParameterDirection.Input;
            p_xPob.MySqlDbType = MySqlDbType.VarChar;
            p_xPob.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_xPob);
            p_xPob.Value = cliente.xPob;

            MySqlParameter p_xDir = new MySqlParameter();
            p_xDir.ParameterName = "p_xDir";
            p_xDir.Direction = ParameterDirection.Input;
            p_xDir.MySqlDbType = MySqlDbType.VarChar;
            p_xDir.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_xDir);
            p_xDir.Value = cliente.xDir;

            MySqlParameter p_xPda = new MySqlParameter();
            p_xPda.ParameterName = "p_xPda";
            p_xPda.Direction = ParameterDirection.Input;
            p_xPda.MySqlDbType = MySqlDbType.VarChar;
            p_xPda.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_xPda);
            p_xPda.Value = cliente.xPda;

            MySqlParameter p_xIdir = new MySqlParameter();
            p_xIdir.ParameterName = "p_xIdir";
            p_xIdir.Direction = ParameterDirection.Input;
            p_xIdir.MySqlDbType = MySqlDbType.VarChar;
            p_xIdir.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_xIdir);
            p_xIdir.Value = cliente.xIDir;

            MySqlParameter p_xTipoDir = new MySqlParameter();
            p_xTipoDir.ParameterName = "p_xTipoDir";
            p_xTipoDir.Direction = ParameterDirection.Input;
            p_xTipoDir.MySqlDbType = MySqlDbType.VarChar;
            p_xTipoDir.Size = 25;
            cmdAltaNuevoParse.Parameters.Add(p_xTipoDir);
            p_xTipoDir.Value = cliente.xTipoDir;

            using (mySqlConnection)
            {
                mySqlConnection.Open();
                cmdAltaNuevoParse.ExecuteNonQuery();
            }
        }
        public void ActualizarNuevoParseCliente(NuevoParseCliente cliente)//Actualiza los datos de un NuevoParseCliente con un idconcreto
        {
            MySqlCommand cmdActualizarNuevoParse = new MySqlCommand();
            cmdActualizarNuevoParse.CommandType = CommandType.Text;
            cmdActualizarNuevoParse.CommandText = "UPDATE bfirst_parse SET division = @p_division,nombre = @p_nombre,nombre_pais = @p_nombrePais,pais = @p_pais,delimitador = @p_delimitador,lineaInicio = @p_linealInicio,camposagrupar = @p_camposAgrupar,trafico = @p_trafico,flete = @p_flete,filtro = @p_filtro,idfacturacion = @p_idFacturacion,iddireccion = @p_idDireccion,idordenante = @p_idOrdenante,idcorresponsal = @p_idCorresponsal,nombredesrem = @p_desRem,direccion = @p_direccion,poblacion = @p_poblacion,cp = @p_cp,telefono = @p_telefono,email = @p_email,horario = @p_horario,bultos = @p_bultos,kilos = @p_kilos,kilos_covia = @p_kilosCovia,kilos_cliente = @p_kilosCliente,kilos_decimales = @p_kilosDec,m3 = @p_m3,m3_covia = @p_m3Covia,unidades = @p_unidades,condiciones = @p_condiciones,valor = @p_valor,divisavalor = @p_divisaValor,refcli = @p_refCliente,vinculacion = @p_vinculacion,valorvinculacion = @p_valorVinculacion,divisavinculacion = @p_divisaVinculacion,observaciones = @p_observaciones,marcas = @p_marcas,tipobultos = @p_tipoBultos,mercancia = @p_mercancia,codBultos = @p_codBultos,servicio = @p_servicio,fecha_recogida = @p_fechaRecogida,fecha_rendezvous = @p_fechaRVouz,adr_clase = @p_adrClase,adr_onu = @p_adrOnu,adr_embalaje = @p_adrEmbalaje,ent_almfis = @p_entAlmFis,ent_almlog = @p_entAlmLog,ent_referencia = @p_entRef,ent_palet = @p_entPalet,ent_ubicacion = @p_entUbicacion,ent_situacion = @p_entSituacion,xid = @p_xId,xtipo = @p_xTipo,xbuscar = @p_xBuscar,xient = @p_xIent,xcent = @p_xCent,xnom = @p_xNom,xcp = @p_xCp,xpob = @p_xPob,xdir = @p_xDir,xpda = @p_xPda,xidir = @p_xIdir,xtipodir = @p_xTipoDir WHERE idparse = @p_idParse";
            cmdActualizarNuevoParse.Connection = mySqlConnection;

            MySqlParameter p_division = new MySqlParameter();
            p_division.ParameterName = "p_division";
            p_division.Direction = ParameterDirection.Input;
            p_division.MySqlDbType = MySqlDbType.VarChar;
            p_division.Size = 7;
            cmdActualizarNuevoParse.Parameters.Add(p_division);
            p_division.Value = cliente.division;

            MySqlParameter p_nombre = new MySqlParameter();
            p_nombre.ParameterName = "p_nombre";
            p_nombre.Direction = ParameterDirection.Input;
            p_nombre.MySqlDbType = MySqlDbType.VarChar;
            p_nombre.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_nombre);
            p_nombre.Value = cliente.nombre;

            MySqlParameter p_nombrePais = new MySqlParameter();
            p_nombrePais.ParameterName = "p_nombrePais";
            p_nombrePais.Direction = ParameterDirection.Input;
            p_nombrePais.MySqlDbType = MySqlDbType.VarChar;
            p_nombrePais.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_nombrePais);
            p_nombrePais.Value = cliente.nombrePais;

            MySqlParameter p_pais = new MySqlParameter();
            p_pais.ParameterName = "p_pais";
            p_pais.Direction = ParameterDirection.Input;
            p_pais.MySqlDbType = MySqlDbType.VarChar;
            p_pais.Size = 2;
            cmdActualizarNuevoParse.Parameters.Add(p_pais);
            p_pais.Value = cliente.pais;

            MySqlParameter p_delimitador = new MySqlParameter();
            p_delimitador.ParameterName = "p_delimitador";
            p_delimitador.Direction = ParameterDirection.Input;
            p_delimitador.MySqlDbType = MySqlDbType.VarChar;
            p_delimitador.Size = 50;
            cmdActualizarNuevoParse.Parameters.Add(p_delimitador);
            p_delimitador.Value = cliente.delimitador;

            MySqlParameter p_linealInicio = new MySqlParameter();
            p_linealInicio.ParameterName = "p_linealInicio";
            p_linealInicio.Direction = ParameterDirection.Input;
            p_linealInicio.MySqlDbType = MySqlDbType.VarChar;
            p_linealInicio.Size = 4;
            cmdActualizarNuevoParse.Parameters.Add(p_linealInicio);
            p_linealInicio.Value = cliente.linealInicio;

            MySqlParameter p_camposAgrupar = new MySqlParameter();
            p_camposAgrupar.ParameterName = "p_camposAgrupar";
            p_camposAgrupar.Direction = ParameterDirection.Input;
            p_camposAgrupar.MySqlDbType = MySqlDbType.VarChar;
            p_camposAgrupar.Size = 200;
            cmdActualizarNuevoParse.Parameters.Add(p_camposAgrupar);
            p_camposAgrupar.Value = cliente.camposAgrupar;

            MySqlParameter p_trafico = new MySqlParameter();
            p_trafico.ParameterName = "p_trafico";
            p_trafico.Direction = ParameterDirection.Input;
            p_trafico.MySqlDbType = MySqlDbType.VarChar;
            p_trafico.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_trafico);
            p_trafico.Value = cliente.trafico;

            MySqlParameter p_flete = new MySqlParameter();
            p_flete.ParameterName = "p_flete";
            p_flete.Direction = ParameterDirection.Input;
            p_flete.MySqlDbType = MySqlDbType.VarChar;
            p_flete.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_flete);
            p_flete.Value = cliente.flete;

            MySqlParameter p_filtro = new MySqlParameter();
            p_filtro.ParameterName = "p_filtro";
            p_filtro.Direction = ParameterDirection.Input;
            p_filtro.MySqlDbType = MySqlDbType.VarChar;
            p_filtro.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_filtro);
            p_filtro.Value = cliente.filtro;

            MySqlParameter p_idFacturacion = new MySqlParameter();
            p_idFacturacion.ParameterName = "p_idFacturacion";
            p_idFacturacion.Direction = ParameterDirection.Input;
            p_idFacturacion.MySqlDbType = MySqlDbType.VarChar;
            p_idFacturacion.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_idFacturacion);
            p_idFacturacion.Value = cliente.idFacturacion;

            MySqlParameter p_idDireccion = new MySqlParameter();
            p_idDireccion.ParameterName = "p_idDireccion";
            p_idDireccion.Direction = ParameterDirection.Input;
            p_idDireccion.MySqlDbType = MySqlDbType.VarChar;
            p_idDireccion.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_idDireccion);
            p_idDireccion.Value = cliente.idDireccion;

            MySqlParameter p_idOrdenante = new MySqlParameter();
            p_idOrdenante.ParameterName = "p_idOrdenante";
            p_idOrdenante.Direction = ParameterDirection.Input;
            p_idOrdenante.MySqlDbType = MySqlDbType.VarChar;
            p_idOrdenante.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_idOrdenante);
            p_idOrdenante.Value = cliente.idOrdenante;

            MySqlParameter p_idCorresponsal = new MySqlParameter();
            p_idCorresponsal.ParameterName = "p_idCorresponsal";
            p_idCorresponsal.Direction = ParameterDirection.Input;
            p_idCorresponsal.MySqlDbType = MySqlDbType.VarChar;
            p_idCorresponsal.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_idCorresponsal);
            p_idCorresponsal.Value = cliente.idCorresponsal;

            MySqlParameter p_desRem = new MySqlParameter();
            p_desRem.ParameterName = "p_desRem";
            p_desRem.Direction = ParameterDirection.Input;
            p_desRem.MySqlDbType = MySqlDbType.VarChar;
            p_desRem.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_desRem);
            p_desRem.Value = cliente.nombreDesRem;

            MySqlParameter p_direccion = new MySqlParameter();
            p_direccion.ParameterName = "p_direccion";
            p_direccion.Direction = ParameterDirection.Input;
            p_direccion.MySqlDbType = MySqlDbType.VarChar;
            p_direccion.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_direccion);
            p_direccion.Value = cliente.direccion;

            MySqlParameter p_poblacion = new MySqlParameter();
            p_poblacion.ParameterName = "p_poblacion";
            p_poblacion.Direction = ParameterDirection.Input;
            p_poblacion.MySqlDbType = MySqlDbType.VarChar;
            p_poblacion.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_poblacion);
            p_poblacion.Value = cliente.poblacion;

            MySqlParameter p_cp = new MySqlParameter();
            p_cp.ParameterName = "p_cp";
            p_cp.Direction = ParameterDirection.Input;
            p_cp.MySqlDbType = MySqlDbType.VarChar;
            p_cp.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_cp);
            p_cp.Value = cliente.cp;

            MySqlParameter p_telefono = new MySqlParameter();
            p_telefono.ParameterName = "p_telefono";
            p_telefono.Direction = ParameterDirection.Input;
            p_telefono.MySqlDbType = MySqlDbType.VarChar;
            p_telefono.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_telefono);
            p_telefono.Value = cliente.telefono;

            MySqlParameter p_email = new MySqlParameter();
            p_email.ParameterName = "p_email";
            p_email.Direction = ParameterDirection.Input;
            p_email.MySqlDbType = MySqlDbType.VarChar;
            p_email.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_email);
            p_email.Value = cliente.email;

            MySqlParameter p_horario = new MySqlParameter();
            p_horario.ParameterName = "p_horario";
            p_horario.Direction = ParameterDirection.Input;
            p_horario.MySqlDbType = MySqlDbType.VarChar;
            p_horario.Size = 75;
            cmdActualizarNuevoParse.Parameters.Add(p_horario);
            p_horario.Value = cliente.horario;

            MySqlParameter p_bultos = new MySqlParameter();
            p_bultos.ParameterName = "p_bultos";
            p_bultos.Direction = ParameterDirection.Input;
            p_bultos.MySqlDbType = MySqlDbType.VarChar;
            p_bultos.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_bultos);
            p_bultos.Value = cliente.bultos;

            MySqlParameter p_kilos = new MySqlParameter();
            p_kilos.ParameterName = "p_kilos";
            p_kilos.Direction = ParameterDirection.Input;
            p_kilos.MySqlDbType = MySqlDbType.VarChar;
            p_kilos.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_kilos);
            p_kilos.Value = cliente.kilos;

            MySqlParameter p_kilosCovia = new MySqlParameter();
            p_kilosCovia.ParameterName = "p_kilosCovia";
            p_kilosCovia.Direction = ParameterDirection.Input;
            p_kilosCovia.MySqlDbType = MySqlDbType.VarChar;
            p_kilosCovia.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_kilosCovia);
            p_kilosCovia.Value = cliente.kilosCovia;

            MySqlParameter p_kilosCliente = new MySqlParameter();
            p_kilosCliente.ParameterName = "p_kilosCliente";
            p_kilosCliente.Direction = ParameterDirection.Input;
            p_kilosCliente.MySqlDbType = MySqlDbType.VarChar;
            p_kilosCliente.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_kilosCliente);
            p_kilosCliente.Value = cliente.kilosCliente;

            MySqlParameter p_kilosDec = new MySqlParameter();
            p_kilosDec.ParameterName = "p_kilosDec";
            p_kilosDec.Direction = ParameterDirection.Input;
            p_kilosDec.MySqlDbType = MySqlDbType.VarChar;
            p_kilosDec.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_kilosDec);
            p_kilosDec.Value = cliente.kilosDecimales;

            MySqlParameter p_m3 = new MySqlParameter();
            p_m3.ParameterName = "p_m3";
            p_m3.Direction = ParameterDirection.Input;
            p_m3.MySqlDbType = MySqlDbType.VarChar;
            p_m3.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_m3);
            p_m3.Value = cliente.m3;

            MySqlParameter p_m3Covia = new MySqlParameter();
            p_m3Covia.ParameterName = "p_m3Covia";
            p_m3Covia.Direction = ParameterDirection.Input;
            p_m3Covia.MySqlDbType = MySqlDbType.VarChar;
            p_m3Covia.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_m3Covia);
            p_m3Covia.Value = cliente.m3Covia;

            MySqlParameter p_unidades = new MySqlParameter();
            p_unidades.ParameterName = "p_unidades";
            p_unidades.Direction = ParameterDirection.Input;
            p_unidades.MySqlDbType = MySqlDbType.VarChar;
            p_unidades.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_unidades);
            p_unidades.Value = cliente.unidades;

            MySqlParameter p_condiciones = new MySqlParameter();
            p_condiciones.ParameterName = "p_condiciones";
            p_condiciones.Direction = ParameterDirection.Input;
            p_condiciones.MySqlDbType = MySqlDbType.VarChar;
            p_condiciones.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_condiciones);
            p_condiciones.Value = cliente.condiciones;

            MySqlParameter p_valor = new MySqlParameter();
            p_valor.ParameterName = "p_valor";
            p_valor.Direction = ParameterDirection.Input;
            p_valor.MySqlDbType = MySqlDbType.VarChar;
            p_valor.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_valor);
            p_valor.Value = cliente.valor;

            MySqlParameter p_divisaValor = new MySqlParameter();
            p_divisaValor.ParameterName = "p_divisaValor";
            p_divisaValor.Direction = ParameterDirection.Input;
            p_divisaValor.MySqlDbType = MySqlDbType.VarChar;
            p_divisaValor.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_divisaValor);
            p_divisaValor.Value = cliente.divisaValor;

            MySqlParameter p_refCliente = new MySqlParameter();
            p_refCliente.ParameterName = "p_refCliente";
            p_refCliente.Direction = ParameterDirection.Input;
            p_refCliente.MySqlDbType = MySqlDbType.VarChar;
            p_refCliente.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_refCliente);
            p_refCliente.Value = cliente.refCliente;

            MySqlParameter p_vinculacion = new MySqlParameter();
            p_vinculacion.ParameterName = "p_vinculacion";
            p_vinculacion.Direction = ParameterDirection.Input;
            p_vinculacion.MySqlDbType = MySqlDbType.VarChar;
            p_vinculacion.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_vinculacion);
            p_vinculacion.Value = cliente.vinculacion;

            MySqlParameter p_valorVinculacion = new MySqlParameter();
            p_valorVinculacion.ParameterName = "p_valorVinculacion";
            p_valorVinculacion.Direction = ParameterDirection.Input;
            p_valorVinculacion.MySqlDbType = MySqlDbType.VarChar;
            p_valorVinculacion.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_valorVinculacion);
            p_valorVinculacion.Value = cliente.valorVinculacion;

            MySqlParameter p_divisaVinculacion = new MySqlParameter();
            p_divisaVinculacion.ParameterName = "p_divisaVinculacion";
            p_divisaVinculacion.Direction = ParameterDirection.Input;
            p_divisaVinculacion.MySqlDbType = MySqlDbType.VarChar;
            p_divisaVinculacion.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_divisaVinculacion);
            p_divisaVinculacion.Value = cliente.divisaVinculacion;

            MySqlParameter p_observaciones = new MySqlParameter();
            p_observaciones.ParameterName = "p_observaciones";
            p_observaciones.Direction = ParameterDirection.Input;
            p_observaciones.MySqlDbType = MySqlDbType.VarChar;
            p_observaciones.Size = 100;
            cmdActualizarNuevoParse.Parameters.Add(p_observaciones);
            p_observaciones.Value = cliente.observaciones;

            MySqlParameter p_marcas = new MySqlParameter();
            p_marcas.ParameterName = "p_marcas";
            p_marcas.Direction = ParameterDirection.Input;
            p_marcas.MySqlDbType = MySqlDbType.VarChar;
            p_marcas.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_marcas);
            p_marcas.Value = cliente.marcas;

            MySqlParameter p_tipoBultos = new MySqlParameter();
            p_tipoBultos.ParameterName = "p_tipoBultos";
            p_tipoBultos.Direction = ParameterDirection.Input;
            p_tipoBultos.MySqlDbType = MySqlDbType.VarChar;
            p_tipoBultos.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_tipoBultos);
            p_tipoBultos.Value = cliente.tipoBultos;

            MySqlParameter p_mercancia = new MySqlParameter();
            p_mercancia.ParameterName = "p_mercancia";
            p_mercancia.Direction = ParameterDirection.Input;
            p_mercancia.MySqlDbType = MySqlDbType.VarChar;
            p_mercancia.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_mercancia);
            p_mercancia.Value = cliente.mercancia;

            MySqlParameter p_codBultos = new MySqlParameter();
            p_codBultos.ParameterName = "p_codBultos";
            p_codBultos.Direction = ParameterDirection.Input;
            p_codBultos.MySqlDbType = MySqlDbType.VarChar;
            p_codBultos.Size = 200;
            cmdActualizarNuevoParse.Parameters.Add(p_codBultos);
            p_codBultos.Value = cliente.codBultos;

            MySqlParameter p_servicio = new MySqlParameter();
            p_servicio.ParameterName = "p_servicio";
            p_servicio.Direction = ParameterDirection.Input;
            p_servicio.MySqlDbType = MySqlDbType.VarChar;
            p_servicio.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_servicio);
            p_servicio.Value = cliente.servicio;

            MySqlParameter p_fechaRecogida = new MySqlParameter();
            p_fechaRecogida.ParameterName = "p_fechaRecogida";
            p_fechaRecogida.Direction = ParameterDirection.Input;
            p_fechaRecogida.MySqlDbType = MySqlDbType.VarChar;
            p_fechaRecogida.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_fechaRecogida);
            p_fechaRecogida.Value = cliente.fechaRecogida;

            MySqlParameter p_fechaRVouz = new MySqlParameter();
            p_fechaRVouz.ParameterName = "p_fechaRVouz";
            p_fechaRVouz.Direction = ParameterDirection.Input;
            p_fechaRVouz.MySqlDbType = MySqlDbType.VarChar;
            p_fechaRVouz.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_fechaRVouz);
            p_fechaRVouz.Value = cliente.fechaRVouz;

            MySqlParameter p_adrClase = new MySqlParameter();
            p_adrClase.ParameterName = "p_adrClase";
            p_adrClase.Direction = ParameterDirection.Input;
            p_adrClase.MySqlDbType = MySqlDbType.VarChar;
            p_adrClase.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_adrClase);
            p_adrClase.Value = cliente.adrClase;

            MySqlParameter p_adrOnu = new MySqlParameter();
            p_adrOnu.ParameterName = "p_adrOnu";
            p_adrOnu.Direction = ParameterDirection.Input;
            p_adrOnu.MySqlDbType = MySqlDbType.VarChar;
            p_adrOnu.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_adrOnu);
            p_adrOnu.Value = cliente.adrOnu;

            MySqlParameter p_adrEmbalaje = new MySqlParameter();
            p_adrEmbalaje.ParameterName = "p_adrEmbalaje";
            p_adrEmbalaje.Direction = ParameterDirection.Input;
            p_adrEmbalaje.MySqlDbType = MySqlDbType.VarChar;
            p_adrEmbalaje.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_adrEmbalaje);
            p_adrEmbalaje.Value = cliente.adrEmbalaje;

            MySqlParameter p_entAlmFis = new MySqlParameter();
            p_entAlmFis.ParameterName = "p_entAlmFis";
            p_entAlmFis.Direction = ParameterDirection.Input;
            p_entAlmFis.MySqlDbType = MySqlDbType.VarChar;
            p_entAlmFis.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_entAlmFis);
            p_entAlmFis.Value = cliente.entAlmFis;

            MySqlParameter p_entAlmLog = new MySqlParameter();
            p_entAlmLog.ParameterName = "p_entAlmLog";
            p_entAlmLog.Direction = ParameterDirection.Input;
            p_entAlmLog.MySqlDbType = MySqlDbType.VarChar;
            p_entAlmLog.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_entAlmLog);
            p_entAlmLog.Value = cliente.entAlmLog;

            MySqlParameter p_entRef = new MySqlParameter();
            p_entRef.ParameterName = "p_entRef";
            p_entRef.Direction = ParameterDirection.Input;
            p_entRef.MySqlDbType = MySqlDbType.VarChar;
            p_entRef.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_entRef);
            p_entRef.Value = cliente.entReferencia;

            MySqlParameter p_entPalet = new MySqlParameter();
            p_entPalet.ParameterName = "p_entPalet";
            p_entPalet.Direction = ParameterDirection.Input;
            p_entPalet.MySqlDbType = MySqlDbType.VarChar;
            p_entPalet.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_entPalet);
            p_entPalet.Value = cliente.entPalet;

            MySqlParameter p_entEmbalaje = new MySqlParameter();
            p_entEmbalaje.ParameterName = "p_entEmbalaje";
            p_entEmbalaje.Direction = ParameterDirection.Input;
            p_entEmbalaje.MySqlDbType = MySqlDbType.VarChar;
            p_entEmbalaje.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_entEmbalaje);
            p_entEmbalaje.Value = cliente.entEmbalaje;


            MySqlParameter p_entUbicacion = new MySqlParameter();
            p_entUbicacion.ParameterName = "p_entUbicacion";
            p_entUbicacion.Direction = ParameterDirection.Input;
            p_entUbicacion.MySqlDbType = MySqlDbType.VarChar;
            p_entUbicacion.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_entUbicacion);
            p_entUbicacion.Value = cliente.entUbicacion;

            MySqlParameter p_entSituacion = new MySqlParameter();
            p_entSituacion.ParameterName = "p_entSituacion";
            p_entSituacion.Direction = ParameterDirection.Input;
            p_entSituacion.MySqlDbType = MySqlDbType.VarChar;
            p_entSituacion.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_entSituacion);
            p_entSituacion.Value = cliente.entSituacion;

            MySqlParameter p_xId = new MySqlParameter();
            p_xId.ParameterName = "p_xId";
            p_xId.Direction = ParameterDirection.Input;
            p_xId.MySqlDbType = MySqlDbType.VarChar;
            p_xId.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_xId);
            p_xId.Value = cliente.xId;

            MySqlParameter p_xTipo = new MySqlParameter();
            p_xTipo.ParameterName = "p_xTipo";
            p_xTipo.Direction = ParameterDirection.Input;
            p_xTipo.MySqlDbType = MySqlDbType.VarChar;
            p_xTipo.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_xTipo);
            p_xTipo.Value = cliente.xTipo;

            MySqlParameter p_xBuscar = new MySqlParameter();
            p_xBuscar.ParameterName = "p_xBuscar";
            p_xBuscar.Direction = ParameterDirection.Input;
            p_xBuscar.MySqlDbType = MySqlDbType.VarChar;
            p_xBuscar.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_xBuscar);
            p_xBuscar.Value = cliente.xBuscar;

            MySqlParameter p_xIent = new MySqlParameter();
            p_xIent.ParameterName = "p_xIent";
            p_xIent.Direction = ParameterDirection.Input;
            p_xIent.MySqlDbType = MySqlDbType.VarChar;
            p_xIent.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_xIent);
            p_xIent.Value = cliente.xIent;

            MySqlParameter p_xCent = new MySqlParameter();
            p_xCent.ParameterName = "p_xCent";
            p_xCent.Direction = ParameterDirection.Input;
            p_xCent.MySqlDbType = MySqlDbType.VarChar;
            p_xCent.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_xCent);
            p_xCent.Value = cliente.xCent;

            MySqlParameter p_xNom = new MySqlParameter();
            p_xNom.ParameterName = "p_xNom";
            p_xNom.Direction = ParameterDirection.Input;
            p_xNom.MySqlDbType = MySqlDbType.VarChar;
            p_xNom.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_xNom);
            p_xNom.Value = cliente.xNom;

            MySqlParameter p_xCp = new MySqlParameter();
            p_xCp.ParameterName = "p_xCp";
            p_xCp.Direction = ParameterDirection.Input;
            p_xCp.MySqlDbType = MySqlDbType.VarChar;
            p_xCp.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_xCp);
            p_xCp.Value = cliente.xCp;

            MySqlParameter p_xPob = new MySqlParameter();
            p_xPob.ParameterName = "p_xPob";
            p_xPob.Direction = ParameterDirection.Input;
            p_xPob.MySqlDbType = MySqlDbType.VarChar;
            p_xPob.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_xPob);
            p_xPob.Value = cliente.xPob;

            MySqlParameter p_xDir = new MySqlParameter();
            p_xDir.ParameterName = "p_xDir";
            p_xDir.Direction = ParameterDirection.Input;
            p_xDir.MySqlDbType = MySqlDbType.VarChar;
            p_xDir.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_xDir);
            p_xDir.Value = cliente.xDir;

            MySqlParameter p_xPda = new MySqlParameter();
            p_xPda.ParameterName = "p_xPda";
            p_xPda.Direction = ParameterDirection.Input;
            p_xPda.MySqlDbType = MySqlDbType.VarChar;
            p_xPda.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_xPda);
            p_xPda.Value = cliente.xPda;

            MySqlParameter p_xIdir = new MySqlParameter();
            p_xIdir.ParameterName = "p_xIdir";
            p_xIdir.Direction = ParameterDirection.Input;
            p_xIdir.MySqlDbType = MySqlDbType.VarChar;
            p_xIdir.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_xIdir);
            p_xIdir.Value = cliente.xIDir;

            MySqlParameter p_xTipoDir = new MySqlParameter();
            p_xTipoDir.ParameterName = "p_xTipoDir";
            p_xTipoDir.Direction = ParameterDirection.Input;
            p_xTipoDir.MySqlDbType = MySqlDbType.VarChar;
            p_xTipoDir.Size = 25;
            cmdActualizarNuevoParse.Parameters.Add(p_xTipoDir);
            p_xTipoDir.Value = cliente.xTipoDir;

            MySqlParameter p_idParse = new MySqlParameter();
            p_idParse.ParameterName = "p_idParse";
            p_idParse.MySqlDbType = MySqlDbType.Int32;
            p_idParse.Direction = ParameterDirection.Input;
            cmdActualizarNuevoParse.Parameters.Add(p_idParse);
            p_idParse.Value = cliente.idParse;

            using (mySqlConnection)
            {
                mySqlConnection.Open();
                cmdActualizarNuevoParse.ExecuteNonQuery();
            }
        }
    }
}
