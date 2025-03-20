using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JWptz.Entities
{
    public class APIBaseResponse
    {
        #region Properties
        public bool Successful { get; set; } = true;
        public string Message { get; set; } = string.Empty;
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
        public Image? Image { get; set; }
        #endregion

        #region Constructors
        public APIImageResponse(Image? image)
        {
            Successful = image is null ? false : true;
            Image = image;
        }

        public APIImageResponse(Exception exception)
        {
            Successful = false;
            Message = exception.Message;
        }
        #endregion
    }
}
