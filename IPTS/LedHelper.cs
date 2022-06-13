using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DragonFactory.TcpIPClient;
using TraceTool;

namespace IPTS
{
    public class LedHelper
    {
        public LedHelper()
        {

        }
        WinTrace _trace = new WinTrace("LedTrace", "LedTrace");
        ICustomClient _ledClient;

        public bool StartLedConnection(string ip, string port, EventHandler conn, EventHandler disconn)
        {
            CloseLedConnection();
            try
            {
                _ledClient = CustomClientFactory.CreateClient(new CustomClientTcpEndPoint(ip, int.Parse(port)));
                _ledClient.PingAndDisconnectInterval = 5000;
                _ledClient.ReconnectInterval = 5000;
                _ledClient.Connected += conn;
                _ledClient.Disconnected += disconn;
                _ledClient.TryToConnectContinuosly();
                return true;
            }
            catch (Exception ex)
            {
                _trace.SendValue("CloseLedConnection", ex);
                return false;
            }

        }



        public void CloseLedConnection()
        {
            try
            {
                if (_ledClient != null)
                {
                    _ledClient.Disconnect();
                }
            }
            catch (Exception ex)
            {
                _trace.SendValue("CloseLedConnection", ex);
            }
        }
        public enum LedLocation
        {
            TopLeft = 0,
            BottomLeft = 1,
            //BiColorMode = 2
        }

        public enum MessageDisplayMode
        {
            Constant = 1,
            Running = 2,
            Flash = 3
        }
        private readonly static string _topLeftId = "TA";
        private readonly static string _bottomLeftId = "GB";
        public bool SendToLedDisplay(LedLocation location, MessageDisplayMode mode, string message)
        {
            try
            {
                _trace.Send("Received - Location:" + location + " Mode:" + mode + " Message:" + message);
                if (_ledClient == null || _ledClient.CommunicationState == ClientCommunicationStates.Disconnected)
                {
                    return false;
                }

                if (location == LedLocation.TopLeft)
                {
                    switch (mode)
                    {
                        case MessageDisplayMode.Running:
                            _ledClient.SendMessage(_topLeftId + ((int)mode).ToString() + message + "~");
                            _trace.Send(_topLeftId + ((int)mode).ToString() + message + "~");
                            break;
                        case MessageDisplayMode.Flash:
                            _ledClient.SendMessage(_topLeftId + ((int)mode).ToString() + message + "~");
                            _trace.Send(_topLeftId + ((int)mode).ToString() + message + "~");
                            break;
                        case MessageDisplayMode.Constant:
                            _ledClient.SendMessage(_topLeftId + ((int)mode).ToString() + message + "~");
                            _trace.Send(_topLeftId + ((int)mode).ToString() + message + "~");
                            break;
                    }
                }
                else
                {
                    switch (mode)
                    {
                        case MessageDisplayMode.Running:
                            _ledClient.SendMessage(_bottomLeftId + ((int)mode).ToString() + message + "~");
                            _trace.Send(_bottomLeftId + ((int)mode).ToString() + message + "~");
                            break;
                        case MessageDisplayMode.Flash:
                            _ledClient.SendMessage(_bottomLeftId + ((int)mode).ToString() + message + "~");
                            _trace.Send(_bottomLeftId + ((int)mode).ToString() + message + "~");
                            break;
                        case MessageDisplayMode.Constant:
                            _ledClient.SendMessage(_bottomLeftId + ((int)mode).ToString() + message + "~");
                            _trace.Send(_bottomLeftId + ((int)mode).ToString() + message + "~");
                            break;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _trace.SendValue("SendToLedDisplay", ex);
                return false;
                //throw;
            }
        }

        public bool SendToSiren(Siran soundformat)
        {
            try
            {
                if (_ledClient.CommunicationState == ClientCommunicationStates.Disconnected)
                {
                    return false;
                }

                switch (soundformat)
                {
                    case Siran.TenSeconds:
                        _ledClient.SendMessage("GCA~");
                        break;
                    case Siran.TwentySeconds:
                        _ledClient.SendMessage("GCB~");
                        break;
                    case Siran.FortySeconds:
                        _ledClient.SendMessage("GCC~");
                        break;
                    case Siran.Continous:
                        _ledClient.SendMessage("GCD~");
                        break;
                    case Siran.Stop:
                        _ledClient.SendMessage("GCE~");
                        break;
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
                //throw;
            }
        }

        public enum Siran
        {
            TenSeconds = 0,
            TwentySeconds = 1,
            FortySeconds = 2,
            Continous = 3,
            Stop = 4
        }

        public enum BiColorBoardMessage
        {
            CompleteGreen = 0,
            CompleteRed = 1,
            GreenTickMark = 2,
            RedCrossMark = 3
        }

        public bool SendToBiColorBoard(BiColorBoardMessage msgformat)
        {
            try
            {
                if (_ledClient == null || _ledClient.CommunicationState == ClientCommunicationStates.Disconnected)
                {
                    return false;
                }

                switch (msgformat)
                {
                    case BiColorBoardMessage.CompleteGreen:
                        _ledClient.SendMessage("GCG~");
                        break;
                    case BiColorBoardMessage.CompleteRed:
                        _ledClient.SendMessage("GCR~");
                        break;
                    case BiColorBoardMessage.GreenTickMark:
                        _ledClient.SendMessage("GCV~");
                        break;
                    case BiColorBoardMessage.RedCrossMark:
                        _ledClient.SendMessage("GCI~");
                        break;
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
                //throw;
            }
        }
    }
}
