using System;
using System.Runtime.InteropServices;

namespace Dynv6Updater.Helpers
{
    /// <summary>
    /// Enumeracion con los diferentes estados de un servicio en WinApi32
    /// </summary>
    public enum ServiceState
    {
        SERVICE_STOPPED = 0x00000001,
        SERVICE_START_PENDING = 0x00000002,
        SERVICE_STOP_PENDING = 0x00000003,
        SERVICE_RUNNING = 0x00000004,
        SERVICE_CONTINUE_PENDING = 0x00000005,
        SERVICE_PAUSE_PENDING = 0x00000006,
        SERVICE_PAUSED = 0x00000007,
    }

    /// <summary>
    /// Estructura para declarar el estado de un servicio en WinApi32
    /// </summary>
    /// <see cref="https://docs.microsoft.com/es-es/windows/desktop/api/winsvc/ns-winsvc-_service_status"/>
    [StructLayout(LayoutKind.Sequential)]
    public struct ServiceStatus
    {
        public int dwServiceType;
        public ServiceState dwCurrentState;
        public int dwControlsAccepted;
        public int dwWin32ExitCode;
        public int dwServiceSpecificExitCode;
        public int dwCheckPoint;
        public int dwWaitHint;
    };

    /// <summary>
    /// Clase con funciones de ayuda para acceder a WinApi32
    /// </summary>
    public static class WinApi32Helpers
    {
        /// <summary>
        /// Funcion WinApi32 para establecer el estado de un servicio y que el ServiceManager de Windows tenga constancia.
        /// </summary>
        /// <param name="handle">handle del servicio</param>
        /// <param name="serviceStatus">Estado del servicio</param>
        /// <returns>True si la llamada es correcta, false si ocurre un error</returns>
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);
    }
}
