using JWptz.Entities;
using JWptz.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace JWptz.Services
{
    public static class APIs
    {
        public static async Task<APIBaseResponse> SendCommand(PTZCommand ptzCommand)
        {
            string endpoint = string.Empty;
            HttpStatusCode? statusCode = null;
            try
            {
                #region Validations
                if (ptzCommand.CommandType == CommandType.None)
                { throw new Exception("The 'CommandType' parameter is set to 'None'."); }

                if (ptzCommand.Camera.IP.INOE())
                { throw new Exception("The camera IP is not set."); }
                #endregion

                using HttpClient client = new HttpClient();
                SetAuthorization(client, ptzCommand.Camera);
                client.Timeout = TimeSpan.FromMilliseconds(Settings.CommandTimeout);
                endpoint = GetCameraEndpoint(ptzCommand);

                HttpResponseMessage response = await client.GetAsync(endpoint);
                string responseBody = await response.Content.ReadAsStringAsync();
                statusCode = response.StatusCode;

                response.EnsureSuccessStatusCode();
                return new APIBaseResponse(successful: true, message: responseBody)
                {
                    Endpoint = endpoint,
                    StatusCode = statusCode
                };
            }
            catch (Exception ex)
            {
                Helpers.WriteLog(exception: ex);
                return new APIBaseResponse(ex)
                {
                    Endpoint = endpoint,
                    StatusCode = statusCode
                };
            }
        }

        public static async Task<APIImageResponse> GetSnapshot(PTZCamera ptzCamera)
        {
            string endpoint = string.Empty;
            HttpStatusCode? statusCode = null;
            try
            {
                using HttpClient client = new HttpClient();
                SetAuthorization(client, ptzCamera);
                client.Timeout = TimeSpan.FromMilliseconds(Settings.CommandTimeout);
                endpoint = GetSnapshotEndpoint(ptzCamera);

                HttpResponseMessage response = await client.GetAsync(endpoint);
                byte[] contentBytes = await response.Content.ReadAsByteArrayAsync();
                statusCode = response.StatusCode;

                response.EnsureSuccessStatusCode();
                BitmapImage bitmapImage = Helpers.ByteArrayToBitmapImage(contentBytes);

                return new APIImageResponse(bitmapImage)
                {
                    Endpoint = endpoint,
                    StatusCode = statusCode
                };
            }
            catch (Exception ex)
            {
                Helpers.WriteLog(exception: ex);
                return new APIImageResponse(ex)
                {
                    Endpoint = endpoint,
                    StatusCode = statusCode
                };
            }
        }

        public static void SetAuthorization(HttpClient httpClient, PTZCamera ptzCamera)
        {
            if (ptzCamera.UseAuth)
            {
                if (ptzCamera.Username.INOE()) { throw new Exception("The username field for authorization is null or empty."); }
                if (ptzCamera.Password.INOE()) { throw new Exception("The password field for authorization is null or empty."); }

                string authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{ptzCamera.Username}:{ptzCamera.Password}"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
            }
        }

        public static string GetCameraEndpoint(PTZCommand ptzCommand)
        {
            string apiControlPath = "cgi-bin/ptzctrl.cgi?ptzcmd";
            string apiParamPath = "cgi-bin/param.cgi";
            string apiSettingsPath = "cgi-bin/ptzctrl.cgi";
            string ptSpeeds = $"{ptzCommand.PTZFSpeeds.PanSpeed}&{ptzCommand.PTZFSpeeds.TiltSpeed}";
            string result = $"{ptzCommand.Camera.ProtocolType.ToLowerString()}://{ptzCommand.Camera.IP}/";

            switch (ptzCommand.CommandType)
            {
                case CommandType.PanLeft: result += $"{apiControlPath}&left&{ptSpeeds}"; break;
                case CommandType.PanRight: result += $"{apiControlPath}&right&{ptSpeeds}"; break;
                case CommandType.TiltUp: result += $"{apiControlPath}&up&{ptSpeeds}"; break;
                case CommandType.TiltDown: result += $"{apiControlPath}&down&{ptSpeeds}"; break;
                case CommandType.PanTiltStop: result += $"{apiControlPath}&ptzstop&0&0"; break;
                case CommandType.PanLeftTiltUp: result += $"{apiControlPath}&leftup&{ptSpeeds}"; break;
                case CommandType.PanRightTiltUp: result += $"{apiControlPath}&rightup&{ptSpeeds}"; break;
                case CommandType.PanLeftTiltDown: result += $"{apiControlPath}&leftdown&{ptSpeeds}"; break;
                case CommandType.PanRightTiltDown: result += $"{apiControlPath}&rightdown&{ptSpeeds}"; break;
                case CommandType.PanTiltFine: result += $"{apiControlPath}&rel&{ptSpeeds}"; break; // &{currentPanPosition}&{currentTiltPosition} !!!
                case CommandType.PanTiltReset: result += $"{apiParamPath}?pan_tiltdrive_reset"; break;
                case CommandType.ZoomIn: result += $"{apiControlPath}&zoomin&{ptzCommand.PTZFSpeeds.ZoomSpeed}"; break;
                case CommandType.ZoomOut: result += $"{apiControlPath}&zoomout&{ptzCommand.PTZFSpeeds.ZoomSpeed}"; break;
                case CommandType.ZoomStop: result += $"{apiControlPath}&zoomstop&0"; break;
                case CommandType.ZoomFine: result += $"{apiControlPath}&zoomto&{ptzCommand.PTZFSpeeds.ZoomSpeed}"; break; // &currentZoomPosition !!!
                case CommandType.FocusIn: result += $"{apiControlPath}&focusin&{ptzCommand.PTZFSpeeds.FocusSpeed}"; break;
                case CommandType.FocusOut: result += $"{apiControlPath}&focusout&{ptzCommand.PTZFSpeeds.FocusSpeed}"; break;
                case CommandType.FocusStop: result += $"{apiControlPath}&focusstop&0"; break;
                case CommandType.ActivateAutoFocus: result += $"{apiParamPath}?set_focus&one_shot_focus"; break;
                case CommandType.GoHome: result += $"{apiControlPath}&home"; break;
                case CommandType.SetPreset: result += $"{apiControlPath}&posset&{ptzCommand.Preset}"; break;
                case CommandType.CallPreset: result += $"{apiControlPath}&poscall&{ptzCommand.Preset}"; break;

                case CommandType.AdjustLuminance: result += $"{apiSettingsPath}?post_image_value&luminance&{ptzCommand.ImageSettings.Luminance}"; break;
                case CommandType.AdjustSaturation: result += $"{apiSettingsPath}?post_image_value&saturation&{ptzCommand.ImageSettings.Saturation}"; break;
                case CommandType.AdjustContrast: result += $"{apiSettingsPath}?post_image_value&contrast&{ptzCommand.ImageSettings.Contrast}"; break;
                case CommandType.AdjustSharpness: result += $"{apiSettingsPath}?post_image_value&sharpness&{ptzCommand.ImageSettings.Sharpness}"; break;
                case CommandType.AdjustHue: result += $"{apiSettingsPath}?post_image_value&hue&{ptzCommand.ImageSettings.Hue}"; break;

                case CommandType.None:
                default: result = string.Empty; break;
            }

            return result;
        }

        public static string GetSnapshotEndpoint(PTZCamera ptzCamera)
        {
            return $"{ptzCamera.ProtocolType.ToLowerString()}://{ptzCamera.IP}/snapshot.jpg";
        }

        public static CommandType GetStopCommandType(CommandType commandType)
        {
            switch (commandType)
            {
                case CommandType.PanLeft:
                case CommandType.PanRight:
                case CommandType.TiltUp:
                case CommandType.TiltDown:
                case CommandType.PanLeftTiltUp:
                case CommandType.PanRightTiltUp:
                case CommandType.PanLeftTiltDown:
                case CommandType.PanRightTiltDown:
                case CommandType.PanTiltFine:
                case CommandType.PanTiltReset:
                    return CommandType.PanTiltStop;

                case CommandType.ZoomIn:
                case CommandType.ZoomOut:
                case CommandType.ZoomFine:
                    return CommandType.ZoomStop;

                case CommandType.FocusIn:
                case CommandType.FocusOut:
                    return CommandType.FocusStop;

                default: return CommandType.None;
            }
        }

        public static string GetCommandParams(PTZCommand ptzCommand)
        {
            string ptSpeeds = $"{ptzCommand.PTZFSpeeds.PanSpeed}, {ptzCommand.PTZFSpeeds.TiltSpeed}";
            string result = string.Empty;

            switch (ptzCommand.CommandType)
            {
                case CommandType.PanLeft:
                case CommandType.PanRight:
                case CommandType.TiltUp:
                case CommandType.TiltDown: result = ptSpeeds; break;

                case CommandType.PanTiltStop: result = "0, 0"; break;

                case CommandType.PanLeftTiltUp:
                case CommandType.PanRightTiltUp:
                case CommandType.PanLeftTiltDown:
                case CommandType.PanRightTiltDown:
                case CommandType.PanTiltFine: result = ptSpeeds; break;

                case CommandType.PanTiltReset: result = ""; break;

                case CommandType.ZoomIn:
                case CommandType.ZoomOut: result = $"{ptzCommand.PTZFSpeeds.ZoomSpeed}"; break;

                case CommandType.ZoomStop: result = "0"; break;

                case CommandType.ZoomFine: result = $"{ptzCommand.PTZFSpeeds.ZoomSpeed}"; break;

                case CommandType.FocusIn:
                case CommandType.FocusOut: result = $"{ptzCommand.PTZFSpeeds.FocusSpeed}"; break;

                case CommandType.FocusStop: result = "0"; break;

                case CommandType.ActivateAutoFocus:
                case CommandType.GoHome: result = ""; break;

                case CommandType.SetPreset:
                case CommandType.CallPreset: result = $"{ptzCommand.Preset}"; break;

                case CommandType.AdjustLuminance: result = $"{ptzCommand.ImageSettings.Luminance}"; break;
                case CommandType.AdjustSaturation: result = $"{ptzCommand.ImageSettings.Saturation}"; break;
                case CommandType.AdjustContrast: result = $"{ptzCommand.ImageSettings.Contrast}"; break;
                case CommandType.AdjustSharpness: result = $"{ptzCommand.ImageSettings.Sharpness}"; break;
                case CommandType.AdjustHue: result = $"{ptzCommand.ImageSettings.Hue}"; break;

                default: result = string.Empty; break;
            }

            return result;
        }
    }
}
