using JWptz.Entities;
using JWptz.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace JWptz.Services
{
    public static class APIs
    {
        public static async Task<APIBaseResponse> SendCommand(PTZCommand ptzCommand)
        {
            try
            {
                if (ptzCommand.CommandType == CommandType.None)
                { throw new Exception("The 'CommandType' parameter is set to 'None'."); }

                using HttpClient client = new HttpClient();
                string endpoint = GetCameraEndpoint(ptzCommand);

                HttpResponseMessage response = await client.GetAsync(endpoint);
                string responseBody = await response.Content.ReadAsStringAsync();

                response.EnsureSuccessStatusCode();
                return new APIBaseResponse(successful: true, message: responseBody);
            }
            catch (Exception ex)
            {
                Helpers.WriteLog(exception: ex);
                return new APIBaseResponse(ex);
            }
        }

        public static async Task<APIImageResponse> GetSnapshot(PTZCamera ptzCamera)
        {
            try
            {
                using HttpClient client = new HttpClient();
                string endpoint = GetSnapshotEndpoint(ptzCamera);

                byte[] imageBytes = await client.GetByteArrayAsync(endpoint);
                Image image = Helpers.ByteArrayToImage(imageBytes);

                return new APIImageResponse(image);
            }
            catch (Exception ex)
            {
                Helpers.WriteLog(exception: ex);
                return new APIImageResponse(ex);
            }
        }

        public static string GetCameraEndpoint(PTZCommand ptzCommand)
        {
            string apiControlPath = "cgi-bin/ptzctrl.cgi?ptzcmd";
            string apiParamPath = "cgi-bin/param.cgi";
            string apiSettingsPath = "cgi-bin/ptzctrl.cgi";
            string ptSpeeds = $"{ptzCommand.PTZFSpeeds.PanSpeed}&{ptzCommand.PTZFSpeeds.TiltSpeed}";
            string result = $"{ptzCommand.Camera.ProtocolType}://{ptzCommand.Camera.IP}/";

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
                case CommandType.ActivateSnapFocus: result += $"{apiParamPath}?set_focus&one_shot_focus"; break;
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
            return $"{ptzCamera.ProtocolType}://{ptzCamera.IP}/snapshot.jpg";
        }
    }
}
