using System;
using System.Net.Sockets;
using Modbus.Device;

namespace WagoControl
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Variables declaradas, dirección IP de Wago y puerto de comunicación
            string ipAddress = "192.168.100.19";
            int port = 502;

            // Creamos la instancia del cliente Modbus TCP
            TcpClient client = new TcpClient(ipAddress, port);
            ModbusIpMaster master = ModbusIpMaster.CreateIp(client);

            // Direcciones de las entradas y salidas del Wago PFC200
            ushort inputStartAddress = 0; // Dirección de inicio de las entradas
            ushort outputStartAddress = 0; // Dirección de inicio de las salidas

            try
            {
                while (true)
                {
                    // Leer el estado de las entradas
                    bool[] inputs = master.ReadInputs(1, inputStartAddress, 16); // Se leen 16 entradas a partir de la dirección 0

                    //Tiempo entre cada escaneo 
                    Thread.Sleep(1000);

                    // Verificar si alguna entrada está en 'true'
                    if (inputs.Length >= 1 && inputs[0] && inputs[1] && inputs[2])
                    {
                        // Si la entrada "0" está activa, escribir en la salida 0
                        await master.WriteSingleCoilAsync(0, true);
                        // Si la entrada "1" está activa, escribir en la salida 1
                        await master.WriteSingleCoilAsync(1, true);
                        // Si la entrada "2" está activa, escribir en la salida 2
                        await master.WriteSingleCoilAsync(2, true);
                        // Se imprime el estado de las salidas activadas
                        Console.WriteLine(" Sensor de presencia activo " + " " + " Paro de emergencia Desactivado " + " " + " electrovalvula activada ");
                    }
                    // Si la entrda 0 se encuentra activa se continua el proceso
                    if (inputs.Length >= 1 && inputs[0] && inputs[1] && inputs[2])
                    {
                        Console.WriteLine("Podemos continuar con el proceso");
                    }
                    else
                    {
                        //Si la salida 0 dejo de estar activa se apaga la salida 0
                        await master.WriteSingleCoilAsync(0, false);
                        await master.WriteSingleCoilAsync(1, false);
                        await master.WriteSingleCoilAsync(2, false);
                        Console.WriteLine("Proceso detenido por falla de sensor o paro de emergencia activado");
                    }
                    // Leer el estado de las salidas
                    bool[] outputs = master.ReadCoils(1, outputStartAddress, 8); // Se leen 8 salidas a partir de la dirección 1000


                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}