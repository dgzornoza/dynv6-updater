using Dynv6Updater.Helpers;
using Dynv6Updater.Services;
using log4net;
using System;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace Dynv6Updater
{
    public partial class Win32Service : ServiceBase
    {
        #region [Variables miembro]

        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IUpdaterService _updaterService;


        /// <summary>Variable con el objeto timer para ejecutar el proceso cada x tiempo</summary>
        private System.Timers.Timer _timer;

        /// <summary>variable para guardar el intervalo en el que sera lanzado el proceso (en minutos)</summary>
        private int _processInterval;
        /// <summary>variable para guardar la hora de inicio que sera lanzado el proceso</summary>
        private DateTime _processStartTime;
        /// <summary>flag para indicar que es la primera vez que se ejecuta el timer con el tiempo de inicializacion</summary>
        private bool _flagFirstStartTime;
        /// <summary>flag indicando si existe un proceso en ejecucion actualmente</summary>
        private bool _isBusy;

        /// <summary>
        /// constante para guardar el tiempo por defecto para iniciarse la primera vez (en minutos).
        /// NOTA: valor en minutos
        /// </summary>
        private const int DEFAULT_FIRST_TIME = 1;
        /// <summary>
        /// constante para guardar el intervalo por defecto en el que se ejecutara el proceso
        /// Sera usado si no existe alguno en la configuracion 'PROCESS_INTERVAL' o si este no es correcto.
        /// NOTA: valor en minutos
        /// </summary>
        private const int DEFAULT_INTERVAL = 60 * 24;

        #endregion [Variables miembro]



        #region [Inicializacion]

        public Win32Service(IUpdaterService updaterService)
        {
            InitializeComponent();

            this._updaterService = updaterService;

            // fecha inicio del proceso
            this._processStartTime = DateTime.Now.AddMinutes(DEFAULT_FIRST_TIME);

            // obtener el intervalo del archivo de configuracion (si no existe, sera el intervalo por defecto 'DEFAULT_INTERVAL')
            // NOTA: para facilitar la insercion, el valor del archivo de configuracion se consideran minutos, de modo que se multiplica por 1000 * 60
            // para crear los milisegundos del timer.
            string interval = System.Configuration.ConfigurationManager.AppSettings["PROCESS_INTERVAL"];
            this._processInterval = 1000 * 60 * ((string.IsNullOrWhiteSpace(interval)) ? DEFAULT_INTERVAL : Convert.ToInt32(interval));

            // log
            _log.Info($"Servicio configurado para ejecutarse con los siguientes parámetros" +
                $"Primera ejecución: {this._processStartTime}" +
                $"Siguientes ejecuciones en intervalos de: {this._processInterval / 60 / 1000} minutos."

                );

            // Inicializar el timer
            this._timer = new System.Timers.Timer();
            this._timer.Elapsed += new System.Timers.ElapsedEventHandler(Timer_Tick);
        }

        #endregion [Inicializacion]



        #region [Metodos de ServiceBase]

        /// <summary>
        /// Evento ocurrido al iniciarse el servicio de windows
        /// </summary>
        /// <param name="args">Argumentos de inicio</param>
        protected override void OnStart(string[] args)
        {
            _log.Info($"Iniciado el servicio '{nameof(Win32Service)}'");

#if DEBUG
            // En depuracion se inicia directamente
            this.Timer_Tick(null, null);
#else

            // Actualizar el estado del servicio en el ServiceManager de windows a inicio Pendiente
            ServiceStatus serviceStatus = new ServiceStatus { dwCurrentState = ServiceState.SERVICE_START_PENDING, dwWaitHint = 100000 };
            WinApi32Helpers.SetServiceStatus(this.ServiceHandle, ref serviceStatus);


            // la primera vez que arranca el servicio, se inicia el proceso en la hora especificada de inicio y se guarda el flag
            // NOTA: si la hora especificada es menor que la hora actual mas el tiempo minimo por defecto, se usa la hora del dia siguiente
            double interval = (this._processStartTime.TimeOfDay - DateTime.Now.AddMinutes(DEFAULT_FIRST_TIME).TimeOfDay).TotalMinutes;
            _timer.Interval = 1000 * 60 * ((interval < 0) ? (this._processStartTime.TimeOfDay.Add(new TimeSpan(1, 0, 0, 0)) - DateTime.Now.TimeOfDay).TotalMinutes : interval);
            _flagFirstStartTime = true;

            // iniciar el timer
            _timer.Start();

            // Actualizar el estado del servicio en el ServiceManager de windows a ejecutandose
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            WinApi32Helpers.SetServiceStatus(this.ServiceHandle, ref serviceStatus);
#endif

        }

        /// <summary>
        /// Evento ocurrido al detenerse el servicio de windows
        /// </summary>
        protected override void OnStop()
        {
            _log.Info($"Detenido el servicio '{nameof(Win32Service)}'");

            // Actualizar el estado del servicio en el ServiceManager de windows a inicio Pendiente
            ServiceStatus serviceStatus = new ServiceStatus { dwCurrentState = ServiceState.SERVICE_START_PENDING, dwWaitHint = 100000 };
            WinApi32Helpers.SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            // detener el timer
            _timer.Stop();

            // Actualizar el estado del servicio en el ServiceManager de windows a ejecutandose
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            WinApi32Helpers.SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        /// <summary>
        /// Evento ocurrido al pausarse el servicio de windows        
        /// </summary>
        protected override void OnPause()
        {
            _log.Info($"Pausado el servicio '{nameof(Win32Service)}'");

            // detener el timer
            _timer.Stop();
        }

        /// <summary>
        /// Evento ocurrido al reanudar el servicio de windows        
        /// </summary>
        protected override void OnContinue()
        {
            _log.Info($"Reanudando el servicio '{nameof(Win32Service)}'");

            // iniciar el timer
            _timer.Start();
        }

        /// <summary>
        /// Evento ocurrido al apagar el servicio de windows        
        /// </summary>
        protected override void OnShutdown()
        {
            _log.Info($"Apagado el servicio '{nameof(Win32Service)}'");

            // detener el timer
            _timer.Stop();
        }

        #endregion [Metodos de ServiceBase]



        #region [Eventos]

        /// <summary>
        /// Evento ocurrido en cada Tick del timer principal, en este evento, se accede a toda la logica del servicio para la actualizacion de datos desde SAP
        /// </summary>
        /// <param name="sender">objeto remitente</param>
        /// <param name="e">argumentos del evento</param>
        private async void Timer_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            // quitar el flag de tiempo de inicio del timer para comenzar a usar el tiempo del intervalo configurado
            if (this._flagFirstStartTime)
            {
                // establecer el intervalo del timer real
                this._flagFirstStartTime = false;
                this._timer.Interval = this._processInterval;
            }

            // ejecutar el proceso
            if (!this._isBusy)
            {
                this._isBusy = true;
                await this.Execute();
                this._isBusy = false;
            }
        }

        #endregion [Eventos]



        #region [funciones privadas]

        private async Task Execute()
        {
            try
            {
                _log.Info("Actualizando IP ....");

                await _updaterService.UpdateIp();

                _log.Info("IP actualizada");
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
            
        }

        #endregion [funciones privadas]
    }
}
