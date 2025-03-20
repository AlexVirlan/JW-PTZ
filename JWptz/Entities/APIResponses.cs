using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace JWptz.Entities
{
    public class APIBaseResponse
    {
        #region Properties
        public bool Successful { get; set; } = true;
        public string Message { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public HttpStatusCode? StatusCode { get; set; }
        #endregion

        #region Constructors
        public APIBaseResponse() { }

        public APIBaseResponse(bool successful)
        {
            Successful = successful;
        }

        public APIBaseResponse(bool successful, string message)
        {
            Successful = successful;
            Message = message;
        }

        public APIBaseResponse(Exception exception)
        {
            Successful = false;
            Message = exception.Message;
        }
        #endregion
    }

    public class APIImageResponse : APIBaseResponse
    {
        #region Properties
        public BitmapImage? BitmapImage { get; set; }
        #endregion

        #region Constructors
        public APIImageResponse(BitmapImage bitmapImage)
        {
            Successful = true;
            BitmapImage = bitmapImage;
        }

        public APIImageResponse(Exception exception)
        {
            Successful = false;
            Message = exception.Message;
        }
        #endregion
    }
}
